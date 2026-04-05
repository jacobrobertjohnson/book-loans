# QA Run Log

Record each QA run in this file.

## Columns
- Date (UTC)
- Commit
- Suites Run
- Mode
- Result
- Findings
- Cleanup Verified
- Notes

## Entries

| Date (UTC) | Commit | Suites Run | Mode | Result | Findings | Cleanup Verified | Notes |
|---|---|---|---|---|---|---|---|
| 2026-04-04 | bdbd178 | Smoke, Home Search, Admin E2E, Regression | Manual | Pass with one fixed defect | Found and fixed FK constraint failure when deleting books with checkout history | Yes | Console 404s only from `Admin/BookDefaultPhoto/{id}` fallback path |
| 2026-04-04 | 59ef931 | Smoke, Home Search, Admin E2E, Regression, Photo Regression | Manual | Pass | No new defects found | Yes | Verified photo default switching and fallback icon after photo deletion; expected 404 fallback noise observed |
| 2026-04-05 | 5b50116 | Bulk Import | Manual | Pass | No defects found | Yes | QA-BULK-001 through QA-BULK-009 all passed. Console 404s only from `Admin/BookDefaultPhoto/{id}` fallback path. |
