(**
---
title: NewApp
category: docs
index: 0
---

# NewApp

An F# AWS Lambda application with CDK infrastructure.

## Project Structure

- `src/NewApp` - Lambda function implementation
- `cdk/NewApp.CDK` - CDK infrastructure using FsCDK
- `tests/NewApp.UnitTests` - Unit tests with Expecto
- `tests/NewApp.IntegrationTests` - Integration tests

## Getting Started

### Build and Test

```bash
dotnet fsi build.fsx
```

### Run Tests Locally

```bash
# Start DynamoDB Local
docker compose up -d dynamodb init-dynamo

# Run integration tests
docker compose run --rm integration_tests
```

### Deploy

```bash
cd cdk
npx cdk deploy
```

## Documentation

Build the documentation:

```bash
dotnet fsi build.fsx -- -p docs
```

Watch mode for development:

```bash
dotnet fsi build.fsx -- -p docs:watch
```
*)
