#!/bin/bash

cd ../..

sln="solution.sln"
if [ -f "$sln" ]
then
	echo "Performing minicover execution"
else
	echo "$sln not found."
	exit 1
fi

echo "Perform testing SimpleAuth.Core|Services|Shared the 3 project made the core value of this solution"

dotnet restore
dotnet build
minicover instrument --workdir . --assemblies "Test2/**/bin/**/*.dll" --sources "SimpleAuth.Core/**/*.cs" --sources "SimpleAuth.Services/**/*.cs" --sources "SimpleAuth.Shared/**/*.cs" --exclude-sources "SimpleAuth.Shared/Exceptions/*.cs"
minicover reset
dotnet test --no-build
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
