# 0002. Utilize SQL Server as primary database engine

* **Status:** Accepted
* **Date:** 2026-09-18
* **Decision-maker:** Developer Joaquin Sosa

## Context
The application requires a database engine to store persistent data. When evaluating database solutions within the .NET ecosystem, the primary goal was to choose a technology that is widely adopted in industry standard applications, minimizing integration friction and leveraging community best practices.

* **Ecosystem Compatibility:** Seamless integration with Entity Framework Core and .NET ORM tooling.
* **Industry Standard:** Adopting a widely used database in enterprise .NET applications to maintain consistency with market practices and leverage extensive community support.
* **Developer Familiarity:** Standardizing on a database solution with abundant tooling, documentation, and operational experience in the .NET environment.

## Decision
We will use **Microsoft SQL Server** as the primary relational database management system (RDBMS) for the application.

### Pros
* **Native .NET Integration:** First-class support and optimizations within the Microsoft/.NET software stack and Entity Framework Core.
* **Industry Standard & Tooling:** Highly standard in enterprise .NET projects and widespread community knowledge.
* **Ecosystem Support:** Comprehensive support for migrations, integration testing containers (Testcontainers), and hosting options (Azure SQL, Docker containers, on-premise).

### Cons
* **Resource Consumption:** Generally higher memory and resource footprint in small container environments compared to lightweight open-source database engines.