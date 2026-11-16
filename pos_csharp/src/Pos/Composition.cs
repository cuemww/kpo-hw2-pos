using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos;

public static class Composition
{
    public static ServiceProvider Build(string? dataDir)
    {
        var s = new ServiceCollection();
        if (dataDir is null)
        {
            s.AddSingleton<IStudentRepository, InMemoryStudents>();
            s.AddSingleton<IRubricRepository, InMemoryRubric>();
            s.AddSingleton<IEvaluationRepository, InMemoryEvaluations>();
        }
        else
        {
            s.AddSingleton<IStudentRepository>(_ => new JsonStudents(Path.Combine(dataDir, "students.json")));
            s.AddSingleton<IRubricRepository>(_ => new JsonRubric(Path.Combine(dataDir, "rubric.json")));
            s.AddSingleton<IEvaluationRepository>(p => new JsonEvaluations(
                Path.Combine(dataDir, "evaluations.json"), p.GetRequiredService<IRubricRepository>()));
        }
        s.AddSingleton<IResultExporter, CsvResultExporter>();
        s.AddSingleton<IScoringStrategy>(new RoundingDecorator(new WeightedAverageStrategy(), 2));
        s.AddSingleton<FinalScoreCalculator>();
        s.AddSingleton<StudentUseCases>();
        s.AddSingleton<RubricUseCases>();
        s.AddSingleton<EvaluationUseCases>();
        s.AddSingleton<ResultUseCases>();
        s.AddSingleton<IGradeImporter, CsvGradeImporter>();
        s.AddSingleton<IGradeImporter, JsonGradeImporter>();
        s.AddSingleton<PosFacade>();
        return s.BuildServiceProvider();
    }
}
