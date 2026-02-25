module Logging

open System
open AsyncWriterResult


let mapSpanLogs f (logs: Log list) =
    let spanProperties, otherLogs =
        List.foldBack
            (fun log (span, others) ->
                match log with
                | Log.Span props -> props :: span, others
                | x -> span, x :: others)
            logs
            ([], [])

    otherLogs @ (spanProperties |> f |> List.map Log.Span)

let private handleDuplicateKeys (logs: (string * obj) list) : (string * obj) list =
    let keyCount = logs |> List.countBy fst |> Map

    List.fold
        (fun (foundKeys, logs) (key, value as log) ->
            if Map.find key keyCount = 1 then
                key :: foundKeys, List.appendSingle logs log
            else
                let timesFound = foundKeys |> List.filter ((=) key) |> List.length
                key :: foundKeys, List.appendSingle logs ($"{key}-{timesFound + 1}", value))
        ([], [])
        logs
    |> snd

let rec private keyValue log =
    match log with
    | Value (name, value) -> [ name, value ]
    | Elapsed elapsed -> [ log.name, elapsed.TotalMilliseconds |> int :> obj ]
    | Group (name, logs)
    | RootGroup (name, logs) ->
        let kvps = logs |> List.collect keyValue
        [ name, kvps |> handleDuplicateKeys |> dict :> obj ]

let moveRootLogsProperties (logs: LogProperty list) : LogProperty list =
    let rec loop (rootGroups, currentLogs) =
        function
        | [] -> rootGroups, currentLogs
        | RootGroup (name, logsInGroup) :: rest ->
            let rootGroupsInGroup, otherLogsInGroup = loop ([], []) logsInGroup
            let rootGroup = RootGroup (name, otherLogsInGroup)
            loop (rootGroups @ rootGroup :: rootGroupsInGroup, currentLogs) rest
        | Group (name, logsInGroup) :: rest ->
            let rootGroupsInGroup, otherLogsInGroup = loop ([], []) logsInGroup
            let group = Group (name, otherLogsInGroup)
            loop (rootGroups @ rootGroupsInGroup, List.appendSingle currentLogs group) rest
        | log :: rest -> loop (rootGroups, List.appendSingle currentLogs log) rest

    loop ([], []) logs ||> (@)

let runWriter writer =
    let result, logs = Writer.run writer

    let globalProperties =
        logs
        |> List.choose (function
            | Log.Global x -> Some x
            | _ -> None)
        |> moveRootLogsProperties

    let eventLogs =
        logs
        |> List.choose (function
            | Log.Event x -> Some x
            | _ -> None)
        |> List.map (EventLog.addGlobalProperties globalProperties)
        |> List.map (fun log -> { log with Data = moveRootLogsProperties log.Data })

    let spanProperties =
        logs
        |> List.choose (function
            | Log.Span x -> Some x
            | _ -> None)
        |> fun props -> globalProperties @ props
        |> moveRootLogsProperties

    {| Result = result
       EventLogs = eventLogs
       SpanLog = spanProperties |}

let getErrorLogDetails (error: Error) =
    [ LogProperty.Value ("ErrorType", nameof error.Type)
      LogProperty.Value ("ErrorMessage", error.Message)
      LogProperty.Value ("ErrorDetails", error.Details) ]

type private LogType =
    | Span
    | Event

let private writeLog (logType: LogType) (timestamp: DateTimeOffset) logLevel logProperties =
    [ yield "Timestamp", timestamp :> obj
      yield "Level", LogLevel.toString logLevel
      yield "LogType", logType

      yield
          "Properties",
          logProperties
          |> List.collect keyValue
          |> handleDuplicateKeys
          |> dict
          :> obj ]
    |> handleDuplicateKeys
    |> dict
    |> Json.serialize
    |> Console.Write

let private writeSpanLog logLevel (error: Error option) (spanLog: LogProperty seq) =
    writeLog
        Span
        DateTimeOffset.Now
        logLevel
        [ yield! spanLog
          yield!
              error
              |> Option.map getErrorLogDetails
              |> Option.defaultValue [] ]

let private writeEventLog (log: EventLog) =
    writeLog
        Event
        log.Timestamp
        log.Level
        [ yield LogProperty.Value ("Message", log.Message)
          yield! log.Data ]

let log minimumLogLevel writer =
    let data = runWriter writer

    let logLevel =
        match data.Result with
        | Ok _ -> LogLevel.Info
        | Error _ -> LogLevel.Error

    let error = data.Result |> Option.ofError

    do
        data.EventLogs
        |> List.sortBy _.Timestamp
        |> List.filter (fun log ->
            LogLevel.priority minimumLogLevel
            <= LogLevel.priority log.Level)
        |> List.iter writeEventLog

    do writeSpanLog logLevel error data.SpanLog

    Console.Out.Flush ()

    data.Result
