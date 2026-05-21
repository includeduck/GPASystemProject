# GPA System

![Status](https://img.shields.io/badge/Status-Phase%205%20Complete-brightgreen?style=for-the-badge)
![Backend](https://img.shields.io/badge/Backend-ASP.NET_Core_8.0-512BD4?style=for-the-badge&logo=dotnet)
![Frontend](https://img.shields.io/badge/Frontend-React_19_%2B_Vite-61DAFB?style=for-the-badge&logo=react)
![Database](https://img.shields.io/badge/Database-SQL_Server-CC292B?style=for-the-badge&logo=microsoftsqlserver)

A web-based Student Management and GPA calculation system for university administration, enrollment, grade entry, reporting, and role-based access.

## Overview

The GPA System is a modular ASP.NET Core and React application. It supports department, student, instructor, course, semester, offering, prerequisite, enrollment, grading, GPA/CGPA, reporting, export, and authentication workflows.

### Key Features

- Centralized CRUD for departments, students, instructors, and courses.
- Semester, offering, prerequisite, and enrollment management with capacity and prerequisite validation.
- Grade components, mark entry, grade finalization, GPA/CGPA calculation, and configurable grading policies.
- Transcript, semester, course, department, warning, ranking, CSV, and PDF report workflows.
- Manual JWT authentication with admin, instructor, and student role-based authorization.
- Password change, admin password reset, login activity audit logging, and 15-minute session expiry.

## Architecture & Tech Stack

- Backend: ASP.NET Core 8 Web API, C#, Entity Framework Core, JWT bearer auth
- Frontend: React 19, TypeScript, Vite, axios, react-router-dom, lucide-react
- Database: SQL Server for development/runtime, SQLite in-memory for tests
- Testing: xUnit, EF Core SQLite, ASP.NET Core WebApplicationFactory integration tests

## Project Structure

```text
GPA_System/
|-- backend/
|   |-- GpaSystem.API/                # ASP.NET Core Web API
|   |   |-- Controllers/              # Auth, admin, academic, grading, enrollment, reports
|   |   |-- Data/                     # EF Core DbContext and SQL schema mappings
|   |   |-- DTOs/                     # API request/response contracts
|   |   |-- Exceptions/               # HTTP-aware API exceptions
|   |   |-- Models/                   # EF Core entity models
|   |   |-- Repositories/             # Entity-specific data access wrappers
|   |   |-- Services/                 # Business logic, auth, grading, reports
|   |   `-- Program.cs                # DI, CORS, Swagger, JWT auth, endpoints
|   `-- GpaSystem.API.Tests/          # xUnit service and authorization tests
|-- frontend/
|   `-- gpa-frontend/                 # React + TypeScript + Vite SPA
|       |-- src/
|       |   |-- auth/                 # AuthProvider and session handling
|       |   |-- components/           # Reusable UI components
|       |   |-- pages/                # Login, profile, admin, student, instructor, reports
|       |   |-- services/             # Axios API client and auth token interceptor
|       |   |-- types/                # TypeScript API models/forms
|       |   `-- utils/                # Date helpers
|       `-- package.json
|-- scripts/                          # Local dev start/stop scripts
|-- DBtests/                          # Database testing scripts
|-- SystemInfo/                       # SRS, use cases, schema, implementation order, structure
|-- Progress.md                       # Phase progress tracker
|-- Structure.md                      # High-level architecture overview
`-- README.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server Express or LocalDB

### Backend Configuration

`backend/GpaSystem.API/appsettings.Development.json` contains the local SQL Server connection string and JWT settings:

```json
"Jwt": {
  "Issuer": "GpaSystem",
  "Audience": "GpaSystemClient",
  "SigningKey": "GpaSystemPhase5LocalDevelopmentSigningKey2026",
  "ExpiryMinutes": 15
}
```

For non-local environments, replace the signing key with a secure secret of at least 32 characters.

### Running Locally

Start both development servers:

```powershell
scripts\dev-start.bat
```

Stop both development servers:

```powershell
scripts\dev-stop.bat
```

Manual startup:

```powershell
cd backend\GpaSystem.API
dotnet run
```

```powershell
cd frontend\gpa-frontend
npm install
npm run dev
```

### First Admin Login

In development, open the frontend login page and use **Bootstrap Admin**. This calls:

```http
POST /api/admin/bootstrap-admin
```

The endpoint is development-only and succeeds only when no administrator exists. It returns a generated temporary password that can be used to sign in.

## Current Progress

The original five implementation phases are complete.

- Completed Phase 0: Project Setup & Infrastructure
- Completed Phase 1: Core Data Management
- Completed Phase 2: Enrollment Basics
- Completed Phase 3: Grade Entry & Calculation
- Completed Phase 4: Reporting & Searching
- Completed Phase 5: Authentication & Authorization

See [Progress.md](./Progress.md) for detailed phase notes.

## Verification

```powershell
dotnet test backend\GpaSystem.API.Tests\GpaSystem.API.Tests.csproj
dotnet build backend\GpaSystem.API\GpaSystem.API.csproj --no-restore
cd frontend\gpa-frontend
npm run lint
npm run build
```

