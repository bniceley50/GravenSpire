@echo off
rem PROTOTYPE - NOT FOR PRODUCTION
rem Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
rem Date: 2026-04-26

setlocal
set "PROJECT_ROOT=%~dp0"
if "%PROJECT_ROOT:~-1%"=="\" set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"
set "UNITY_EXE="

for /d %%D in ("C:\Program Files\Unity\Hub\Editor\6000.3.*") do (
  if exist "%%~fD\Editor\Unity.exe" set "UNITY_EXE=%%~fD\Editor\Unity.exe"
)

if not defined UNITY_EXE (
  for /d %%D in ("C:\Program Files\Unity\Hub\Editor\6000.4.*") do (
    if exist "%%~fD\Editor\Unity.exe" set "UNITY_EXE=%%~fD\Editor\Unity.exe"
  )
)

if not defined UNITY_EXE (
  echo Could not find Unity 6000.3.x or 6000.4.x under:
  echo C:\Program Files\Unity\Hub\Editor
  echo.
  echo Install Unity 6000.3.x LTS for authoritative prototype findings.
  pause
  exit /b 1
)

echo Using Unity: %UNITY_EXE%
echo Building CombatFeelPrototype.exe...
"%UNITY_EXE%" -batchmode -nographics -quit -projectPath "%PROJECT_ROOT%" -executeMethod Gravenspire.Prototypes.CombatFeel.Editor.CombatFeelBuildRunner.BuildWindowsPlayer -logFile "%PROJECT_ROOT%\combat-feel-build.log"
if errorlevel 1 (
  echo.
  echo Build failed. See combat-feel-build.log in this folder.
  pause
  exit /b 1
)

echo.
echo Build complete.
echo Run RUN_COMBAT_FEEL.bat to play.
pause
