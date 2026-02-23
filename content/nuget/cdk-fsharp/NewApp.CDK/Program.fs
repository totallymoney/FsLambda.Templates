namespace NewApp.CDK

open Amazon.CDK
open Amazon.CDK.AWS.Lambda
open FsCDK

module Program =
    [<EntryPoint>]
    let main _argv =
        // Define a stack with a simple HelloWorld Lambda and a public Function URL
        stack "NewApp" {

            lambda "HelloWorld" {
                runtime Runtime.LambdaRuntimePlaceholder
                // Namespace: NewApp; Module: Handler; Function: sayHello
                handler "NewApp::NewApp.Handler::sayHello"
                // Use the published output from the top-level publish directory
                code "../publish"

                // Public function URL (no auth) for quick testing
                functionUrl { authType FunctionUrlAuthType.NONE }
            }
        }
        |> ignore

        0
