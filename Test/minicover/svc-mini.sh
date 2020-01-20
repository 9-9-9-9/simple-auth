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
minicover instrument --workdir . --assemblies "Test/Test.SimpleAuth.Services/bin/**/SimpleAuth.Services.dll" --sources "SimpleAuth.Services/**/*.cs"
minicover reset
dotnet test --no-build Test/Test.SimpleAuth.Services/Test.SimpleAuth.Services.csproj
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
