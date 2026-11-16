namespace Pos.Domain;

public sealed class Student
{
    public Guid Id { get; }
    public string Name { get; private set; }

    public Student(string name, Guid? id = null)
    {
        Name = Validate(name);
        Id = id ?? Guid.NewGuid();
    }

    public void Rename(string name) => Name = Validate(name);

    private static string Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Имя студента не может быть пустым.");
        return name.Trim();
    }
}

public sealed class Criterion
{
    public Guid Id { get; }
    public string Title { get; }
    public double MaxScore { get; }
    public Weight Weight { get; }

    public Criterion(string title, double maxScore, Weight weight, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Название критерия не может быть пустым.");
        if (double.IsNaN(maxScore) || maxScore <= 0)
            throw new DomainException("max_score должен быть больше 0.");
        Title = title.Trim();
        MaxScore = maxScore;
        Weight = weight;
        Id = id ?? Guid.NewGuid();
    }

    public bool Accepts(Score score) => score.Value <= MaxScore;
}

public sealed class Rubric
{
    private readonly List<Criterion> _criteria = new();

    public Rubric(IEnumerable<Criterion>? criteria = null)
    {
        if (criteria is null) return;
        foreach (var c in criteria) Add(c);
    }

    public IReadOnlyList<Criterion> Criteria => _criteria;

    public void Add(Criterion c)
    {
        if (_criteria.Any(x => string.Equals(x.Title, c.Title, StringComparison.OrdinalIgnoreCase)))
            throw new DuplicateException($"Критерий '{c.Title}' уже есть в рубрикаторе.");
        _criteria.Add(c);
    }

    public void Remove(Guid id) => _criteria.Remove(Get(id));

    public Criterion Get(Guid id) =>
        _criteria.FirstOrDefault(c => c.Id == id)
        ?? throw new UnknownCriterionException($"Критерия '{id}' нет в рубрикаторе.");

    public Criterion FindByTitle(string title) =>
        _criteria.FirstOrDefault(c => string.Equals(c.Title, title.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new UnknownCriterionException($"Критерия '{title}' нет в рубрикаторе.");
}
public sealed class Evaluation
{
    public Guid Id { get; }
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public Score Score { get; private set; }
    public string Comment { get; private set; }
    public DateOnly Date { get; private set; }

    public Evaluation(Guid studentId, Criterion criterion, Score score,
                      string? comment = null, DateOnly? date = null, Guid? id = null)
    {
        EnsureInRange(criterion, score);
        StudentId = studentId;
        CriterionId = criterion.Id;
        Score = score;
        Comment = comment ?? "";
        Date = date ?? DateOnly.FromDateTime(DateTime.Today);
        Id = id ?? Guid.NewGuid();
    }

    public void Correct(Criterion criterion, Score score, string? comment = null)
    {
        if (criterion.Id != CriterionId)
            throw new UnknownCriterionException("Оценка относится к другому критерию.");
        EnsureInRange(criterion, score);
        Score = score;
        if (comment is not null) Comment = comment;
        Date = DateOnly.FromDateTime(DateTime.Today);
    }

    private static void EnsureInRange(Criterion c, Score s)
    {
        if (!c.Accepts(s))
            throw new ScoreOutOfRangeException(
                $"Балл {s.Value} больше максимума {c.MaxScore} по критерию '{c.Title}'.");
    }
}
