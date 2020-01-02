#!/bin/bash

rm -f sa.server.db
dotnet ef migrations remove --context SimpleAuth.Server.DbContext --verbose
if [ $? -ne 0 ];
then
    echo 'Remove failure'
fi

./make-db.sh
