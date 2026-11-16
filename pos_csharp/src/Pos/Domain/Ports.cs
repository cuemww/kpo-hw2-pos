namespace Pos.Domain;

public interface IStudentRepository
{
    void Add(Student s);
    Student Get(Guid id);
    IReadOnlyList<Student> List();
    void Remove(Guid id);
}

public interface IRubricRepository
{
    Rubric Get();
    void Save(Rubric rubric);
}

public interface IEvaluationRepository
{
    void Save(Evaluation e);
    Evaluation? Find(Guid studentId, Guid criterionId);
    IReadOnlyList<Evaluation> ByStudent(Guid studentId);
    void Delete(Guid id);
}

public sealed record ResultRow(Guid StudentId, string Student, double Final);

public interface IResultExporter
{
    void Export(IEnumerable<ResultRow> rows, string path);
}
