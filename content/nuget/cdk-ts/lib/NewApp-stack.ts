import { Construct } from "constructs";
import * as cdk from "aws-cdk-lib";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as path from "path";

interface NewAppStackProps extends cdk.StackProps {
  readonly envName: string;
  readonly version: string;
  readonly minimumLogLevel: string;
}

export class NewAppStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: NewAppStackProps) {
    super(scope, id, props);

    const getItemFunction = new lambda.Function(this, "GetItem", {
      functionName: `NewApp-${props.envName}-getItem`,
      runtime: lambda.Runtime.LambdaRuntimePlaceholder,
      handler: "NewApp::Lambda.Handlers::getItem",
      code: lambda.Code.fromAsset(path.join(__dirname, "../../publish")),
      architecture: lambda.Architecture.ARM_64,
      memorySize: 512,
      timeout: cdk.Duration.seconds(30),
      environment: {
        VERSION: props.version,
        ENVIRONMENT: props.envName,
        MINIMUM_LOG_LEVEL: props.minimumLogLevel,
      },
    });

    const functionUrl = getItemFunction.addFunctionUrl({
      authType: lambda.FunctionUrlAuthType.NONE,
    });

    new cdk.CfnOutput(this, "FunctionUrl", {
      value: functionUrl.url,
      description: "Lambda Function URL",
    });
  }
}
