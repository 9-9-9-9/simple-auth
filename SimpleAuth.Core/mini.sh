#!/bin/bash
cd ~/gq/SimpleAuth/SimpleAuth.Core
minicover instrument --workdir ../ --assemblies "Test/**/bin/**/*.dll" --sources "**/*.cs"
minicover reset
cd ..
dotnet test --no-build ./Test/Test.SimpleAuth.Core/Test.SimpleAuth.Core.csproj
cd ./SimpleAuth.Core/
minicover uninstrument --workdir ../
minicover report --workdir ../ --threshold 5
