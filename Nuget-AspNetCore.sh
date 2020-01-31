#!/bin/bash

VERSION=$1
if [ -z "$VERSION" ]
then
    echo 'Version must be specified correctly'
    exit 1
fi

if [ -z "$NUGET_SERVER" ]
then
    echo 'NUGET_SERVER must be set as environment variable'
    exit 1
fi

if [ -z "$NUGET_URL" ]
then
    echo 'NUGET_URL must be set as environment variable'
    exit 1
fi


dotnet build . -c Release

dotnet pack SimpleAuth.Client.AspNetCore/SimpleAuth.Client.AspNetCore.csproj -c Release -p:PackageVersion=$VERSION -o .
dotnet pack SimpleAuth.Client/SimpleAuth.Client.csproj -c Release -p:PackageVersion=$VERSION -o .
dotnet pack SimpleAuth.Shared/SimpleAuth.Shared.csproj -c Release -p:PackageVersion=$VERSION -o .
dotnet pack SimpleAuth.Core/SimpleAuth.Core.csproj -c Release -p:PackageVersion=$VERSION -o .

dotnet nuget push SimpleAuth.Client.AspNetCore.$VERSION.nupkg -k $NUGET_SERVER -s $NUGET_URL
if [ $? -ne 0 ]
then
    echo 'Failure pushing package SimpleAuth.Client.AspNetCore to nuget server'
    exit 2
fi

dotnet nuget push SimpleAuth.Client.$VERSION.nupkg -k $NUGET_SERVER -s $NUGET_URL
if [ $? -ne 0 ]
then
    echo 'Failure pushing package SimpleAuth.Client to nuget server'
    exit 2
fi

dotnet nuget push SimpleAuth.Shared.$VERSION.nupkg -k $NUGET_SERVER -s $NUGET_URL
if [ $? -ne 0 ]
then
    echo 'Failure pushing package SimpleAuth.Shared to nuget server'
    exit 2
fi

dotnet nuget push SimpleAuth.Core.$VERSION.nupkg -k $NUGET_SERVER -s $NUGET_URL
if [ $? -ne 0 ]
then
    echo 'Failure pushing package SimpleAuth.Core to nuget server'
    exit 2
fi

rm -f *.nupkg

echo $(date)' > '$VERSION >> nuget.version.log
