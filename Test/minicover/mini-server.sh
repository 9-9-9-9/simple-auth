#!/bin/bash

cd ..
cd ..

sln="solution.sln"
if [ -f "$sln" ]
then
	echo "Performing minicover execution"
else
	echo "$sln not found."
	exit 1
fi

dotnet restore
dotnet build
minicover instrument --workdir . --assemblies "Test/Test.SimpleAuth.Server/bin/**/SimpleAuth.Server.dll" --sources "SimpleAuth.Server/**/*.cs"
minicover reset
dotnet test --no-build Test/Test.SimpleAuth.Server/Test.SimpleAuth.Server.csproj
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 5
minicover report --workdir ./ --threshold 5
