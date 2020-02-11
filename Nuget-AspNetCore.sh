#!/bin/bash

PATCH=$1
if [ -z "$PATCH" ]
then
    echo 'Patch part must be specified correctly'
    exit 1
fi

BETA=$2
if [ ! -z "$BETA" ]
then
    PATCH="$PATCH-beta$BETA"
fi

if [ -z "$NUGET_KEY" ]
then
    echo 'NUGET_KEY must be set as environment variable'
    exit 1
fi

NUGET_ORG="https://api.nuget.org/v3/index.json"

dotnet build . -c Release
if [ $? -ne 0 ]
then
    echo 'Build failure'
    exit 1
fi

release_aspnetcore() {
	PROJECT=$1
	MAJOR_MINOR=$2
	VERSION="$MAJOR_MINOR.$PATCH"

	echo 'Project: '$PROJECT
	echo 'Version: '$VERSION

	dotnet pack AspNetCore/SimpleAuth.Client.$PROJECT/SimpleAuth.Client.$PROJECT.csproj -c Release -p:PackageVersion=$VERSION -o .
	dotnet pack SimpleAuth.Client/SimpleAuth.Client.csproj -c Release -p:PackageVersion=$VERSION -o .
	dotnet pack SimpleAuth.Shared/SimpleAuth.Shared.csproj -c Release -p:PackageVersion=$VERSION -o .
	dotnet pack SimpleAuth.Core/SimpleAuth.Core.csproj -c Release -p:PackageVersion=$VERSION -o .

	dotnet nuget push SimpleAuth.Client.AspNetCore.$VERSION.nupkg -k $NUGET_KEY -s $NUGET_ORG
	if [ $? -ne 0 ]
	then
	    echo 'Failure pushing package SimpleAuth.Client.AspNetCore to nuget server'
	    exit 2
	fi

	dotnet nuget push SimpleAuth.Client.$VERSION.nupkg -k $NUGET_KEY -s $NUGET_ORG
	if [ $? -ne 0 ]
	then
	    echo 'Failure pushing package SimpleAuth.Client to nuget server'
	    exit 2
	fi

	dotnet nuget push SimpleAuth.Shared.$VERSION.nupkg -k $NUGET_KEY -s $NUGET_ORG
	if [ $? -ne 0 ]
	then
	    echo 'Failure pushing package SimpleAuth.Shared to nuget server'
	    exit 2
	fi

	dotnet nuget push SimpleAuth.Core.$VERSION.nupkg -k $NUGET_KEY -s $NUGET_ORG
	if [ $? -ne 0 ]
	then
	    echo 'Failure pushing package SimpleAuth.Core to nuget server'
	    exit 2
	fi

	echo $(date)' > '$VERSION >> nuget.version.log
}

release_aspnetcore AspNetCore21 2.1
release_aspnetcore AspNetCore22 2.2

rm -f *.nupkg

echo 'Success'
