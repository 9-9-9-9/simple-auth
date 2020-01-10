﻿using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdministratorConsole.Commands
{
    public class DecryptCommand : RequestCommandWithResponse<string>
    {
        private readonly IAdministrationService _administrationService;

        public DecryptCommand(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        protected override Task<string> DoRequest(params string[] args)
        {
            return _administrationService.DecryptUsingMasterEncryptionKey(args[0]);
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int NumberOfParameters => 1;
        protected override int[] IdxParametersCanNotBeBlank => new[] { 0 };
    }
}
