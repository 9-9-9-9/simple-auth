using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace Test.SimpleAuth.Services.Test.Services
{
    public class TestIRoleService : BaseTestService<IRoleRepository, Role, string>
    {
        [Test]
        public async Task AddRoleAsync()
        {
            var svc = Prepare(out var mockRoleRepository).GetRequiredService<IRoleService>();

            mockRoleRepository = BasicSetup<IRoleRepository, Role, string>(mockRoleRepository);

            mockRoleRepository.Setup(x => x.Find(It.IsAny<string>())).Returns((Role) null);

            await AddRoleAsync_Mini();

            // ReSharper disable PossibleMultipleEnumeration
            mockRoleRepository.Verify(m => m.CreateManyAsync(It.Is<IEnumerable<Role>>(args =>
                args.Count() == 1 && args.First().Id == "c.a.e.t.m" && args.First().Locked == false)));
            // ReSharper restore PossibleMultipleEnumeration


            mockRoleRepository.Setup(x => x.Find(It.IsAny<string>())).Returns(new Role
            {
                Id = "c.a.e.t.m"
            });

            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await AddRoleAsync_Mini());

            // ReSharper disable once InconsistentNaming
            Task AddRoleAsync_Mini()
            {
                return svc.AddRoleAsync(new CreateRoleModel
                {
                    Corp = "c",
                    App = "a",
                    Env = "e",
                    Tenant = "t",
                    Module = "m",
                });
            }
        }

        [Test]
        public async Task UpdateLockAsync()
        {
            var svc = Prepare(out var mockRoleRepository).GetRequiredService<IRoleService>();

            mockRoleRepository = BasicSetup<IRoleRepository, Role, string>(mockRoleRepository);
            
            var role = new global::SimpleAuth.Shared.Domains.Role
            {
                RoleId = "c.a.e.t.m",
                Locked = true
            };
            
            mockRoleRepository.Setup(x => x.Find(It.IsAny<string>())).Returns((Role) null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UpdateLockStatus(role));
            
            mockRoleRepository.Setup(x => x.Find(It.IsAny<string>())).Returns(new Role());
            await svc.UpdateLockStatus(role);
            
            mockRoleRepository.Verify(m => m.UpdateManyAsync(It.Is<IEnumerable<Role>>(args => args.First().Locked == role.Locked)));
        }

        [Test]
        public void SearchRoles()
        {
            var svc = Prepare(out var mockRoleRepository).GetRequiredService<IRoleService>();

            mockRoleRepository.Setup(x =>
                    x.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FindOptions>()))
                .Returns(new[]
                {
                    new Role
                    {
                        Id = "c.a.e.t.m",
                        Locked = true // lock status does not effect searching
                    }
                });

            var result = svc.SearchRoles("term", "c", "a").ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("c.a.e.t.m", result[0].RoleId);
            Assert.AreEqual(Permission.None, result[0].Permission);

            mockRoleRepository.Setup(x =>
                    x.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FindOptions>()))
                .Returns(new Role[0]);

            result = svc.SearchRoles("term", "c", "a").ToArray();
            Assert.AreEqual(0, result.Length);
        }
    }
}