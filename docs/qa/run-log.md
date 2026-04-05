# QA Run Log

Record each QA run in this file.

## Columns
- Date (UTC)
- Commit
- Suites Run
- Result
- Findings
- Cleanup Verified
- Notes

## Entries

| Date (UTC) | Commit | Suites Run | Result | Findings | Cleanup Verified | Notes |
|---|---|---|---|---|---|---|
| 2026-04-04 | bdbd178 | Smoke, Home Search, Admin E2E, Regression | Pass with one fixed defect | Found and fixed FK constraint failure when deleting books with checkout history | Yes | Console 404s only from `Admin/BookDefaultPhoto/{id}` fallback path |
| 2026-04-05 | 5b50116 | Bulk Import | Pass | No defects found | Yes | QA-BULK-001 through QA-BULK-009 all passed. Console 404s only from `Admin/BookDefaultPhoto/{id}` fallback path. |
