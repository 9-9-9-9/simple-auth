using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;

namespace Test.Integration.Repositories
{
    public class TestIPermissionGroupRepositories : BaseTestRepo
    {
        [Test]
        public async Task Search()
        {
            var (repo, corp) = await GenerateGroupsAsync();

            Assert.Catch<ArgumentNullException>(() => repo.Search("any", corp, null));
            Assert.Catch<ArgumentNullException>(() => repo.Search("any", null, "a1"));

            var groups = repo.Search("g", corp, "a1");
            Assert.AreEqual(2, groups.Count());
            groups = repo.Search("g11", corp, "a1");
            Assert.AreEqual(1, groups.Count());
            groups = repo.Search("*", corp, "a1");
            Assert.AreEqual(2, groups.Count());
            groups = repo.Search(null, corp, "a1");
            Assert.AreEqual(2, groups.Count());

            // should not contains Locked
            groups = repo.Search("g", corp, "a2");
            Assert.IsFalse(groups.IsAny());
        }

        [TestCase("g", "a1", ExpectedResult = 2)]
        [TestCase("g11", "a1", ExpectedResult = 1)]
        [TestCase("*", "a1", ExpectedResult = 2)]
        [TestCase(null, "a1", ExpectedResult = 2)]
        [TestCase(null, "a2", ExpectedResult = 0)]
        public async Task<int> Search(string term, string app)
        {
            var (repo, corp) = await GenerateGroupsAsync();
            return repo.Search(term, corp, app).OrEmpty().Count();
        }

        [Test]
        public async Task UpdatePermissionRecordsAsync()
        {
            var sp = Prepare();
            var permissionGroupRepository = sp.GetRequiredService<IPermissionGroupRepository>();
            var roleRepository = sp.GetRequiredService<IRoleRepository>();
            var corp = RandomCorp();

            var groupId = Guid.NewGuid();

            await permissionGroupRepository.CreateAsync(new PermissionGroup
            {
                Id = groupId,
                Corp = corp,
                App = "a1",
                Name = "g11",
                Locked = false,
            });

            await CreateRoles(1, 2, 3);

            //
            await permissionGroupRepository.UpdatePermissionRecordsAsync(new PermissionGroup
            {
                Id = groupId
            }, new List<PermissionRecord>
            {
                new PermissionRecord
                {
                    RoleId = $"{corp}.a1.e.t.m1",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Add | Verb.Edit
                }
            });
            Assert.AreEqual(1, GetGroup().PermissionRecords.Count());

            // this method should wipe all old records
            await permissionGroupRepository.UpdatePermissionRecordsAsync(new PermissionGroup
            {
                Id = groupId
            }, new List<PermissionRecord>
            {
                new PermissionRecord
                {
                    RoleId = $"{corp}.a1.e.t.m2",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Add | Verb.Edit
                },
                new PermissionRecord
                {
                    RoleId = $"{corp}.a1.e.t.m3",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Add | Verb.Edit
                }
            });
            Assert.AreEqual(2, GetGroup().PermissionRecords.Count());

            // this method override old permission if old exists
            await permissionGroupRepository.UpdatePermissionRecordsAsync(new PermissionGroup
            {
                Id = groupId
            }, new List<PermissionRecord>
            {
                new PermissionRecord
                {
                    RoleId = $"{corp}.a1.e.t.m2",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Delete
                }
            });
            Assert.AreEqual(Verb.Delete,
                GetGroup().PermissionRecords.First(x => x.RoleId == $"{corp}.a1.e.t.m2").Verb);

            Task CreateRoles(params int[] ids)
            {
                return roleRepository.CreateManyAsync(ids.Select(x =>
                        new Role
                        {
                            Corp = corp,
                            App = "a1",
                            Env = "e",
                            Tenant = "t",
                            Module = $"m{x}"
                        }.ComputeId()
                    )
                );
            }

            PermissionGroup GetGroup() => permissionGroupRepository.Find(groupId);
        }

        [Test]
        public async Task DeleteManyAsync()
        {
            var sp = Prepare();
            var permissionGroupRepository = sp.GetRequiredService<IPermissionGroupRepository>();
            var roleRepository = sp.GetRequiredService<IRoleRepository>();
            var permissionRecordRepository = sp.GetRequiredService<IPermissionRecordRepository>();
            var userRepository = sp.GetRequiredService<IUserRepository>();

            var corp = RandomCorp();
            var env = RandomEnv();
            var tenant = RandomTenant();
            var groupId = Guid.NewGuid();
            var userId = RandomUser();

            await permissionGroupRepository.CreateAsync(new PermissionGroup
            {
                Id = groupId,
                Corp = corp,
                App = "a1",
                Name = "g11",
                Locked = false,
            });

            await CreateRoles(1, 2, 3);
            await AddRoles(1, 2, 3);

            // delete groups which does not exists
            Assert.CatchAsync<EntityNotExistsException>(async () => await permissionGroupRepository.DeleteManyAsync(new[]
            {
                new PermissionGroup
                {
                    Id = Guid.NewGuid()
                },
                new PermissionGroup
                {
                    Id = Guid.NewGuid()
                }
            }));

            // groups which are being used by any user should not be deleted
            await AddGroupToUser();
            Assert.CatchAsync<SimpleAuthException>(async () => await permissionGroupRepository.DeleteAsync(new PermissionGroup
            {
                Id = groupId
            }));
            await RemoveUserFromGroup();

            // should wipe all related permission records
            Assert.AreEqual(3, GetGroup().PermissionRecords.Count);
            await permissionGroupRepository.DeleteAsync(new PermissionGroup
            {
                Id = groupId
            });
            Assert.IsNull(GetGroup());
            Assert.IsEmpty(permissionRecordRepository.Find(x => x.Env == env));


            #region Local functions

            Task CreateRoles(params int[] ids)
            {
                return roleRepository.CreateManyAsync(ids.Select(x =>
                        new Role
                        {
                            Corp = corp,
                            App = "a1",
                            Env = env,
                            Tenant = tenant,
                            Module = $"m{x}"
                        }.ComputeId()
                    )
                );
            }

            PermissionGroup GetGroup() => permissionGroupRepository.Find(groupId);

            Task AddRoles(params int[] ids)
            {
                return permissionGroupRepository.UpdatePermissionRecordsAsync(new PermissionGroup
                {
                    Id = groupId
                }, ids.Select(x =>
                    new PermissionRecord
                    {
                        RoleId = $"{corp}.a1.{env}.{tenant}.m{x}",
                        Env = env,
                        Tenant = tenant,
                        Verb = Verb.Add
                    }
                ).ToList());
            }

            Task AddGroupToUser()
            {
                return userRepository.CreateAsync(new User
                {
                    Id = userId,
                    NormalizedId = userId,
                }).ContinueWith(async (x) =>
                {
                    await x;
                    return userRepository.CreateUserAsync(new User
                    {
                        Id = userId,
                    }, new LocalUserInfo
                    {
                        Corp = corp,
                        UserId = userId
                    }.WithRandomId());
                }).ContinueWith(async (x) =>
                {
                    await x;
                    return userRepository.AssignUserToGroups(new User
                    {
                        Id = userId
                    }, new[]
                    {
                        new PermissionGroup
                        {
                            Corp = corp,
                            App = "a1",
                            Id = groupId,
                            Name = "g11"
                        }
                    });
                });
            }

            Task RemoveUserFromGroup()
            {
                return userRepository.UnAssignUserFromGroups(new User
                {
                    Id = userId
                }, new[]
                {
                    new PermissionGroup
                    {
                        Id = groupId,
                        Name = "g11",
                        Corp = corp,
                        App = "a1"
                    }
                });
            }

            #endregion
        }
    }
}