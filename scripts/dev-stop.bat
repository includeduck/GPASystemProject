@echo off

echo Stopping frontend (node/vite)...

powershell -NoProfile -Command ^
"Get-Process node -ErrorAction SilentlyContinue | Stop-Process -Force"

echo Stopping backend (.NET)...

powershell -NoProfile -Command ^
"Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force"

echo Done.
pause