using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AddPermissionGroupCommand : AbstractCommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IPermissionGroupManagementService _permissionGroupManagementService;

        public AddPermissionGroupCommand(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IPermissionGroupManagementService permissionGroupManagementService)
        {
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
            _permissionGroupManagementService = permissionGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            if (!TrySplittingRoleGroups(args[1], out var roleGroups, out var errMessage))
                throw new InvalidOperationException(errMessage);
            return _permissionGroupManagementService.AddRoleGroupAsync(new CreatePermissionGroupModel
            {
                Name = args[0],
                Corp = _simpleAuthConfigurationProvider.Corp,
                App = _simpleAuthConfigurationProvider.App,
                CopyFromPermissionGroups = roleGroups
            }).ContinueWith(_ => Print("Added"));
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