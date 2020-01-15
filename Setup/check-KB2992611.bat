@echo off

wmic qfe get hotfixid | FindStr "KB2992611" > NUL
if errorlevel 1 (
	echo Update is not installed
	exit /b %errorlevel%
)