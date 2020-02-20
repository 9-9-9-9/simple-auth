using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

#pragma warning disable 162

namespace DummyTmlGenerator
{
    internal static class Program
    {
        private const int NumberOfUsers = 2;
        private const int NumberOfPermissionGroups = 5;
        private const int NumberOfRoleIds = 200;
        private static readonly string[] GenerateForEnvironments = {"prod", "d"};

        private const bool
            AssignUserToPermissionGroupRandomly =
                true; // false: assign all users to all groups, true: use random method

        //
        private const string Corp = "test";
        private const string App = "wap";

        private static readonly Random Rad = new Random();

        internal static async Task Main()
        {
            await Task.CompletedTask;

            if (NumberOfUsers < 1)
                throw new ArgumentException(nameof(NumberOfUsers));

            if (NumberOfPermissionGroups < 1)
                throw new ArgumentException(nameof(NumberOfPermissionGroups));

            if (NumberOfRoleIds < 1)
                throw new ArgumentException(nameof(NumberOfRoleIds));

            if (GenerateForEnvironments.Length == 0 || GenerateForEnvironments.Any(x => x.IsBlank()))
                throw new ArgumentException(nameof(GenerateForEnvironments));

            var users = RandomManyText(NumberOfUsers).ToList();
            var groups = RandomManyText(NumberOfPermissionGroups).ToList();
            var roleIds = RandomRoleIds(NumberOfRoleIds).ToList();

            var sb = new StringBuilder();

            sb.AppendLine("Target");
            sb.AppendLine("\tcorp");
            sb.AppendLine($"\t\t{Corp}");
            sb.AppendLine("\tapp");
            sb.AppendLine($"\t\t{App}");
            sb.AppendLine();

            sb.AppendLine("Users");
            // First user will be assigned into all groups
            sb.AppendLine($"\t{users.First()}");
            groups.ForEach(g => sb.AppendLine($"\t\t{g}"));
            // Second user onward will be randomly being assigned to groups (50% chance)
            foreach (var user in users.Skip(1))
            {
                sb.AppendLine($"\t{user}");

                foreach (var @group in groups)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (AssignUserToPermissionGroupRandomly && Rad.Next() % 2 == 0)
                        continue;
                    sb.AppendLine($"\t\t{@group}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("Groups");
            // First group will have all role ids
            sb.AppendLine($"\t{groups.First()}");
            roleIds.ForEach(r => sb.AppendLine($"\t\t{r}\t{string.Join('\t', RandomVerb())}"));
            // Second group onward will have small change to have an role id (20%)
            foreach (var @group in groups.Skip(1))
            {
                sb.AppendLine($"\t{@group}");

                foreach (var roleId in roleIds)
                {
                    if (Rad.Next() % 5 != 0)
                        continue;
                    sb.AppendLine($"\t\t{roleId}\t{string.Join('\t', RandomVerb())}");
                }
            }
            
            File.WriteAllText($"{App}-dummy.tml", sb.ToString());

            IEnumerable<Verb> RandomVerb() => RoleUtils.ParseToMinimum((Verb) Rad.Next(1, 15));
            
            IEnumerable<string> RandomManyText(int count, int len = 5)
            {
                while (count-- > 0)
                    yield return RandomText(len);
            }

            IEnumerable<string> RandomRoleIds(int count)
            {
                var noOfChar = NumberOfRoleIds.ToString().Length;
                while (count-- > 0)
                {
                    var noOfSubModules = Rad.Next(0, 5);
                    var model = new ClientPermissionModel
                    {
                        Corp = Corp,
                        App = App,
                        Env = null,
                        Tenant = "0",
                        Module = RandomText(noOfChar),
                        SubModules = YieldSubModules(noOfSubModules).ToArray(),
                    };

                    var atLeastOneReturned = false;
                    foreach (var env in GenerateForEnvironments)
                    {
                        if (atLeastOneReturned && Rad.Next() % 3 == 0)
                            continue;
                        atLeastOneReturned = true;
                        model.Env = env;
                        yield return model.ComputeId();
                    }
                }
            }

            IEnumerable<string> YieldSubModules(int count)
            {
                while (count-- > 0)
                    yield return RandomText(2);
            }

            string RandomText(int len = 5) => Guid.NewGuid().ToString().Replace("-", "").Substring(0, len);
        }
    }
}