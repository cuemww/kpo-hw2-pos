using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.UI;

public sealed class ConsoleApp
{
    private readonly PosFacade _app;

    public ConsoleApp(PosFacade app) => _app = app;

    private const string Menu = @"
преподавательская оценка студентов
 1. Добавить студента           2. Список студентов
 3. Добавить критерий           4. Список критериев
 5. Выставить/исправить оценку  6. Оценки студента
 7. Отменить оценку             8. Итог студента
 9. Таблица итогов             10. Экспорт итогов в CSV
11. Импорт ведомости (CSV/JSON) 0. Выход";

    public void Run()
    {
        while (true)
        {
            Console.WriteLine(Menu);
            Console.Write("> ");
            var cmd = Console.ReadLine()?.Trim();
            if (cmd is null || cmd == "0") return;
            try { Handle(cmd); }
            catch (DomainException ex) { Console.WriteLine("Ошибка предметной области: " + ex.Message); }
            catch (FormatException) { Console.WriteLine("Некорректный ввод."); }
            catch (ArgumentOutOfRangeException) { Console.WriteLine("Нет такого номера."); }
            catch (IOException ex) { Console.WriteLine("Ошибка файла: " + ex.Message); }
        }
    }

    private void Handle(string cmd)
    {
        switch (cmd)
        {
            case "1":
                var s = _app.Students.Add(Ask("Имя"));
                Console.WriteLine($"Добавлен: {s.Name}");
                break;
            case "2":
                foreach (var st in _app.Students.List()) Console.WriteLine($"  {st.Name}");
                break;
            case "3":
                var c = _app.Rubric.AddCriterion(Ask("Название"), Num("max_score"), Num("Вес"));
                Console.WriteLine($"Добавлен критерий: {c.Title}");
                break;
            case "4":
                foreach (var cr in _app.Rubric.List())
                    Console.WriteLine($"  {cr.Title}: max={cr.MaxScore}, вес={cr.Weight.Value}");
                break;
            case "5":
                var s5 = Pick(_app.Students.List(), x => x.Name);
                var c5 = Pick(_app.Rubric.List(), x => $"{x.Title} (max {x.MaxScore})");
                _app.Evaluations.Grade(s5.Id, c5.Id, Num("Балл"), Ask("Комментарий"));
                Console.WriteLine("Готово.");
                break;
            case "6":
                var s6 = Pick(_app.Students.List(), x => x.Name);
                var titles = _app.Rubric.List().ToDictionary(x => x.Id, x => x.Title);
                foreach (var e in _app.Evaluations.OfStudent(s6.Id))
                    Console.WriteLine($"  {titles.GetValueOrDefault(e.CriterionId, "?")}: {e.Score.Value} ({e.Date}) {e.Comment}");
                break;
            case "7":
                var s7 = Pick(_app.Students.List(), x => x.Name);
                var c7 = Pick(_app.Rubric.List(), x => x.Title);
                _app.Evaluations.Cancel(s7.Id, c7.Id);
                Console.WriteLine("Отменено.");
                break;
            case "8":
                var s8 = Pick(_app.Students.List(), x => x.Name);
                Console.WriteLine($"Итог: {_app.Results.Final(s8.Id).ToString(CultureInfo.InvariantCulture)}");
                break;
            case "9":
                foreach (var r in _app.Results.Table())
                    Console.WriteLine($"  {r.Student}: {r.Final.ToString(CultureInfo.InvariantCulture)}");
                break;
            case "10":
                var path = Ask("Файл [results.csv]");
                if (path == "") path = "results.csv";
                _app.Results.Export(path);
                Console.WriteLine($"Сохранено в {path}");
                break;
            case "11":
                var res = _app.ImportGrades(Ask("Путь к файлу"));
                Console.WriteLine($"Импортировано: {res.Imported}");
                foreach (var err in res.Errors) Console.WriteLine("  ошибка " + err);
                break;
            default:
                Console.WriteLine("Неизвестная команда.");
                break;
        }
    }

    private static string Ask(string label)
    {
        Console.Write(label + ": ");
        return Console.ReadLine() ?? "";
    }

    private static double Num(string label) =>
        double.Parse(Ask(label).Replace(',', '.'), CultureInfo.InvariantCulture);

    private static T Pick<T>(IReadOnlyList<T> items, Func<T, string> label)
    {
        if (items.Count == 0) throw new DomainException("Список пуст.");
        for (int i = 0; i < items.Count; i++) Console.WriteLine($"  {i + 1}. {label(items[i])}");
        return items[int.Parse(Ask("Номер")) - 1];
    }
}
