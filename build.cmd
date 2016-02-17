@echo off
cls

.paket\paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

@echo off
powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& { Import-Module .\tools\psake\psake.psm1; Invoke-psake .\build\build.ps1 %*; exit !($psake.build_success) }" 
