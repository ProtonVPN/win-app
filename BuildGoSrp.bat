pushd %~dp0\src\srp\windows\cshared
set CGO_ENABLED=1
go build -buildmode=c-shared -v -ldflags="-s -w" -o GoSrp.dll main.go
echo F| xcopy GoSrp.dll %~dp0%1\GoSrp.dll