dotnet test Test.SimpleAuth.Core/Test.SimpleAuth.Core.csproj --results-directory:CodeCoverageResult.Core --collect:"Code Coverage" --settings:Test.SimpleAuth.Core/CodeCoverage.runsettings
dotnet test Test.SimpleAuth.Shared/Test.SimpleAuth.Shared.csproj --results-directory:CodeCoverageResult.Shared --collect:"Code Coverage" --settings:Test.SimpleAuth.Shared/CodeCoverage.runsettings
rem dotnet test Test.SimpleAuth.Server/Test.SimpleAuth.Server.csproj --results-directory:CodeCoverageResult.Server --collect:"Code Coverage" --settings:Test.SimpleAuth.Server/CodeCoverage.runsettings
pause