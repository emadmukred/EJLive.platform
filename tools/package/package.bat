@echo off
REM ============================================================
REM  EJLIVE.PLATFORM packaging - v4.0.0
REM  Produces three endpoint payloads from a completed build:
REM    EJLive_Client_v4.0.0.zip   agent service + session companion + installer
REM    EJLive_Server_v4.0.0.zip   archiving server + WinForms host
REM    EJLive_NOC_v4.0.0.zip      operations console
REM  Usage:  package.bat [Release^|Debug]
REM  Precondition: tools\build\build.ps1 -Configuration %CONFIG% has run.
REM ============================================================
setlocal EnableDelayedExpansion

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release
set VERSION=4.0.0
set SRC=%~dp0..\..\src
set OUT_DIR=%~dp0dist
set STAGE=%~dp0stage
set TFM=net8.0-windows

echo.
echo  == EJLIVE.PLATFORM packaging v%VERSION% ==================================
echo  Config: %CONFIG%   Stage: %STAGE%   Output: %OUT_DIR%
echo.

if exist "%OUT_DIR%" rd /s /q "%OUT_DIR%"
if exist "%STAGE%" rd /s /q "%STAGE%"
md "%OUT_DIR%"
md "%STAGE%"

REM --- client payload: service host, session companion, installer -------------
call :copybin "%SRC%\EJLive.Client.Service\bin\%CONFIG%\%TFM%" "%STAGE%\client"  client
call :copybin "%SRC%\EJLive.Client.WinForms\bin\%CONFIG%\%TFM%" "%STAGE%\client"  client
call :copybin "%SRC%\EJLive.Installer.WinForms\bin\%CONFIG%\%TFM%" "%STAGE%\client" client
call :copybin "%SRC%\EJLive.Core\bin\%CONFIG%\%TFM%" "%STAGE%\client" client
call :copybin "%SRC%\EJLive.Shared\bin\%CONFIG%\%TFM%" "%STAGE%\client" client

REM --- server payload: engine library, headless services, WinForms host -------
call :copybin "%SRC%\EJLive.Core\bin\%CONFIG%\%TFM%" "%STAGE%\server" server
call :copybin "%SRC%\EJLive.Shared\bin\%CONFIG%\%TFM%" "%STAGE%\server" server
call :copybin "%SRC%\EJLive.Server\bin\%CONFIG%\%TFM%" "%STAGE%\server" server
call :copybin "%SRC%\EJLive.Server.WinForms\bin\%CONFIG%\%TFM%" "%STAGE%\server" server
call :copybin "%SRC%\EJLive.Business\bin\%CONFIG%\%TFM%" "%STAGE%\server" server
call :copybin "%SRC%\EJLive.Application\bin\%CONFIG%\%TFM%" "%STAGE%\server" server

REM --- NOC payload: operations console ---------------------------------------
call :copybin "%SRC%\EJLive.Monitoring.WinForms\bin\%CONFIG%\%TFM%" "%STAGE%\noc" noc
call :copybin "%SRC%\EJLive.Core\bin\%CONFIG%\%TFM%" "%STAGE%\noc" noc
call :copybin "%SRC%\EJLive.Shared\bin\%CONFIG%\%TFM%" "%STAGE%\noc" noc

REM --- journal fixtures travel with the client so field installs self-test ----
if exist "%SRC%\EJLive.Tests\Samples" (
  md "%STAGE%\client\Samples" 2>nul
  copy /y "%SRC%\EJLive.Tests\Samples\*.LOG" "%STAGE%\client\Samples\" >nul
)

> "%STAGE%\client\install.cmd" echo @echo off
>>"%STAGE%\client\install.cmd" echo EJLive.Installer.exe --install --service EJLive.Client.Service --silent
> "%STAGE%\server\start.cmd" echo @echo off
>>"%STAGE%\server\start.cmd" echo start "" EJLive.Server.WinForms.exe
> "%STAGE%\noc\start.cmd" echo @echo off
>>"%STAGE%\noc\start.cmd" echo start "" EJLive.Monitoring.exe

set FAIL=0
for %%P in (client server noc) do (
  if not exist "%STAGE%\%%P\" set FAIL=1
)
if "%FAIL%"=="1" (
  echo  ERROR: a payload stage is empty -- run tools\build\build.ps1 first. 1>&2
  exit /b 2
)

for %%P in (client:EJLive_Client server:EJLive_Server noc:EJLive_NOC) do (
  for /f "tokens=1,2 delims=:" %%A in ("%%P") do (
    powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\%%A\*' -DestinationPath '%OUT_DIR%\%%B_v%VERSION%.zip' -Force"
    echo  packaged  %OUT_DIR%\%%B_v%VERSION%.zip
  )
)
echo.
echo  packaging: OK
exit /b 0

REM ---------------------------------------------------------------------------
:copybin
REM  %1 = project bin directory, %2 = payload stage, %3 = payload name (for log)
if not exist "%~f1" (
  echo   skip      %3  ^(no build output at %~1^) 1>&2
  exit /b 0
)
md "%~2" 2>nul
xcopy /y /q /s "%~f1\*" "%~2\" | find /c /v "" >nul
echo   staged    %3  <- %~nx1
exit /b 0
