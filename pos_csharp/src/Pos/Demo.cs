using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;

namespace Pos;

public static class Demo
{
    public static void Run()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "pos-demo-" + Guid.NewGuid().ToString("N"));
        using var provider = Composition.Build(Path.Combine(tmp, "data"));
        var app = provider.GetRequiredService<PosFacade>();

        var anna = app.Students.Add("Анна Иванова");
        var boris = app.Students.Add("Борис Петров");
        var hw = app.Rubric.AddCriterion("Домашние задания", 10, 3);
        var exam = app.Rubric.AddCriterion("Экзамен", 10, 5);
        var practice = app.Rubric.AddCriterion("Практика", 5, 2);

        app.Evaluations.Grade(anna.Id, hw.Id, 9);
        app.Evaluations.Grade(anna.Id, exam.Id, 8, "хороший ответ");
        app.Evaluations.Grade(anna.Id, practice.Id, 5);
        app.Evaluations.Grade(boris.Id, hw.Id, 6);
        app.Evaluations.Grade(boris.Id, exam.Id, 7);

        TryPrint(() => app.Evaluations.Grade(boris.Id, practice.Id, 12));
        TryPrint(() => app.Evaluations.Grade(boris.Id, Guid.NewGuid(), 3));

        app.Evaluations.Grade(boris.Id, practice.Id, 4);
        app.Evaluations.Grade(boris.Id, hw.Id, 8);
        app.Evaluations.Cancel(boris.Id, practice.Id);

        foreach (var r in app.Results.Table())
            Console.WriteLine($"Итог: {r.Student} = {r.Final.ToString(CultureInfo.InvariantCulture)}");

        var csv = Path.Combine(tmp, "results.csv");
        app.Results.Export(csv);
        Console.WriteLine(File.ReadAllText(csv));
    }

    private static void TryPrint(Action a)
    {
        try { a(); }
        catch (DomainException ex) { Console.WriteLine($"Отклонено: {ex.GetType().Name} - {ex.Message}"); }
    }
}
