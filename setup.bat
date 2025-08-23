@echo off
echo ========================================
echo    Library Management System Setup
echo ========================================
echo.

echo [1/3] Setting up database...
cd LibraryManagement.DAL
dotnet ef database update
if %errorlevel% neq 0 (
    echo ERROR: Database setup failed!
    pause
    exit /b 1
)
echo Database setup completed successfully!
echo.

echo [2/3] Building the application...
cd ..
dotnet build
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build completed successfully!
echo.

echo [3/3] Starting the application...
cd LibraryManagement.MVC
echo.
echo ========================================
echo    Application is starting...
echo    Opening browser in 3 seconds...
echo ========================================
echo.
timeout /t 3 /nobreak >nul
start https://localhost:7001
dotnet run

pause
