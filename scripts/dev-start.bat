@echo off
echo Starting GPA System...

echo.
echo Starting Backend (ASP.NET Core)...
start "Backend API" cmd /k "cd ..\backend\GpaSystem.API && dotnet run"

echo.
echo Starting Frontend (Vite React)...
start "Frontend" cmd /k "cd ..\frontend\gpa-frontend && npm run dev"

echo.
echo System started.
pause