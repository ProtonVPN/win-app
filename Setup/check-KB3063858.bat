@echo off

wmic qfe get hotfixid | FindStr "KB3063858 KB2533623 KB4457144 KB3126587 KB3126593 KB3146706 KB4014793" > NUL
if errorlevel 1 (
	echo Update is not installed
	exit /b %errorlevel%
)