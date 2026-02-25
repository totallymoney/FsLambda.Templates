module Lambda.Handlers

open System.IO
open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.SystemTextJson
open AsyncWriterResult


type HandleRequest =
    static member inline directInvoke
        (lambdaContext: ILambdaContext, performWork: AppContext.AppContext -> 'Args -> AWR<'Output>, input: 'Args)
        : Stream =
        let purpose = getPurpose input

        handleRequest lambdaContext LogCategory.APIRequest purpose (fun appContext ->
            asyncWriterResult {
                let! data = performWork appContext input

                let json = data |> Json.serialize
                return json
            })
        |> function
            | Ok json -> respondWithStream json
            | Error _ -> respondWithStream "An error has occurred"


[<LambdaSerializer(typeof<DefaultLambdaJsonSerializer>)>]
let getItem (dto: RequestDTOs.GetItem) (lambdaContext: ILambdaContext) =
    HandleRequest.directInvoke (lambdaContext, Workflows.getItem, dto)
