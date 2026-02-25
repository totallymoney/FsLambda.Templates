[<AutoOpen>]
module TypeExtensions

open System

module String =
    let caseInsensitiveEquals (a: string) (b: string) =
        String.Equals (a, b, StringComparison.InvariantCultureIgnoreCase)

    let (|CaseInsensitiveEquals|_|) a b =
        if caseInsensitiveEquals a b then Some () else None

    let (|IsNullOrWhiteSpace|_|) x =
        if String.IsNullOrWhiteSpace x then Some () else None


module Option =
    let ofError =
        function
        | Ok _ -> None
        | Error x -> Some x


module Result =
    open AsyncWriterResult

    let sequenceWriter (x: Result<Writer<_, _>, _>) =
        match x with
        | Ok w -> Writer.map Ok w
        | Error e -> Writer.retn (Error e)

    let flatten (x: Result<Result<_, 'Error>, 'Error>) = Result.bind id x


module List =
    let appendSingle (xs: 'a list) (x: 'a) = xs @ [ x ]

module AsyncWriterResult =
    open FsToolkit.ErrorHandling
    open AsyncWriterResult

    let catch exnToError =
        Async.catchResult
        >> Async.map (
            Result.mapError exnToError
            >> Result.sequenceWriter
            >> Writer.map Result.flatten
        )

    let runSynchronouslyWithTimeout (timeout: TimeSpan) exnToError (f: AsyncWriterResult<_, _, _>) =
        try
            Async.RunSynchronously (f, int timeout.TotalMilliseconds)
        with err ->
            Error (exnToError err) |> Writer.retn
