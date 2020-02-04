using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using Test.Shared;
using LocalUserInfo = SimpleAuth.Shared.Domains.LocalUserInfo;
using User = SimpleAuth.Shared.Domains.User;

namespace Test.SimpleAuth.Shared.Test.Services
{
    public class TestIRoleGroupService : BaseTestClass
    {
        [Test]
        public async Task IncludedPropertyIsCorrectlyPersistedAndRetrieve()
        {
            await Truncate<RoleRecord, RoleGroup, Role>(nameof(IncludedPropertyIsCorrectlyPersistedAndRetrieve));

            const int noOfRoles = 2;
            var testedGr = 0;

            var rSvc = Svc<IRoleService>();
            var gSvc = Svc<IRoleGroupService>();

            await IncludedPropertyIsCorrectlyPersistedAndRetrieveGr("g1");
            await IncludedPropertyIsCorrectlyPersistedAndRetrieveGr("g2");

            async Task IncludedPropertyIsCorrectlyPersistedAndRetrieveGr(string grName)
            {
                await AddRoleGroupAsync(gSvc, grName, "c", "a");

                var roleGroup = await gSvc.GetRoleGroupByNameAsync(grName, "c", "a");
                Assert.NotNull(roleGroup);

                foreach (var role in YieldRoles(testedGr * noOfRoles + 1, (testedGr + 1) * noOfRoles))
                {
                    await AddRoleAsync(rSvc, role.Corp, role.App, role.Env, role.Tenant, role.Module);

                    await gSvc.AddRolesToGroupAsync(roleGroup, new[]
                    {
                        new RoleModel
                        {
                            Role = role.Id,
                            Permission = Permission.View.Serialize()
                        }
                    });
                }

                Assert.AreEqual(noOfRoles, roleGroup.Roles.Length);

                roleGroup = await gSvc.GetRoleGroupByNameAsync(grName, "c", "a");
                Assert.AreEqual(noOfRoles, roleGroup.Roles.Length);
                Assert.IsTrue(roleGroup.Roles.Any(x => x.RoleId.StartsWith("c.a")));

                testedGr++;
            }
        }

        [TestCase(2, "c", "a1", "g11", "g12")]
        [TestCase(0, "c", "a1", "g2", "g1")]
        [TestCase(2, "c", "a2", "g21", "g22")]
        [TestCase(2, "c", "a2", "g21", "g02", "g22")]
        [TestCase(1, "c", "a1", "g11")]
        [TestCase(1, "c", "a2", "g21")]
        [TestCase(0, "c", "a1", "g21")]
        [TestCase(0, "c", "a2", "g11")]
        [TestCase(0, "c", "a2", "g11222")]
        public async Task FindByName(int expectedRecordsFound, string corp, string app, params string[] exactNames)
        {
            await Truncate<RoleRecord, Role, RoleGroup>(nameof(FindByName));

            var gSvc = Svc<IRoleGroupService>();
            ExecuteOne(nameof(FindByName), async () =>
            {
                await AddRoleGroupAsync(gSvc, "g11", "c", "a1");
                await AddRoleGroupAsync(gSvc, "g12", "c", "a1");
                await AddRoleGroupAsync(gSvc, "g21", "c", "a2");
                await AddRoleGroupAsync(gSvc, "g22", "c", "a2");
            });

            Assert.IsTrue(exactNames.IsAny());

            var found = gSvc.FindByName(exactNames, corp, app).ToList();
            Assert.AreEqual(expectedRecordsFound, found.Count);

            if (found.Count == 0)
                return;

            foreach (var group in found)
            {
                Assert.AreEqual(corp, group.Corp);
                Assert.AreEqual(app, group.App);
                Assert.IsTrue(exactNames.Contains(group.Name));
            }
        }

        [Test]
        public async Task UpdateLockStatusAsync()
        {
            var gSvc = Svc<IRoleGroupService>();
            var randomGroupName = Guid.NewGuid().ToString();
            await AddRoleGroupAsync(gSvc, randomGroupName, "c", "a");

            var gr = await Get();
            gr.Locked = true;

            await UpdateLock(true);
            await UpdateLock(false);
            await UpdateLock(true);
            await UpdateLock(true);

            async Task UpdateLock(bool @lock)
            {
                var gr2 = await Get();
                gr2.Locked = @lock;
                await gSvc.UpdateLockStatusAsync(gr2);

                gr2 = await Get();
                Assert.AreEqual(@lock, gr2.Locked);
            }

            async Task<global::SimpleAuth.Shared.Domains.RoleGroup> Get()
            {
                return await gSvc.GetRoleGroupByNameAsync(randomGroupName, "c", "a");
            }
        }

        [Test]
        public async Task AddRolesToGroupAsync_CrossApp()
        {
            await Truncate<RoleRecord, RoleGroup, Role>(nameof(AddRolesToGroupAsync_CrossApp));

            var gSvc = Svc<IRoleGroupService>();
            var rSvc = Svc<IRoleService>();

            await AddRoleGroupAsync(gSvc, "g", "c", "a");
            await AddRoleAsync(rSvc, "c", "a", "e", "t", "m");

            await gSvc.AddRolesToGroupAsync(await Get(), new[]
            {
                new RoleModel
                {
                    Role = "c.a.e.t.m",
                    Permission = Permission.Full.Serialize()
                }
            });

            try
            {
                await gSvc.AddRolesToGroupAsync(await Get(), new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m",
                        Permission = Permission.View.Serialize()
                    },
                    new RoleModel
                    {
                        Role = "c2.a.e.t.m",
                        Permission = Permission.None.Serialize()
                    }
                });

                Assert.Fail("Expected error");
            }
            catch (SimpleAuthSecurityException e)
            {
                Assert.IsTrue(e.Message.Contains("c2.a.e.t.m", StringComparison.InvariantCultureIgnoreCase));
            }

            async Task<global::SimpleAuth.Shared.Domains.RoleGroup> Get()
            {
                return await gSvc.GetRoleGroupByNameAsync("g", "c", "a");
            }
        }

        [Test]
        public async Task AddRolesToGroupAsync_Copy()
        {
            var randomCorp = Guid.NewGuid().ToString();

            var gSvc = Svc<IRoleGroupService>();
            var rSvc = Svc<IRoleService>();

            await AddRoleGroupAsync(gSvc, "g1", randomCorp, "a1");
            await AddRoleGroupAsync(gSvc, "g2", randomCorp, "a2");
            await AddRoleAsync(rSvc, randomCorp, "a1", "e", "t", "m1");
            await AddRoleAsync(rSvc, randomCorp, "a1", "e", "t", "m2");
            await AddRoleAsync(rSvc, randomCorp, "a2", "e", "t", "m3");

            await gSvc.AddRolesToGroupAsync(await Get("g1", "a1"), new[]
            {
                new RoleModel
                {
                    Role = $"{randomCorp}.a1.e.t.m1",
                    Permission = Permission.Full.Serialize()
                },
                new RoleModel
                {
                    Role = $"{randomCorp}.a1.e.t.m2",
                    Permission = Permission.Edit.Serialize()
                }
            });

            await gSvc.AddRolesToGroupAsync(await Get("g2", "a2"), new[]
            {
                new RoleModel
                {
                    Role = $"{randomCorp}.a2.e.t.m3",
                    Permission = Permission.Delete.Serialize()
                }
            });

            await AddRoleGroupAsync(gSvc, "g3", randomCorp, "a1", "g1");

            var g3 = await Get("g3", "a1");
            Assert.AreEqual(2, g3.Roles.Length);
            Assert.IsTrue(g3.Roles.Any(x => x.Permission == Permission.Full && x.RoleId == $"{randomCorp}.a1.e.t.m1"));
            Assert.IsTrue(g3.Roles.Any(x => x.Permission == Permission.Edit && x.RoleId == $"{randomCorp}.a1.e.t.m2"));

            try
            {
                await AddRoleGroupAsync(gSvc, "g4", randomCorp, "a1", "g2");
                Assert.Fail("Error expected");
            }
            catch (EntityNotExistsException) // g2 belong to App 'a2' thus can not be found in a1
            {
                // OK
            }

            try
            {
                await AddRoleGroupAsync(gSvc, "g3", randomCorp, "a1", "g1");
            }
            catch (EntityAlreadyExistsException)
            {
                // OK
            }

            async Task<global::SimpleAuth.Shared.Domains.RoleGroup> Get(string name, string app)
            {
                return await gSvc.GetRoleGroupByNameAsync(name, randomCorp, app);
            }
        }

        [Test]
        public async Task DeleteRolesFromGroupAsync()
        {
            var gSvc = Svc<IRoleGroupService>();
            var rSvc = Svc<IRoleService>();

            #region Setup

            await Truncate<RoleRecord, RoleGroup, Role>(nameof(DeleteRolesFromGroupAsync));
            ExecuteOne(nameof(DeleteRolesFromGroupAsync), async () =>
            {
                await AddRoleGroupAsync(gSvc, "g1", "c", "a");
                await AddRoleGroupAsync(gSvc, "g3", "c", "a");

                await AddRoleAsync(rSvc, "c", "a", "e", "t", "m1");
                await AddRoleAsync(rSvc, "c", "a", "e", "t", "m2");
                await AddRoleAsync(rSvc, "c", "a", "e", "t", "m3");

                var g1 = await Get();
                await gSvc.AddRolesToGroupAsync(g1, new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m1",
                        Permission = Permission.Full.Serialize()
                    }
                });
                await gSvc.AddRolesToGroupAsync(g1, new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m2",
                        Permission = (Permission.View | Permission.Edit | Permission.Delete).Serialize()
                    }
                });
            });

            #endregion

            await RevokePermission(Permission.Delete, "m1");
            Assert.AreEqual(Permission.Add | Permission.View | Permission.Edit,
                (await Get()).Roles.First(x => x.RoleId.EndsWith(".m1")).Permission);

            await RevokePermission(Permission.Delete | Permission.Add, "m2");
            Assert.AreEqual(Permission.View | Permission.Edit,
                (await Get()).Roles.First(x => x.RoleId.EndsWith(".m2")).Permission);

            await RevokePermission(Permission.View | Permission.Edit, "m2");
            Assert.IsNull((await Get()).Roles.FirstOrDefault(x => x.RoleId.EndsWith(".m2")));

            await RevokePermission(Permission.View | Permission.Edit,
                "m3"); // revoke an un-referenced role => does nothing

            await gSvc.AddRolesToGroupAsync(await Get(), new[]
            {
                new RoleModel
                {
                    Role = "c.a.e.t.m2",
                    Permission = (Permission.View | Permission.Edit | Permission.Delete).Serialize()
                }
            });

            Assert.AreEqual(Permission.View | Permission.Edit | Permission.Delete,
                (await Get()).Roles.First(x => x.RoleId.EndsWith(".m2")).Permission);

            await gSvc.DeleteRolesFromGroupAsync(await Get(), new[]
            {
                new RoleModel
                {
                    Role = "c.a.e.t.m2",
                    Permission = Permission.Add.Serialize()
                },
                new RoleModel
                {
                    Role = "c.a.e.t.m2",
                    Permission = Permission.View.Serialize()
                },
                new RoleModel
                {
                    Role = "c.a.e.t.m2",
                    Permission = Permission.Edit.Serialize()
                },
            });

            Assert.AreEqual(Permission.Delete,
                (await Get()).Roles.First(x => x.RoleId.EndsWith(".m2")).Permission);

            await gSvc.DeleteRolesFromGroupAsync(await gSvc.GetRoleGroupByNameAsync("g3", "c", "a"),
                new[]
                {
                    new RoleModel
                    {
                        Role = $"c.a.e.t.m1",
                        Permission = Permission.Delete.Serialize()
                    }
                });

            async Task RevokePermission(Permission p, string module)
            {
                await gSvc.DeleteRolesFromGroupAsync(await Get(), new[]
                {
                    new RoleModel
                    {
                        Role = $"c.a.e.t.{module}",
                        Permission = p.Serialize()
                    }
                });
            }

            async Task<global::SimpleAuth.Shared.Domains.RoleGroup> Get()
            {
                return await gSvc.GetRoleGroupByNameAsync("g1", "c", "a");
            }
        }

        [TestCase("g1")]
        [TestCase("g1")]
        [TestCase("g2")]
        public async Task DeleteAllRolesFromGroupAsync(string groupName)
        {
            var gSvc = Svc<IRoleGroupService>();
            var rSvc = Svc<IRoleService>();

            #region Setup

            await Truncate<RoleRecord, RoleGroup, Role>(nameof(DeleteAllRolesFromGroupAsync));
            ExecuteOne(nameof(DeleteAllRolesFromGroupAsync), async () =>
            {
                await AddRoleGroupAsync(gSvc, "g1", "c", "a");
                await AddRoleGroupAsync(gSvc, "g2", "c", "a");

                await AddRoleAsync(rSvc, "c", "a", "e", "t", "m1");
                await AddRoleAsync(rSvc, "c", "a", "e", "t", "m2");
                await AddRoleAsync(rSvc, "c", "a", "e", "t", "m3");

                var g1 = await gSvc.GetRoleGroupByNameAsync("g1", "c", "a");
                var g2 = await gSvc.GetRoleGroupByNameAsync("g2", "c", "a");
                await gSvc.AddRolesToGroupAsync(g1, new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m1",
                        Permission = Permission.View.Serialize()
                    }
                });
                await gSvc.AddRolesToGroupAsync(g1, new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m2",
                        Permission = Permission.View.Serialize()
                    }
                });
                await gSvc.AddRolesToGroupAsync(g1, new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m3",
                        Permission = Permission.View.Serialize()
                    }
                });
                await gSvc.AddRolesToGroupAsync(g2, new[]
                {
                    new RoleModel
                    {
                        Role = "c.a.e.t.m3",
                        Permission = Permission.View.Serialize()
                    }
                });
            });

            #endregion

            await gSvc.DeleteAllRolesFromGroupAsync(await gSvc.GetRoleGroupByNameAsync(groupName, "c", "a"));
            Assert.IsTrue((await gSvc.GetRoleGroupByNameAsync(groupName, "c", "a")).Roles.IsEmpty());
        }

        [Test]
        public async Task GetEntity_WhenRoleGroupDoesNotExists()
        {
            try
            {
                await Svc<IRoleGroupService>().DeleteAllRolesFromGroupAsync(
                    new global::SimpleAuth.Shared.Domains.RoleGroup
                    {
                        Name = "ThisNotExists",
                        Corp = "ThisNotExists",
                        App = "ThisNotExists",
                        Roles = new[]
                        {
                            new global::SimpleAuth.Shared.Domains.Role()
                            {
                                RoleId = "ThisNotExists",
                                Permission = Permission.Add
                            }
                        }
                    });
                Assert.Fail("Must error");
            }
            catch (Exception e)
            {
                if (!(e is EntityNotExistsException))
                {
                    Assert.Fail($"Actual: {e.GetType()}");
                }
            }
        }

        [Test]
        public async Task SearchRoleGroups()
        {
            var randomCorp = Guid.NewGuid().ToString()
                .Replace("a", "")
                .Replace("e", "")
                .Replace("1", "")
                .Replace("2", "")
                .Replace("3", "")
                .Replace("4", "")
                .Replace("-", "")
                .Substring(5);
            var svc = Svc<IRoleGroupService>();

            await AddRoleGroupAsync(svc, "n1", randomCorp, "a1");
            await AddRoleGroupAsync(svc, "n1", randomCorp, "a2");
            await AddRoleGroupAsync(svc, "n2", randomCorp, "a1");
            await AddRoleGroupAsync(svc, "n2", randomCorp, "a2");
            await AddRoleGroupAsync(svc, "n3", randomCorp, "a1");

            Assert.AreEqual(3, svc.SearchRoleGroups("n", randomCorp, "a1").ToArray().Length);
            Assert.AreEqual(2, svc.SearchRoleGroups("n", randomCorp, "a2").ToArray().Length);
            Assert.AreEqual(1, svc.SearchRoleGroups("n1", randomCorp, "a1").ToArray().Length);
            Assert.AreEqual(0, svc.SearchRoleGroups("n3", randomCorp, "a2").ToArray().Length);

            var gr = await svc.GetRoleGroupByNameAsync("n1", randomCorp, "a1");
            gr.Locked = true;
            await svc.UpdateLockStatusAsync(gr);
            Assert.AreEqual(2, svc.SearchRoleGroups("n", randomCorp, "a1").ToArray().Length);

            gr.Locked = false;
            await svc.UpdateLockStatusAsync(gr);
            Assert.AreEqual(3, svc.SearchRoleGroups("n", randomCorp, "a1").ToArray().Length);
        }

        [Test]
        public async Task DeleteRoleGroupAsync()
        {
            var gSvc = Svc<IRoleGroupService>();
            var rSvc = Svc<IRoleService>();
            var uSvc = Svc<IUserService>();

            #region Setup

            var corp = Guid.NewGuid().ToString().Substring(5);

            await AddRoleGroupAsync(gSvc, "nr", corp, "a"); // without roles, without user
            await AddRoleGroupAsync(gSvc, "wr", corp, "a"); // with roles, without user
            await AddRoleGroupAsync(gSvc, "wu", corp, "a"); // with user

            await AddRoleAsync(rSvc, corp, "a", "*", "*", "*");

            await uSvc.CreateUserAsync(new User
            {
                Id = "1"
            }, new LocalUserInfo
            {
                Corp = corp
            });

            await gSvc.AddRolesToGroupAsync(await Group("wr"), new[]
            {
                new RoleModel
                {
                    Role = $"{corp}.a.*.*.*",
                    Permission = Permission.Crud.Serialize()
                }
            });

            await uSvc.AssignUserToGroupsAsync(new User
            {
                Id = "1",
            }, new[]
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup()
                {
                    Name = "wu",
                    Corp = corp,
                    App = "a"
                }
            });

            var usr = uSvc.GetUser("1", corp);
            Assert.IsTrue(usr.RoleGroups.IsAny());

            #endregion

            await gSvc.DeleteRoleGroupAsync(await Group("nr"));

            await gSvc.DeleteRoleGroupAsync(await Group("wr"));

            try
            {
                await gSvc.DeleteRoleGroupAsync(await Group("wu"));
                Assert.Fail("Expect error");
            }
            catch (SimpleAuthException e) when (e.Message.Contains("in use"))
            {
                // OK
            }
            
            Assert.IsNull(await Group("nr"));
            Assert.IsNull(await Group("wr"));
            Assert.NotNull(await Group("wu"));

            async Task<global::SimpleAuth.Shared.Domains.RoleGroup> Group(string name)
            {
                return await gSvc.GetRoleGroupByNameAsync(name, corp, "a");
            }
        }
    }
}