# Align Donations WebApi DI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align `Fcs.Donations.WebApi` composition, pipeline, Swagger, observability, and operational endpoints with the established Identity and Campaign pattern.

**Architecture:** Keep `Program.cs` as a composition root and move runtime pipeline concerns to `PipelineDependencyInjection`. Split Swagger and OpenTelemetry into focused modules, validate typed settings during startup, and protect existing Donations-specific OData, HTTP, Kafka, authentication, and persistence configuration.

**Tech Stack:** .NET 8, ASP.NET Core, EF Core SQL Server, Swashbuckle, OpenTelemetry, Prometheus, Serilog, xUnit.

---

### Task 1: Specify operational WebApi behavior

**Files:**
- Create: `tests/Fcs.Donations.IntegratedTests/WebApi/OperationalEndpointsTests.cs`
- Create: `tests/Fcs.Donations.IntegratedTests/WebApi/TrimStringsActionFilterTests.cs`
- Modify: `tests/Fcs.Donations.IntegratedTests/Configurations/CustomWebApplicationFactory.cs`

- [ ] Add tests for Bearer Swagger metadata on protected operations and absence of a global security requirement.
- [ ] Add tests for correlation ID propagation and generation.
- [ ] Add tests for unhealthy database response on `/health` and Prometheus output on `/metrics`.
- [ ] Configure an invalid, short-timeout SQL Server connection in the `Test` factory to prove startup does not apply migrations.
- [ ] Add a focused test proving writable string properties are trimmed.
- [ ] Run the tests and verify the missing behavior fails for the expected reasons.

### Task 2: Separate Swagger, pipeline, filters, and settings

**Files:**
- Create: `src/Fcs.Donations.WebApi/DependencyInjection/PipelineDependencyInjection.cs`
- Create: `src/Fcs.Donations.WebApi/Swagger/SwaggerDependencyInjection.cs`
- Create: `src/Fcs.Donations.WebApi/Swagger/SwaggerAuthorizationOperationFilter.cs`
- Create: `src/Fcs.Donations.WebApi/Swagger/SwaggerConstants.cs`
- Create: `src/Fcs.Donations.WebApi/Filters/TrimStringsActionFilter.cs`
- Create: `src/Fcs.Donations.WebApi/Settings/CorsSettings.cs`
- Create: `src/Fcs.Donations.WebApi/Middlewares/RequestFlowLoggingMiddleware.cs`
- Modify: `src/Fcs.Donations.WebApi/Program.cs`
- Modify: `src/Fcs.Donations.WebApi/DependencyInjection/DependencyInjection.cs`
- Modify: `src/Fcs.Donations.WebApi/Extensions/ApiBuilderExtensions.cs`
- Modify: `src/Fcs.Donations.WebApi/appsettings.json`

- [ ] Remove migration execution from `Program.cs`.
- [ ] Apply migrations only in `Development` and `Docker`.
- [ ] Order correlation, request logging, exception handling, operational endpoints, HTTPS, CORS, authentication, authorization, and controllers consistently.
- [ ] Register CORS, versioning, OData/controllers, trim filter, Swagger, routing, health checks, observability, and logging through focused methods.
- [ ] Configure Bearer security only for actions or controllers carrying `[Authorize]`.
- [ ] Run operational tests and verify this slice passes.

### Task 3: Align observability and dependencies

**Files:**
- Replace: `src/Fcs.Donations.WebApi/Observability/ObservabilityOptions.cs`
- Create: `src/Fcs.Donations.WebApi/Observability/ObservabilitySettings.cs`
- Create: `src/Fcs.Donations.WebApi/Observability/ObservabilityTelemetry.cs`
- Modify: `src/Fcs.Donations.WebApi/Fcs.Donations.WebApi.csproj`
- Modify: `Directory.Packages.props`

- [ ] Bind and validate observability settings at startup.
- [ ] Configure service resources, ASP.NET Core, HTTP client, SQL client, runtime, Prometheus, and optional OTLP exporters.
- [ ] Configure Serilog enrichment and optional OTLP log export.
- [ ] Add the package versions and references required by the aligned setup.
- [ ] Run build and integrated tests.

### Task 4: Final verification

**Files:**
- Verify all changed files without modifying the existing local changes in `launchSettings.json` or `NullMessagePublisher.cs`.

- [ ] Run `dotnet format --verify-no-changes`.
- [ ] Run `dotnet build --no-restore`.
- [ ] Run `dotnet test --no-build --no-restore`.
- [ ] Run `git diff --check`.
- [ ] Inspect `git diff` and confirm unrelated local changes were preserved.
