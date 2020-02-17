using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;

namespace Test.SimpleAuth.Services.Test.Services
{
    public class TestIUserService : BaseTestService<IUserRepository, User, string>
    {
        [Test]
        public void GetUser()
        {
            var svc = Prepare(out var mockUserRepository).GetRequiredService<IUserService>();

            Assert.Catch<ArgumentNullException>(() => svc.GetUser(string.Empty, "c"));
            Assert.Catch<ArgumentNullException>(() => svc.GetUser("user", string.Empty));

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();

            var user = NowGet(new User
            {
                UserInfos = null
            }, corp1);
            Assert.IsNull(user);

            SetupReturnUser(new User
            {
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        Corp = corp2
                    }
                }
            });

            Assert.IsNull(svc.GetUser(userId, corp1));
            user = svc.GetUser(userId, corp2);
            Assert.NotNull(user);
            Assert.AreEqual(1, user.LocalUserInfos.Length);

            SetupReturnUser(new User
            {
                Id = userId,
                NormalizedId = userId.NormalizeInput(),
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        Corp = corp1
                    },

                    new LocalUserInfo
                    {
                        Corp = corp2
                    }
                },
                RoleGroupUsers = new List<RoleGroupUser>
                {
                    new RoleGroupUser
                    {
                        UserId = userId,
                        User = new User
                        {
                            Id = userId,
                            NormalizedId = userId
                        },
                        RoleGroupId = Guid.NewGuid(),
                        RoleGroup = new RoleGroup
                        {
                            Name = "g11",
                            Corp = corp1,
                            App = "a",
                            Locked = true,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m1",
                                    Verb = Verb.View,
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m2",
                                    Verb = Verb.Edit,
                                }
                            }
                        }
                    },
                    new RoleGroupUser
                    {
                        UserId = userId,
                        RoleGroup = new RoleGroup
                        {
                            Name = "g12",
                            Corp = corp1,
                            App = "a",
                            Locked = false,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m3",
                                    Verb = Verb.View,
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m4",
                                    Verb = Verb.View,
                                }
                            }
                        }
                    },
                    new RoleGroupUser
                    {
                        UserId = userId,
                        RoleGroup = new RoleGroup
                        {
                            Name = "g21",
                            Corp = corp2,
                            App = "a",
                            Locked = true,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m5",
                                    Verb = Verb.View,
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m6",
                                    Verb = Verb.View,
                                }
                            }
                        }
                    }
                }
            });

            user = svc.GetUser(userId, corp1);
            Assert.NotNull(user);
            Assert.AreEqual(1, user.LocalUserInfos.Length);
            Assert.AreEqual(corp1, user.LocalUserInfos.First().Corp);
            Assert.AreEqual(2, user.RoleGroups.Length);
            Assert.AreEqual("g11", user.RoleGroups[0].Name);
            Assert.AreEqual("g12", user.RoleGroups[1].Name);
            Assert.AreEqual(2, user.RoleGroups[0].Roles.Length);
            Assert.AreEqual(2, user.RoleGroups[1].Roles.Length);
            Assert.AreEqual(corp1, user.RoleGroups[0].Corp);
            Assert.AreEqual("a", user.RoleGroups[0].App);
            Assert.AreEqual(true, user.RoleGroups[0].Locked);
            Assert.AreEqual(false, user.RoleGroups[1].Locked);
            Assert.AreEqual($"{corp1}.a.e.t.m1", user.RoleGroups[0].Roles[0].RoleId);
            Assert.AreEqual(Verb.View, user.RoleGroups[0].Roles[0].Verb);
            Assert.AreEqual($"{corp1}.a.e.t.m2", user.RoleGroups[0].Roles[1].RoleId);
            Assert.AreEqual(Verb.Edit, user.RoleGroups[0].Roles[1].Verb);

            ISetup<IUserRepository, User> SetupUser() => mockUserRepository.Setup(x => x.Find(userId));

            void SetupReturnUser(User u)
            {
                if (!u.Id.IsBlank())
                    u.NormalizedId = u.Id.NormalizeInput();
                SetupUser().Returns(u);
            }

            global::SimpleAuth.Shared.Domains.User NowGet(User u, string corp)
            {
                SetupReturnUser(u);
                return svc.GetUser(userId, corp);
            }
        }

        [Test]
        public async Task CreateUserAsync()
        {
            var sp = Prepare(out var mockUserRepository);
            var svc = sp.GetRequiredService<IUserService>();

            mockUserRepository.Setup(x =>
                x.CreateUserAsync(It.IsAny<User>(), It.IsAny<LocalUserInfo>())).Returns(Task.CompletedTask);

            var userId = RandomUser();
            var corp = RandomCorp();
            var email = RandomEmail();
            var password = "plain/text";
            var encryptedPassword = sp.GetRequiredService<IEncryptionService>().Encrypt(password);

            await svc.CreateUserAsync(new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            }, new global::SimpleAuth.Shared.Domains.LocalUserInfo
            {
                Corp = corp,
                Email = email,
                PlainPassword = string.Empty,
                Locked = true
            });

            mockUserRepository.Verify(m => m.CreateUserAsync(
                    It.Is<User>(u => u.Id == userId),
                    It.Is<LocalUserInfo>(lu =>
                        lu.Corp == corp && lu.UserId == userId && lu.Email == email && lu.EncryptedPassword.IsBlank() &&
                        lu.Locked)
                )
            );

            await svc.CreateUserAsync(new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            }, new global::SimpleAuth.Shared.Domains.LocalUserInfo
            {
                Corp = corp,
                Email = email,
                PlainPassword = password,
                Locked = true
            });

            mockUserRepository.Verify(m => m.CreateUserAsync(
                    It.Is<User>(u => u.Id == userId),
                    It.Is<LocalUserInfo>(lu =>
                        encryptedPassword.Equals(lu.EncryptedPassword))
                )
            );

            mockUserRepository.Setup(x =>
                    x.CreateUserAsync(It.IsAny<User>(), It.IsAny<LocalUserInfo>()))
                .ThrowsAsync(new EntityAlreadyExistsException(string.Empty));

            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await svc.CreateUserAsync(
                new global::SimpleAuth.Shared.Domains.User
                {
                    Id = userId
                }, new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp,
                }));
        }

        [Test]
        public async Task AssignUserToGroupsAsync()
        {
            var svc = Prepare<IRoleGroupRepository, RoleGroup, Guid>(out var mockUserRepository,
                out var mockRoleGroupRepository).GetRequiredService<IUserService>();

            mockUserRepository.Setup(x => x.AssignUserToGroups(It.IsAny<User>(), It.IsAny<RoleGroup[]>()))
                .Returns(Task.CompletedTask);

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var app1 = RandomApp();
            var app2 = RandomApp();
            var roleGroup1 = RandomRoleGroup();
            var roleGroup2 = RandomRoleGroup();

            var user = new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.AssignUserToGroupsAsync(null, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1
                }
            }));

            Assert.CatchAsync<ArgumentException>(async () =>
                await svc.AssignUserToGroupsAsync(user, new global::SimpleAuth.Shared.Domains.PermissionGroup[0]));

            Assert.CatchAsync<ArgumentException>(async () => await svc.AssignUserToGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1
                },
                null
            }));

            Assert.CatchAsync<InvalidOperationException>(async () => await svc.AssignUserToGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1,
                    App = app1
                },
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1,
                    App = app2
                }
            }));

            mockRoleGroupRepository.Setup(x =>
                    x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>()))
                .Returns((IEnumerable<RoleGroup>) null);

            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.AssignUserToGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1,
                    App = app1
                },
            }));

            mockRoleGroupRepository.Setup(x =>
                    x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>()))
                .Returns(new[]
                {
                    new RoleGroup
                    {
                        Name = roleGroup1,
                        Corp = corp1,
                        App = app1
                    }
                });

            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.AssignUserToGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup2,
                    Corp = corp1,
                    App = app1
                },
            }));

            await svc.AssignUserToGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1
                },
            });

            mockUserRepository.Verify(m => m.AssignUserToGroups(It.Is<User>(u => u.Id == userId),
                It.Is<RoleGroup[]>(rgs => rgs.Length == 1 && rgs[0].Name == roleGroup1)));
        }

        [Test]
        public async Task UnAssignUserFromGroupsAsync()
        {
            var svc = Prepare<IRoleGroupUserRepository, RoleGroupUser>(out var mockUserRepository,
                out var mockRoleGroupUserRepository).GetRequiredService<IUserService>();

            mockUserRepository.Setup(x => x.UnAssignUserFromGroups(It.IsAny<User>(), It.IsAny<RoleGroup[]>()))
                .Returns(Task.CompletedTask);

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var app1 = RandomApp();
            var app2 = RandomApp();
            var roleGroup1 = RandomRoleGroup();
            var roleGroup2 = RandomRoleGroup();

            var user = new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UnAssignUserFromGroupsAsync(null, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1
                }
            }));

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UnAssignUserFromGroupsAsync(user, null));

            await svc.UnAssignUserFromGroupsAsync(user, new global::SimpleAuth.Shared.Domains.PermissionGroup[0]);
            mockRoleGroupUserRepository.VerifyNoOtherCalls();

            Assert.CatchAsync<ArgumentException>(async () => await svc.UnAssignUserFromGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1
                },
                null
            }));

            Assert.CatchAsync<InvalidOperationException>(async () => await svc.UnAssignUserFromGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1,
                    App = app1
                },
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1,
                    App = app2
                }
            }));

            SetupFindManyReturns(null);

            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UnAssignUserFromGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Corp = corp1,
                    App = app1
                },
            }));

            SetupFindManyReturns(new[]
            {
                new RoleGroupUser
                {
                    UserId = userId,
                    RoleGroup = new RoleGroup
                    {
                        Name = roleGroup1,
                        Corp = corp1,
                        App = app1
                    }
                }
            });

            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UnAssignUserFromGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1
                },
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup2,
                    Corp = corp1,
                    App = app1
                },
            }));

            await svc.UnAssignUserFromGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1
                },
            });

            mockUserRepository.Verify(m => m.UnAssignUserFromGroups(It.Is<User>(u => u.Id == userId),
                It.Is<RoleGroup[]>(rgs => rgs.Length == 1 && rgs[0].Name == roleGroup1)));

            void SetupFindManyReturns(IEnumerable<RoleGroupUser> roleGroupUsers) =>
                mockRoleGroupUserRepository.Setup(x =>
                        x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroupUser, bool>>>>(),
                            It.IsAny<FindOptions>()))
                    .Returns(roleGroupUsers);
        }

        [Test]
        public async Task UnAssignUserFromAllGroupsAsync()
        {
            var svc = Prepare(out var mockUserRepository).GetRequiredService<IUserService>();

            mockUserRepository.Setup(x => x.UnAssignUserFromGroups(It.IsAny<User>(), It.IsAny<RoleGroup[]>()))
                .Returns(Task.CompletedTask);

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var corp3 = RandomCorp();
            var roleGroup1 = RandomRoleGroup();
            var roleGroup2 = RandomRoleGroup();
            var roleGroup3 = RandomRoleGroup();

            var user = new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UnAssignUserFromAllGroupsAsync(null, corp1));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.UnAssignUserFromAllGroupsAsync(user, string.Empty));

            SetupFindReturns(null);
            Assert.CatchAsync<EntityNotExistsException>(async () =>
                await svc.UnAssignUserFromAllGroupsAsync(user, corp1));

            SetupFindReturns(new User
            {
                RoleGroupUsers = null
            });
            await svc.UnAssignUserFromAllGroupsAsync(user, corp1);
            mockUserRepository.Verify(m => m.Find(It.IsAny<string>()));
            mockUserRepository.VerifyNoOtherCalls();

            SetupFindReturns(new User
            {
                RoleGroupUsers = new List<RoleGroupUser>
                {
                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = roleGroup1,
                            Corp = corp1
                        },
                    },
                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = roleGroup2,
                            Corp = corp1
                        },
                    },
                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = roleGroup3,
                            Corp = corp3
                        },
                    }
                }
            });
            await svc.UnAssignUserFromAllGroupsAsync(user, corp2);
            mockUserRepository.Verify(m => m.Find(It.IsAny<string>()));
            mockUserRepository.VerifyNoOtherCalls();

            await svc.UnAssignUserFromAllGroupsAsync(user, corp1);

            mockUserRepository.Verify(m => m.UnAssignUserFromGroups(It.Is<User>(u => u.Id == userId),
                It.Is<RoleGroup[]>(rgs =>
                    rgs.Length == 2 && rgs[0].Name == roleGroup1 && rgs.All(x => x.Corp == corp1))));

            void SetupFindReturns(User u) => mockUserRepository.Setup(x => x.Find(It.IsAny<string>())).Returns(u);
        }

        [Test]
        public async Task GetActiveRolesAsync()
        {
            var svc = Prepare<IRoleRepository, Role, string>(out var mockUserRepository, out var mockRoleRepository)
                .GetRequiredService<IUserService>();

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var app2 = RandomApp();
            var env = RandomEnv();
            var tenant = RandomTenant();

            #region Arguments validation

            // catch null arg
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.GetActiveRolesAsync(null, corp1, app1, env, tenant));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.GetActiveRolesAsync(userId, null, app1, env, tenant));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.GetActiveRolesAsync(userId, corp1, null, env, tenant));

            // catch wildcard not supported
            Assert.CatchAsync<ArgumentException>(async () =>
                await svc.GetActiveRolesAsync(userId, corp1, app1, env, Constants.WildCard));
            Assert.CatchAsync<ArgumentException>(async () =>
                await svc.GetActiveRolesAsync(userId, corp1, app1, Constants.WildCard, tenant));

            #endregion

            // catch user not found
            SetupFindUserReturns(null);
            Assert.CatchAsync<EntityNotExistsException>(async () =>
                await svc.GetActiveRolesAsync(userId, corp1, app1));

            // normal

            #region SetupFindUserReturns 3 RoleGroupUsers of groups named g11, g12, g21

            SetupFindUserReturns(new User
            {
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        UserId = userId,
                        Corp = corp1
                    },
                    new LocalUserInfo
                    {
                        UserId = userId,
                        Corp = corp2
                    }
                },
                RoleGroupUsers = new List<RoleGroupUser>
                {
                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = "g11",
                            Corp = corp1,
                            App = app1,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m111",
                                    Env = env,
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m112",
                                    Env = env,
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m113",
                                    Env = RandomEnv(),
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                            }
                        }
                    },

                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = "g12",
                            Corp = corp1,
                            App = app1,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m121",
                                    Env = env,
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m122",
                                    Env = env,
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m123",
                                    Env = RandomEnv(),
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                            }
                        }
                    },

                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = "g21",
                            Corp = corp2,
                            App = app2,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m211",
                                    Env = env,
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m212",
                                    Env = env,
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                                new RoleRecord
                                {
                                    RoleId = "c.a.e.t.m213",
                                    Env = RandomEnv(),
                                    Tenant = tenant,
                                    Verb = Verb.Add
                                },
                            }
                        }
                    }
                }
            });

            #endregion

            #region Test without any locked role

            SetupFindRolesReturns(null);

            var roles = await svc.GetActiveRolesAsync(userId, corp1, app1);
            Assert.AreEqual(6, roles.Count);
            Assert.IsTrue(roles.All(x => x.RoleId.Contains("t.m1")));

            roles = await svc.GetActiveRolesAsync(userId, corp1, app1, env);
            Assert.AreEqual(4, roles.Count);
            Assert.IsFalse(roles.Any(x => x.RoleId.EndsWith("3")));

            roles = await svc.GetActiveRolesAsync(userId, corp1, app1, tenant: tenant);
            Assert.AreEqual(6, roles.Count);

            roles = await svc.GetActiveRolesAsync(userId, corp2, app2);
            Assert.AreEqual(3, roles.Count);
            Assert.IsTrue(roles.All(x => x.RoleId.Contains("t.m2")));

            roles = await svc.GetActiveRolesAsync(userId, corp2, RandomApp());
            Assert.AreEqual(0, roles.Count);

            #endregion

            #region Test when some locked roles

            SetupFindRolesReturns(new List<Role>
            {
                new Role
                {
                    Id = "c.a.e.t.m112",
                    Locked = true,
                },
                new Role
                {
                    Id = "c.a.e.t.m113",
                    Locked = true,
                },
                new Role
                {
                    Id = "c.a.e.t.m122",
                    Locked = true,
                },
                new Role
                {
                    Id = "c.a.e.t.m123",
                    Locked = true,
                },
                new Role
                {
                    Id = "c.a.e.t.m212",
                    Locked = true,
                },
                new Role
                {
                    Id = "c.a.e.t.m213",
                    Locked = true,
                }
            });

            roles = await svc.GetActiveRolesAsync(userId, corp1, app1);
            Assert.AreEqual(2, roles.Count);
            Assert.IsTrue(roles.All(x => x.RoleId.EndsWith("1")));

            roles = await svc.GetActiveRolesAsync(userId, corp1, app1, env);
            Assert.AreEqual(2, roles.Count);

            roles = await svc.GetActiveRolesAsync(userId, corp1, app1, tenant: tenant);
            Assert.AreEqual(2, roles.Count);

            roles = await svc.GetActiveRolesAsync(userId, corp2, app2);
            Assert.AreEqual(1, roles.Count);
            Assert.IsTrue(roles.First().RoleId.EndsWith("1"));

            roles = await svc.GetActiveRolesAsync(userId, corp2, RandomApp());
            Assert.AreEqual(0, roles.Count);

            #endregion

            void SetupFindUserReturns(User u) =>
                mockUserRepository.Setup(x => x.FindAsync(It.IsAny<string>())).ReturnsAsync(u);

            void SetupFindRolesReturns(ICollection<Role> rs) =>
                mockRoleRepository
                    .Setup(x =>
                        x.FindMany(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>(), It.IsAny<FindOptions>())
                    ).Returns(rs);
        }

        [Test]
        public async Task GetMissingRolesAsync()
        {
            var svc = Prepare<IRoleRepository, Role, string>(out var mockUserRepository, out var mockRoleRepository)
                .GetRequiredService<IUserService>();

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var app2 = RandomApp();

            #region Arguments validation

            // catch null arg
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.GetMissingRolesAsync(string.Empty, new (string, Verb)[0], corp1, app1));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.GetMissingRolesAsync(userId, new (string, Verb)[0], string.Empty, app1));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.GetMissingRolesAsync(userId, new (string, Verb)[0], corp1, string.Empty));

            // not allow arg contains permission None
            Assert.CatchAsync<ArgumentException>(async () =>
                await svc.GetMissingRolesAsync(userId, new (string, Verb)[]
                {
                    ("any", Verb.None)
                }, corp1, app1)
            );

            #endregion

            // catch user not found
            SetupFindUserReturns(null);
            Assert.CatchAsync<EntityNotExistsException>(async () =>
                await svc.GetMissingRolesAsync(userId, new (string, Verb)[]
                {
                    ("any", Verb.Add)
                }, corp1, app1)
            );

            // normal

            #region SetupFindUserReturns

            SetupFindUserReturns(new User
            {
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        UserId = userId,
                        Corp = corp1
                    },
                    new LocalUserInfo
                    {
                        UserId = userId,
                        Corp = corp2
                    }
                },
                RoleGroupUsers = new List<RoleGroupUser>
                {
                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = "g1",
                            Corp = corp1,
                            App = app1,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.{app1}.e.t.crud",
                                    Verb = Verb.Crud
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.{app1}.e.t.full",
                                    Verb = Verb.Full
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.{app1}.e.t.ae",
                                    Verb = Verb.Add | Verb.Edit
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.{app1}.e.t.d",
                                    Verb = Verb.Delete
                                },
                            }
                        }
                    },
                    new RoleGroupUser
                    {
                        RoleGroup = new RoleGroup
                        {
                            Name = "g2",
                            Corp = corp2,
                            App = app2,
                            RoleRecords = new List<RoleRecord>
                            {
                                new RoleRecord
                                {
                                    RoleId = $"{corp2}.{app2}.e.t.crud",
                                    Verb = Verb.Crud
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp2}.{app2}.e.t.full",
                                    Verb = Verb.Full
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp2}.{app2}.e.t.ae",
                                    Verb = Verb.Add | Verb.Edit
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp2}.{app2}.e.t.d",
                                    Verb = Verb.Delete
                                },
                            }
                        }
                    }
                }
            });

            #endregion

            SetupFindRolesReturns(null);

            #region corp1

            var missingRoles = await svc.GetMissingRolesAsync(userId, new[]
            {
                ($"{corp1}.{app1}.e.t.crud", Verb.Edit), // usr has
                ($"{corp1}.{app1}.e.t.ae", Verb.Add | Verb.View), // usr has Add but not View
                ($"{corp1}.{app1}.e.t.d", Verb.Edit), // expect
            }, corp1, app1);

            Assert.AreEqual(2, missingRoles.Count);
            var ae = missingRoles.FirstOrDefault(x => x.RoleId == $"{corp1}.{app1}.e.t.ae");
            Assert.NotNull(ae);
            Assert.AreEqual(Verb.View, ae.Verb);
            var d = missingRoles.FirstOrDefault(x => x.RoleId == $"{corp1}.{app1}.e.t.d");
            Assert.NotNull(ae);
            Assert.AreEqual(Verb.Edit, d.Verb);

            #endregion

            #region corp2

            missingRoles = await svc.GetMissingRolesAsync(userId, new[]
            {
                ($"{corp2}.{app2}.e.t.crud", Verb.Edit), // usr has
                ($"{corp2}.{app2}.e.t.ae", Verb.Add | Verb.View), // usr has Add but not View
                ($"{corp2}.{app2}.e.t.d", Verb.Edit), // expect
            }, corp2, app2);

            Assert.AreEqual(2, missingRoles.Count);
            ae = missingRoles.FirstOrDefault(x => x.RoleId == $"{corp2}.{app2}.e.t.ae");
            Assert.NotNull(ae);
            Assert.AreEqual(Verb.View, ae.Verb);
            d = missingRoles.FirstOrDefault(x => x.RoleId == $"{corp2}.{app2}.e.t.d");
            Assert.NotNull(ae);
            Assert.AreEqual(Verb.Edit, d.Verb);

            #endregion

            void SetupFindUserReturns(User u) =>
                mockUserRepository.Setup(x => x.FindAsync(It.IsAny<string>())).ReturnsAsync(u);

            void SetupFindRolesReturns(ICollection<Role> rs) =>
                mockRoleRepository
                    .Setup(x =>
                        x.FindMany(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>(), It.IsAny<FindOptions>())
                    ).Returns(rs);
        }

        [Test]
        public async Task UpdateLockStatusAsync()
        {
            var svc = Prepare<ILocalUserInfoRepository, LocalUserInfo, Guid>(out var mockUserRepository,
                out var mockLocalUserInfoRepository).GetRequiredService<IUserService>();
            BasicSetup<ILocalUserInfoRepository, LocalUserInfo, Guid>(mockLocalUserInfoRepository);
            
            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var corp3 = RandomCorp();

            var user = new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdateLockStatusAsync(null));
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdateLockStatusAsync(user));

            user.LocalUserInfos = new global::SimpleAuth.Shared.Domains.LocalUserInfo[0];
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdateLockStatusAsync(user));
            user.LocalUserInfos = new global::SimpleAuth.Shared.Domains.LocalUserInfo[] {null};
            Assert.CatchAsync<ArgumentException>(async () => await svc.UpdateLockStatusAsync(user));

            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo()
            };

            SetupFindReturns(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UpdateLockStatusAsync(user));

            SetupFindReturns(new User
            {
                Id = userId,
                UserInfos = null
            });

            await svc.UpdateLockStatusAsync(user);
            mockLocalUserInfoRepository.VerifyNoOtherCalls();

            SetupFindReturns(new User
            {
                Id = userId,
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        Corp = corp1
                    },
                    new LocalUserInfo
                    {
                        Corp = corp2
                    }
                }
            });
            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp3
                }
            };
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UpdateLockStatusAsync(user));

            SetupFindReturns(new User
            {
                Id = userId,
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        Corp = corp1,
                        Locked = true
                    },
                    new LocalUserInfo
                    {
                        Corp = corp2,
                        Locked = true
                    }
                }
            });

            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp1,
                    Locked = true
                },
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp2,
                    Locked = true
                }
            };
            await svc.UpdateLockStatusAsync(user);
            mockLocalUserInfoRepository.VerifyNoOtherCalls();

            user.LocalUserInfos[0].Locked = false;
            await svc.UpdateLockStatusAsync(user);
            // ReSharper disable PossibleMultipleEnumeration
            mockLocalUserInfoRepository.Verify(m =>
                m.UpdateManyAsync(
                    It.Is<IEnumerable<LocalUserInfo>>(lus => lus.Count() == 1 && lus.First().Corp == corp1)));
            // ReSharper restore PossibleMultipleEnumeration


            void SetupFindReturns(User u) => mockUserRepository.Setup(x => x.Find(It.IsAny<string>())).Returns(u);
        }

        [Test]
        public async Task UpdatePasswordAsync()
        {
            var sp = Prepare<ILocalUserInfoRepository, LocalUserInfo, Guid>(out var mockUserRepository,
                out var mockLocalUserInfoRepository);
            var svc = sp.GetRequiredService<IUserService>();
            BasicSetup<ILocalUserInfoRepository, LocalUserInfo, Guid>(mockLocalUserInfoRepository);


            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var corp3 = RandomCorp();

            var pass1 = RandomText();
            var pass2 = RandomText();
            var pass3 = RandomText();

            var encryptionService = sp.GetRequiredService<IEncryptionService>();
            var pass1Enc = encryptionService.Encrypt(pass1);
            var pass3Enc = encryptionService.Encrypt(pass3);

            var user = new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdatePasswordAsync(null));
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdatePasswordAsync(user));

            user.LocalUserInfos = new global::SimpleAuth.Shared.Domains.LocalUserInfo[0];
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdatePasswordAsync(user));
            user.LocalUserInfos = new global::SimpleAuth.Shared.Domains.LocalUserInfo[] {null};
            Assert.CatchAsync<ArgumentException>(async () => await svc.UpdatePasswordAsync(user));

            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo()
            };

            SetupFindReturns(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UpdatePasswordAsync(user));

            SetupFindReturns(new User
            {
                Id = userId,
                UserInfos = null
            });

            await svc.UpdatePasswordAsync(user);
            mockLocalUserInfoRepository.VerifyNoOtherCalls();

            SetupFindReturns(new User
            {
                Id = userId,
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        Corp = corp1
                    },
                    new LocalUserInfo
                    {
                        Corp = corp2
                    }
                }
            });
            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp3
                }
            };
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.UpdatePasswordAsync(user));

            SetupFindReturns(new User
            {
                Id = userId,
                UserInfos = new List<LocalUserInfo>
                {
                    new LocalUserInfo
                    {
                        Corp = corp1,
                        EncryptedPassword = pass1Enc
                    },
                    new LocalUserInfo
                    {
                        Corp = corp2,
                        EncryptedPassword = null
                    },
                    new LocalUserInfo
                    {
                        Corp = corp3,
                        EncryptedPassword = pass3Enc
                    }
                }
            });

            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp1,
                    PlainPassword = pass1
                },
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp2,
                    PlainPassword = pass2
                }
            };
            await svc.UpdatePasswordAsync(user);
            // ReSharper disable PossibleMultipleEnumeration
            mockLocalUserInfoRepository.Verify(m =>
                m.UpdateManyAsync(It.Is<IEnumerable<LocalUserInfo>>(lus => lus.Count() == 2)));
            // ReSharper restore PossibleMultipleEnumeration

            user.LocalUserInfos = new[]
            {
                new global::SimpleAuth.Shared.Domains.LocalUserInfo
                {
                    Corp = corp1,
                    PlainPassword = string.Empty
                }
            };
            await svc.UpdatePasswordAsync(user);
            // ReSharper disable PossibleMultipleEnumeration
            mockLocalUserInfoRepository.Verify(m =>
                m.UpdateManyAsync(It.Is<IEnumerable<LocalUserInfo>>(lus =>
                    lus.Count() == 1 && lus.First().EncryptedPassword == null)));
            // ReSharper restore PossibleMultipleEnumeration


            void SetupFindReturns(User u) => mockUserRepository.Setup(x => x.Find(It.IsAny<string>())).Returns(u);
        }
    }
}