#!/bin/bash

./base-mini.sh TestSimpleAuth.Services SimpleAuth.Services --exclude-sources SimpleAuth.Services/ProjectRegistrableModules.cs --exclude-sources SimpleAuth.Services/Services/ServiceModules.cs --exclude-sources SimpleAuth.Services/Repositories/**/*.cs
