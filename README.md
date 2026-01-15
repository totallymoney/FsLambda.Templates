# FsLambda Templates

F# AWS Lambda templates with CDK infrastructure.

## Quick Start

### Install

**From NuGet.org:**
```sh
dotnet new install FsLambda.Templates
```

**From GitHub Packages (alternative):**
```sh
dotnet nuget add source https://nuget.pkg.github.com/totallymoney/index.json \
  --name github --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_PAT
dotnet new install FsLambda.Templates --nuget-source github
```

### Create a New App

**NuGet-based (recommended):**
```sh
dotnet new fslambda -n MyApp
cd MyApp
dotnet fsi build.fsx
```

**Paket-based:**
```sh
dotnet new fslambda-paket -n MyApp
cd MyApp
dotnet tool restore && dotnet paket install
dotnet fsi build.fsx
```

### Deploy to AWS

```sh
cd cdk
npx cdk deploy
```

## Templates

| Short Name       | Description                                      |
|------------------|--------------------------------------------------|
| `fslambda`       | NuGet with central package management (default)  |
| `fslambda-paket` | Paket dependency management                      |

## Project Structure

```
MyApp/
├── src/MyApp/              # Lambda handler
├── cdk/MyApp.CDK/          # CDK infrastructure (FsCDK)
├── tests/
│   ├── MyApp.UnitTests/    # Unit tests (Expecto)
│   └── MyApp.IntegrationTests/
├── build.fsx               # Build script
├── docker-compose.yml      # Local DynamoDB
└── .github/workflows/      # CI pipeline
```

## Local Development

Run DynamoDB locally for integration tests:

```sh
docker compose up -d dynamodb init-dynamo
docker compose run --rm integration_tests
```

## Update Templates

```sh
dotnet new uninstall FsLambda.Templates
dotnet new install FsLambda.Templates
```

## License

MIT
