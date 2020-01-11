using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using LocalUserInfo = SimpleAuth.Shared.Domains.LocalUserInfo;
using User = SimpleAuth.Shared.Domains.User;

namespace Test.SimpleAuth.Shared.Test.Services
{
    public class TestIUserService : BaseTestClass
    {
        [Test]
        public async Task GetUser()
        {
            const string email = "me@yahoo.com";
            var randomCorp1 = Guid.NewGuid().ToString().Substring(0, 5);

            var user = new User
            {
                Id = "AbCd" + Guid.NewGuid().ToString().Substring(0, 5),
            };

            var uSvc = Svc<IUserService>();
            await uSvc.CreateUserAsync(user,
                new LocalUserInfo
                {
                    Corp = randomCorp1,
                    Email = email
                });

            var gSvc = Svc<IRoleGroupService>();
            await AddRoleGroupAsync(gSvc, "g1", randomCorp1, "a");
            await AddRoleGroupAsync(gSvc, "g2", randomCorp1, "a");

            await AssignUserToGroups(uSvc, user.Id, $"g1.{randomCorp1}.a", $"g2.{randomCorp1}.a");

            user = uSvc.GetUser(user.Id, randomCorp1);
            Assert.AreEqual(email, user.LocalUserInfos.First().Email);
            Assert.AreEqual(2, user.RoleGroups?.Length ?? -1);

            Assert.AreEqual(0, user.RoleGroups?.SelectMany(g => g.Roles).Count() ?? 0);

            var rSvc = Svc<IRoleService>();
            await AddRoleAsync(rSvc, randomCorp1, "a", "e", "t", "m11");
            await AddRoleAsync(rSvc, randomCorp1, "a", "e", "t", "m12");
            await AddRoleAsync(rSvc, randomCorp1, "a", "e", "t", "m20");

            await gSvc.AddRolesToGroupAsync(await gSvc.GetRoleGroupByName("g1", randomCorp1, "a"), new[]
            {
                new RoleModel
                {
                    Role = $"{randomCorp1}.a.e.t.m11",
                    Permission = Permission.Add.Serialize(),
                },

                new RoleModel
                {
                    Role = $"{randomCorp1}.a.e.t.m12",
                    Permission = Permission.View.Serialize(),
                }
            });

            await gSvc.AddRolesToGroupAsync(await gSvc.GetRoleGroupByName("g2", randomCorp1, "a"), new[]
            {
                new RoleModel
                {
                    Role = $"{randomCorp1}.a.e.t.m20",
                    Permission = Permission.Edit.Serialize(),
                }
            });

            user = uSvc.GetUser(user.Id, randomCorp1);
            Assert.AreEqual(3, user.RoleGroups.SelectMany(g => g.Roles).Count());
            Assert.AreEqual(2, user.RoleGroups.First(x => x.Name == "g1").Roles.Count());
            Assert.AreEqual(1, user.RoleGroups.First(x => x.Name == "g2").Roles.Count());
            Assert.AreEqual(Permission.Edit, user.RoleGroups.First(x => x.Name == "g2").Roles.First().Permission);


            try
            {
                uSvc.GetUser(" ", randomCorp1);
                Assert.Fail("Expect error");
            }
            catch (ArgumentNullException)
            {
                // OK
            }

            try
            {
                uSvc.GetUser(user.Id, " ");
                Assert.Fail("Expect error");
            }
            catch (ArgumentNullException)
            {
                // OK
            }

            Assert.IsNull(uSvc.GetUser(user.Id, "c"));
            Assert.IsNull(uSvc.GetUser("2", "c"));
        }

        [TestCase(true, null, "1", "c1", "a", 1)]
        [TestCase(false, typeof(EntityAlreadyExistsException), "1", "c1", "a", 1)]
        [TestCase(true, null, "1", "c2", "a", 2)]
        [TestCase(false, typeof(EntityAlreadyExistsException), "1", "c2", "a", 2)]
        [TestCase(true, null, "1", "c3", "", 3)]
        [TestCase(false, typeof(EntityAlreadyExistsException), "1", "c3", "", 3)]
        [TestCase(true, null, "1", "c4", "    ", 4)]
        [TestCase(false, typeof(EntityAlreadyExistsException), "1", "c4", "", 4)]
        public async Task CreateUserAsync(bool expectCreated, Type expectedException, string userId, string corp,
            string password, int expectedNumberOfCorps)
        {
            await Truncate<global::SimpleAuth.Services.Entities.LocalUserInfo, global::SimpleAuth.Services.Entities.User
            >(nameof(CreateUserAsync));

            var svc = Svc<IUserService>();
            try
            {
                await svc.CreateUserAsync(new User
                {
                    Id = userId,
                }, new LocalUserInfo
                {
                    Corp = corp,
                    PlainPassword = password
                });

                if (!expectCreated)
                {
                    Assert.Fail();
                }

                var lookupUser = Svc<IUserRepository>().Find(userId);
                Assert.NotNull(lookupUser);
                Assert.NotNull(lookupUser.UserInfos);
                Assert.AreEqual(expectedNumberOfCorps, lookupUser.UserInfos.Count);

                var userInfo = lookupUser.UserInfos.First(x => x.Corp == corp);
                if (password.IsBlank())
                {
                    Assert.IsNull(userInfo.EncryptedPassword);
                }
                else if (password != null)
                {
                    var dSvc = Svc<IEncryptionService>();
                    Assert.AreEqual(password, dSvc.Decrypt(userInfo.EncryptedPassword));
                }
                else
                {
                    Assert.IsNull(userInfo.EncryptedPassword);
                }
            }
            catch (Exception e)
            {
                if (expectCreated)
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.AreEqual(expectedException, e.GetType());
                }
            }
        }

        [TestCase(true, null, "1", 1, "g11.c.a1")]
        [TestCase(false, typeof(EntityNotExistsException), "2", 0, "g11.c.a1")]
        [TestCase(true, null, "1", 1, "g11.c.a1")]
        [TestCase(true, null, "1", 2, "g12.c.a1")]
        [TestCase(false, typeof(EntityNotExistsException), "1", 0, "g12.c.a3")]
        [TestCase(false, typeof(InvalidOperationException), "1", 0, "g12.c.a1", "g22.c.a2")]
        public async Task AssignUserToGroups(bool expectedSuccess, Type expectedException,
            string userId,
            int expectedNoOfGroup,
            params string[] tps)
        {
            #region Setup

            await Truncate<RoleGroupUser, global::SimpleAuth.Services.Entities.LocalUserInfo,
                global::SimpleAuth.Services.Entities.User, RoleRecord, RoleGroup>(nameof(AssignUserToGroups));
            var uSvc = Svc<IUserService>();
            ExecuteOne(nameof(AssignUserToGroups), async () =>
            {
                await uSvc.CreateUserAsync(new User
                {
                    Id = "1",
                }, new LocalUserInfo
                {
                    Corp = "c",
                });

                var gSvc = Svc<IRoleGroupService>();
                await AddRoleGroupAsync(gSvc, "g11", "c", "a1");
                await AddRoleGroupAsync(gSvc, "g12", "c", "a1");
                await AddRoleGroupAsync(gSvc, "g21", "c", "a2");
                await AddRoleGroupAsync(gSvc, "g22", "c", "a2");
            });

            #endregion

            // Begin Test

            try
            {
                await AssignUserToGroups(uSvc, userId, tps);
                var lookupUser = Svc<IUserRepository>().Find(userId);
                var newGroupsCnt = lookupUser.RoleGroupUsers.OrEmpty().Count();
                Assert.AreEqual(expectedNoOfGroup, newGroupsCnt);

                if (!expectedSuccess)
                {
                    Assert.Fail("Expected non-success");
                }
            }
            catch (Exception e)
            {
                if (expectedSuccess)
                {
                    Assert.Fail(e.Message);
                }
                else
                {
                    Assert.AreEqual(expectedException, e.GetType());
                }
            }
        }

        [TestCase(true, null, "1", 5, "g11.c.a1")]
        [TestCase(false, typeof(EntityNotExistsException), "2", 0, "g11.c.a1")] // No User
        [TestCase(false, typeof(EntityNotExistsException), "1", 0, "g12.c.a3")] // No Group
        [TestCase(false, typeof(InvalidOperationException), "1", 0, "g12.c.a1", "g22.c.a2")] // Not same company
        [TestCase(true, null, "1", 4, "g12.c.a1")]
        [TestCase(false, typeof(EntityNotExistsException), "1", 0, "g12.c.a1")] // Delete again
        [TestCase(false, typeof(EntityNotExistsException), "1", 0, "g41.c.a4", "g42.c.a4")]
        public async Task UnAssignUserFromGroups(bool expectedSuccess, Type expectedException,
            string userId,
            int expectedNoOfGroup,
            params string[] tps)
        {
            #region Setup

            await Truncate<RoleGroupUser, global::SimpleAuth.Services.Entities.LocalUserInfo,
                global::SimpleAuth.Services.Entities.User, RoleRecord, RoleGroup>(nameof(UnAssignUserFromGroups));
            var uSvc = Svc<IUserService>();
            ExecuteOne(nameof(UnAssignUserFromGroups), async () =>
            {
                await uSvc.CreateUserAsync(new User
                {
                    Id = "1",
                }, new LocalUserInfo
                {
                    Corp = "c",
                });

                var gSvc = Svc<IRoleGroupService>();
                await AddRoleGroupAsync(gSvc, "g11", "c", "a1");
                await AddRoleGroupAsync(gSvc, "g12", "c", "a1");
                await AddRoleGroupAsync(gSvc, "g21", "c", "a2");
                await AddRoleGroupAsync(gSvc, "g22", "c", "a2");
                await AddRoleGroupAsync(gSvc, "g31", "c", "a3");
                await AddRoleGroupAsync(gSvc, "g32", "c", "a3");
                await AddRoleGroupAsync(gSvc, "g41", "c", "a4");
                await AddRoleGroupAsync(gSvc, "g42", "c", "a4");

                await AssignUserToGroup(uSvc, "1", "g11", "c", "a1");
                await AssignUserToGroup(uSvc, "1", "g12", "c", "a1");
                await AssignUserToGroup(uSvc, "1", "g21", "c", "a2");
                await AssignUserToGroup(uSvc, "1", "g22", "c", "a2");
                await AssignUserToGroup(uSvc, "1", "g31", "c", "a3");
                await AssignUserToGroup(uSvc, "1", "g32", "c", "a3");
            });

            #endregion

            // Begin Test

            try
            {
                await uSvc.UnAssignUserFromGroupsAsync(new User
                    {
                        Id = userId,
                    },
                    YieldGroup(tps).ToArray());

                var lookupUser = Svc<IUserRepository>().Find(userId);
                var newGroupsCnt = lookupUser.RoleGroupUsers.OrEmpty().Count();
                Assert.AreEqual(expectedNoOfGroup, newGroupsCnt);

                if (!expectedSuccess)
                {
                    Assert.Fail("Expected non-success");
                }
            }
            catch (Exception e)
            {
                if (expectedSuccess)
                {
                    Assert.Fail($"{e.Message} ({e.GetType().Name})");
                }
                else
                {
                    Assert.AreEqual(expectedException, e.GetType());
                }
            }
        }

        [Test]
        public async Task UnAssignUserFromAllGroups()
        {
            var randomCorp1 = Guid.NewGuid().ToString();
            var randomCorp2 = Guid.NewGuid().ToString();

            var user = new User
            {
                Id = "1",
            };

            var uSvc = Svc<IUserService>();
            await uSvc.CreateUserAsync(user,
                new LocalUserInfo
                {
                    Corp = randomCorp1,
                });
            await uSvc.CreateUserAsync(user,
                new LocalUserInfo
                {
                    Corp = randomCorp2,
                });

            var gSvc = Svc<IRoleGroupService>();
            await AddRoleGroupAsync(gSvc, "g1", randomCorp1, "a");
            await AddRoleGroupAsync(gSvc, "g2", randomCorp1, "a");
            await AddRoleGroupAsync(gSvc, "g3", randomCorp2, "a");

            await AssignUserToGroups(uSvc, user.Id, $"g1.{randomCorp1}.a", $"g2.{randomCorp1}.a");
            await AssignUserToGroups(uSvc, user.Id, $"g3.{randomCorp2}.a");


            Assert.That(async () => await uSvc.UnAssignUserFromAllGroupsAsync(user, null),
                Throws.TypeOf<ArgumentNullException>());

            await uSvc.UnAssignUserFromAllGroupsAsync(user, randomCorp1);

            user = uSvc.GetUser(user.Id, randomCorp1);
            Assert.AreEqual(0, user.RoleGroups?.Length ?? -1);

            user = uSvc.GetUser(user.Id, randomCorp2);
            Assert.AreEqual(1, user.RoleGroups?.Length ?? -1);
        }

        [Test]
        public async Task GetActiveRoles()
        {
            var uSvc = Svc<IUserService>();
            var rSvc = Svc<IRoleService>();
            var gSvc = Svc<IRoleGroupService>();

            #region Setup

            var corp = Guid.NewGuid().ToString().Substring(5);
            var user = new User
            {
                Id = "1",
            };

            await uSvc.CreateUserAsync(user,
                new LocalUserInfo
                {
                    Corp = corp,
                });

            await AddRoleGroupAsync(gSvc, "gg", corp, "a"); // all roles are not locked
            await AddRoleGroupAsync(gSvc, "gr", corp, "a"); // 1 role locked
            await AddRoleGroupAsync(gSvc, "gl", corp, "a"); // group locked, role not locked

            await AssignUserToGroups(uSvc, user.Id, $"gg.{corp}.a", $"gr.{corp}.a", $"gl.{corp}.a");

            await AddRoleAsync(rSvc, corp, "a", "nl1", "*", "*");
            await AddRoleAsync(rSvc, corp, "a", "nl2", "*", "*");
            await AddRoleAsync(rSvc, corp, "a", "l1", "*", "*");
            await AddRoleAsync(rSvc, corp, "a", "nl3", "*", "*");


            await gSvc.AddRolesToGroupAsync(await gSvc.GetRoleGroupByName("gg", corp, "a"), new[]
            {
                new RoleModel
                {
                    Role = $"{corp}.a.nl1.*.*",
                    Permission = (Permission.Add | Permission.View | Permission.Edit).Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp}.a.nl2.*.*",
                    Permission = Permission.Edit.Serialize()
                },
            });

            await gSvc.AddRolesToGroupAsync(await gSvc.GetRoleGroupByName("gr", corp, "a"), new[]
            {
                new RoleModel
                {
                    Role = $"{corp}.a.nl1.*.*",
                    Permission = Permission.Delete.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp}.a.l1.*.*",
                    Permission = Permission.Full.Serialize()
                },
            });

            await gSvc.AddRolesToGroupAsync(await gSvc.GetRoleGroupByName("gl", corp, "a"), new[]
            {
                new RoleModel
                {
                    Role = $"{corp}.a.nl3.*.*",
                    Permission = Permission.Full.Serialize()
                }
            });

            var l1 = rSvc.SearchRoles(".l1.", corp, "a").First();
            l1.Locked = true;
            await rSvc.UpdateLockStatus(l1);

            var gl = await gSvc.GetRoleGroupByName("gl", corp, "a");
            gl.Locked = true;
            await gSvc.UpdateLockStatusAsync(gl);

            #endregion

            var roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a");

            Assert.AreEqual(2, roles.Count);
            Assert.IsNull(roles.FirstOrDefault(x => x.RoleId.Contains(".l1.")));
            Assert.IsNull(roles.FirstOrDefault(x => x.RoleId.Contains(".nl3.")));
            Assert.AreEqual(Permission.Crud, roles.First(x => x.RoleId.Contains(".nl1.")).Permission);

            roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a");
            Assert.AreEqual(2, roles.Count);

            Assert.That(() => uSvc.GetActiveRolesAsync(user.Id, Guid.NewGuid().ToString(), "a"),
                Throws.TypeOf<EntityNotExistsException>());
            Assert.That(() => uSvc.GetActiveRolesAsync(null, corp, "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => uSvc.GetActiveRolesAsync(user.Id, null, "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => uSvc.GetActiveRolesAsync(user.Id, corp, null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public async Task GetActiveRoles_WithFilter()
        {
            var uSvc = Svc<IUserService>();
            var rSvc = Svc<IRoleService>();
            var gSvc = Svc<IRoleGroupService>();

            #region Setup

            var corp = Guid.NewGuid().ToString().Substring(5);
            var user = new User
            {
                Id = "1",
            };

            await uSvc.CreateUserAsync(user,
                new LocalUserInfo
                {
                    Corp = corp,
                });

            await AddRoleGroupAsync(gSvc, "g", corp, "a"); // all roles are not locked

            await AssignUserToGroups(uSvc, user.Id, $"g.{corp}.a");

            await AddRoleAsync(rSvc, corp, "a", "*", "*", "*");
            await AddRoleAsync(rSvc, corp, "a", "e", "*", "*");
            await AddRoleAsync(rSvc, corp, "a", "*", "t1", "*");
            await AddRoleAsync(rSvc, corp, "a", "e", "t1", "*");
            await AddRoleAsync(rSvc, corp, "a", "e", "t2", "*");

            await gSvc.AddRolesToGroupAsync(await gSvc.GetRoleGroupByName("g", corp, "a"), new[]
            {
                new RoleModel
                {
                    Role = $"{corp}.a.*.*.*",
                    Permission = Permission.Crud.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp}.a.e.*.*",
                    Permission = Permission.Crud.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp}.a.*.t1.*",
                    Permission = Permission.Crud.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp}.a.e.t1.*",
                    Permission = Permission.Crud.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp}.a.e.t2.*",
                    Permission = Permission.Crud.Serialize()
                }
            });

            #endregion

            var roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a");
            Assert.AreEqual(1, roles.Count);

            Assert.That(async () => await uSvc.GetActiveRolesAsync(user.Id, corp, "a", "*"), Throws.TypeOf<ArgumentException>());
            
            roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a", null, "t1");
            Assert.AreEqual(1, roles.Count);
            
            roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a", null, "t2");
            Assert.AreEqual(1, roles.Count);
            
            roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a", "e", "t2");
            Assert.AreEqual(1, roles.Count);
            
            roles = await uSvc.GetActiveRolesAsync(user.Id, corp, "a", "e2", "t2");
            Assert.AreEqual(1, roles.Count);
        }

        [Test]
        public async Task UpdateLockStatusAsync()
        {
            var uSvc = Svc<IUserService>();

            var uid = Guid.NewGuid().ToString().Substring(5);

            var corp1 = Guid.NewGuid().ToString();
            var corp2 = Guid.NewGuid().ToString();

            await uSvc.CreateUserAsync(new User
            {
                Id = uid
            }, new LocalUserInfo
            {
                Corp = corp1
            });

            await uSvc.CreateUserAsync(new User
            {
                Id = uid
            }, new LocalUserInfo
            {
                Corp = corp2
            });

            await uSvc.UpdateLockStatusAsync(new User
            {
                Id = uid,
                LocalUserInfos = new[]
                {
                    new LocalUserInfo
                    {
                        Corp = corp1,
                        Locked = true
                    }
                }
            });

            var usr = Svc<IUserRepository>().Find(uid);
            Assert.AreEqual(2, usr.UserInfos.Count);
            Assert.IsTrue(usr.UserInfos.First(x => x.Corp == corp1).Locked);
            Assert.IsFalse(usr.UserInfos.First(x => x.Corp == corp2).Locked);

            await uSvc.UpdateLockStatusAsync(new User
            {
                Id = uid,
                LocalUserInfos = new[]
                {
                    new LocalUserInfo
                    {
                        Corp = corp1,
                        Locked = false
                    },

                    new LocalUserInfo
                    {
                        Corp = corp2,
                        Locked = true
                    }
                }
            });

            usr = Svc<IUserRepository>().Find(uid);
            Assert.IsFalse(usr.UserInfos.First(x => x.Corp == corp1).Locked);
            Assert.IsTrue(usr.UserInfos.First(x => x.Corp == corp2).Locked);

            try
            {
                await uSvc.UpdateLockStatusAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = corp1,
                        }
                    }
                });
                Assert.Fail("Expect error");
            }
            catch (EntityNotExistsException)
            {
                //
            }

            try
            {
                await uSvc.UpdateLockStatusAsync(new User
                {
                    Id = uid,
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = Guid.NewGuid().ToString(),
                        }
                    }
                });
                Assert.Fail("Expect error");
            }
            catch (EntityNotExistsException)
            {
                //
            }
        }

        [Test]
        public async Task UpdatePasswordAsync()
        {
            var uSvc = Svc<IUserService>();

            var uid = Guid.NewGuid().ToString().Substring(5);

            var corp1 = Guid.NewGuid().ToString();
            var corp2 = Guid.NewGuid().ToString();

            await uSvc.CreateUserAsync(new User
            {
                Id = uid
            }, new LocalUserInfo
            {
                Corp = corp1
            });

            await uSvc.CreateUserAsync(new User
            {
                Id = uid
            }, new LocalUserInfo
            {
                Corp = corp2
            });

            await uSvc.UpdatePasswordAsync(new User
            {
                Id = uid,
                LocalUserInfos = new[]
                {
                    new LocalUserInfo
                    {
                        Corp = corp1,
                        PlainPassword = "a"
                    }
                }
            });

            var eSvc = Svc<IEncryptionService>();

            var usr = Svc<IUserRepository>().Find(uid);
            Assert.AreEqual(2, usr.UserInfos.Count);
            Assert.AreEqual("a", eSvc.Decrypt(usr.UserInfos.First(x => x.Corp == corp1).EncryptedPassword));
            Assert.IsNull(usr.UserInfos.First(x => x.Corp == corp2).EncryptedPassword);

            await uSvc.UpdatePasswordAsync(new User
            {
                Id = uid,
                LocalUserInfos = new[]
                {
                    new LocalUserInfo
                    {
                        Corp = corp1,
                        PlainPassword = "                ",
                    },

                    new LocalUserInfo
                    {
                        Corp = corp2,
                        PlainPassword = "b"
                    }
                }
            });

            usr = Svc<IUserRepository>().Find(uid);
            Assert.IsNull(usr.UserInfos.First(x => x.Corp == corp1).EncryptedPassword);
            Assert.AreEqual("b", eSvc.Decrypt(usr.UserInfos.First(x => x.Corp == corp2).EncryptedPassword));

            try
            {
                await uSvc.UpdatePasswordAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = corp1,
                            PlainPassword = "                ",
                        }
                    }
                });
                Assert.Fail("Expect error");
            }
            catch (EntityNotExistsException)
            {
                //
            }

            try
            {
                await uSvc.UpdatePasswordAsync(new User
                {
                    Id = uid,
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = Guid.NewGuid().ToString(),
                            PlainPassword = "                ",
                        }
                    }
                });
                Assert.Fail("Expect error");
            }
            catch (EntityNotExistsException)
            {
                //
            }
        }
    }
}