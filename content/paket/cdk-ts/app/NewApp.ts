#!/usr/bin/env node
import "source-map-support/register";
import * as cdk from "aws-cdk-lib";
import { NewAppStack } from "../lib/NewApp-stack";

const app = new cdk.App();
const version =
  app.node.tryGetContext("version") ?? process.env.VERSION ?? "v0.0.0";

function stack(envName: string) {
  new NewAppStack(app, `NewApp-${envName}`, {
    // TODO: Configure your AWS account and region
    // env: { account: "YOUR_ACCOUNT_ID", region: "eu-west-1" },
    stackName: `NewApp-${envName}`,
    envName,
    version,
  });
}

stack("stage");
stack("prod");
