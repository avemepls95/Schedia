using Avemepls.Core.Models;
using Avemepls.Domain.Commands;
using Avemepls.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Avemepls.Mvc.Controllers;

/// <summary>
/// CQRS CRUD controller.
/// </summary>
/// <typeparam name="TEntity">Сущность в БД</typeparam>
/// <typeparam name="TCreateUpdateModel">Модель для создания/обновления сущности</typeparam>
/// <typeparam name="TSlimModel">Упрощённая модель для запроса на получение списка элементов</typeparam>
/// <typeparam name="TDetailedModel">Детализированная модель для запроса на получение элемента по id</typeparam>
/// <typeparam name="TListQuery">Запрос на поиск списка</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TCreateUpdateModel, TSlimModel, TDetailedModel, TListQuery>(IMediator mediator)
    : CrudController<TEntity, TCreateUpdateModel, TCreateUpdateModel, TSlimModel, TDetailedModel, TListQuery>(mediator)
    where TEntity : class, IHasId<TEntity>
    where TCreateUpdateModel : class, IHasId<TCreateUpdateModel>
    where TSlimModel : class, IHasId<TSlimModel>
    where TDetailedModel : class, IHasId<TDetailedModel>
    where TListQuery : IRequest<PagedResponse<TSlimModel>>
{
    protected abstract CreateUpdateCommand<TEntity> BuildCreateUpdateCommand(Id<TEntity>? id, TCreateUpdateModel model);

    protected override IRequest<Id<TEntity>> BuildCreateCommand(TCreateUpdateModel model)
    {
        return BuildCreateUpdateCommand(null, model);
    }

    protected override IRequest BuildUpdateCommand(Id<TEntity> id, TCreateUpdateModel model)
    {
        return BuildCreateUpdateCommand(id, model) as IRequest;
    }
}

/// <summary>
/// CQRS CRUD controller.
/// </summary>
/// <typeparam name="TEntity">Сущность в БД</typeparam>
/// <typeparam name="TCreateModel">Модель для создания сущности</typeparam>
/// <typeparam name="TUpdateModel">Модель для обновления сущности</typeparam>
/// <typeparam name="TSlimModel">Упрощённая модель для запроса на получение списка элементов</typeparam>
/// <typeparam name="TDetailedModel">Детализированная модель для запроса на получение элемента по id</typeparam>
/// <typeparam name="TListQuery">Запрос на поиск списка</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TCreateModel, TUpdateModel, TSlimModel, TDetailedModel, TListQuery> : ControllerBase
    where TEntity : class, IHasId<TEntity>
    where TCreateModel : class
    where TUpdateModel : class
    where TSlimModel : class, IHasId<TSlimModel>
    where TDetailedModel : class, IHasId<TDetailedModel>
    where TListQuery : IRequest<PagedResponse<TSlimModel>>
{
    protected IMediator Mediator { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CrudController{TEntity,TModel,TSlimModel,TDetailedModel,TListQuery,TId}"/>.
    /// </summary>
    protected CrudController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Создание сущности.
    /// </summary>
    /// <param name="model">Информация о сущности.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync([FromBody] TCreateModel model, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(BuildCreateCommand(model), cancellationToken);

        // ReSharper disable once Mvc.ActionNotResolved
        return CreatedAtAction("Get", new { id }, await Mediator.Send(BuildGetByIdQuery(id), cancellationToken));
    }

    /// <summary>
    /// Обновление сущности.
    /// </summary>
    /// <param name="id">Тип идентификатора.</param>
    /// <param name="model">Информация о сущности.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    [HttpPut("{id}")]
    public virtual async Task<IActionResult> UpdateAsync(Id<TUpdateModel> id, [FromBody] TUpdateModel model, CancellationToken cancellationToken)
    {
        await Mediator.Send(BuildUpdateCommand(new Id<TEntity>(id.Value), model), cancellationToken);

        return Ok(await Mediator.Send(BuildGetByIdQuery(new Id<TEntity>(id.Value)), cancellationToken));
    }

    /// <summary>
    /// Удаление сущности.
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="cancellationToken">Токен для отмены операции</param>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> DeleteAsync([FromRoute] Id<TEntity> id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCommand<TEntity>(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Получение детализированной информации о сущности.
    /// </summary>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetAsync([FromRoute] Id<TEntity> id, CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(BuildGetByIdQuery(id), cancellationToken));
    }

    /// <summary>
    /// Получение списка элементов.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    [HttpGet]
    public virtual async Task<IActionResult> ListAsync([FromQuery] TListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Фабрика для построения запроса на получение модели по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    protected abstract GetEntityByIdQuery<TDetailedModel> BuildGetByIdQuery(in Id<TEntity> id);

    protected abstract IRequest<Id<TEntity>> BuildCreateCommand(TCreateModel model);

    protected abstract IRequest BuildUpdateCommand(Id<TEntity> id, TUpdateModel model);
}