module Platform.Lambda

open System
open System.Diagnostics
open System.IO
open FsToolkit.ErrorHandling
open AsyncWriterResult
open Amazon.Lambda
open Amazon.Lambda.Model

let getClient () = new AmazonLambdaClient ()

type LambdaError =
    { ErrorType: string
      ErrorMessage: string }

let invoke (client: AmazonLambdaClient) functionName data : AWR<string> =
    asyncWriterResult {
        do! Write.log "FunctionName" functionName

        let stopwatch = Stopwatch.StartNew ()

        let! response =
            InvokeRequest (FunctionName = functionName, InvocationType = InvocationType.RequestResponse, Payload = data)
            |> client.InvokeAsync
            |> Task.catchResult
            |> TaskResult.mapError (fun (ex: exn) ->
                Error.create
                    ErrorType.LambdaInvoke
                    ex.Message
                    {| Operation = "Invoke"
                       FunctionName = functionName |})

        do! Write.elapsed stopwatch.Elapsed
        do! Write.log "FunctionError" response.FunctionError

        let streamReader = new StreamReader (response.Payload)
        let body = streamReader.ReadToEnd ()
        streamReader.Close ()

        if String.IsNullOrWhiteSpace response.FunctionError then
            return body
        else
            let! error = Json.tryDeserialize<LambdaError> body
            do! Write.log "Error" error

            return!
                Error.create
                    ErrorType.LambdaInvoke
                    error.ErrorMessage
                    {| Operation = "Invoke"
                       FunctionName = functionName |}
                |> Error
    }
    |> AsyncWriter.mapLogs (Logging.mapSpanLogs (fun logs -> [ RootGroup ($"Lambda-Invoke-{functionName}", logs) ]))
