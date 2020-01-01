#!/bin/bash

rm -f sa.server.db
dotnet ef migrations remove --context SimpleAuth.Server.DbContext --verbose
if [ $? -ne 0 ];
then
    echo 'Remove failure'
fi
dotnet ef migrations add InitialCreate --context SimpleAuth.Server.DbContext --verbose
if [ $? -ne 0 ];
then
    echo 'InitialCreate failure'
    exit 1
fi
dotnet ef database update --context SimpleAuth.Server.DbContext --verbose
