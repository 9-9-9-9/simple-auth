using ConsoleApps.Shared.Utils;

namespace PermissionSynchronizer
{
    public class AppTokenConverter : FileUtil.IModelConverter<AppTokenModel>
    {
        public AppTokenModel Convert(string[] parts)
        {
            return new AppTokenModel
            {
                Corp = parts[0],
                App = parts[1],
                AppToken = parts[2]
            };
        }
    }
}