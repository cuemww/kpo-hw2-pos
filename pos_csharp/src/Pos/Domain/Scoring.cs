namespace Pos.Domain;

public interface IScoringStrategy
{
    double Compute(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations);
}

public sealed class WeightedAverageStrategy : IScoringStrategy
{
    public double Compute(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations)
    {
        double totalWeight = rubric.Criteria.Sum(c => c.Weight.Value);
        if (totalWeight == 0) return 0;
        double acc = 0;
        foreach (var c in rubric.Criteria)
        {
            var e = evaluations.FirstOrDefault(x => x.CriterionId == c.Id);
            if (e is not null) acc += c.Weight.Value * e.Score.Value / c.MaxScore;
        }
        return 10 * acc / totalWeight;
    }
}

public sealed class RatedOnlyStrategy : IScoringStrategy
{
    public double Compute(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations)
    {
        double acc = 0, weights = 0;
        foreach (var e in evaluations)
        {
            var c = rubric.Get(e.CriterionId);
            acc += c.Weight.Value * e.Score.Value / c.MaxScore;
            weights += c.Weight.Value;
        }
        return weights == 0 ? 0 : 10 * acc / weights;
    }
}

public sealed class RoundingDecorator : IScoringStrategy
{
    private readonly IScoringStrategy _inner;
    private readonly int _digits;

    public RoundingDecorator(IScoringStrategy inner, int digits = 2)
    {
        _inner = inner;
        _digits = digits;
    }

    public double Compute(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations) =>
        Math.Round(_inner.Compute(rubric, evaluations), _digits, MidpointRounding.AwayFromZero);
}

public sealed class FinalScoreCalculator
{
    private readonly IScoringStrategy _strategy;

    public FinalScoreCalculator(IScoringStrategy strategy) => _strategy = strategy;

    public double Calculate(Rubric rubric, IEnumerable<Evaluation> evaluations) =>
        _strategy.Compute(rubric, evaluations.ToList());
}
