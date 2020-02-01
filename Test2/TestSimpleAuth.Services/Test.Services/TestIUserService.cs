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
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using Test.SimpleAuth.Server.Support.Extensions;

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
                                    Permission = Permission.View,
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m2",
                                    Permission = Permission.Edit,
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
                                    Permission = Permission.View,
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m4",
                                    Permission = Permission.View,
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
                                    Permission = Permission.View,
                                },
                                new RoleRecord
                                {
                                    RoleId = $"{corp1}.a.e.t.m6",
                                    Permission = Permission.View,
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
            Assert.AreEqual(Permission.View, user.RoleGroups[0].Roles[0].Permission);
            Assert.AreEqual($"{corp1}.a.e.t.m2", user.RoleGroups[0].Roles[1].RoleId);
            Assert.AreEqual(Permission.Edit, user.RoleGroups[0].Roles[1].Permission);

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
                x.CreateUserAsync(It.IsAny<User>(), It.IsAny<LocalUserInfo>())).ThrowsAsync(new EntityAlreadyExistsException(string.Empty));

            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await svc.CreateUserAsync(new global::SimpleAuth.Shared.Domains.User
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
            var svc = Prepare<IRoleGroupRepository, RoleGroup, Guid>(out var mockUserRepository, out var mockRoleGroupRepository).GetRequiredService<IUserService>();


            mockUserRepository.Setup(x => x.AssignUserToGroups(It.IsAny<User>(), It.IsAny<RoleGroup[]>()))
                .Returns(Task.CompletedTask);

            var userId = RandomUser();
            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var app2 = RandomApp();
            var roleGroup1 = RandomRoleGroup();
            var roleGroup2 = RandomRoleGroup();

            var user = new global::SimpleAuth.Shared.Domains.User
            {
                Id = userId
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await svc.AssignUserToGroupsAsync(null, new []
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Corp = corp1
                }
            }));

            Assert.CatchAsync<ArgumentException>(async () => await svc.AssignUserToGroupsAsync(user, new global::SimpleAuth.Shared.Domains.RoleGroup[0]));

            Assert.CatchAsync<ArgumentException>(async () => await svc.AssignUserToGroupsAsync(user, new []
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Corp = corp1
                },
                null
            }));

            Assert.CatchAsync<InvalidOperationException>(async () => await svc.AssignUserToGroupsAsync(user, new []
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Corp = corp1,
                    App = app1
                },
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Corp = corp1,
                    App = app2
                }
            }));

            mockRoleGroupRepository.Setup(x =>
                    x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>()))
                .Returns((IEnumerable<RoleGroup>) null);
            
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.AssignUserToGroupsAsync(user, new []
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Corp = corp1,
                    App = app1
                },
            }));

            mockRoleGroupRepository.Setup(x =>
                    x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>()))
                .Returns(new []
                {
                    new RoleGroup
                    {
                        Name = roleGroup1,
                        Corp = corp1,
                        App = app1
                    }
                });
            
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.AssignUserToGroupsAsync(user, new []
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Name = roleGroup2,
                    Corp = corp1,
                    App = app1
                },
            }));

            await svc.AssignUserToGroupsAsync(user, new[]
            {
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1
                },
            });
            
            //TODO here
            mockUserRepository.Verify(m => m.AssignUserToGroups(It.Is<User>(u => true), It.Is<RoleGroup[]>(rgs => true)));
        }
    }
}