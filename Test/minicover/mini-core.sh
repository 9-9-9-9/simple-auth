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
minicover instrument --workdir . --assemblies "Test/Test.SimpleAuth.Core/bin/**/SimpleAuth.Core.dll" --sources "SimpleAuth.Core/**/*.cs"
minicover reset
dotnet test --no-build Test/Test.SimpleAuth.Core/Test.SimpleAuth.Core.csproj
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
