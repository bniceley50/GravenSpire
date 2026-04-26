@echo off
rem PROTOTYPE - NOT FOR PRODUCTION
rem Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
rem Date: 2026-04-26

setlocal
set "PROJECT_ROOT=%~dp0"
if "%PROJECT_ROOT:~-1%"=="\" set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"
set "GAME_EXE=%PROJECT_ROOT%\Builds\CombatFeelPrototype\CombatFeelPrototype.exe"

if not exist "%GAME_EXE%" (
  echo CombatFeelPrototype.exe does not exist yet.
  echo.
  echo First run BUILD_COMBAT_FEEL.bat from this same folder.
  echo Then run this file again.
  pause
  exit /b 1
)

start "" "%GAME_EXE%"
