namespace Pos.Domain;

public sealed record Score
{
    public double Value { get; }

    public Score(double value)
    {
        if (double.IsNaN(value) || value < 0)
            throw new ScoreOutOfRangeException("Балл не может быть отрицательным.");
        Value = value;
    }
}

public sealed record Weight
{
    public double Value { get; }

    public Weight(double value)
    {
        if (double.IsNaN(value) || value <= 0)
            throw new DomainException("Вес критерия должен быть положительным.");
        Value = value;
    }
}
