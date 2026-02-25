[<AutoOpen>]
module Setup

open System
open Amazon.Lambda
open AppContext

let getItemOrDefault item defaultValue =
    Environment.tryGetItem item
    |> Option.defaultValue defaultValue

let (<!?>) = getItemOrDefault

let env = {| ExternalApiMockUrl = "FAKE_API_URL" <!?> "http://localhost:9312" |}

let testConfig: Config =
    { Version = ""
      Environment = ""
      FunctionName = ""
      MinimumLogLevel = LogLevel.Info
      ExternalApiFunctionName = "NewApp-dev-externalApi" }

let lambdaClient =
    let config = AmazonLambdaConfig (ServiceURL = env.ExternalApiMockUrl)
    new AmazonLambdaClient ("abc", "xzy", config)

let testContext: AppContext =
    let dateProvider () = DateTimeOffset.Now

    { MinimumLogLevel = testConfig.MinimumLogLevel
      DateProvider = dateProvider
      ExternalAPI = IO.ExternalAPI.Client.configure lambdaClient testConfig.ExternalApiFunctionName }
