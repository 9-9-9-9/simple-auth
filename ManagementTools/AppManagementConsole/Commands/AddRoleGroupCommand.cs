using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    public class AddRoleGroupCommand : AbstractCommand, ICommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IRoleGroupManagementService _roleGroupManagementService;

        public AddRoleGroupCommand(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IRoleGroupManagementService roleGroupManagementService)
        {
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
            _roleGroupManagementService = roleGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            if (!TrySplittingRoleGroups(args[1], out var roleGroups, out var errMessage))
                throw new InvalidOperationException(errMessage);
            return _roleGroupManagementService.AddRoleGroupAsync(new CreateRoleGroupModel
            {
                Name = args[0],
                Corp = _simpleAuthConfigurationProvider.Corp,
                App = _simpleAuthConfigurationProvider.App,
                CopyFromRoleGroups = roleGroups
            });
        }

        public override string[] GetParametersName()
        {
            return new[] {"Name", "Copy from groups (split by ',' comma)"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            if (!TrySplittingRoleGroups(args[1], out _, out var errMessage))
                yield return errMessage;
        }

        private bool TrySplittingRoleGroups(string data, out string[] roleGroups, out string errMessage)
        {
            try
            {
                roleGroups = data.Split(',', StringSplitOptions.RemoveEmptyEntries);
                errMessage = null;
                return true;
            }
            catch (Exception e)
            {
                roleGroups = default;
                errMessage = e.Message;
                return false;
            }
        }

        protected override int[] IdxParametersCanNotBeBlank => new int[0];
    }
}