[<AutoOpen>]
module Utils

module AsyncWriter =
    open global.AsyncWriterResult

    let getItem (x: AsyncWriter<_, 'a>) : 'a =
        x |> Async.RunSynchronously |> Writer.run |> fst
