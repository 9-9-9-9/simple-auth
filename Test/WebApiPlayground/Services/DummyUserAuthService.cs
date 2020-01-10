using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace WebApiPlayground.Services
{
    public class DummyUserAuthService : IUserAuthService
    {
        public Task<ResponseUserModel> GetUserAsync(string userId)
        {
            return Task.FromResult(GetDummyUser(userId));
        }

        public Task<ResponseUserModel> GetUserAsync(string userId, string password)
        {
            return Task.FromResult(GetDummyUser(userId));
        }

        public Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest)
        {
            return Task.FromResult(GetDummyUser(loginByGoogleRequest.Email));
        }

        public Task<bool> DoesUserHavePermissionAsync(string userId, string roleId, Permission permission)
        {
            return Task.FromResult(true);
        }

        private ResponseUserModel GetDummyUser(string userId)
        {
            return new ResponseUserModel
            {
                ActiveRoles = GetDummyRoleModels().ToArray()
            };
        }

        private IEnumerable<RoleModel> GetDummyRoleModels()
        {
            return YieldResults();

            //
            IEnumerable<RoleModel> YieldResults()
            {
                yield return Rm("c.a.e.t.weatherforecast", Permission.View);
                yield return Rm("c.a.e.t.weatherforecast.*", Permission.View);
                yield return Rm("c.a.e.t.best", Permission.View);
                yield return Rm("c.a.e.t.best.a", Permission.View);
            }

            //
            RoleModel Rm(string roleId, Permission permission) => new RoleModel
            {
                Role = roleId,
                Permission = permission.Serialize()
            };
        }
    }
}