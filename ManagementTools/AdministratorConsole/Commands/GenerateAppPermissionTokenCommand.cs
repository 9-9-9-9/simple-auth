﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;

namespace AdministratorConsole.Commands
{
    public class GenerateAppPermissionTokenCommand : AbstractCommand
    {
        private readonly IAdministrationService _administrationService;

        public GenerateAppPermissionTokenCommand(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            var @public = args[2];
            if (IsCorrectBool(@public))
                yield break;
            else
                yield return "Invalid @public, accepted y/n";
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1, 2};

        protected override Task DoMainJob(string[] args)
        {
            return Print(_administrationService.GenerateAppPermissionTokenAsync(args[0], args[1], ParseBoolean(args[2]) ?? true));
        }

        public override string[] GetParametersName()
        {
            return new[] {"Corp", "App", "Public (y/n)"};
        }
    }
}