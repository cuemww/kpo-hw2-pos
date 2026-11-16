using Pos.Domain;

namespace Pos.Application;

public sealed record GradeRow(string Student, string Criterion, double Score, string Comment);

public sealed record ImportResult(int Imported, IReadOnlyList<string> Errors);

public interface IGradeImporter
{
    bool CanHandle(string path);
    ImportResult Run(string path);
}

public abstract class GradeImporter : IGradeImporter
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly EvaluationUseCases _evaluations;

    protected GradeImporter(IStudentRepository students, IRubricRepository rubrics, EvaluationUseCases evaluations)
    {
        _students = students;
        _rubrics = rubrics;
        _evaluations = evaluations;
    }

    public abstract bool CanHandle(string path);

    protected abstract IEnumerable<GradeRow> Parse(string path);

    public ImportResult Run(string path)
    {
        int ok = 0;
        var errors = new List<string>();
        int line = 0;
        foreach (var row in Parse(path))
        {
            line++;
            try
            {
                var student = _students.List().FirstOrDefault(s =>
                                  string.Equals(s.Name, row.Student.Trim(), StringComparison.OrdinalIgnoreCase))
                              ?? throw new NotFoundException($"Студент '{row.Student}' не найден.");
                var criterion = _rubrics.Get().FindByTitle(row.Criterion);
                _evaluations.Grade(student.Id, criterion.Id, row.Score, row.Comment);
                ok++;
            }
            catch (DomainException ex)
            {
                errors.Add($"строка {line}: {ex.Message}");
            }
        }
        return new ImportResult(ok, errors);
    }
}
