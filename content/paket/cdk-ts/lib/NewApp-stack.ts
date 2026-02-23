import { Construct } from "constructs";
import * as cdk from "aws-cdk-lib";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as path from "path";

interface NewAppStackProps extends cdk.StackProps {
  readonly envName: string;
  readonly version: string;
}

export class NewAppStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: NewAppStackProps) {
    super(scope, id, props);

    const helloWorldFunction = new lambda.Function(this, "HelloWorld", {
      functionName: `NewApp-${props.envName}-helloWorld`,
      runtime: lambda.Runtime.LambdaRuntimePlaceholder,
      handler: "NewApp::NewApp.Handler::sayHello",
      code: lambda.Code.fromAsset(path.join(__dirname, "../../publish")),
      architecture: lambda.Architecture.ARM_64,
      memorySize: 512,
      timeout: cdk.Duration.seconds(30),
      environment: {
        VERSION: props.version,
        ENVIRONMENT: props.envName,
      },
    });

    const functionUrl = helloWorldFunction.addFunctionUrl({
      authType: lambda.FunctionUrlAuthType.NONE,
    });

    new cdk.CfnOutput(this, "FunctionUrl", {
      value: functionUrl.url,
      description: "Lambda Function URL",
    });
  }
}
