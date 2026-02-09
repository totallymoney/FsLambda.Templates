#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { NewAppStack } from '../lib/NewApp-stack';

const app = new cdk.App();
new NewAppStack(app, 'NewApp');
