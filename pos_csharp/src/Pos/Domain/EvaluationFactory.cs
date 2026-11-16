namespace Pos.Domain;

public sealed class EvaluationFactory
{
    private readonly Rubric _rubric;

    public EvaluationFactory(Rubric rubric) => _rubric = rubric;

    public Evaluation Create(Guid studentId, Guid criterionId, double value, string? comment = null)
    {
        var criterion = _rubric.Get(criterionId);
        return new Evaluation(studentId, criterion, new Score(value), comment);
    }
}
