#!/bin/bash

./base-mini.sh Test.Integration.Repositories SimpleAuth.Services --exclude-sources SimpleAuth.Services/ProjectRegistrableModules.cs --exclude-sources SimpleAuth.Services/Services/**/*.cs --exclude-sources SimpleAuth.Services/Entities/**/*.cs --exclude-sources SimpleAuth.Services/Repositories/DbContext.cs
