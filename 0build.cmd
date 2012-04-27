@echo off
set MSBuildToolsPath=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727
%MSBuildToolsPath%\MSbuild qdvm.csproj
%MSBuildToolsPath%\MSbuild qdasm.csproj
