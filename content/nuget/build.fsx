#!/usr/bin/env -S dotnet fsi

#r "nuget: Fun.Build, 1.0.3"

open System
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Threading
open Fun.Build

let (</>) a b = Path.Combine(a, b)
let root = __SOURCE_DIRECTORY__
let config = "Release"

let sln = root </> "NewApp.sln"
let src = root </> "src" </> "NewApp"
let unitTests = root </> "tests" </> "NewApp.UnitTests"
let integrationTests = root </> "tests" </> "NewApp.IntegrationTests"
let fakeApiProject = root </> "tests" </> "NewApp.FakeAPI"
let fakeApiPort = 9312
let fakeApiUrl = $"http://localhost:{fakeApiPort}"

let isFakeApiRunning () =
    use client = new HttpClient (Timeout = TimeSpan.FromSeconds 2.0)
    try client.GetAsync($"{fakeApiUrl}/healthcheck").Result.IsSuccessStatusCode
    with _ -> false

let mutable fakeApiProcess: Process option = None

let startFakeApi () =
    if isFakeApiRunning () then
        printfn $"FakeAPI already running on port %d{fakeApiPort}"
    else
        printfn $"Starting FakeAPI on port %d{fakeApiPort}..."
        let psi = ProcessStartInfo ("dotnet", $"run --project {fakeApiProject}")
        psi.Environment["PORT"] <- string fakeApiPort
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        let p = Process.Start psi
        fakeApiProcess <- Some p

        let mutable ready = false
        for _ in 1..30 do
            if not ready then
                Thread.Sleep 1000
                ready <- isFakeApiRunning ()

        if not ready then
            p.Kill true
            failwith "FakeAPI failed to start within 30 seconds"

        printfn $"FakeAPI started on port %d{fakeApiPort} (PID: %d{p.Id})"

let stopFakeApi () =
    match fakeApiProcess with
    | Some p when not p.HasExited ->
        p.Kill true
        p.WaitForExit ()
        printfn $"FakeAPI stopped (PID: %d{p.Id})"
    | _ -> ()

pipeline "ci" {
    description "Build and run all tests"

    stage "restore" { run "dotnet tool restore" }

    stage "build" { run $"dotnet build {sln} -c {config}" }

    stage "unit-tests" { run $"dotnet run --project {unitTests} -c {config} --no-build" }

    stage "fake-api" { run (fun _ -> startFakeApi (); 0) }

    stage "integration-tests" { run $"dotnet run --project {integrationTests} -c {config} --no-build" }

    post [
        stage "cleanup" { run (fun _ -> stopFakeApi (); 0) }
    ]

    runIfOnlySpecified false
}

pipeline "publish" {
    description "Publish Lambda for CDK deployment"

    stage "publish" { run $"dotnet publish {src} -c {config} -o publish" }

    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp()
