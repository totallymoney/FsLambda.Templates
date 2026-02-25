open System
open System.IO
open System.Text.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

let port =
    match Environment.GetEnvironmentVariable "PORT" with
    | null
    | "" -> "80"
    | p -> p

let builder = WebApplication.CreateBuilder ()

builder.WebHost.UseUrls ($"http://0.0.0.0:{port}")
|> ignore

let app = builder.Build ()

let knownItemId = "item-001"

let camelCase =
    JsonSerializerOptions (PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

let itemResponse =
    JsonSerializer.Serialize (
        {| ItemId = "item-001"
           Name = "Sample Item"
           CreatedOn = "2024-01-15T10:30:00+00:00" |},
        camelCase
    )

app.MapGet (
    "/healthcheck",
    Func<HttpContext, _> (fun (context: HttpContext) ->
        app.Logger.Log (LogLevel.Information, "Received a request at '/healthcheck' endpoint")
        context.Response.WriteAsync ("OK"))
)
|> ignore

app.MapPost (
    "/2015-03-31/functions/{functionName}/invocations",
    Func<HttpContext, Task> (fun (context: HttpContext) ->
        task {
            let functionName = context.Request.RouteValues["functionName"] :?> string
            app.Logger.Log (LogLevel.Information, $"Received a Lambda invoke request for {functionName}")

            let! bodyStr =
                use reader = new StreamReader (context.Request.Body)
                reader.ReadToEndAsync ()

            let body = JsonDocument.Parse bodyStr
            let itemId = body.RootElement.GetProperty("ItemId").GetString ()

            app.Logger.Log (LogLevel.Information, $"ItemId: {itemId}")

            if String.Equals (itemId, knownItemId, StringComparison.OrdinalIgnoreCase) then
                context.Response.ContentType <- "application/json"
                do! context.Response.WriteAsync itemResponse
            else
                context.Response.Headers.Append ("X-Amz-Function-Error", "Unhandled")
                context.Response.ContentType <- "application/json"

                do!
                    context.Response.WriteAsync (
                        """{"errorType": "ItemNotFoundError", "errorMessage": "Item not found"}"""
                    )
        }
        :> Task)
)
|> ignore

app.Run ()
