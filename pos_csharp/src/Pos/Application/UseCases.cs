using Pos.Domain;

namespace Pos.Application;

public sealed class StudentUseCases
{
    private readonly IStudentRepository _students;
    private readonly IEvaluationRepository _evals;

    public StudentUseCases(IStudentRepository students, IEvaluationRepository evals)
    {
        _students = students;
        _evals = evals;
    }

    public Student Add(string name)
    {
        var student = new Student(name);
        if (_students.List().Any(s => string.Equals(s.Name, student.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DuplicateException($"Студент '{student.Name}' уже есть.");
        _students.Add(student);
        return student;
    }

    public IReadOnlyList<Student> List() => _students.List();

    public void Remove(Guid id)
    {
        _students.Get(id);
        foreach (var e in _evals.ByStudent(id)) _evals.Delete(e.Id);
        _students.Remove(id);
    }
}

public sealed class RubricUseCases
{
    private readonly IRubricRepository _rubrics;

    public RubricUseCases(IRubricRepository rubrics) => _rubrics = rubrics;

    public Criterion AddCriterion(string title, double maxScore, double weight)
    {
        var rubric = _rubrics.Get();
        var c = new Criterion(title, maxScore, new Weight(weight));
        rubric.Add(c);
        _rubrics.Save(rubric);
        return c;
    }

    public void RemoveCriterion(Guid id)
    {
        var rubric = _rubrics.Get();
        rubric.Remove(id);
        _rubrics.Save(rubric);
    }

    public IReadOnlyList<Criterion> List() => _rubrics.Get().Criteria;
}

public sealed class EvaluationUseCases
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly IEvaluationRepository _evals;

    public EvaluationUseCases(IStudentRepository students, IRubricRepository rubrics, IEvaluationRepository evals)
    {
        _students = students;
        _rubrics = rubrics;
        _evals = evals;
    }

    public Evaluation Grade(Guid studentId, Guid criterionId, double value, string? comment = null)
    {
        _students.Get(studentId);
        var rubric = _rubrics.Get();
        var existing = _evals.Find(studentId, criterionId);
        if (existing is not null)
        {
            existing.Correct(rubric.Get(criterionId), new Score(value),
                             string.IsNullOrEmpty(comment) ? null : comment);
            _evals.Save(existing);
            return existing;
        }
        var created = new EvaluationFactory(rubric).Create(studentId, criterionId, value, comment);
        _evals.Save(created);
        return created;
    }

    public void Cancel(Guid studentId, Guid criterionId)
    {
        var e = _evals.Find(studentId, criterionId)
                ?? throw new NotFoundException("Такой оценки нет.");
        _evals.Delete(e.Id);
    }

    public IReadOnlyList<Evaluation> OfStudent(Guid studentId)
    {
        _students.Get(studentId);
        return _evals.ByStudent(studentId);
    }
}

public sealed class ResultUseCases
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly IEvaluationRepository _evals;
    private readonly FinalScoreCalculator _calculator;
    private readonly IResultExporter _exporter;

    public ResultUseCases(IStudentRepository students, IRubricRepository rubrics, IEvaluationRepository evals,
                          FinalScoreCalculator calculator, IResultExporter exporter)
    {
        _students = students;
        _rubrics = rubrics;
        _evals = evals;
        _calculator = calculator;
        _exporter = exporter;
    }

    public double Final(Guid studentId) =>
        _calculator.Calculate(_rubrics.Get(), _evals.ByStudent(studentId));

    public IReadOnlyList<ResultRow> Table() =>
        _students.List().Select(s => new ResultRow(s.Id, s.Name, Final(s.Id))).ToList();

    public void Export(string path) => _exporter.Export(Table(), path);
}
