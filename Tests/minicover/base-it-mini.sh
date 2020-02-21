#!/bin/bash

export TEST_PRJ_NAME=$1
export COVER_PRJ_SRC=$2

if [ -z "$TEST_PRJ_NAME" ]
then
	echo "Missing first parameter of TEST_PRJ_NAME"
	exit 1
fi

if [ -z "$COVER_PRJ_SRC" ]
then
	echo "Missing second parameter of COVER_PRJ_SRC"
	exit 1
fi

cd ../..

sln="solution.sln"
if [ -f "$sln" ]
then
	echo "Performing minicover execution"
else
	echo "$sln not found."
	exit 1
fi

echo "TEST_PRJ_NAME=$TEST_PRJ_NAME"
echo "COVER_PRJ_SRC=$COVER_PRJ_SRC"

dotnet restore
dotnet build
minicover instrument --workdir . --assemblies "Tests/IntegrationTests/$TEST_PRJ_NAME/bin/**/$COVER_PRJ_SRC.dll" --sources "$COVER_PRJ_SRC/**/*.cs" $3 $4 $5 $6 $7 $8 $9 ${10} ${11} ${12} ${13} ${14}
minicover reset
dotnet test --no-build "Tests/IntegrationTests/$TEST_PRJ_NAME/$TEST_PRJ_NAME.csproj"
minicover uninstrument --workdir ./
minicover htmlreport --workdir ./ --threshold 90
minicover report --workdir ./ --threshold 90
