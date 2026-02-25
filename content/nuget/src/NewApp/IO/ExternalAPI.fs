module IO.ExternalAPI

open Amazon.Lambda
open AsyncWriterResult


type GetItem = string -> AWR<ItemDto>

type Client = { GetItem: GetItem }

let private getItem (client: AmazonLambdaClient) (functionName: string) : GetItem =
    fun itemId ->
        asyncWriterResult {
            let payload = Json.serialize {| ItemId = itemId |}

            let! body = Platform.Lambda.invoke client functionName payload
            let! item = Json.tryDeserialize<ItemDto> body

            return item
        }


module Client =
    let configure (client: AmazonLambdaClient) (functionName: string) : Client =
        { GetItem = getItem client functionName }
