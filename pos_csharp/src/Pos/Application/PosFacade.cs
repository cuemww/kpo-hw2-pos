namespace Pos.Application;

public sealed class PosFacade
{
    private readonly IEnumerable<IGradeImporter> _importers;

    public PosFacade(StudentUseCases students, RubricUseCases rubric, EvaluationUseCases evaluations,
                     ResultUseCases results, IEnumerable<IGradeImporter> importers)
    {
        Students = students;
        Rubric = rubric;
        Evaluations = evaluations;
        Results = results;
        _importers = importers;
    }

    public StudentUseCases Students { get; }
    public RubricUseCases Rubric { get; }
    public EvaluationUseCases Evaluations { get; }
    public ResultUseCases Results { get; }

    public ImportResult ImportGrades(string path)
    {
        var importer = _importers.FirstOrDefault(i => i.CanHandle(path))
                       ?? throw new Pos.Domain.DomainException("Поддерживаются только .csv и .json.");
        return importer.Run(path);
    }
}
