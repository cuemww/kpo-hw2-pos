using Pos.Domain;

namespace Pos.Infrastructure;

public sealed class InMemoryStudents : IStudentRepository
{
    private readonly Dictionary<Guid, Student> _d = new();
    public void Add(Student s) => _d[s.Id] = s;
    public Student Get(Guid id) =>
        _d.TryGetValue(id, out var s) ? s : throw new NotFoundException($"Студент '{id}' не найден.");
    public IReadOnlyList<Student> List() => _d.Values.ToList();
    public void Remove(Guid id) { Get(id); _d.Remove(id); }
}

public sealed class InMemoryRubric : IRubricRepository
{
    private Rubric _r = new();
    public Rubric Get() => _r;
    public void Save(Rubric rubric) => _r = rubric;
}

public sealed class InMemoryEvaluations : IEvaluationRepository
{
    private readonly Dictionary<Guid, Evaluation> _d = new();
    public void Save(Evaluation e) => _d[e.Id] = e;
    public Evaluation? Find(Guid studentId, Guid criterionId) =>
        _d.Values.FirstOrDefault(e => e.StudentId == studentId && e.CriterionId == criterionId);
    public IReadOnlyList<Evaluation> ByStudent(Guid studentId) =>
        _d.Values.Where(e => e.StudentId == studentId).ToList();
    public void Delete(Guid id) => _d.Remove(id);
}
