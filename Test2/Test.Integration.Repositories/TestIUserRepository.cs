using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;

namespace Test.Integration.Repositories
{
    public class TestIUserRepository : BaseTestRepo
    {
        [Test]
        public async Task CreateUserAsync()
        {
            var repo = Svc<IUserRepository>();

            var userId = RandomUser();
            var corp = RandomCorp();
            var user = new User
            {
                Id = userId,
                NormalizedId = userId,
            };
            var userInfo = new LocalUserInfo
            {
                UserId = userId,
                Corp = corp,
            };

            Assert.CatchAsync<ArgumentNullException>(async () => await repo.CreateUserAsync(user, null));
            Assert.CatchAsync<ArgumentNullException>(async () => await repo.CreateUserAsync(null, userInfo));

            // expect success
            await Create();

            // user already at corp
            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await Create());

            // expect success
            await Create(new LocalUserInfo
            {
                UserId = userId,
                Corp = RandomCorp()
            });

            Assert.AreEqual(2, repo.Find(userId).UserInfos.Count);

            Task Create(LocalUserInfo userInfoCustom = null)
            {
                return repo.CreateUserAsync(user, userInfoCustom ?? userInfo);
            }
        }

        [Test]
        public void DeleteManyAsync()
        {
            var repo = Svc<IUserRepository>();
            Assert.CatchAsync<NotSupportedException>(async () => await repo.DeleteManyAsync(null));
        }

        [Test]
        public async Task AssignUserToGroups()
        {
            var sp = Prepare();
            var userRepo = sp.GetRequiredService<IUserRepository>();
            var groupRepo = sp.GetRequiredService<IPermissionGroupRepository>();

            var userId = RandomUser();
            var gId1 = Guid.NewGuid();
            var gId2 = Guid.NewGuid();
            var group1 = RandomPermissionGroup();
            var group2 = RandomPermissionGroup();
            var corp = RandomCorp();
            var app = RandomApp();

            // Validate params
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await userRepo.AssignUserToGroups(null, new[] {new PermissionGroup()}));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await userRepo.AssignUserToGroups(new User(), null));

            await CreateGroup();

            // user not found
            Assert.CatchAsync<EntityNotExistsException>(async () => await userRepo.AssignUserToGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1,
                },
                new PermissionGroup
                {
                    Id = gId2,
                }
            }));

            // create user
            await userRepo.CreateUserAsync(new User
            {
                Id = userId,
                NormalizedId = userId,
            }, new LocalUserInfo
            {
                UserId = userId,
                Corp = corp,
            });
            var user = userRepo.Find(userId);
            Assert.NotNull(user);

            // group not found
            Assert.CatchAsync<EntityNotExistsException>(async () => await userRepo.AssignUserToGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = Guid.NewGuid()
                }
            }));

            // add user to gr 1 success
            await userRepo.AssignUserToGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1
                }
            });
            Assert.AreEqual(1, userRepo.Find(userId).PermissionGroupUsers.Count);

            // add user to gr 1 AGAIN, still success
            await userRepo.AssignUserToGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1
                }
            });
            Assert.AreEqual(1, userRepo.Find(userId).PermissionGroupUsers.Count);

            #region Local methods

            Task CreateGroup()
            {
                return groupRepo.CreateManyAsync(new[]
                {
                    new PermissionGroup
                    {
                        Id = gId1,
                        Name = group1,
                        Corp = corp,
                        App = app,
                    },
                    new PermissionGroup
                    {
                        Id = gId2,
                        Name = group2,
                        Corp = corp,
                        App = app,
                    },
                });
            }

            #endregion
        }

        [Test]
        public async Task UnAssignUserFromGroups()
        {
            var sp = Prepare();
            var userRepo = sp.GetRequiredService<IUserRepository>();
            var groupRepo = sp.GetRequiredService<IPermissionGroupRepository>();

            var userId = RandomUser();
            var gId1 = Guid.NewGuid();
            var gId2 = Guid.NewGuid();
            var group1 = RandomPermissionGroup();
            var group2 = RandomPermissionGroup();
            var corp = RandomCorp();
            var app = RandomApp();

            // Validate params
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await userRepo.UnAssignUserFromGroups(null, new[] {new PermissionGroup()}));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await userRepo.UnAssignUserFromGroups(new User(), null));

            await CreateGroup();

            // user not found
            Assert.CatchAsync<EntityNotExistsException>(async () => await userRepo.UnAssignUserFromGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1,
                },
                new PermissionGroup
                {
                    Id = gId2,
                }
            }));

            // create user
            await userRepo.CreateUserAsync(new User
            {
                Id = userId,
                NormalizedId = userId,
            }, new LocalUserInfo
            {
                UserId = userId,
                Corp = corp,
            });
            var user = userRepo.Find(userId);
            Assert.NotNull(user);

            // group not found
            Assert.CatchAsync<EntityNotExistsException>(async () => await userRepo.UnAssignUserFromGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = Guid.NewGuid()
                }
            }));
            
            // user doesn't belong to any group so no error
            await userRepo.UnAssignUserFromGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1
                }
            });

            // add user to gr 1 success
            await userRepo.AssignUserToGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1
                }
            });
            Assert.AreEqual(1, userRepo.Find(userId).PermissionGroupUsers.Count);

            // Un-assign success
            await userRepo.UnAssignUserFromGroups(new User
            {
                Id = userId
            }, new[]
            {
                new PermissionGroup
                {
                    Id = gId1
                }
            });
            Assert.AreEqual(0, userRepo.Find(userId).PermissionGroupUsers.Count);

            #region Local methods

            Task CreateGroup()
            {
                return groupRepo.CreateManyAsync(new[]
                {
                    new PermissionGroup
                    {
                        Id = gId1,
                        Name = group1,
                        Corp = corp,
                        App = app,
                    },
                    new PermissionGroup
                    {
                        Id = gId2,
                        Name = group2,
                        Corp = corp,
                        App = app,
                    },
                });
            }

            #endregion
        }
    }
}