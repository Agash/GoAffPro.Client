# AGENTS.md — GoAffPro.Client Agent Workflow Rules

This document defines how any AI agent working on the GoAffPro.Client standalone library must behave.

---

## 1. Pre-Task Checklist

Before writing any code:

1. Read the relevant `ARCHITECTURE.md` section.
2. Check if the task involves the generated code (`*.g.cs`) — if yes, **edit the generator, not the generated file**.
3. Confirm the delivery phase (this project has no formal phases; all features are in scope).
4. Identify the target file using the project structure in `ARCHITECTURE.md §3`.

---

## 2. Code Rules

### 2.1 Language

- Multi-target: `net8.0;net10.0`.
- `Nullable enable` everywhere.
- No `#pragma warning disable` without a comment.
- `sealed` by default on concrete classes.
- `record` for DTOs and immutable data.

### 2.2 Async Rules

- All I/O must be async. No `.Result`, `.Wait()`, or `Thread.Sleep`.
- `CancellationToken` accepted and propagated in all public async methods.
- `ValueTask` where synchronous completion is common.

### 2.3 Generated Code Rules

- **Never manually edit `*.g.cs` files.**
- If a generated type needs customization, use `partial class` in a non-generated file.
- If the generated code is wrong, fix `GoAffPro.Client.Generator`, not the output.

### 2.4 Polly Policies

- All HTTP requests through `GoAffProClient` must use the retry policy defined in `RetryPolicies.cs`.
- Never add inline retry loops — use Polly.

---

## 3. Event Detector Rules

- The `GoAffProEventDetector` must remain stateless (no disk I/O).
- Seen-ID sets are in-memory `HashSet<string>`.
- If persistent state is needed, it's the caller's responsibility to wrap the detector.

---

## 4. Testing Rules

### 4.1 Unit Tests

- Every public method with logic must have at least one unit test.
- Test class names: `{ClassUnderTest}Tests`.
- Test method names: `{Method}_{Scenario}_{ExpectedResult}`.

### 4.2 Integration Tests

- Must run only when explicitly requested via `dotnet test --filter "Category=Integration"`.
- Mark integration tests with `[Trait("Category", "Integration")]`.
- Require a valid bearer token in `appsettings.Test.json` or environment variable `GOAFFPRO_TEST_TOKEN`.

### 4.3 Contract Tests

- Snapshot-based: generate client, serialize method signatures, compare to committed snapshot.
- Fail the build if generated code drifts from spec without a version bump.

---

## 5. Commit Rules

- Format: `{type}({scope}): {description}`
- Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`
- Scope: `client`, `generator`, `events`, `tests`, `ci`
- Examples:
  - `feat(events): add NewRewardsAsync event stream`
  - `fix(generator): escape special chars in NSwag output path`
  - `test(client): add retry policy unit tests`

---

## 6. NuGet Packaging Rules

- Version uses calendar format: `YYYY.M.D`.
- Bump version on every spec regeneration (even if no code changes).
- Patch releases (e.g., `2026.2.17.1`) for bug fixes with no spec changes.

---

## 7. What Agents Must NOT Do

- **Do not edit `*.g.cs` files** under any circumstances.
- **Do not add new dependencies** to `Directory.Packages.props` without noting it in the commit.
- **Do not suppress Polly retry policies** in favor of inline retries.
- **Do not persist state to disk** in `GoAffProEventDetector` (caller's responsibility).

---

## 8. When to Stop and Ask

Stop and ask the human when:

- The OpenAPI spec structure has changed in a way that breaks the filter logic (e.g., `/user` paths moved to a different tag).
- NSwag generation fails with an unrecognized error.
- A new external dependency would be required (e.g., a different JSON serializer).
- The task scope expands beyond a single focused session.
