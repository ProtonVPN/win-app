pushd ..\ProtonVPN.LocalAgent\localAgentWin
set GOOS=windows
set GO111MODULE=auto
set CGO_ENABLED=1
set CGO_CFLAGS=-O3 -Wall -Wno-unused-function -Wno-switch -std=gnu11 -DWINVER=0x0601
set CC=x86_64-w64-mingw32-gcc

set GOARCH=386
go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o "..\..\bin\Resources\32-bit\LocalAgent.dll" || exit /b 1

set GOARCH=amd64
go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o "..\..\bin\Resources\64-bit\LocalAgent.dll" || exit /b 1