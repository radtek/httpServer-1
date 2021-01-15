@echo off

if not "%GO_INIT%"=="1" (
set GO_INIT=1
set GOPATH=%GOPATH%;%cd%
)

set GOARCH=386
set CGO_ENABLED=1

go build -ldflags "-s -w" -o bin/release/httpServerGo.dll -buildmode=c-shared

set ROOT_DIR=%~dp0..\
xcopy /y /d "%ROOT_DIR%httpServerGo\bin\release\httpServerGo.dll" "%ROOT_DIR%httpServer\bin\Debug\"
xcopy /y /d "%ROOT_DIR%httpServerGo\bin\release\httpServerGo.dll" "%ROOT_DIR%httpServer\bin\Release\"
