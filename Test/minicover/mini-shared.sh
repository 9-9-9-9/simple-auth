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
minicover instrument --workdir . --assemblies "Test/Test.SimpleAuth.Shared/bin/**/SimpleAuth.Shared.dll" --sources "SimpleAuth.Shared/**/*.cs"
minicover reset
dotnet test --no-build Test/Test.SimpleAuth.Shared/Test.SimpleAuth.Shared.csproj
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
