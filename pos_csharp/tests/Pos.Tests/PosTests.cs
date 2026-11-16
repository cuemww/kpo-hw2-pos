using Microsoft.Extensions.DependencyInjection;
using Pos;
using Pos.Application;
using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public class InvariantTests
{
    private static readonly Guid A = Guid.NewGuid();
    private static readonly Guid B = Guid.NewGuid();

    private static Rubric MakeRubric() => new(new[]
    {
        new Criterion("A", 10, new Weight(1), A),
        new Criterion("B", 5, new Weight(3), B),
    });

    [Fact]
    public void Score_above_max_is_rejected() =>
        Assert.Throws<ScoreOutOfRangeException>(() =>
            new EvaluationFactory(MakeRubric()).Create(Guid.NewGuid(), A, 12));

    [Fact]
    public void Negative_score_is_rejected() =>
        Assert.Throws<ScoreOutOfRangeException>(() => new Score(-1));

    [Fact]
    public void Unknown_criterion_is_rejected() =>
        Assert.Throws<UnknownCriterionException>(() =>
            new EvaluationFactory(MakeRubric()).Create(Guid.NewGuid(), Guid.NewGuid(), 1));

    [Fact]
    public void Correct_keeps_invariant()
    {
        var rubric = MakeRubric();
        var e = new EvaluationFactory(rubric).Create(Guid.NewGuid(), A, 5);
        Assert.Throws<ScoreOutOfRangeException>(() => e.Correct(rubric.Get(A), new Score(11)));
        Assert.Equal(5, e.Score.Value);
    }

    [Fact]
    public void Duplicate_criterion_is_rejected() =>
        Assert.Throws<DuplicateException>(() => MakeRubric().Add(new Criterion("a", 3, new Weight(1))));
}

public class ScoringTests
{
    [Fact]
    public void Weighted_average_counts_missing_as_zero()
    {
        var a = new Criterion("A", 10, new Weight(1));
        var b = new Criterion("B", 5, new Weight(3));
        var rubric = new Rubric(new[] { a, b });
        var student = Guid.NewGuid();
        var evals = new[] { new Evaluation(student, a, new Score(10)), new Evaluation(student, b, new Score(0)) };
        Assert.Equal(2.5, new FinalScoreCalculator(new WeightedAverageStrategy()).Calculate(rubric, evals), 6);
    }

    [Fact]
    public void Rounding_decorator_rounds()
    {
        var a = new Criterion("A", 10, new Weight(1));
        var b = new Criterion("B", 5, new Weight(3));
        var rubric = new Rubric(new[] { a, b });
        var evals = new[] { new Evaluation(Guid.NewGuid(), a, new Score(7)) };
        var rounded = new RoundingDecorator(new WeightedAverageStrategy(), 1);
        Assert.Equal(1.8, rounded.Compute(rubric, evals));
    }
}

public class AppFlowTests
{
    [Fact]
    public void Grade_correct_cancel_flow()
    {
        using var provider = Composition.Build(null);
        var app = provider.GetRequiredService<PosFacade>();
        var s = app.Students.Add("X");
        var c = app.Rubric.AddCriterion("T", 10, 1);
        app.Evaluations.Grade(s.Id, c.Id, 5);
        app.Evaluations.Grade(s.Id, c.Id, 8);
        Assert.Equal(8.0, app.Results.Final(s.Id));
        app.Evaluations.Cancel(s.Id, c.Id);
        Assert.Equal(0.0, app.Results.Final(s.Id));
    }
}
