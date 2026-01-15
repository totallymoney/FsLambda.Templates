# FsLambda Templates

Project templates for building AWS Lambda applications with the AWS CDK using F# and FsCDK.

These templates scaffold a solution similar to the "Highlights" structure: a Lambda function project, a CDK app that deploys it, Expecto-based tests, optional local DynamoDB via Docker Compose, and a CI pipeline using `build.fsx`.

## Available Templates

| Template                  | Short Name       | Description                                                            |
|---------------------------|------------------|------------------------------------------------------------------------|
| F# AWS Lambda App         | `fslambda`       | NuGet-based with central package management (Directory.Packages.props) |
| F# AWS Lambda App (Paket) | `fslambda-paket` | Paket-based dependency management                                      |

## Installation

### From GitHub Packages

1. First, authenticate with GitHub Packages:

```sh
# Add GitHub Packages as a NuGet source
dotnet nuget add source https://nuget.pkg.github.com/totallymoney/index.json \
  --name github \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text
```

To create a Personal Access Token (PAT):
- Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
- Generate a new token with `read:packages` scope
- Use this token as your password

2. Install the templates:

```sh
dotnet new install FsLambda.Templates
```

### From Local Checkout

Install the templates from a local checkout (useful while developing):

```sh
dotnet new install .
```

## Updating Templates

To update to the latest version:

```sh
dotnet new uninstall FsLambda.Templates
dotnet new install FsLambda.Templates
```

Or specify a specific version:

```sh
dotnet new install FsLambda.Templates::0.1.0-ci.123
```

## Create a new app

### NuGet-based (default)

```sh
dotnet new fslambda -n NewApp
cd NewApp
dotnet fsi build.fsx
```

### Paket-based

```sh
dotnet new fslambda-paket -n NewApp
cd NewApp
dotnet tool restore
dotnet paket install
dotnet fsi build.fsx
```

## Generated Structure

This creates the following structure:

- NewApp.sln
- Directory.Packages.props (NuGet) or paket.dependencies (Paket)
- build.fsx (CI pipeline: build, test, publish, docs)
- .editorconfig
- .config/dotnet-tools.json (fsdocs, fantomas, lambda tools)
- docker-compose.yml (DynamoDB local + integration tests)
- .github/workflows/build.yml (GitHub Actions pipeline)
- src/
  - NewApp/ (F# Lambda handler project)
    - Handler.fs
    - NewApp.fsproj
- cdk/
  - NewApp.CDK/ (CDK app using FsCDK)
    - Program.fs
    - NewApp.CDK.fsproj
  - cdk.json
- tests/
  - NewApp.UnitTests/ (Expecto unit tests)
  - NewApp.IntegrationTests/ (Expecto integration tests)
  - NewApp.FakeAPI/ (minimal ASP.NET Core app with /healthcheck)

## Build and Test

From the template root:

```sh
dotnet fsi build.fsx
```

This will:
- Restore tools
- Build the solution
- Run unit and integration tests
- Publish the Lambda project
- Build documentation

You can also run individual steps manually:
- Build: `dotnet build -c Release`
- Run tests: `dotnet run --project tests/NewApp.UnitTests`
- Publish Lambda: `dotnet publish src/NewApp -c Release -f net8.0`
- Build docs: `dotnet fsi build.fsx -- -p docs`

## Deploy

To deploy the Lambda to AWS:

```sh
cd cdk
npx cdk deploy
```

## Local DynamoDB and integration tests

A docker-compose file is included to run DynamoDB Local and integration tests:

```sh
docker compose up -d dynamodb init-dynamo
# Run integration tests
docker compose run --rm integration_tests
```

## Customizing package versions

The template uses central package management and exposes version parameters via `.template.config/template.json`.
You can override them when creating a new app, for example:

```sh
dotnet new fslambda \
  --FsCDKPkgVersion 1.2.3 \
  --FSharpCorePkgVersion 8.0.403 \
  --AmazonLambdaCorePkgVersion 2.7.0
```

See `content/nuget/.template.config/template.json` for the full list of available parameters.

## License

MIT
