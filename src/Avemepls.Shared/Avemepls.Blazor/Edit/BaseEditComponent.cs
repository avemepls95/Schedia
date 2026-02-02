using System.Security.Claims;

using AntDesign;

using Avemepls.Blazor.Common;
using Avemepls.Blazor.MediatR;
using Avemepls.Blazor.Navigation;
using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Commands;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Queries;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Avemepls.Blazor.Edit;

public abstract class BaseEditComponent<TModel, TCreateUpdateModel> : BaseEditComponent<TModel, int, TCreateUpdateModel>
    where TModel : class
    where TCreateUpdateModel : class, new()
{
}

#pragma warning disable SA1402
public abstract class BaseEditComponent<TModel, TId, TCreateUpdateModel> : ServiceScopedComponentBase
#pragma warning restore SA1402
    where TModel : class
    where TCreateUpdateModel : class, new()
{
    /// <summary>
    /// Происходит ли сейчас загрузка
    /// </summary>
#pragma warning disable SA1401
    protected bool IsLoading { get; set; } = true;
#pragma warning restore SA1401

    /// <summary>
    /// Редактируемая модель
    /// </summary>
    protected TCreateUpdateModel? Model { get; set; }

    /// <summary>
    /// Поулченная для редактирования модель
    /// </summary>
    protected TModel? ReadModel { get; set; }

    /// <summary>
    /// Нужно ли возвращаться на предыдущую страницу после сохранения
    /// </summary>
    protected virtual bool GoBackAfterSave => true;

    /// <summary>
    /// Идентификатор редактируемого объекта (0 - создание новаого объекта)
    /// </summary>
    [Parameter]
    public TId Id { get; set; }

    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject]
    protected IStringLocalizer<BaseEditComponent<TModel, TId, TCreateUpdateModel>> Loc { get; set; }

    [Inject]
    public MessageService Messages { get; set; }

    [Inject]
    public IScopedMediator Mediator { get; set; }

    [Inject]
    public IMapper Mapper { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public NavigationHistoryManager NavigationHistoryManager { get; set; }

    [Inject]
    public IAuthorizationService AuthorizationService { get; set; }

    [Inject]
    protected ILogger<BaseEditComponent<TModel, TId, TCreateUpdateModel>> Logger { get; set; }

    protected ClaimsPrincipal User { get; private set; }

    /// <summary>
    /// Url для перехода при успешном сохранении
    /// </summary>
    public virtual string? SuccessUrl => null;

    /// <summary>
    /// Url для перехода на предыдущую страницу
    /// (испльзуется при невозможности использовать <see cref="NavigationHistoryManager"/>
    /// </summary>
    public virtual string? GoBackUrl => null;

    /// <summary>
    /// Название объекта
    /// </summary>
    public abstract string EntityName { get; }

    /// <summary>
    /// Разрешение на создание объекта
    /// </summary>
    public abstract string? AddPermission { get; }

    /// <summary>
    /// Разрешение на редактирование объекта
    /// </summary>
    public abstract string? EditPermission { get; }

    /// <summary>
    /// Разрешение на просмотр объекта
    /// </summary>
    public abstract string OpenPermission { get; }

    /// <summary>
    /// Разрешение на удаление объекта
    /// </summary>
    public virtual string DeletePermission { get; }

    /// <summary>
    /// Разрешение на восстановление объекта
    /// </summary>
    public virtual string RestorePermission { get; }

    /// <summary>
    /// Происходит ли сейчас сохранение
    /// </summary>
    protected bool IsSaving { get; set; }

    /// <summary>
    /// Необходимое разрешение в зависимости от операции
    /// </summary>
    public string? CurrentActionPermission => Equals(Id, default(TId))
        ? AddPermission
        : EditPermission;

    protected bool HasErrors { get; set; }

    protected override async Task OnInitializedAsync()
    {
        User = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
    }

    /// <summary>
    /// Создание GetById запроса для указанного типа
    /// </summary>
    protected abstract GetEntityByIdQuery<TModel, TId> BuildGetQuery(TId id);

    /// <summary>
    /// Создание команды создания/обновления на основе редактируемой модели
    /// </summary>
    protected abstract IRequest<TId> BuildCreateUpdateCommand();

    /// <summary>
    /// Occurs when save button is pressed
    /// </summary>
    protected virtual async Task OnSave()
    {
        if (!await OnBeforeSave() || IsSaving)
        {
            return;
        }

        IsSaving = true;
        StateHasChanged();
        try
        {
            if ((Equals(Id, default(TId)) && AddPermission != null
                                          && !(await AuthorizationService.AuthorizeAsync(User, AddPermission))
                                              .Succeeded)
                || (!Equals(Id, default(TId)) && EditPermission != null
                                              && !(await AuthorizationService.AuthorizeAsync(User, EditPermission))
                                                  .Succeeded))
            {
                throw new AccessDeniedException(Loc["Отказано в доступе."]);
            }

            var command = BuildCreateUpdateCommand();
            var result = await Mediator.Send(command);
            Id = result;
            StateHasChanged();

            await OnAfterSave(result);

            _ = Messages.SuccessAsync(string.Format(Loc["'{0}' успешно сохранен(а)"], EntityName));
        }
        catch (ValidationException ex)
        {
            await OnSaveValidationError(ex);
        }
        catch (Exception ex)
        {
            await OnSaveError(ex);
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handler for validation exceptions during save. Default - shows message
    /// </summary>
    /// <param name="ex">ValidationException</param>
    protected virtual Task OnSaveValidationError(ValidationException ex)
    {
        var message = ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message;
        _ = Messages.WarningAsync(Loc["Предупреждение: "] + message);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handler for exceptions during save. Default - shows message and throws
    /// </summary>
    /// <param name="ex">Exception</param>
    protected virtual Task OnSaveError(Exception ex)
    {
        Logger.LogError(ex, "{Message}", Loc["Ошибка при сохранении"]);
        Messages.ErrorAsync(Loc["Ошибка при сохранении: "] + ex.Message);

        if (ex.InnerException != null)
            Messages.ErrorAsync(Loc["Подробности: "] + ex.InnerException.Message);

        throw ex;
    }

    /// <summary>
    /// Действия при успешном сохранении
    /// </summary>
    protected virtual async Task OnAfterSave(TId id)
    {
        if (GoBackAfterSave)
        {
            if (string.IsNullOrEmpty(SuccessUrl) || await IsPrevPage(SuccessUrl))
            {
                await GoBack();

                return;
            }

            NavigationManager.NavigateTo(SuccessUrl);
        }
    }

    private async Task<bool> IsPrevPage(string url)
    {
        var prevPage = await NavigationHistoryManager.GetPrevPage();

        if (prevPage is not null)
        {
            var prevPageUri = new Uri(prevPage.Url);

            var urlIsPrevPage = prevPageUri.AbsolutePath.Trim('/') == url.Trim('/');

            if (urlIsPrevPage)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Действия перед сохранением
    /// </summary>
    protected virtual Task<bool> OnBeforeSave()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Действия при ошибке валидации
    /// </summary>
    protected virtual Task OnFinishFailed(EditContext context)
    {
        HasErrors = true;
        _ = Messages.ErrorAsync(Loc["Ошибка при сохранении"].Value);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Действия при успешном удалении
    /// </summary>
    protected virtual async Task OnDelete<TDModel>()
    {
        if (IsSaving)
        {
            return;
        }

        try
        {
            IsSaving = true;

            var command = new DeleteCommand<TDModel, TId>(Id);
            await Mediator.Send(command);

            await GoBack();

            _ = Messages.SuccessAsync(string.Format(Loc["'{0}' успешно удален(а)"], EntityName));
        }
        catch (Exception ex)
        {
            _ = Messages.ErrorAsync(ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Вернуться к предыдущему Url
    /// </summary>
    protected virtual async Task GoBack()
    {
        if (await NavigationHistoryManager.CanGoBack())
        {
            NavigationManager.NavigateTo((await NavigationHistoryManager.GoToPreviousPage()).Url);
        }
        else if (!string.IsNullOrEmpty(GoBackUrl))
        {
            NavigationManager.NavigateTo(GoBackUrl);
        }
    }

    /// <summary>
    /// Действия при успешном восстановлении
    /// </summary>
    protected virtual async Task OnRestore<TRModel>()
        where TRModel : IHasDateDeleted
    {
        if (IsSaving)
        {
            return;
        }

        try
        {
            IsSaving = true;

            var command = new RestoreCommand<TRModel, TId>(Id);
            await Mediator.Send(command);

            _ = Messages.SuccessAsync(string.Format(Loc["'{0}' успешно восстановлен(а)"], EntityName));
            await Reload();
        }
        catch (Exception ex)
        {
            _ = Messages.ErrorAsync(ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Конвертация Id со специальным значением в default, чтобы активировать создание нового объекта
        // вместо поиска несуществующей записи в базе данных
        if (Id is string strId && strId == "0")
            Id = default!;

        await base.OnParametersSetAsync();
        await Reload();
    }

    /// <summary>
    /// Обновление/сброс данных редактируемой модели
    /// </summary>
    protected async Task Reload()
    {
        try
        {
            IsLoading = true;

            if (!Equals(Id, default(TId)) && !Equals(Id, default))
            {
                ReadModel = await Mediator.Send(BuildGetQuery(Id));
                Model = Mapper.Map<TCreateUpdateModel>(ReadModel);
            }
            else
            {
                Model = CreateUpdateModelBuilder();
            }
        }
#pragma warning disable S2139
        catch (Exception ex)
#pragma warning restore S2139
        {
            Logger.LogError(ex, "{Message}", Loc["Ошибка при загрузке карточки"]);
            _ = Messages.ErrorAsync(ex.Message);

            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Создание редактируемой модели по умолчанию для добавления объекта
    /// </summary>
    protected virtual TCreateUpdateModel CreateUpdateModelBuilder()
    {
        return new TCreateUpdateModel();
    }
}