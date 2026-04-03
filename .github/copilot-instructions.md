---
applyTo: "**"
description: "Repository-wide coding style and architecture preferences for BookLoans"
---

# BookLoans Coding Preferences

## Stack and Architecture
- Target framework: .NET 10.
- App type: ASP.NET Core MVC.
- Data access: Entity Framework Core with SQLite in Code First mode.
- Keep web-specific code in the BookLoans.Web project.
- Maintain a separation of models between the layers:
  - Entities in BookLoans.Data
  - DTOs in BookLoans.Abstractions
  - View models in BookLoans.Web.Models
- One class per model/record

## Naming and Namespaces
- Use namespace prefix matching the project name.
- Use PascalCase for types, methods, properties, and public members.
- Use camelCase for local variables and method parameters.
- Use camelCase with a leading underscore for private fields.
- Avoid abbreviations unless they are well-known (Id, Url, Db).
- Prefer to shorten names for brevity over precisely matching the class name.
    - Example: a `PublicHomepageQueryService` instance should be called `queryService` instead of `publicHomepageQueryService`
- Call `CancellationToken` instances `ct` everywhere

## C# Conventions
- Nullable reference types stay enabled.
- Treat warnings as errors
- Prefer constructor injection.
- Keep methods focused and short; extract private helpers when logic grows.
- Use expression-bodied members for one-liner methods and properties. Put the arrow on a new line when it improves readability.
- Prefer explicit types, using `new()` to avoid duplication when assignment occurs at the same time as declaration.
- Never assign `default` to a `CancellationToken` parameter. It should always be passed through from the entrypoint to the service method, and ultimately to EF Core or any async APIs.
- Leave off brackets on one-liner `if`, `else`, `for`, and `foreach` statements, but always use brackets for multi-line blocks.

## MVC Conventions
- Controllers should stay thin.
- Move business logic out of controllers and into services.
- Keep views presentation-focused; no data access in views.
- Use strongly typed view models for non-trivial views.

## EF Core Conventions
- Use explicit entity classes and DbSet properties in AppDbContext.
- Add migrations for schema changes; do not edit applied migrations unless explicitly requested to do so.
- Keep InitialCreate migration as baseline, and add new migrations incrementally.
- Configure connection string in appsettings.json.

## Cleanup
- Don't add unecessary classes until called for by the requirements or code
- Remove unused usings and code.
- Don't leave multiple blank lines; one blank line is sufficient to separate code blocks.
- Add blank lines around method definitions and between logical sections of code for readability.

## Quality and Safety
- Prefer async APIs for I/O and database operations. But don't force a `Task.FromResult` just to make a method async if it doesn't need to be.
- Validate inputs and fail with clear messages.
- Add or update tests when behavior changes.
- Keep diffs focused; avoid unrelated refactors in feature changes.

## Repo-specific Notes
- The SQLite file path is configured in appsettings.json.
- The current baseline DB should start with no application tables until entities are added.
