module AppContext

open System
open Amazon.Lambda
open FsToolkit.ErrorHandling
open AsyncWriterResult


module Environment =
    let envVars =
        seq {
            let vars = Environment.GetEnvironmentVariables ()
            for key in vars.Keys -> string key, string vars[key]
        }
        |> Map.ofSeq

    let tryGetItem key = envVars |> Map.tryFind key

    let getItem key =
        tryGetItem key
        |> function
            | Some x -> Ok x
            | None ->
                Error.create ErrorType.Configuration "Missing environment variable" {| Variable = key |}
                |> Error

    let getAndLogItem key logName =
        writerResult {
            let! value = getItem key
            do! Write.globalLog logName value
            return value
        }


type Config =
    { Version: string
      Environment: string
      FunctionName: string
      MinimumLogLevel: LogLevel
      ExternalApiFunctionName: string }


module Config =
    open Environment

    let get functionName =
        writerResult {
            let! version = getAndLogItem "VERSION" "Version"
            let! environment = getAndLogItem "ENVIRONMENT" "Environment"

            let! minimumLogLevel =
                getItem "MINIMUM_LOG_LEVEL"
                |> Result.bind LogLevel.ofString

            let! externalApiFunctionName = getItem "EXTERNAL_API_FUNCTION_NAME"

            return
                { Version = version
                  Environment = environment
                  FunctionName = functionName
                  MinimumLogLevel = minimumLogLevel
                  ExternalApiFunctionName = externalApiFunctionName }
        }


type AppContext =
    { DateProvider: DateProvider
      MinimumLogLevel: LogLevel
      ExternalAPI: IO.ExternalAPI.Client }

let setup functionName =
    writerResult {
        let! config = Config.get functionName
        let dateProvider = fun _ -> DateTimeOffset.Now
        let lambdaClient = new AmazonLambdaClient ()

        return
            { DateProvider = dateProvider
              MinimumLogLevel = config.MinimumLogLevel
              ExternalAPI = IO.ExternalAPI.Client.configure lambdaClient config.ExternalApiFunctionName }
    }
