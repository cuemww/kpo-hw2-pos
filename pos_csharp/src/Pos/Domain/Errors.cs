namespace Pos.Domain;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public sealed class ScoreOutOfRangeException : DomainException
{
    public ScoreOutOfRangeException(string message) : base(message) { }
}

public sealed class UnknownCriterionException : DomainException
{
    public UnknownCriterionException(string message) : base(message) { }
}

public sealed class DuplicateException : DomainException
{
    public DuplicateException(string message) : base(message) { }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}
