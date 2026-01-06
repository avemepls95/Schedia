namespace Avemepls.Blazor.Navigation;

/// <summary>
/// Отвечает за стек навигации в приложении, позволяя отслеживать историю навигации, реализует возможность навигации "назад".
/// </summary>
public class NavigationHistoryManager
{
    private Stack<NavigationHistoryRecord> _navStack = new();

    /// <summary>
    /// Добавляет страницу в стек навигации
    /// </summary>
    public Task AddPageToHistory(NavigationHistoryRecord page)
    {
        if (string.IsNullOrWhiteSpace(page.PageName))
        {
            return Task.CompletedTask;
        }

        if (_navStack.TryPeek(out var current) &&
            (current.PageName == page.PageName || current.Url == page.Url))
        {
            _navStack.Pop();
        }

        _navStack.Push(page);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Удаление из всей истории ссылки на страницу, например когда удалили какую-то запись
    /// </summary>
    public Task RemoveUrlFromHistory(string url)
    {
        _navStack = new Stack<NavigationHistoryRecord>(_navStack.Where(x => x.Url != url).Reverse());

        return Task.CompletedTask;
    }

    /// <summary>
    /// Указывает, возможна ли навигация "назад"
    /// </summary>
    public Task<bool> CanGoBack()
    {
        return Task.FromResult(_navStack.Count > 1);
    }

    /// <summary>
    /// Возвращает параметры страницы для навигации назад
    /// </summary>
    public Task<NavigationHistoryRecord> GoToPreviousPage()
    {
        _navStack.Pop();
        var prevPage = _navStack.Pop();

        return Task.FromResult(prevPage);
    }

    /// <summary>
    /// Возвращает параметры текущей страницы
    /// </summary>
    public Task<NavigationHistoryRecord> GetCurrentPage()
    {
        var prevPage = _navStack.Peek();

        return Task.FromResult(prevPage);
    }

    /// <summary>
    /// Возвращает параметры текущей страницы
    /// </summary>
    public Task<NavigationHistoryRecord?> GetPrevPage()
    {
        var prevPage = _navStack.ToArray().Skip(1).FirstOrDefault();

        return Task.FromResult(prevPage);
    }
}