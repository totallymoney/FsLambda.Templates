#!/usr/bin/env -S dotnet fsi

#r "nuget: Fun.Build, 1.0.3"

open System.IO
open Fun.Build

let (</>) a b = Path.Combine(a, b)
let root = __SOURCE_DIRECTORY__
let config = "Release"

let sln = root </> "NewApp.sln"
let src = root </> "src" </> "NewApp"
let unitTests = root </> "tests" </> "NewApp.UnitTests"
let integrationTests = root </> "tests" </> "NewApp.IntegrationTests"

pipeline "ci" {
    description "Build, test, publish, and generate docs"

    stage "restore" { run "dotnet tool restore" }

    stage "build" { run $"dotnet build {sln} -c {config}" }

    stage "test" {
        run $"dotnet run --project {unitTests} -c {config} --no-build"
        run $"dotnet run --project {integrationTests} -c {config} --no-build"
    }

    stage "publish" {
        workingDir src
        run $"dotnet publish -c {config} -f TargetFrameworkValue"
    }

    stage "docs" {
        run $"dotnet fsdocs build --properties Configuration={config} --eval --strict"
    }

    runIfOnlySpecified false
}

pipeline "docs" {
    description "Build the documentation site"

    stage "build" {
        run "dotnet tool restore"
        run $"dotnet publish {src} -c {config} -f TargetFrameworkValue"
        run $"dotnet fsdocs build --properties Configuration={config} --eval --strict"
    }

    runIfOnlySpecified true
}

pipeline "docs:watch" {
    description "Watch and rebuild the documentation site"

    stage "build" { run $"dotnet publish {src} -c {config} -f TargetFrameworkValue" }
    stage "watch" { run "dotnet fsdocs watch --eval --clean" }

    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp()
