module Json

open System.Text.Json
open System.Text.Json.Serialization

let setupJsonSerializationOptions (options: JsonSerializerOptions) =
    options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
    options.PropertyNameCaseInsensitive <- true

    let jsonFsOptions = JsonFSharpOptions.FSharpLuLike().WithTypes JsonFSharpTypes.All
    jsonFsOptions.AddToJsonSerializerOptions options

let jsonSerializationOptions =
    let options = JsonSerializerOptions ()
    setupJsonSerializationOptions options
    options

let inline serialize obj =
    JsonSerializer.Serialize (obj, jsonSerializationOptions)

let tryDeserialize<'T> (obj: string) =
    try
        match JsonSerializer.Deserialize<'T> (obj, jsonSerializationOptions) with
        | null ->
            Error.create ErrorType.JSON "Deserialized object is null" {| Operation = "Deserialize" |}
            |> Error
        | x -> Ok x
    with ex ->
        Error.ofExn ErrorType.JSON {| Operation = "Deserialize" |} ex
        |> Error
