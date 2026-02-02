using Avemepls.Core.Models;
using Avemepls.Domain.Commands;
using Avemepls.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Avemepls.Mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TCreateUpdateModel, TSlimModel, TDetailedModel, TListQuery>(IMediator mediator)
    : CrudController<TEntity, TCreateUpdateModel, TSlimModel, TDetailedModel, TListQuery, int>(mediator)
    where TEntity : class, IHasId<int>
    where TCreateUpdateModel : class
    where TSlimModel : IHasId<int>
    where TDetailedModel : IHasId<int>
    where TListQuery : IRequest<PagedResponse<TSlimModel>>
{
    protected abstract override GetEntityByIdQuery<TDetailedModel, int> BuildGetByIdQuery(in int id);
}

/// <summary>
/// CQRS CRUD controller.
/// </summary>
/// <typeparam name="TEntity">Сущность в БД</typeparam>
/// <typeparam name="TCreateUpdateModel">Модель для создания/обновления сущности</typeparam>
/// <typeparam name="TSlimModel">Упрощённая модель для запроса на получение списка элементов</typeparam>
/// <typeparam name="TDetailedModel">Детализированная модель для запроса на получение элемента по id</typeparam>
/// <typeparam name="TListQuery">Запрос на поиск списка</typeparam>
/// <typeparam name="TId">Тип идентификатора</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TCreateUpdateModel, TSlimModel, TDetailedModel, TListQuery, TId
>(IMediator mediator) : CrudController<TEntity, TCreateUpdateModel, TCreateUpdateModel, TSlimModel, TDetailedModel, TListQuery, TId>(mediator)
    where TEntity : class, IHasId<TId>
    where TCreateUpdateModel : class
    where TSlimModel : IHasId<TId>
    where TDetailedModel : IHasId<TId>
    where TListQuery : IRequest<PagedResponse<TSlimModel>>
{
    protected abstract CreateUpdateCommand<TId, TCreateUpdateModel> BuildCreateUpdateCommand(TId? id, TCreateUpdateModel model);

    protected override IRequest<TId> BuildCreateCommand(TCreateUpdateModel model)
    {
        return BuildCreateUpdateCommand(default, model);
    }

    protected override IRequest<TId> BuildUpdateCommand(TId id, TCreateUpdateModel model)
    {
        return BuildCreateUpdateCommand(id, model);
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
/// <typeparam name="TId">Тип идентификатора</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TCreateModel, TUpdateModel, TSlimModel, TDetailedModel, TListQuery, TId> : ControllerBase
    where TEntity : class, IHasId<TId>
    where TCreateModel : class
    where TUpdateModel : class
    where TSlimModel : IHasId<TId>
    where TDetailedModel : IHasId<TId>
    where TListQuery : IRequest<PagedResponse<TSlimModel>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of <see cref="CrudController{TEntity,TModel,TSlimModel,TDetailedModel,TListQuery,TId}"/>.
    /// </summary>
    protected CrudController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Создание сущности.
    /// </summary>
    /// <param name="model">Информация о сущности.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync([FromBody] TCreateModel model, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(BuildCreateCommand(model), cancellationToken);

        // ReSharper disable once Mvc.ActionNotResolved
        return CreatedAtAction("Get", new { id }, await _mediator.Send(BuildGetByIdQuery(id), cancellationToken));
    }

    /// <summary>
    /// Обновление сущности.
    /// </summary>
    /// <param name="id">Тип идентификатора.</param>
    /// <param name="model">Информация о сущности.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    [HttpPut("{id}")]
    public virtual async Task<IActionResult> UpdateAsync(TId id, [FromBody] TUpdateModel model, CancellationToken cancellationToken)
    {
        await _mediator.Send(BuildUpdateCommand(id, model), cancellationToken);

        return Ok(await _mediator.Send(BuildGetByIdQuery(id), cancellationToken));
    }

    /// <summary>
    /// Удаление сущности.
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="cancellationToken">Токен для отмены операции</param>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> DeleteAsync([FromRoute] TId id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCommand<TEntity, TId>(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Получение детализированной информации о сущности.
    /// </summary>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetAsync([FromRoute] TId id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(BuildGetByIdQuery(id), cancellationToken));
    }

    /// <summary>
    /// Получение списка элементов.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    [HttpGet]
    public virtual async Task<IActionResult> ListAsync([FromQuery] TListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Фабрика для построения запроса на получение модели по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    protected abstract GetEntityByIdQuery<TDetailedModel, TId> BuildGetByIdQuery(in TId id);

    protected abstract IRequest<TId> BuildCreateCommand(TCreateModel model);

    protected abstract IRequest<TId> BuildUpdateCommand(TId id, TUpdateModel model);
}