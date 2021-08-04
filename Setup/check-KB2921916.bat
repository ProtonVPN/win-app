@echo off

wmic qfe get hotfixid | FindStr "KB2921916" > NUL
if errorlevel 1 (
	echo Update is not installed
	exit /b %errorlevel%
)