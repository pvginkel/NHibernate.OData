@echo off

pushd "%~dp0"

powershell .\Build.ps1

pause

popd
