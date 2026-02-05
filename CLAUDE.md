# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FsLambda.Templates is a .NET template package (`dotnet new`) that scaffolds AWS Lambda functions in F# with AWS CDK infrastructure. Published to NuGet.org by TotallyMoney.

## Template Structure

Two template variants exist in `content/`:
- **`nuget/`** (short name: `fslambda`) - Recommended. Uses central package management via `Directory.Packages.props`. Supports `-f net8.0` or `-f net10.0`.
- **`paket/`** (short name: `fslambda-paket`) - Uses Paket dependency manager. Fixed to .NET 8.0.

Each template has CDK variants in separate directories:
- **`cdk-ts/`** - TypeScript CDK (default). Uses `npx ts-node`.
- **`cdk-fsharp/`** - F# CDK using FsCDK library.

Each template generates:
- `src/NewApp/` - Lambda handler code
- `cdk/` - AWS CDK infrastructure (TypeScript or F# based on `--cdk` option)
- `tests/NewApp.UnitTests/` - Expecto unit tests
- `tests/NewApp.IntegrationTests/` - Integration tests with Docker
- `tests/NewApp.FakeAPI/` - Mock API server for testing

## Build Commands

Templates use Fun.Build with `build.fsx`:

```bash
# Full CI pipeline (restore, build, test, publish, docs)
dotnet fsi build.fsx

# Documentation only
dotnet fsi build.fsx docs
dotnet fsi build.fsx docs:watch
```

## Testing

```bash
# Run tests via build script
dotnet fsi build.fsx

# Run unit tests directly
dotnet test tests/NewApp.UnitTests

# Integration tests require Docker
docker compose up -d dynamodb init-dynamo
docker compose run --rm integration_tests
```

## Template Development

### Installing/Testing Local Changes
```bash
# Uninstall existing, then install from local source
dotnet new uninstall FsLambda.Templates
dotnet new install ./

# Test template generation (TypeScript CDK, default)
dotnet new fslambda -n TestApp -o /tmp/TestApp

# Test F# CDK variant
dotnet new fslambda -n TestApp -o /tmp/TestApp --cdk fsharp
```

### Key Configuration Files
- `FsLambda.Templates.proj` - Main package definition
- `content/*/template.config/template.json` - Template metadata and symbols
- `content/nuget/Directory.Packages.props` - Central package versions (NuGet template)
- `content/paket/paket.dependencies` - Package versions (Paket template)

### Template Symbols
Templates use `sourceName="NewApp"` for project name substitution. Package versions and framework are configurable via template.json symbols. The `TargetFrameworkValue` placeholder gets replaced based on `-f` parameter. The `cdk` symbol controls which CDK variant is used (`ts` or `fsharp`).

## Code Formatting

F# code is formatted with Fantomas. Configuration is in `.editorconfig`:
- 4-space indentation
- Max if-then-else width: 75 chars
- Number-based multiline formatting for records/arrays

## CDK Deployment

**TypeScript CDK (default):**
```bash
cd cdk && npm install && npx cdk deploy
```

**F# CDK:**
```bash
cd cdk && npx cdk deploy
```

## Docker Development Ports

- DynamoDB Local: `localhost:9310`
- FakeAPI: `localhost:9312`
