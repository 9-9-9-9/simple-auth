using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace WebApiPlayground.Services
{
    public class DummyUserAuthService : IUserAuthService
    {
        public Task<PermissionModel[]> GetActiveRolesAsync(string userId)
        {
            return Task.FromResult(GetDummyUser(userId).ActiveRoles);
        }

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

        public Task<bool> DoesUserHavePermissionAsync(string userId, string roleId, Verb verb)
        {
            return Task.FromResult(true);
        }

        public Task<PermissionModel[]> GetMissingRolesAsync(string userId, PermissionModels permissionModels)
        {
            return Task.FromResult(new PermissionModel[0]);
        }

        // ReSharper disable once UnusedParameter.Local
        private ResponseUserModel GetDummyUser(string userId)
        {
            return new ResponseUserModel
            {
                ActiveRoles = GetDummyRoleModels().ToArray()
            };
        }

        private IEnumerable<PermissionModel> GetDummyRoleModels()
        {
            return YieldResults();

            //
            IEnumerable<PermissionModel> YieldResults()
            {
                yield return Rm("test.a.e.t.weatherforecast", Verb.View);
                yield return Rm("test.a.e.t.weatherforecast.*", Verb.View);
                yield return Rm("test.a.e.t.best", Verb.View);
                yield return Rm("test.a.e.t.best.a", Verb.View);
            }

            //
            PermissionModel Rm(string roleId, Verb permission) => new PermissionModel
            {
                Role = roleId,
                Verb = permission.Serialize()
            };
        }
    }
}