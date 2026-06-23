# 🎓 GPA System

![Status](https://img.shields.io/badge/Status-Phase%205%20Complete-brightgreen?style=for-the-badge)
[![Backend](https://img.shields.io/badge/Backend-ASP.NET_Core_8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Frontend](https://img.shields.io/badge/Frontend-React_19_%2B_Vite-61DAFB?style=for-the-badge&logo=react)](https://react.dev/)
[![Database](https://img.shields.io/badge/Database-SQL_Server-CC292B?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)

> A comprehensive web-based Student Management and GPA calculation system for university administration, enrollment, grade entry, reporting, and role-based access.

## 📋 Overview

The GPA System is a modular ASP.NET Core and React application designed for comprehensive university administration. It supports department, student, instructor, course, semester, offering, prerequisite, enrollment, grading, GPA/CGPA, reporting, and role-based access management with full audit logging and security controls.

### ✨ Key Features

- 📚 **Centralized CRUD** — Departments, students, instructors, and courses management
- 🎓 **Academic Planning** — Semester, offering, prerequisite, and enrollment management with capacity and prerequisite validation
- 📊 **Grading System** — Grade components, mark entry, grade finalization, GPA/CGPA calculation, and configurable grading policies
- 📄 **Comprehensive Reports** — Transcript, semester, course, department, warning, ranking, CSV, and PDF exports
- 🔐 **Enterprise Security** — Manual JWT authentication with admin, instructor, and student role-based authorization
- 🛡️ **Audit & Compliance** — Password change, admin password reset, login activity audit logging, and 15-minute session expiry

## 🏗️ Architecture & Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend API** | [ASP.NET Core 8](https://dotnet.microsoft.com/) — C#, Entity Framework Core, JWT Bearer Auth |
| **Frontend** | [React 19](https://react.dev/) — TypeScript, [Vite](https://vitejs.dev/), axios, react-router-dom, lucide-react |
| **Database** | [SQL Server](https://www.microsoft.com/sql-server) (Development/Runtime), SQLite In-Memory (Tests) |
| **Testing** | xUnit, EF Core SQLite, ASP.NET Core WebApplicationFactory Integration Tests |

## 📁 Project Structure

```
GPA_System/
├── backend/
│   ├── GpaSystem.API/                    # 🔧 ASP.NET Core Web API
│   │   ├── Controllers/                 # Auth, Admin, Academic, Grading, Enrollment, Reports
│   │   ├── Data/                        # EF Core DbContext & SQL Schema Mappings
│   │   ├── DTOs/                        # API Request/Response Contracts
│   │   ├── Exceptions/                  # HTTP-Aware API Exceptions
│   │   ├── Models/                      # EF Core Entity Models
│   │   ├── Repositories/                # Entity-Specific Data Access Wrappers
│   │   ├── Services/                    # Business Logic, Auth, Grading, Reports
│   │   └── Program.cs                   # DI, CORS, Swagger, JWT Auth, Endpoints
│   └── GpaSystem.API.Tests/             # ✅ xUnit Service & Authorization Tests
├── frontend/
│   └── gpa-frontend/                    # ⚛️ React + TypeScript + Vite SPA
│       ├── src/
│       │   ├── auth/                    # AuthProvider & Session Handling
│       │   ├── components/              # Reusable UI Components
│       │   ├── pages/                   # Login, Profile, Admin, Student, Instructor, Reports
│       │   ├── services/                # Axios API Client & Auth Interceptor
│       │   ├── types/                   # TypeScript API Models/Forms
│       │   └── utils/                   # Date Helpers
│       └── package.json
├── scripts/                             # 🚀 Local Dev Start/Stop Scripts
├── DBtests/                             # 🗄️ Database Testing Scripts
├── SystemInfo/                          # 📚 SRS, Use Cases, Schema, Implementation Order
├── Progress.md                          # 📈 Phase Progress Tracker
├── Structure.md                         # 🏛️ High-Level Architecture Overview
└── README.md                            # 📖 This File
```

## 🚀 Getting Started

### 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) or LocalDB

### ⚙️ Backend Configuration

The `backend/GpaSystem.API/appsettings.Development.json` file contains local SQL Server connection string and JWT settings:

```json
"Jwt": {
  "Issuer": "GpaSystem",
  "Audience": "GpaSystemClient",
  "SigningKey": "GpaSystemPhase5LocalDevelopmentSigningKey2026",
  "ExpiryMinutes": 15
}
```

> ⚠️ **Security Note:** For non-local environments, replace the signing key with a secure secret of at least 32 characters.

### 🏃 Running Locally

**Quick Start (Both Servers):**

```powershell
scripts\dev-start.bat
```

**Stop Both Servers:**

```powershell
scripts\dev-stop.bat
```

**Manual Startup:**

Backend:
```powershell
cd backend\GpaSystem.API
dotnet run
```

Frontend:
```powershell
cd frontend\gpa-frontend
npm install
npm run dev
```

### 🔓 First Admin Login

In development, open the frontend login page and use **Bootstrap Admin**. This calls:

```http
POST /api/admin/bootstrap-admin
```

The endpoint is development-only and succeeds only when no administrator exists. It returns a generated temporary password that can be used to sign in.

## 📈 Current Progress

All original five implementation phases are complete:

- ✅ **Phase 0** — Project Setup & Infrastructure
- ✅ **Phase 1** — Core Data Management
- ✅ **Phase 2** — Enrollment Basics
- ✅ **Phase 3** — Grade Entry & Calculation
- ✅ **Phase 4** — Reporting & Searching
- ✅ **Phase 5** — Authentication & Authorization

See [Progress.md](./Progress.md) for detailed phase notes and implementation details.

## ✔️ Verification

Run tests and build verification:

```powershell
# Run all unit tests
dotnet test backend\GpaSystem.API.Tests\GpaSystem.API.Tests.csproj

# Build backend
dotnet build backend\GpaSystem.API\GpaSystem.API.csproj --no-restore

# Verify frontend
cd frontend\gpa-frontend
npm run lint
npm run build
```

