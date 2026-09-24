# 0006. Target Azure instead of AWS

* **State:** Accepted
* **Date:** 2026-09-24
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code
* **Amends:** ADR 0001 (the "Robust automation" goal mentions AWS: Lambda, ECS, EC2)

## Context
The original brief targeted AWS for deployment (roadmap step 5), DynamoDB as the key-value/document store (step 10) and AWS Lambda for the serverless component (step 12). Nothing has been deployed yet, so no infrastructure depends on that choice.

This project is also a career tool: it should build experience that is directly useful when applying for back-end and DevOps roles. The stack is .NET, SQL Server, ASP.NET Core and EF Core. The developer already has hands-on AWS experience (Lambda, API Gateway, DynamoDB, S3, Cognito, CloudWatch, IAM, CloudFormation, Boto3), and cloud concepts (compute, managed databases, IAM, IaC, budgets) transfer between providers.

## Decision
**Azure is the target cloud.** The roadmap changes as follows:

| Step | Before | After |
|---|---|---|
| 5. Deploy | AWS (IaC, free plan) | Azure (IaC, free account, budget alert first) |
| 10. NoSQL | DynamoDB | Cosmos DB (free tier: 1000 RU/s, 25 GB) |
| 12. Serverless | AWS Lambda | Azure Functions |

Deploying to AWS remains a possible later exercise, likely from a fork taken after the Azure deployment works.

The architecture is unaffected. Clean Architecture (ADR 0001) and repositories per aggregate (ADR 0004) keep cloud-specific code in Infrastructure and in the entry points, so this decision changes deployment and adapters, not the Domain or Application layers.

### Pros
* **Fits the stack.** .NET, SQL Server and Azure are the usual combination in .NET job postings, and first-party tooling (Azure SQL, App Service/Container Apps, Entra ID, Azure Functions isolated worker) supports .NET first.
* **Better job-market return.** Azure experience is more often requested for .NET back-end roles than AWS.
* **Broader profile.** Existing AWS foundations plus hands-on Azure gives experience with both major clouds instead of deepening only one.
* **Free options for every step.** Azure free account, Cosmos DB free tier, and the Azure Functions consumption plan.

### Cons
* Less reuse of existing AWS experience in this project. Each Azure service has an AWS counterpart already known (Functions ↔ Lambda, Cosmos DB ↔ DynamoDB, Entra ID ↔ Cognito, Bicep ↔ CloudFormation), which shortens the learning curve.
* The Python microservice (step 7) will use the Azure SDK instead of Boto3.
* Cosmos DB's free tier is limited to one account per subscription, and RU-based pricing needs watching outside it.

## Alternatives considered

* **Stay on AWS.** Builds on existing knowledge and has a strong free tier, but it is less aligned with the .NET job market being targeted. Kept as a later exercise.
* **Both clouds from the start (multi-cloud IaC).** Doubles deployment work before a single deployment exists. Rejected for now.
* **Cloud-agnostic only (containers on a VPS or Kubernetes).** Teaches portability but misses managed-service experience (managed SQL, serverless, managed NoSQL), which is what the roadmap is meant to practise. Rejected.