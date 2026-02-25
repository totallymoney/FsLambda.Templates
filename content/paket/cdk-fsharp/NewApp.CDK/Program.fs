namespace NewApp.CDK

open Amazon.CDK
open Amazon.CDK.AWS.Lambda
open FsCDK

module Program =
    [<EntryPoint>]
    let main _argv =
        stack "NewApp" {

            lambda "GetItem" {
                runtime Runtime.LambdaRuntimePlaceholder
                handler "NewApp::Lambda.Handlers::getItem"
                code "../publish"
                environment [ "MINIMUM_LOG_LEVEL", "info" ]

                functionUrl { authType FunctionUrlAuthType.NONE }
            }
        }
        |> ignore

        0
