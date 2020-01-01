#!/bin/bash
dotnet restore
dotnet build
minicover instrument --workdir . --assemblies "Test/**/bin/**/*.dll" --sources "**/*.cs" --exclude-sources "Test/**/*.cs" --exclude-sources "SimpleAuth.Server/Migrations/*"
minicover reset
dotnet test --no-build
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
