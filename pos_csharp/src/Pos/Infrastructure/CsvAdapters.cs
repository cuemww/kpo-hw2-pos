using System.Globalization;
using System.Text;
using System.Text.Json;
using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public sealed class CsvResultExporter : IResultExporter
{
    public void Export(IEnumerable<ResultRow> rows, string path)
    {
        var sb = new StringBuilder("student_id,student,final\n");
        foreach (var r in rows)
            sb.Append(r.StudentId).Append(',')
              .Append('"').Append(r.Student.Replace("\"", "\"\"")).Append('"').Append(',')
              .Append(r.Final.ToString(CultureInfo.InvariantCulture)).Append('\n');
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }
}

public sealed class CsvGradeImporter : GradeImporter
{
    public CsvGradeImporter(IStudentRepository s, IRubricRepository r, EvaluationUseCases e) : base(s, r, e) { }

    public override bool CanHandle(string path) => path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    protected override IEnumerable<GradeRow> Parse(string path)
    {
        foreach (var line in File.ReadLines(path).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var p = line.Split(',');
            double score = p.Length > 2 && double.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v : double.NaN;
            yield return new GradeRow(p[0], p.Length > 1 ? p[1] : "", score, p.Length > 3 ? p[3] : "");
        }
    }
}

public sealed class JsonGradeImporter : GradeImporter
{
    private sealed record Dto(string Student, string Criterion, double Score, string? Comment);

    public JsonGradeImporter(IStudentRepository s, IRubricRepository r, EvaluationUseCases e) : base(s, r, e) { }

    public override bool CanHandle(string path) => path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

    protected override IEnumerable<GradeRow> Parse(string path)
    {
        var items = JsonSerializer.Deserialize<List<Dto>>(File.ReadAllText(path),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        return items.Select(d => new GradeRow(d.Student, d.Criterion, d.Score, d.Comment ?? ""));
    }
}
