using System.Collections.Concurrent;

namespace Avemepls.Infrastructure.Identity;

/// <summary>
/// Хранилище пользователей, которые находятся в данный момент онлайн
/// </summary>
public class OnlineUsersStore
{
    private readonly ConcurrentDictionary<string, OnlineUser> _onlineUsers = new();

    /// <summary>
    /// Добавить пользователя в список онлайн
    /// </summary>
    public async Task Online(OnlineUser onlineUser)
    {
        await onlineUser.Initialize();
        _onlineUsers.TryAdd(onlineUser.Name!, onlineUser);
    }

    /// <summary>
    /// Удалить пользователя из списка онлайн
    /// </summary>
    /// <param name="identity">Пользователь</param>
    public void Offline(OnlineUser? identity)
    {
        if (identity?.Name is null)
            return;

        _onlineUsers.TryRemove(identity.Name, out _);
    }

    /// <summary>
    /// Сбросить (очистить) список онлайн-пользователей
    /// </summary>
    public void Reset()
    {
        _onlineUsers.Clear();
    }

    /// <summary>
    /// Получить информацию по онлайн-пользователю по его логину
    /// </summary>
    public OnlineUser? GetOnlineUser(string name)
    {
        return _onlineUsers.GetValueOrDefault(name);
    }

    /// <summary>
    /// Список онлайн-пользователей
    /// </summary>
    public IEnumerable<OnlineUser> OnlineUsers => _onlineUsers.Values;
}