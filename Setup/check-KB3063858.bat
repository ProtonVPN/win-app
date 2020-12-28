@echo off

wmic qfe get hotfixid | FindStr "KB3063858" > NUL
if errorlevel 1 (
	echo Update is not installed
	exit /b %errorlevel%
)