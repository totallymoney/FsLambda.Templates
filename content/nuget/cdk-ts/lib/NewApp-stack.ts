import * as cdk from 'aws-cdk-lib';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import { Construct } from 'constructs';
import * as path from 'path';

export class NewAppStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const helloWorldFunction = new lambda.Function(this, 'HelloWorld', {
      // Note: Update to DOTNET_10 when available in AWS CDK
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'NewApp::NewApp.Handler::sayHello',
      code: lambda.Code.fromAsset(
        path.join(__dirname, '../../src/NewApp/bin/Release/TargetFrameworkValue/publish')
      ),
      memorySize: 256,
      timeout: cdk.Duration.seconds(30),
    });

    const functionUrl = helloWorldFunction.addFunctionUrl({
      authType: lambda.FunctionUrlAuthType.NONE,
    });

    new cdk.CfnOutput(this, 'FunctionUrl', {
      value: functionUrl.url,
      description: 'Lambda Function URL',
    });
  }
}
