using System.Diagnostics;

namespace Avemepls.Domain.Models;

/// <summary>
/// Модель, предоставляющая информацию о возможности выполнения действия
/// </summary>
[DebuggerDisplay("Can be executed: {CanExecute}, can be viewed: {CanView}")]
public class CommandInfoModel
{
    /// <summary>
    /// Может ли быть выполнено
    /// </summary>
    public bool CanExecute { get; set; }

    /// <summary>
    /// Нужно ли показывать команду
    /// </summary>
    public bool CanView { get; set; }

    /// <summary>
    /// Сообщения об ошибках
    /// Заполняется, если выполнить команду нельзя
    /// </summary>
    public string[]? UnavailabilityReasons { get; set; }

    public CommandInfoModel(bool canExecute, bool canView = true)
    {
        CanView = canView;
        CanExecute = canExecute && canView;
    }

    public CommandInfoModel(params string[] unavailabilityReasons)
    {
        if (unavailabilityReasons.Length == 0)
        {
            CanExecute = true;
            CanView = true;
        }

        UnavailabilityReasons = unavailabilityReasons;
    }

    public CommandInfoModel()
    {
        // parameterless constructor for deserialization
        CanExecute = true;
        CanView = true;
    }
}