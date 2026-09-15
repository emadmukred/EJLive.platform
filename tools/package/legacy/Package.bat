@echo off
REM ============================================================
REM  EJLive Enterprise Ultimate v4.0.0 — Package & Zip Script
REM  يحزم مخرجات البناء في حزمة منفصلة:
REM    EJLive_Client_v4.0.0.zip  — تطبيق الصراف
REM    EJLive_Server_v4.0.0.zip  — خادم الأرشفة + واجهة المراقبة الموحّدة
REM  الاستخدام: Package.bat [Release|Debug]
REM
REM  ملاحظة (Wave 6 / C-34): هذه النسخة القديمة من سكربت التحزيم، وهي محفوظة هنا
REM  لأنها كانت تتعارض مع package.bat في نفس المجلد باسم يختلف بحالة الأحرف فقط —
REM  وهو تعارض لا يستطيع ويندوز تمثيله عند السحب (checkout). النسخة المعتمدة هي
REM  tools/package/package.bat. كما لم تعد هناك حزمة NOC منفصلة: لوحة المراقبة
REM  وُحِّدت داخل السيرفر المركزي (تبويب NOC Monitoring).
REM ============================================================
setlocal EnableDelayedExpansion

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release
set VERSION=4.0.0
set OUT_DIR=%~dp0dist

echo.
echo  ══ EJLive Enterprise Ultimate — Packaging v%VERSION% ══════
echo  Config: %CONFIG%  ^|  Output: %OUT_DIR%
echo.

REM ── تنظيف وإنشاء مجلد dist ─────────────────────────────────
if exist "%OUT_DIR%" rd /s /q "%OUT_DIR%"
md "%OUT_DIR%"

REM ── تحقق وجود 7-Zip أو PowerShell للضغط ─────────────────────
set ZIP_CMD=""
for %%z in (
    "C:\Program Files\7-Zip\7z.exe"
    "C:\Program Files (x86)\7-Zip\7z.exe"
) do ( if exist %%z set ZIP_CMD=%%z )

if %ZIP_CMD%=="" (
    echo  [INFO] 7-Zip غير موجود، سيُستخدم PowerShell Compress-Archive
    set USE_POWERSHELL=1
) else (
    echo  [OK] 7-Zip: %ZIP_CMD%
    set USE_POWERSHELL=0
)

REM ── دالة الضغط ───────────────────────────────────────────────
REM  ستُستخدم: ZipFolder <src_folder> <dest_zip>

echo.
echo  [1/2] Packaging Client (ATM Agent)...
set CLIENT_SRC=%~dp0EJLive.Client.WinForms\bin\%CONFIG%
set CLIENT_ZIP=%OUT_DIR%\EJLive_Client_v%VERSION%.zip
if not exist "%CLIENT_SRC%" ( echo  [WARN] لم يُبنَ Client بعد. شغّل Build.bat أولًا. ) else (
    call :ZipFolder "%CLIENT_SRC%" "%CLIENT_ZIP%"
    if exist "%CLIENT_ZIP%" echo  [OK] %CLIENT_ZIP%
)

echo.
echo  [2/2] Packaging Server (Archive Server + unified NOC console)...
set SERVER_SRC=%~dp0EJLive.Server.WinForms\bin\%CONFIG%
set SERVER_ZIP=%OUT_DIR%\EJLive_Server_v%VERSION%.zip
if not exist "%SERVER_SRC%" ( echo  [WARN] لم يُبنَ Server بعد. شغّل Build.bat أولًا. ) else (
    call :ZipFolder "%SERVER_SRC%" "%SERVER_ZIP%"
    if exist "%SERVER_ZIP%" echo  [OK] %SERVER_ZIP%
)

echo.
REM ── Wave 6 / C-34: لا توجد حزمة NOC منفصلة ─────────────────────
REM واجهة المراقبة (EJLive.Monitoring.WinForms) أُحيلت إلى src/_reference/ بعد
REM توحيدها داخل EJLive.Server.WinForms.exe — يفتحها الأمر:
REM     EJLive.Server.WinForms.exe --noc

REM ── إنشاء README في dist ─────────────────────────────────────
echo EJLive Enterprise Ultimate v%VERSION% > "%OUT_DIR%\README.txt"
echo Built: %DATE% %TIME%               >> "%OUT_DIR%\README.txt"
echo Config: %CONFIG%                   >> "%OUT_DIR%\README.txt"
echo.                                   >> "%OUT_DIR%\README.txt"
echo Packages:                          >> "%OUT_DIR%\README.txt"
echo   EJLive_Client_v%VERSION%.zip — Install on ATM machines (NCR/GRG/WN) >> "%OUT_DIR%\README.txt"
echo   EJLive_Server_v%VERSION%.zip — Install on the central archive server >> "%OUT_DIR%\README.txt"
echo   NOC console: unified into the Server payload (EJLive.Server.WinForms.exe --noc) >> "%OUT_DIR%\README.txt"
echo.                                   >> "%OUT_DIR%\README.txt"
echo Prerequisites: .NET Framework 4.8, Windows 7 SP1 or later >> "%OUT_DIR%\README.txt"
echo TCP Port: 5656 (ensure firewall allows inbound on server)  >> "%OUT_DIR%\README.txt"

echo.
echo  ══ Packaging Complete — Output: %OUT_DIR% ═══════════════
echo.
explorer "%OUT_DIR%" 2>nul
pause
endlocal
exit /b 0

REM ─────────────────────────────────────────────────────────────
:ZipFolder
REM  %1 = source folder  %2 = destination zip
if %USE_POWERSHELL%==1 (
    powershell -NoProfile -Command ^
        "Compress-Archive -Path '%~1\*' -DestinationPath '%~2' -Force"
) else (
    %ZIP_CMD% a -tzip "%~2" "%~1\*" -mx=5 >nul
)
exit /b
