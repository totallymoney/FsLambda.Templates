[<AutoOpen>]
module Lambda.Utils

open System
open System.Diagnostics
open System.IO
open System.Text
open global.AsyncWriterResult
open Amazon.Lambda.Core

[<RequireQualifiedAccess>]
type LogCategory =
    | APIRequest

    static member toString =
        function
        | APIRequest -> "API Request"


let functionId = Guid.NewGuid ()

let handleRequest
    (lambdaContext: ILambdaContext)
    (category: LogCategory)
    (purpose: string)
    (performRequest: AppContext.AppContext -> AWR<'Response>)
    : Result<'Response, Error>
    =
    let stopwatch = Stopwatch.StartNew ()
    let appContext = AppContext.setup lambdaContext.FunctionName

    let minimumLogLevel =
        appContext
        |> Writer.run
        |> fst
        |> Result.map _.MinimumLogLevel
        |> Result.defaultValue LogLevel.Info

    asyncWriterResult {
        do! Write.globalLog "FunctionName" lambdaContext.FunctionName
        do! Write.globalLog "FunctionId" functionId
        do! Write.globalLog "Category" (LogCategory.toString category)

        let! purpose =
            match purpose with
            | String.IsNullOrWhiteSpace ->
                Error.create ErrorType.Input "Purpose not set" {| |}
                |> Error
            | x -> Ok x

        do! Write.globalLog "Purpose" purpose

        let! appContext = appContext
        let response = performRequest appContext |> Async.RunSynchronously

        do! Write.elapsed stopwatch.Elapsed
        return! response
    }
    |> AsyncWriterResult.catch (Error.ofExn ErrorType.Unhandled {| |})
    |> AsyncWriterResult.runSynchronouslyWithTimeout
        (lambdaContext.RemainingTime.Add (TimeSpan.FromSeconds -2.0))
        (Error.ofExn ErrorType.Timeout {| |})
    |> Logging.log minimumLogLevel


let inline getPurpose<'a when 'a: (member Purpose: string)> (x: 'a) = x.Purpose

let respondWithStream (output: string) =
    let stream = new MemoryStream ()
    let writer = new StreamWriter (stream, UTF8Encoding false)

    writer.Write output
    writer.Flush ()

    stream.Position <- 0
    stream :> Stream
