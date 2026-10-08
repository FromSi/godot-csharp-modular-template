---
name: add-tests
description: Write or update NUnit + Moq tests for a Service/Domain class, mirroring the src/Game folder structure under tests/Game.Tests. Use when asked to add, update, or backfill tests.
---

# Add tests

Test what is engine-free: `Service`, `Domain`, and adapters in `Level/Adapter/`. UI and
Level screens are not unit-tested. Stack is NUnit + Moq (already in `tests/Game.Tests/Game.Tests.csproj`). Always-present references:
[SaveServiceTests](../../../tests/Game.Tests/Common/Service/SaveServiceTests.cs),
[ObservableStateTests](../../../tests/Game.Tests/Common/Domain/ObservableStateTests.cs).

Rules:

1. **Location mirrors source**: `src/Game/<Module>/Service/<X>.cs` → `tests/Game.Tests/<Module>/Service/<X>Tests.cs`;
   `src/Game/Level/Adapter/<X>.cs` → `tests/Game.Tests/Level/Adapter/<X>Tests.cs`.
   Test namespace: `Game.Tests.<Module>.<Layer>` (no `Game.Game` prefix here).

2. **Mock `Common` interfaces** with Moq — `IIdService`, `IRandomGeneratorService`,
   `IClockService`, `IJsonStateFileHandlerService`, and other modules' service interfaces /
   the consumer-side interfaces adapters implement. Use real in-memory repositories
   (`SingleRepository<T>`) for the module's own state.

3. **Build everything mutable in `[SetUp]`, never in field initializers.** NUnit creates
   **one instance of the fixture class for all its tests**; a field initialized inline
   (`private readonly List<X> _calls = new();`, `private int _next = 1;`, a mock, a
   repository) is shared and leaks state from one test into the next — order-dependent
   failures. Declare `private Foo _foo = null!;` and assign it in `[SetUp]`. Only `const`
   and truly immutable values may be initialized inline.

4. **Structure**: `[TestFixture]`, `[SetUp]` to build the system under test, one `[Test]` per
   behavior, named `Method_ExpectedBehavior`. Assert with `Assert.That(actual, Is.EqualTo(...))`.

5. **Cover**: happy path, edge/empty cases, and that the service calls its dependencies
   correctly (`mock.Verify(...)`). For an entity reference, test both "not found by id" and
   "found, snapshot differs". Time: mock `IClockService.Now` instead of reading the real clock.

6. **New persistable state** → add it to the round trip in
   [JsonConverterStateServiceTests](../../../tests/Game.Tests/Common/Service/JsonConverterStateServiceTests.cs).

7. **No redundant tests** — don't test framework code, trivial getters, or Godot behavior.
   Keep tests current with the code.

8. After writing, run the `verify` skill.

Example shape:
```csharp
[TestFixture]
public class ScoreServiceTests
{
    private ISingleRepository<ScoreState> _repository = null!;
    private Mock<IClockService> _clock = null!;
    private ScoreService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new SingleRepository<ScoreState>();
        _repository.Update(new ScoreState());
        _clock = new Mock<IClockService>();
        _clock.Setup(c => c.Now).Returns(new DateTime(2026, 1, 1, 12, 0, 0));

        _service = new ScoreService(_repository, _clock.Object);
    }

    [Test]
    public void AddPoints_IncreasesScoreAndStampsTime()
    {
        _service.AddPoints(5);

        Assert.That(_repository.GetOne().Score, Is.EqualTo(5));
        Assert.That(_repository.GetOne().UpdatedAt, Is.EqualTo(new DateTime(2026, 1, 1, 12, 0, 0)));
    }
}
```
