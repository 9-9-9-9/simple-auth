using System;
using System.Linq;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Shared.Utils
{
    public static class RoleUtils
    {
        public enum RolePart
        {
            Corp = 1,
            App = 2,
            Env = 3,
            Tenant = 4,
            Module = 5,
            SubModules = 6
        }

        public static string JoinPartsFromModule(string module, string[] subModules)
        {
            return subModules.IsEmpty()
                ? module
                : $"{module}{Constants.SplitterRoleParts}{string.Join(Constants.SplitterSubModules, subModules)}";
        }

        public static string CutPartsBefore(RolePart takeFromRolePart, string text)
        {
            if (takeFromRolePart > RolePart.Module)
                throw new ArgumentException(nameof(takeFromRolePart));

            var spl = text.Split(new[] {Constants.chSplitterRoleParts}, StringSplitOptions.None);
            if (spl.Length < 5) // invalid number of parts
                throw new ArgumentException($"{nameof(text)} ({text})");

            return string.Join(Constants.SplitterRoleParts, spl.Skip((int) takeFromRolePart - 1));
        }
    }
}