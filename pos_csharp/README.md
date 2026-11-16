# ПОС - Преподавательская оценка студентов (ДЗ №2, КПО)

C# / .NET 8. DI-контейнер: Microsoft.Extensions.DependencyInjection. Тесты: xUnit.

    dotnet run --project src/Pos            # консольное приложение (данные в ./data)
    dotnet run --project src/Pos -- --demo  # сквозной сценарий
    dotnet test tests/Pos.Tests             # тесты

## Архитектура: Ports and Adapters (Hexagonal)

    UI (ConsoleApp) -> Application (use cases, Facade) -> Domain (model, services, ports)
    Infrastructure (JSON/InMemory, CSV) -> Domain (реализует порты)

Зависимости направлены внутрь. Домен не знает про JSON, CSV и консоль.
`Composition.cs` - composition root, где DI собирает граф.

| Слой | Папка / namespace |
|---|---|
| Domain | `Domain` |
| Application | `Application` |
| Infrastructure | `Infrastructure` |
| UI | `UI` |

## Карта DDD

- Ubiquitous Language: Student, Criterion, Rubric, Evaluation, итог (Final).
- Entity: Student, Criterion, Evaluation (есть Id, состояние меняется).
- Value Object: Score, Weight - неизменяемые record, сами проверяют значение.
- Aggregate: Rubric (корень, владеет Criterion); Student и Evaluation - отдельные агрегаты, ссылки по Id.
- Инварианты: балл >= 0 и <= max критерия; оценка только по критерию из рубрикатора; названия критериев уникальны; вес > 0. Evaluation нельзя создать или исправить в обход проверки (конструктор и Correct).
- Repository (порты): IStudentRepository, IRubricRepository, IEvaluationRepository.
- Domain Service: FinalScoreCalculator - итог не хранится, а считается из оценок и рубрикатора.

Итог = 10 * sum(w_i * s_i / m_i) / sum(w_i). Неоценённый критерий считается как 0.

## GoF

| Группа | Паттерн | Где | Какую боль закрывает |
|---|---|---|---|
| Порождающий | Factory | EvaluationFactory | единая точка создания Evaluation с проверкой критерия и диапазона |
| Структурный | Decorator | RoundingDecorator | округление поверх любой стратегии без правки её кода |
| Структурный | Facade | PosFacade | консоль не знает о множестве use cases и репозиториев |
| Поведенческий | Strategy | IScoringStrategy | сменяемое правило итога |
| Поведенческий | Template Method | GradeImporter | общий каркас импорта, различается только Parse (CSV/JSON) |

## Формат импорта

CSV: `student,criterion,score,comment` (без запятых внутри значений). JSON: массив объектов с теми же полями.
Ошибочные строки пропускаются и выводятся списком.

## Роль ИИ
- **Архитектура.** Помогал спланировать решение: разбиение на слои Ports and Adapters, выделение агрегата Rubric и отдельных сущностей Student и Evaluation, выбор паттернов GoF (Factory, Decorator, Facade, Strategy, Template Method).
- **Код.** Помогал подготовить прототип доменной модели, сценариев приложения, адаптеров хранения (JSON) и консольного интерфейса.
- **Тесты и сценарии.** Участвовал в подготовке демонстрационного сценария и unit-тестов на инварианты, Domain Service и сквозной сценарий выставления, исправления и отмены оценки.
