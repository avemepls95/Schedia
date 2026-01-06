namespace Avemepls.Core.Models;

public interface IPagedResponse<TModel>
{
    TModel[] Results { get; set; }

    long Count { get; set; }
}