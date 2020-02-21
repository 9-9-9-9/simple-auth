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

echo "Perform testing SimpleAuth.* projects"

dotnet restore
dotnet build
minicover instrument --workdir . --assemblies "Tests/**/bin/**/*.dll" --sources "SimpleAuth.*/**/*.cs" --exclude-sources "SimpleAuth.Shared/Exceptions/*.cs" --exclude-sources "Tests/**/*.cs"
minicover reset
dotnet test --no-build
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
