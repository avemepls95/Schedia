namespace Avemepls.Core.Models;

public readonly record struct Id<T>(int Value)
    where T : class
{
    public static explicit operator int(Id<T> id) => id.Value;
    public static explicit operator Id<T>(int value) => new(value);

    public override string ToString() => $"{typeof(T).Name}({Value})";

    public bool IsEmpty => Value == 0;
    public bool IsNotEmpty => Value != 0;

    public static bool operator ==(Id<T> left, int right)
        => left.Value == right;

    public static bool operator !=(Id<T> left, int right)
        => left.Value != right;

    public static bool operator ==(int left, Id<T> right)
        => left == right.Value;

    public static bool operator !=(int left, Id<T> right)
        => left != right.Value;
}