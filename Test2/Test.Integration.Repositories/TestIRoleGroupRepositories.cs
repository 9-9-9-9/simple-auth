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
    public class TestIRoleGroupRepositories : BaseTestRepo
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
        public async Task UpdateRoleRecordsAsync()
        {
            var sp = Prepare();
            var roleGroupRepository = sp.GetRequiredService<IRoleGroupRepository>();
            var roleRepository = sp.GetRequiredService<IRoleRepository>();
            var corp = RandomCorp();

            var roleGroupId = Guid.NewGuid();

            await roleGroupRepository.CreateAsync(new RoleGroup
            {
                Id = roleGroupId,
                Corp = corp,
                App = "a1",
                Name = "g11",
                Locked = false,
            });

            await CreateRoles(1, 2, 3);

            //
            await roleGroupRepository.UpdateRoleRecordsAsync(new RoleGroup
            {
                Id = roleGroupId
            }, new List<RoleRecord>
            {
                new RoleRecord
                {
                    RoleId = $"{corp}.a1.e.t.m1",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Add | Verb.Edit
                }
            });
            Assert.AreEqual(1, GetGroup().RoleRecords.Count());

            // this method should wipe all old records
            await roleGroupRepository.UpdateRoleRecordsAsync(new RoleGroup
            {
                Id = roleGroupId
            }, new List<RoleRecord>
            {
                new RoleRecord
                {
                    RoleId = $"{corp}.a1.e.t.m2",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Add | Verb.Edit
                },
                new RoleRecord
                {
                    RoleId = $"{corp}.a1.e.t.m3",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Add | Verb.Edit
                }
            });
            Assert.AreEqual(2, GetGroup().RoleRecords.Count());

            // this method override old permission if old exists
            await roleGroupRepository.UpdateRoleRecordsAsync(new RoleGroup
            {
                Id = roleGroupId
            }, new List<RoleRecord>
            {
                new RoleRecord
                {
                    RoleId = $"{corp}.a1.e.t.m2",
                    Env = "e",
                    Tenant = "t",
                    Verb = Verb.Delete
                }
            });
            Assert.AreEqual(Verb.Delete,
                GetGroup().RoleRecords.First(x => x.RoleId == $"{corp}.a1.e.t.m2").Verb);

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

            RoleGroup GetGroup() => roleGroupRepository.Find(roleGroupId);
        }

        [Test]
        public async Task DeleteManyAsync()
        {
            var sp = Prepare();
            var roleGroupRepository = sp.GetRequiredService<IRoleGroupRepository>();
            var roleRepository = sp.GetRequiredService<IRoleRepository>();
            var roleRecordRepository = sp.GetRequiredService<IRoleRecordRepository>();
            var userRepository = sp.GetRequiredService<IUserRepository>();

            var corp = RandomCorp();
            var env = RandomEnv();
            var tenant = RandomTenant();
            var roleGroupId = Guid.NewGuid();
            var userId = RandomUser();

            await roleGroupRepository.CreateAsync(new RoleGroup
            {
                Id = roleGroupId,
                Corp = corp,
                App = "a1",
                Name = "g11",
                Locked = false,
            });

            await CreateRoles(1, 2, 3);
            await AddRoles(1, 2, 3);

            // delete groups which does not exists
            Assert.CatchAsync<EntityNotExistsException>(async () => await roleGroupRepository.DeleteManyAsync(new[]
            {
                new RoleGroup
                {
                    Id = Guid.NewGuid()
                },
                new RoleGroup
                {
                    Id = Guid.NewGuid()
                }
            }));

            // groups which are being used by any user should not be deleted
            await AddGroupToUser();
            Assert.CatchAsync<SimpleAuthException>(async () => await roleGroupRepository.DeleteAsync(new RoleGroup
            {
                Id = roleGroupId
            }));
            await RemoveUserFromGroup();

            // should wipe all related role records
            Assert.AreEqual(3, GetGroup().RoleRecords.Count);
            await roleGroupRepository.DeleteAsync(new RoleGroup
            {
                Id = roleGroupId
            });
            Assert.IsNull(GetGroup());
            Assert.IsEmpty(roleRecordRepository.Find(x => x.Env == env));


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

            RoleGroup GetGroup() => roleGroupRepository.Find(roleGroupId);

            Task AddRoles(params int[] ids)
            {
                return roleGroupRepository.UpdateRoleRecordsAsync(new RoleGroup
                {
                    Id = roleGroupId
                }, ids.Select(x =>
                    new RoleRecord
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
                        new RoleGroup
                        {
                            Corp = corp,
                            App = "a1",
                            Id = roleGroupId,
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
                    new RoleGroup
                    {
                        Id = roleGroupId,
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