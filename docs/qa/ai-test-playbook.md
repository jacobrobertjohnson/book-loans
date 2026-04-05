# AI QA Test Playbook

## Purpose
Use this document as the canonical, repeatable QA plan for BookLoans.

## Scope
- Manual UI smoke checks for Home and Admin pages.
- Manual authenticated Admin workflows.
- Manual regression checks for known bugs.
- Deterministic creation and cleanup of QA data.

## Execution Mode
- This playbook is for manual QA execution in the integrated browser.
- Do not substitute unit tests for these checks.
- Do not treat scripted API-only checks as a full replacement for these manual UI steps.

## Preconditions
- App runs at `http://localhost:5171`.
- Development admin credentials come from `src/BookLoans.Web/appsettings.Development.json`.
- Agent has integrated browser tools enabled.

## Commands
- Build solution: `dotnet build BookLoans.sln`
- Run unit tests: `dotnet test tests/BookLoans.UnitTests/BookLoans.UnitTests.csproj`
- Run app: `dotnet run --project src/BookLoans.Web/BookLoans.Web.csproj`

## Data Rules
- Prefix all temporary records with `QA` and a unique suffix.
- Create records in dependency-safe order:
  1. Condition
  2. Author
  3. Borrower
  4. Book
  5. Checkout
- Cleanup in reverse dependency order:
  1. Return or delete checkout rows
  2. Delete book
  3. Delete borrower
  4. Delete author
  5. Delete condition
- Verify cleanup by checking all admin lists after run.

## Known Expected Noise
- `404` responses for `Admin/BookDefaultPhoto/{id}` are expected for books without uploaded photos and should trigger fallback icon behavior.

## Test Case Template
Copy this block for each case:

```yaml
id: QA-CASE-XXX
name: Short descriptive name
suite: smoke|regression|admin-e2e|auth
risk: low|medium|high
mode: manual
preconditions:
  - condition
steps:
  - action
expected:
  - expected result
created_data:
  - entity names
cleanup:
  - cleanup actions
notes:
  - extra observations
```

## Core Suites

### 1) Smoke Suite
- Home loads.
- Admin login page loads.
- Authenticated Admin landing loads.
- Nav links work: Checkouts, Borrowers, Books, Authors, Conditions.

### 2) Home Search Suite
- Search matches title.
- Search matches borrower and keeps all borrower tiles visible.
- Search is case-insensitive.
- Search trims whitespace before comparisons.
- Clear button appears only when input has text.
- Clear button resets filter and hides itself.

### 3) Admin E2E CRUD + Linking
- Create condition, author, borrower.
- Create book linked to condition and author.
- Edit book and verify update in list.
- Create checkout and verify active row.
- Mark returned and verify returned timestamp.
- Delete book and verify success.
- Delete created borrower/author/condition and verify removal.

### 4) Regression Suite
- Deleting a book with checkout history should succeed and remove dependent history rows.
- Book list sort rule:
  - series name (if present) first, otherwise title
  - ignore leading `a`, `an`, `the`

### 5) Photo Management Regression Suite
- Create a QA book (or reuse one created in the same run).
- Open `Admin/EditBook/{id}` for the QA book.
- Upload two photos with distinct captions.
- Verify the first uploaded photo is default in the Photos section.
- Change default to the other photo using the "Set default" action.
- Verify the default indicator changes immediately in `Edit Book`.
- Navigate to Home and verify the same book tile uses the newly selected default photo.
- Delete all photos for that book from `Edit Book`.
- Return to Home and verify the tile uses the fallback icon when no photos exist.
- Confirm `404` responses for `Admin/BookDefaultPhoto/{id}` in this case are expected noise (fallback path), not failures.

## Agent Prompt (Reusable)
Use this prompt for repeat QA runs:

```text
Run the BookLoans QA playbook in docs/qa/ai-test-playbook.md.
Execute smoke + home search + admin e2e + regression suites.
Create only QA-prefixed data and fully clean it up before finishing.
Report pass/fail per suite, include repro for failures, and list all cleanup verification results.
```
