# 🎓 GPA System

![Status](https://img.shields.io/badge/Status-In%20Development-orange?style=for-the-badge)
![Backend](https://img.shields.io/badge/Backend-ASP.NET_Core_8.0-512BD4?style=for-the-badge&logo=dotnet)
![Frontend](https://img.shields.io/badge/Frontend-React_19_%2B_Vite-61DAFB?style=for-the-badge&logo=react)
![Database](https://img.shields.io/badge/Database-SQL_Server-CC292B?style=for-the-badge&logo=microsoftsqlserver)

> A comprehensive, modern web-based Student Management and Grade Point Average (GPA) calculation system designed for university administration.

---

## 📖 Overview

The **GPA System** is a modular enterprise web application aimed at streamlining university operations. It enables academic administrators to efficiently manage departments, instructors, students, courses, enrollments, and academic records while automatically calculating complex grading metrics.

### ✨ Key Features
- **Centralized Data Management**: CRUD operations for students, instructors, and departments.
- **Academic Enrollment**: Robust prerequisites handling, semester tracking, and seat availability checking.
- **Grade Evaluation** *(Upcoming)*: Dynamic GPA & CGPA calculation with custom grading policies.
- **Secure Access** *(Upcoming)*: Role-based JWT authentication.

---

## 🏗️ Architecture & Tech Stack

The application uses a cleanly decoupled architecture:

- **Backend**: ASP.NET Core 8 Web API, C#, Entity Framework Core
- **Frontend**: React (v19) with TypeScript, bundled by Vite, styled for modern administrative UX
- **Database**: SQL Server (LocalDB for development)
- **Testing**: xUnit with EF Core In-Memory SQLite provider

---

## 🗺️ Project Structure

Here is the updated layout of the entire project ecosystem:

```text
GPA_System/
├── backend/
│   ├── GpaSystem.API/                      # ASP.NET Core Web API (net8.0)
│   │   ├── Controllers/                    # REST API Controllers (Phase 1 & 2)
│   │   │   ├── CourseOfferingsController.cs
│   │   │   ├── CoursesController.cs
│   │   │   ├── DepartmentsController.cs
│   │   │   ├── EnrollmentsController.cs
│   │   │   ├── InstructorsController.cs
│   │   │   ├── PrerequisitesController.cs
│   │   │   ├── SemestersController.cs
│   │   │   └── StudentsController.cs
│   │   ├── Data/                           # EF Core DbContext
│   │   ├── DTOs/                           # API request/response contracts
│   │   ├── Exceptions/                     # Custom API exceptions
│   │   ├── Models/                         # Domain & EF Core entity models
│   │   ├── Repositories/                   # Entity-specific data wrappers
│   │   ├── Services/                       # Business logic and validation
│   │   └── Program.cs                      # Application entry point
│   └── GpaSystem.API.Tests/                # xUnit test project
│       ├── CourseOfferingServiceTests.cs
│       ├── EnrollmentServiceTests.cs
│       ├── PrerequisiteServiceTests.cs
│       └── TestData.cs
├── frontend/
│   └── gpa-frontend/                       # React + TypeScript + Vite SPA
│       ├── src/
│       │   ├── components/                 # Reusable UI components
│       │   ├── pages/                      # Application views
│       │   │   ├── CoursesPage.tsx
│       │   │   ├── DepartmentsPage.tsx
│       │   │   ├── EnrollmentsPage.tsx
│       │   │   ├── InstructorsPage.tsx
│       │   │   ├── OfferingsPage.tsx
│       │   │   ├── PrerequisitesPage.tsx
│       │   │   ├── SemestersPage.tsx
│       │   │   └── StudentsPage.tsx
│       │   ├── services/                   # API client configuration
│       │   ├── types/                      # TypeScript interfaces
│       │   ├── utils/                      # Helper functions
│       │   └── App.tsx                     # Main routing component
│       ├── package.json
│       └── vite.config.ts
├── scripts/                                # Utility scripts for dev environment
│   ├── dev-start.bat
│   └── dev-stop.bat
├── DBtests/                                # Database testing scripts
├── SystemInfo/                             # Project documentation and specifications
│   ├── GPASystem_SRS.md
│   ├── ImplementationOrder.md
│   ├── Schema.sql
│   ├── UseCases.md
│   └── Structure.txt
├── Progress.md                             # Detailed implementation tracking
├── Structure.md                            # High-level architecture overview
└── README.md                               # This file
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18+)
- [SQL Server Express / LocalDB](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Running Locally

To start the full development environment, use the provided scripts:

1. **Start the environment:**
   Double-click or run `scripts\dev-start.bat`. This will boot up both the .NET API and the Vite React server.
2. **Stop the environment:**
   Use `scripts\dev-stop.bat` to gracefully shut down the dev servers.

Alternatively, you can run them manually:

**Backend:**
```bash
cd backend/GpaSystem.API
dotnet run
```

**Frontend:**
```bash
cd frontend/gpa-frontend
npm install
npm run dev
```

---

## 📈 Current Progress

The development is divided into 5 major phases. Currently, **Phase 2 is completed**.
For an in-depth progress tracker, check out [Progress.md](./Progress.md).

- ✅ **Phase 0:** Project Setup & Infrastructure
- ✅ **Phase 1:** Core Data Management (CRUD)
- ✅ **Phase 2:** Enrollment Basics (Semesters, Offerings, Prerequisites)
- ⏳ **Phase 3:** Grade Entry & Calculation *(Pending)*
- ⏳ **Phase 4:** Reporting & Searching *(Pending)*
- ⏳ **Phase 5:** Authentication & Authorization *(Pending)*

---

*Designed and developed by the GPA System Administration Team.*
