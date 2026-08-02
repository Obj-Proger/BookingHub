namespace BookingHub.Domain.Tests.TestDoubles;

/// <summary>A minimal concrete <see cref="ValueObject"/>, used only to exercise the base class's equality logic.</summary>
internal sealed class SampleValueObject(string first, int second) : ValueObject
{
    public string First { get; } = first;
    public int Second { get; } = second;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return First;
        yield return Second;
    }
}