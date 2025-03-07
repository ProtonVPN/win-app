@echo off
setlocal

:: Define source and destination directories
set "SOURCE=%~dp0CSharp"
set "DEST=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\ItemTemplates"

:: Check if source exists
if not exist "%SOURCE%" (
    echo Source folder "%SOURCE%" not found!
    pause
    exit /b 1
)

:: Copy folder with all contents
xcopy "%SOURCE%" "%DEST%\CSharp" /E /I /Y

:: Check if the copy was successful
if %ERRORLEVEL% neq 0 (
    echo Copy failed! Maybe you forgot to run as Administrator?
    pause
    exit /b 1
)

echo Copy completed successfully!
pause