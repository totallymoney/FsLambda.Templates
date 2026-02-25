[<AutoOpen>]
module Infrastructure

open System
open AsyncWriterResult

type DateProvider = unit -> DateTimeOffset

[<RequireQualifiedAccess>]
type ErrorType =
    | Configuration
    | Input
    | JSON
    | LambdaInvoke
    | Timeout
    | Unhandled

type Error =
    { Type: ErrorType
      Message: string
      Details: obj }

module Error =
    let ofExn errorType details (exn: exn) =
        { Type = errorType
          Message = exn.Message
          Details = details }

    let create errorType message details =
        { Type = errorType
          Message = message
          Details = details }


let (|InvalidConfigValue|) name value =
    Error.create
        ErrorType.Configuration
        "Invalid value for config item"
        {| Name = name
           Value = value |}


[<RequireQualifiedAccess>]
type LogLevel =
    | Info
    | Warning
    | Error
    | Debug

    static member toString =
        function
        | Info -> "Information"
        | Warning -> "Warning"
        | Error -> "Error"
        | Debug -> "Debug"

    static member ofString =
        function
        | String.CaseInsensitiveEquals "Information"
        | String.CaseInsensitiveEquals "Info" -> Ok Info
        | String.CaseInsensitiveEquals "Warning" -> Ok Warning
        | String.CaseInsensitiveEquals "Error" -> Ok Error
        | String.CaseInsensitiveEquals "Debug" -> Ok Debug
        | InvalidConfigValue "LogLevel" error -> Result.Error error

    static member priority =
        function
        | Info -> 1
        | Warning -> 2
        | Error -> 3
        | Debug -> 0

type LogProperty =
    | Value of name: string * value: obj
    | Elapsed of TimeSpan
    | Group of name: string * LogProperty list
    | RootGroup of name: string * LogProperty list

    member this.name =
        match this with
        | Value (name, _)
        | Group (name, _)
        | RootGroup (name, _) -> name
        | Elapsed _ -> "Elapsed"

type EventLog =
    { Timestamp: DateTimeOffset
      Level: LogLevel
      Message: string
      Data: LogProperty list }

module EventLog =
    let addGlobalProperties globalProperties log =
        { log with Data = globalProperties @ log.Data }


[<RequireQualifiedAccess>]
type Log =
    | Global of LogProperty
    | Event of EventLog
    | Span of LogProperty

type Write =
    static member globalLog name value =
        Value (name, value) |> Log.Global |> Writer.write

    static member log name value =
        Value (name, value) |> Log.Span |> Writer.write

    static member elapsed value =
        Elapsed value |> Log.Span |> Writer.write


type AWR<'a> = AsyncWriterResult<'a, Error, Log>


// Sample domain types

type ItemDto =
    { ItemId: string
      Name: string
      CreatedOn: DateTimeOffset }

let validateItemId (itemId: string) =
    match itemId with
    | String.IsNullOrWhiteSpace ->
        Error.create ErrorType.Input "ItemId must not be empty" {| |}
        |> Error
    | x -> Ok x

module RequestDTOs =
    type GetItem = { Purpose: string; ItemId: string }

module ResponseDTOs =
    type GetItem =
        { ItemId: string
          Name: string
          CreatedOn: DateTimeOffset }

    module GetItem =
        let ofDomain (item: ItemDto) : GetItem =
            { ItemId = item.ItemId
              Name = item.Name
              CreatedOn = item.CreatedOn }
