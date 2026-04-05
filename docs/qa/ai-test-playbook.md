# AI QA Test Playbook

## Purpose
Use this document as the canonical, repeatable QA plan for BookLoans.

## Scope
- Manual UI smoke checks for Home and Admin pages.
- Manual authenticated Admin workflows.
- Manual regression checks for known bugs.
- Manual bulk import CSV workflow.
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

### 6) Bulk Import Suite

#### QA-BULK-001: Import page loads
- Navigate to `/Admin/BulkImportBooks` while authenticated as Admin.
- Page title shows "Import Books".
- Form contains a CSV file input and an Import button.
- A "Download CSV template" link is present.
- A back-arrow link pointing to `/Admin/Books` is present.

#### QA-BULK-002: Submit with no file shows error
- Navigate to `/Admin/BulkImportBooks`.
- Click Import without selecting a file.
- Expect model error: "Please select a CSV file to import."
- Expect page to remain on the Import Books view.

#### QA-BULK-003: Header-only CSV shows "no data rows"
- Upload a CSV that contains only the header row and no data rows.
- Expect the result card to show: "No data rows found in the uploaded file."
- Expect success count = 0 and error count = 0.

#### QA-BULK-004: Missing required columns shows error
- Upload a CSV missing the required columns (`Condition`, `YearFirstPublished`).
- Expect row 0 error: "CSV is missing required columns: Title, Authors, Condition, and YearFirstPublished are required."

#### QA-BULK-005: Unknown condition reports row error
- Upload a CSV with a `Condition` value that does not match any existing condition.
- Expect a row error identifying the unknown condition name and listing valid values.
- Expect no books created.

#### QA-BULK-006: Invalid YearFirstPublished reports row error
- Upload a CSV with a non-integer value in `YearFirstPublished`.
- Expect a row error: `Invalid YearFirstPublished value "…". Must be an integer.`
- Expect no books created.

#### QA-BULK-007: Valid CSV imports all books
- Upload a CSV with two valid data rows (distinct titles, valid conditions, pipe-separated authors).
- Expect: "Successfully imported 2 books."
- Verify both books appear in the Books list with correct titles and authors.

#### QA-BULK-008: Mixed valid/invalid rows shows partial success
- Upload a CSV with one valid row and one row with an invalid condition.
- Expect: "Successfully imported 1 book." and a 1-row error table.
- Verify only the valid book appears in the Books list.

#### QA-BULK-009: CSV template downloads correctly
- Click "Download CSV template" (or navigate to `/Admin/BulkImportBooksTemplate`).
- Expect a file named `books-import-template.csv` to download.
- Expect the file to contain the header: `Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series`.

#### Bulk Import Cleanup
- Delete all QA books created during the bulk import run.
- Delete all QA authors created during the bulk import run.
- Verify the Books and Authors admin lists are clear of QA entries.

## Agent Prompt (Reusable)
Use this prompt for repeat QA runs:

```text
Run the BookLoans QA playbook in docs/qa/ai-test-playbook.md.
Execute smoke + home search + admin e2e + regression + bulk import suites.
Create only QA-prefixed data and fully clean it up before finishing.
Report pass/fail per suite, include repro for failures, and list all cleanup verification results.
```
