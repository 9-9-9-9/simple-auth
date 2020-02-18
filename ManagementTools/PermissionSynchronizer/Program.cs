using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApps.Shared.Utils;
using SimpleAuth.Core.Extensions;

namespace PermissionSynchronizer
{
    internal static class Program
    {
        internal static async Task Main(string[] args)
        {
            var appTokens = FileUtil.ReadTsvToModel(Constants.FileAppToken, new AppTokenConverter()).ToList();

            var corp = Console.ReadLine()?.Trim();
            var app = Console.ReadLine()?.Trim();

            if (corp.IsBlank() || app.IsBlank())
                throw new ArgumentNullException($"{nameof(corp)}.{nameof(app)}");

            var appToken = appTokens.FirstOrDefault(x => x.Corp == corp && x.App == app)?.AppToken;
            if (appToken.IsBlank())
                throw new NotSupportedException($"{nameof(corp)}.{nameof(app)} ({nameof(AppTokenModel.AppToken)})");

            var appDir = Path.Combine(corp, app);

            var users = GetFiles(corp, app, "Users");
            var groups = GetFiles(corp, app, "Groups");

            var userDict = users.ToDictionary(Path.GetFileNameWithoutExtension, x => x);
            var groupDict = groups.ToDictionary(Path.GetFileNameWithoutExtension, x => x);
        }

        private static string[] GetFiles(string corp, string app, string subDir)
        {
            return Directory.GetFiles(Path.Combine(corp, app, subDir));
        }

        private static UserModel LoadUserModel(string file)
        {
            return new UserModel
            {
                PermissionGroups = File.ReadAllLines(file).Select(x => x.Trim()).Where(x => !x.IsBlank()).ToArray()
            };
        }

/*
        private static PermissionGroupModel LoadPermissionGroupModel(string file)
        {
            return new PermissionGroupModel
            {
                PermissionRecordModels = File.ReadAllLines(file)
                    .Select(x => x.Trim())
                    .Where(x => !x.IsBlank())
                    .Select(x => new PermissionRecordModel
                    {
                        Role = 
                    })
                    .ToArray()
            };
        }
        */
    }
}