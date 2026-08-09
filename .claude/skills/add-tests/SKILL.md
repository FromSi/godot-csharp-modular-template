---
name: add-tests
description: Write or update NUnit + Moq tests for a Service/Domain class, mirroring the src/Game folder structure under tests/Game.Tests. Use when asked to add, update, or backfill tests.
---

# Add tests

Test what is engine-free: `Service` and `Domain`. UI/Level are not unit-tested.
Reference: [NoteServiceTests.cs](../../../tests/Game.Tests/Example/Service/NoteServiceTests.cs).
Stack is NUnit + Moq (already in `tests/Game.Tests/Game.Tests.csproj`).

Rules:

1. **Location mirrors source**: `src/Game/<Module>/Service/<X>.cs` → `tests/Game.Tests/<Module>/Service/<X>Tests.cs`.
   Test namespace: `Game.Tests.<Module>.<Layer>`.

2. **Mock `Common` interfaces** with Moq — `IJsonStateFileHandlerService`, `IRandomGeneratorService`, `IIdService`, etc. Use real in-memory repositories (`SingleRepository<T>` / `CollectionRepository<...>`) when convenient.

3. **Structure**: `[TestFixture]`, `[SetUp]` to build the system under test, one `[Test]` per behavior. Assert with `Assert.That(actual, Is.EqualTo(...))`.

4. **Cover**: happy path, edge/empty cases, and that the service calls its dependencies correctly (`mock.Verify(...)`). When a method returns a file/IO result, assert the returned `FileError`/bool and the resulting in-memory state.

5. **No redundant tests** — don't test framework code, trivial getters, or Godot behavior. Keep tests current with the code.

6. After writing, run the `verify` skill.

Example shape:
```csharp
[SetUp] public void SetUp() { _dep = new Mock<IDep>(); _sut = new XService(_repo, _dep.Object); }

[Test]
public void DoesThing()
{
    _dep.Setup(d => d.Call(It.IsAny<...>())).Returns(...);
    var result = _sut.DoThing();
    Assert.That(result, Is.EqualTo(...));
    _dep.Verify(d => d.Call(It.IsAny<...>()), Times.Once);
}
```
