using Microsoft.EntityFrameworkCore;

namespace Avemepls.ServiceBus.Common;

/// <summary>
/// Для поддержки транзакционности публикаций событий cross domain
/// </summary>
public class CrossDomainOutBox
{
    internal DbContext? DbContext { get; set; }
}