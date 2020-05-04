pushd ..\srp\windows\cshared
set fn=GoSrp
set gn=main.go
set CGO_ENABLED=1

set GOARCH=386
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\x86\%fn%.dll %gn%

set GOARCH=amd64
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\x64\%fn%.dll %gn%