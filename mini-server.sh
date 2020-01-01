#!/bin/bash
dotnet restore
dotnet build
minicover instrument --workdir . --assemblies "Test/Test.SimpleAuth.Server/bin/**/SimpleAuth.Server.dll" --sources "SimpleAuth.Server/**/*.cs"
minicover reset
dotnet test --no-build Test/Test.SimpleAuth.Server/Test.SimpleAuth.Server.csproj
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 5
minicover report --workdir ./ --threshold 5
