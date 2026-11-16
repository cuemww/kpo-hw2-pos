using System.Text.Json;
using Pos.Domain;

namespace Pos.Infrastructure;

internal sealed class JsonFile<T>
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;

    public JsonFile(string path) => _path = path;

    public List<T> Read()
    {
        if (!File.Exists(_path)) return new List<T>();
        return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(_path)) ?? new List<T>();
    }

    public void Write(IEnumerable<T> items)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (dir is not null) Directory.CreateDirectory(dir);
        File.WriteAllText(_path, JsonSerializer.Serialize(items, Options));
    }
}

public sealed class JsonStudents : IStudentRepository
{
    private sealed record Dto(Guid Id, string Name);
    private readonly JsonFile<Dto> _file;

    public JsonStudents(string path) => _file = new JsonFile<Dto>(path);

    private List<Student> Load() => _file.Read().Select(d => new Student(d.Name, d.Id)).ToList();
    private void Dump(IEnumerable<Student> items) => _file.Write(items.Select(s => new Dto(s.Id, s.Name)));

    public void Add(Student s) => Dump(Load().Append(s));
    public Student Get(Guid id) =>
        Load().FirstOrDefault(s => s.Id == id) ?? throw new NotFoundException($"Студент '{id}' не найден.");
    public IReadOnlyList<Student> List() => Load();
    public void Remove(Guid id) { Get(id); Dump(Load().Where(s => s.Id != id)); }
}

public sealed class JsonRubric : IRubricRepository
{
    private sealed record Dto(Guid Id, string Title, double MaxScore, double Weight);
    private readonly JsonFile<Dto> _file;

    public JsonRubric(string path) => _file = new JsonFile<Dto>(path);

    public Rubric Get() =>
        new(_file.Read().Select(d => new Criterion(d.Title, d.MaxScore, new Weight(d.Weight), d.Id)));

    public void Save(Rubric rubric) =>
        _file.Write(rubric.Criteria.Select(c => new Dto(c.Id, c.Title, c.MaxScore, c.Weight.Value)));
}

public sealed class JsonEvaluations : IEvaluationRepository
{
    private sealed record Dto(Guid Id, Guid StudentId, Guid CriterionId, double Score, string Comment, string Date);
    private readonly JsonFile<Dto> _file;
    private readonly IRubricRepository _rubrics;

    public JsonEvaluations(string path, IRubricRepository rubrics)
    {
        _file = new JsonFile<Dto>(path);
        _rubrics = rubrics;
    }

    private List<Evaluation> Load()
    {
        var rubric = _rubrics.Get();
        return _file.Read().Select(d => new Evaluation(d.StudentId, rubric.Get(d.CriterionId),
            new Score(d.Score), d.Comment, DateOnly.Parse(d.Date), d.Id)).ToList();
    }

    private void Dump(IEnumerable<Evaluation> items) =>
        _file.Write(items.Select(e => new Dto(e.Id, e.StudentId, e.CriterionId, e.Score.Value,
                                              e.Comment, e.Date.ToString("yyyy-MM-dd"))));

    public void Save(Evaluation e) => Dump(Load().Where(x => x.Id != e.Id).Append(e));
    public Evaluation? Find(Guid studentId, Guid criterionId) =>
        Load().FirstOrDefault(e => e.StudentId == studentId && e.CriterionId == criterionId);
    public IReadOnlyList<Evaluation> ByStudent(Guid studentId) => Load().Where(e => e.StudentId == studentId).ToList();
    public void Delete(Guid id) => Dump(Load().Where(e => e.Id != id));
}
