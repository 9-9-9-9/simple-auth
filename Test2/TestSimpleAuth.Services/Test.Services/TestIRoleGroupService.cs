using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace Test.SimpleAuth.Services.Test.Services
{
    public class TestIRoleGroupService : BaseTestService<IPermissionGroupRepository, PermissionGroup, Guid>
    {
        [Test]
        public void SearchRoleGroups()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IPermissionGroupService>();

            var roleGroups = SetupSearchReturns(null).ToList();
            Assert.NotNull(roleGroups);
            Assert.IsEmpty(roleGroups);

            roleGroups = SetupSearchReturns(new[]
            {
                new PermissionGroup
                {
                    Id = Guid.NewGuid(),
                    Name = RandomRoleGroup(),
                    Corp = RandomCorp(),
                    App = RandomApp(),
                    Locked = true,
                    PermissionRecords = new List<PermissionRecord>
                    {
                        new PermissionRecord(),
                        new PermissionRecord(),
                        new PermissionRecord()
                    }
                },
                new PermissionGroup
                {
                    Id = Guid.NewGuid(),
                    Name = RandomRoleGroup(),
                    Corp = RandomCorp(),
                    App = RandomApp(),
                    Locked = true,
                    PermissionRecords = new List<PermissionRecord>
                    {
                        new PermissionRecord(),
                    }
                },
            }).ToList();

            Assert.AreEqual(2, roleGroups.Count);
            VerifyObject(roleGroups.Skip(0).First(), 3);
            VerifyObject(roleGroups.Skip(1).First(), 1);

            void VerifyObject(global::SimpleAuth.Shared.Domains.PermissionGroup rg, int noOfRoles)
            {
                Assert.NotNull(rg);
                Assert.IsFalse(rg.Name.IsBlank());
                Assert.IsFalse(rg.Corp.IsBlank());
                Assert.IsFalse(rg.App.IsBlank());
                Assert.IsTrue(rg.Locked);
                Assert.AreEqual(noOfRoles, rg.Permissions.Length);
            }

            IEnumerable<global::SimpleAuth.Shared.Domains.PermissionGroup> SetupSearchReturns(IEnumerable<PermissionGroup> rg)
            {
                mockRoleGroupRepo.Setup(x =>
                        x.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FindOptions>()))
                    .Returns(rg);
                return svc.SearchGroups(null, null, null);
            }
        }

        [Test]
        public async Task GetRoleGroupByNameAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IPermissionGroupService>();

            var roleGroup = SetupReturns(null).Result;
            Assert.IsNull(roleGroup);

            roleGroup = SetupReturns(
                new PermissionGroup
                {
                    Id = Guid.NewGuid(),
                    Name = RandomRoleGroup(),
                    Corp = RandomCorp(),
                    App = RandomApp(),
                    Locked = true,
                    PermissionRecords = new List<PermissionRecord>
                    {
                        new PermissionRecord(),
                        new PermissionRecord(),
                        new PermissionRecord()
                    }
                }).Result;

            VerifyObject(roleGroup, 3);

            void VerifyObject(global::SimpleAuth.Shared.Domains.PermissionGroup rg, int noOfRoles)
            {
                Assert.NotNull(rg);
                Assert.IsFalse(rg.Name.IsBlank());
                Assert.IsFalse(rg.Corp.IsBlank());
                Assert.IsFalse(rg.App.IsBlank());
                Assert.IsTrue(rg.Locked);
                Assert.AreEqual(noOfRoles, rg.Permissions.Length);
            }

            Task<global::SimpleAuth.Shared.Domains.PermissionGroup> SetupReturns(PermissionGroup rg)
            {
                mockRoleGroupRepo.Setup(x =>
                        x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                    .ReturnsAsync(rg);
                return svc.GetGroupByNameAsync(null, null, null);
            }

            await Task.CompletedTask;
        }

        [TestCase("c", "a", "g1")]
        [TestCase("c", "a", "g1", "g2")]
        [TestCase("c2", "a2", "g1", "g2", "g3")]
        public void FindByName(string corp, string app, params string[] nameList)
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IPermissionGroupService>();

            mockRoleGroupRepo.Setup(x =>
                x.FindMany(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>(), It.IsAny<FindOptions>())
            ).Returns(nameList.Select(x => new PermissionGroup
            {
                Name = x,
                Corp = corp,
                App = app,
            }));

            var roleGroups = svc.FindByName(nameList, corp, app)?.ToList();

            mockRoleGroupRepo.Verify(m =>
                m.FindMany(It.Is<IEnumerable<Expression<Func<PermissionGroup, bool>>>>(args => args.ToArray().Length == 2),
                    It.Is<FindOptions>(opt => true)));

            Assert.NotNull(roleGroups);
            Assert.AreEqual(nameList.Length, roleGroups.Count);
            Assert.IsTrue(nameList.SequenceEqual(roleGroups.Select(x => x.Name).ToArray()));
            Assert.IsTrue(roleGroups.All(x => x.Corp == corp));
            Assert.IsTrue(roleGroups.All(x => x.App == app));
        }

        [Test]
        public void FindByName_WithoutNameList()
        {
            var corp = RandomCorp();
            var app = RandomApp();

            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IPermissionGroupService>();

            mockRoleGroupRepo.Setup(x =>
                x.FindMany(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>(), It.IsAny<FindOptions>())
            ).Returns(new[] {"1", "2", "3"}.Select(x => new PermissionGroup
            {
                Name = x,
                Corp = corp,
                App = app,
            }));

            var roleGroups = svc.FindByName(null, corp, app)?.ToList();

            mockRoleGroupRepo.Verify(m =>
                m.FindMany(It.Is<IEnumerable<Expression<Func<PermissionGroup, bool>>>>(args => args.ToArray().Length == 1),
                    It.Is<FindOptions>(opt => true)));

            Assert.NotNull(roleGroups);
            Assert.AreEqual(3, roleGroups.Count);
            Assert.IsTrue(roleGroups.All(x => x.Corp == corp));
            Assert.IsTrue(roleGroups.All(x => x.App == app));
        }

        [Test]
        public async Task AddRoleGroupAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IPermissionGroupService>();

            var roleGroup = RandomRoleGroup();
            var corp = RandomCorp();
            var app = RandomApp();

            // If role group already exists, then throwing EntityAlreadyExistsException
            SetupFindReturns(new PermissionGroup());
            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await PerformAdd());

            // If role group is not exists then new group would be created correctly
            // WITHOUT option copy from group
            SetupFindReturns(null);
            mockRoleGroupRepo.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<PermissionGroup>>())).ReturnsAsync(1);
            await PerformAdd();
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(m => m.CreateManyAsync(It.Is<IEnumerable<PermissionGroup>>(rgs =>
                rgs.Count() == 1 && rgs.Any(x =>
                    x.Id != Guid.Empty && x.Name == roleGroup && x.Corp == corp && x.App == app && !x.Locked))));
            // ReSharper restore PossibleMultipleEnumeration

            // If role group is not exists then new group would be created correctly
            // When specify group to be copied from, but group does not exists then EntityNotExistsException is expected
            SetupFindReturns(null);
            SetupFindManyReturns(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await PerformAdd(true));

            // If role group is not exists then new group would be created correctly
            // When specify group to be copied from, AND group exists then should be executed without problem
            SetupFindReturns(null);
            SetupFindManyReturns(new[]
            {
                new PermissionGroup
                {
                    Name = "gr1",
                    PermissionRecords = new List<PermissionRecord>
                    {
                        new PermissionRecord
                        {
                            RoleId = "c.a.e.t11.m"
                        },
                        new PermissionRecord
                        {
                            RoleId = "c.a.e.t12.m"
                        },
                        new PermissionRecord
                        {
                            RoleId = "c.a.e.t13.m"
                        }
                    }
                },
                new PermissionGroup
                {
                    Name = "gr2",
                    PermissionRecords = new List<PermissionRecord>
                    {
                        new PermissionRecord
                        {
                            RoleId = "c.a.e.t21.m"
                        },
                        new PermissionRecord
                        {
                            RoleId = "c.a.e.t22.m"
                        }
                    }
                }
            });
            await PerformAdd(true);
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(m => m.CreateManyAsync(It.Is<IEnumerable<PermissionGroup>>(rgs =>
                rgs.Count() == 1 && rgs.First().PermissionRecords.Count == 5 &&
                rgs.SelectMany(x => x.PermissionRecords).All(x => x.RoleId.EndsWith(".m")))));
            // ReSharper restore PossibleMultipleEnumeration


            #region Local functions

            void SetupFindReturns(PermissionGroup rg) => mockRoleGroupRepo
                .Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>())
                )
                .ReturnsAsync(rg);

            void SetupFindManyReturns(IEnumerable<PermissionGroup> rgs2) => mockRoleGroupRepo
                .Setup(x =>
                    x.FindMany(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>(), It.IsAny<FindOptions>())
                )
                .Returns(rgs2);

            Task PerformAdd(bool specificGroupToBeCopiedFrom = false) => svc.AddGroupAsync(new CreatePermissionGroupModel
            {
                Name = roleGroup,
                Corp = corp,
                App = app,
                CopyFromPermissionGroups = specificGroupToBeCopiedFrom ? new[] {"gr1", "gr2"} : null
            });

            #endregion
        }

        [Test]
        public async Task UpdateLockStatusAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IPermissionGroupService>();

            var corp = RandomCorp();
            var app = RandomApp();
            var roleGroup = RandomRoleGroup();
            var @lock = RandomBool();

            // argument validation
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.UpdateLockStatusAsync(null));

            SetupFindSingleReturns(null);

            // if entity not found then throw EntityNotExistsException
            Assert.CatchAsync<EntityNotExistsException>(async () => await PerformUpdate());

            // if entity found then update correctly (when lock status should be changed)
            SetupFindSingleReturns(new PermissionGroup
            {
                Name = roleGroup,
                Corp = corp,
                App = app,
                Locked = !@lock
            });
            mockRoleGroupRepo.Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<PermissionGroup>>())).ReturnsAsync(1);
            await PerformUpdate();
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(
                m => m.UpdateManyAsync(
                    It.Is<IEnumerable<PermissionGroup>>(rgs => rgs.Count() == 1 && rgs.First().Locked == @lock)
                )
            );
            // ReSharper restore PossibleMultipleEnumeration

            // if entity found, but lock status already the same then don't perform any other execution
            SetupFindSingleReturns(new PermissionGroup
            {
                Name = roleGroup,
                Corp = corp,
                App = app,
                Locked = @lock
            });
            await PerformUpdate();
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.Is<IEnumerable<Expression<Func<PermissionGroup, bool>>>>(args => true)));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            Task PerformUpdate() => svc.UpdateLockStatusAsync(
                new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup,
                    Corp = corp,
                    App = app,
                    Locked = @lock
                });

            void SetupFindSingleReturns(PermissionGroup rg) => mockRoleGroupRepo
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                .ReturnsAsync(rg);
        }

        [Test]
        public async Task AddRolesToGroupAsync()
        {
            var svc = Prepare<IRoleRepository, Role, string>(out var mockRoleGroupRepo, out var mockRoleRepo)
                .GetRequiredService<IPermissionGroupService>();

            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.AddPermissionsToGroupAsync(null, new[]
            {
                new PermissionModel()
            }));
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.AddPermissionsToGroupAsync(rg1, null));
            Assert.CatchAsync<ArgumentException>(async () => await svc.AddPermissionsToGroupAsync(rg1, new PermissionModel[0]));
            Assert.CatchAsync<ArgumentException>(async () => await svc.AddPermissionsToGroupAsync(rg1, new[]
            {
                new PermissionModel(), null
            }));
            // RoleIds must from corp and app of the provided domain role group
            Assert.CatchAsync<SimpleAuthSecurityException>(async () => await svc.AddPermissionsToGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp2}.{app1}.e.t.m"
                }
            }));
            // Domain object should not store value in property Roles to prevent un-expected behavior
            rg1.Permissions = new[] {new global::SimpleAuth.Shared.Domains.Permission()};
            Assert.CatchAsync<InvalidOperationException>(async () => await svc.AddPermissionsToGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m"
                }
            }));
            rg1.Permissions = null;

            // if role not found so throw EntityNotExistsException
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync((Role) null);
            mockRoleGroupRepo
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                .ReturnsAsync(new PermissionGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1,
                });
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.AddPermissionsToGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Verb = Verb.Add.Serialize()
                },
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m2",
                    Verb = Verb.Add.Serialize()
                }
            }));

            // normal without any existing role
            mockRoleGroupRepo.Setup(x => x.UpdatePermissionRecordsAsync(It.IsAny<PermissionGroup>(), It.IsAny<List<PermissionRecord>>()))
                .ReturnsAsync(1);
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync(new Role
                {
                    Env = "e",
                    Tenant = "t"
                });
            await SetupFindSingleReturnsAndThenUpdate();
            mockRoleGroupRepo.Verify(m => m.UpdatePermissionRecordsAsync(It.Is<PermissionGroup>(rg => true),
                It.Is<List<PermissionRecord>>(rrs => rrs.Count == 2 && rrs.All(x =>
                                                   x.Verb != Verb.None && x.Id != Guid.Empty &&
                                                   !x.RoleId.IsBlank() && !x.Env.IsBlank() && !x.Tenant.IsBlank()))));

            // normal with some existing roles
            mockRoleGroupRepo.Setup(x => x.UpdatePermissionRecordsAsync(It.IsAny<PermissionGroup>(), It.IsAny<List<PermissionRecord>>()))
                .ReturnsAsync(1);
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync(new Role
                {
                    Env = "e",
                    Tenant = "t"
                });
            await SetupFindSingleReturnsAndThenUpdate(new List<PermissionRecord>
            {
                new PermissionRecord
                {
                    RoleId = $"{corp1}.{app1}.e.t.m3",
                    Verb = Verb.Edit
                }
            });
            mockRoleGroupRepo.Verify(m => m.UpdatePermissionRecordsAsync(It.Is<PermissionGroup>(rg => true),
                It.Is<List<PermissionRecord>>(rrs => rrs.Count == 2 /*still 2*/)));

            Task SetupFindSingleReturnsAndThenUpdate(
                ICollection<PermissionRecord> existingRoleRecords = null)
            {
                mockRoleGroupRepo
                    .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                    .ReturnsAsync(new PermissionGroup
                    {
                        Name = roleGroup1,
                        Corp = corp1,
                        App = app1,
                        PermissionRecords = existingRoleRecords
                    });

                return svc.AddPermissionsToGroupAsync(new global::SimpleAuth.Shared.Domains.PermissionGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1
                }, new[]
                {
                    new PermissionModel
                    {
                        Role = $"{corp1}.{app1}.e.t.m1",
                        Verb = Verb.Add.Serialize()
                    },
                    new PermissionModel
                    {
                        Role = $"{corp1}.{app1}.e.t.m2",
                        Verb = Verb.Add.Serialize()
                    }
                });
            }
        }

        [Test]
        public async Task DeleteRolesFromGroupAsync()
        {
            var svc = Prepare<IRoleRepository, Role, string>(out var mockRoleGroupRepo, out var mockRoleRepo)
                .GetRequiredService<IPermissionGroupService>();

            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeletePermissionsFromGroupAsync(null, new[]
            {
                new PermissionModel()
            }));
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeletePermissionsFromGroupAsync(rg1, null));
            Assert.CatchAsync<ArgumentException>(async () =>
                await svc.DeletePermissionsFromGroupAsync(rg1, new PermissionModel[0]));
            Assert.CatchAsync<ArgumentException>(async () => await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel(), null
            }));
            // RoleIds must from corp and app of the provided domain role group
            Assert.CatchAsync<SimpleAuthSecurityException>(async () => await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp2}.{app1}.e.t.m"
                }
            }));
            // Domain object should not store value in property Roles to prevent un-expected behavior
            rg1.Permissions = new[] {new global::SimpleAuth.Shared.Domains.Permission()};
            Assert.CatchAsync<InvalidOperationException>(async () => await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m"
                }
            }));
            rg1.Permissions = null;

            // if role group does not exists, so throwing EntityNotExistsException
//TODO

            // if role not found so throw EntityNotExistsException
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync((Role) null);
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                PermissionRecords = new List<PermissionRecord>
                {
                    new PermissionRecord
                    {
                        RoleId = $"{corp1}.{app1}.e.t.m1"
                    }
                }
            });
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Verb = Verb.Add.Serialize()
                },
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m2",
                    Verb = Verb.Add.Serialize()
                }
            }));

            // if role group not found so throw EntityNotExistsException
            SetupFindSingleRoleGroup(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Verb = Verb.Add.Serialize()
                }
            }));

            // if role group does not contains any roles, so stop execution immediately
            ResetMocks();
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Id = Guid.NewGuid(),
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                PermissionRecords = null
            });
            await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Verb = Verb.Add.Serialize()
                }
            });
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()));
            mockRoleGroupRepo.VerifyNoOtherCalls();
            mockRoleRepo.VerifyNoOtherCalls();

            // Normal
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Id = Guid.NewGuid(),
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                PermissionRecords = new List<PermissionRecord>
                {
                    RoleRecord(1, corp1, app1, Verb.Add),
                    RoleRecord(2, corp1, app1, Verb.Add, Verb.Edit),
                    RoleRecord(3, corp1, app1, Verb.Crud),
                }
            });
            SetupFindSingleRoleAsyncReturns(1, out var expectedEnv, out var expectedTenant);
            mockRoleGroupRepo.Setup(x => x.UpdatePermissionRecordsAsync(It.IsAny<PermissionGroup>(), It.IsAny<List<PermissionRecord>>()))
                .ReturnsAsync(1);
            await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Verb = Verb.Add.Serialize()
                }
            });
            mockRoleGroupRepo.Verify(m =>
                m.UpdatePermissionRecordsAsync(It.Is<PermissionGroup>(rg => rg.Name == roleGroup1),
                    It.Is<List<PermissionRecord>>(rrs =>
                        rrs.Count == 3
                        && 1 == rrs.Count(x =>
                            x.RoleId.EndsWith(".m1") && x.Verb == Verb.None && x.Env == expectedEnv &&
                            x.Tenant == expectedTenant
                        )
                    )
                )
            );
            //
            SetupFindSingleRoleAsyncReturns(2, out expectedEnv, out expectedTenant);
            await svc.DeletePermissionsFromGroupAsync(rg1, new[]
            {
                new PermissionModel
                {
                    Role = $"{corp1}.{app1}.e.t.m2",
                    Verb = (Verb.Edit | Verb.Delete).Serialize()
                }
            });
            mockRoleGroupRepo.Verify(m =>
                m.UpdatePermissionRecordsAsync(It.Is<PermissionGroup>(rg => rg.Name == roleGroup1),
                    It.Is<List<PermissionRecord>>(rrs =>
                        rrs.Count == 3
                        && 1 == rrs.Count(x =>
                            x.RoleId.EndsWith(".m2") && x.Verb == Verb.Add && x.Env == expectedEnv &&
                            x.Tenant == expectedTenant
                        )
                    )
                )
            );

            #region Local methods

            void SetupFindSingleRoleGroup(PermissionGroup rg) => mockRoleGroupRepo.Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                .ReturnsAsync(rg);

            void ResetMocks()
            {
                mockRoleGroupRepo.Reset();
                mockRoleRepo.Reset();
            }

            PermissionRecord RoleRecord(int moduleNo, string corp, string app, params Verb[] permissions)
            {
                return new PermissionRecord
                {
                    Id = Guid.NewGuid(),
                    RoleId = $"{corp}.{app}.e.t.m{moduleNo}",
                    Verb = Verb.None.Grant(permissions),
                    Env = "e",
                    Tenant = "t"
                };
            }

            void SetupFindSingleRoleAsyncReturns(int moduleNo, out string randomEnv, out string randomTenant)
            {
                randomEnv = RandomEnv();
                randomTenant = RandomTenant();
                var env = randomEnv;
                var tenant = randomTenant;
                mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                    .ReturnsAsync(new Role
                    {
                        Id = $"{corp1}.{app1}.e.t.m{moduleNo}",
                        Env = env,
                        Tenant = tenant
                    });
            }

            #endregion
        }

        [Test]
        public async Task DeleteAllRolesFromGroupAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo)
                .GetRequiredService<IPermissionGroupService>();

            var corp1 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeleteAllPermissionsFromGroupAsync(null));

            // if role group does not exists, so throwing EntityNotExistsException
            SetupFindSingleRoleGroup(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeleteAllPermissionsFromGroupAsync(rg1));

            // if role group does not have any RoleRecord, so stop execution, nothing more to do
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                PermissionRecords = null
            });
            await svc.DeleteAllPermissionsFromGroupAsync(rg1);
            mockRoleGroupRepo.Verify(x =>
                x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            // if role group contains RoleRecord(s), check if Repository.UpdateRoleRecordsAsync(RoleGroup roleGroup, List<RoleRecord> newRoles) receives an empty `newRoles`
            mockRoleGroupRepo.Reset();
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                PermissionRecords = new List<PermissionRecord>
                {
                    new PermissionRecord()
                }
            });
            mockRoleGroupRepo.Setup(x => x.UpdatePermissionRecordsAsync(It.IsAny<PermissionGroup>(), It.IsAny<List<PermissionRecord>>()))
                .ReturnsAsync(1);
            await svc.DeleteAllPermissionsFromGroupAsync(rg1);
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()));
            mockRoleGroupRepo.Verify(m => m.UpdatePermissionRecordsAsync(It.Is<PermissionGroup>(rg => rg.Name == roleGroup1), It.Is<List<PermissionRecord>>(rrs => !rrs.IsAny())));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            #region Local methods

            void SetupFindSingleRoleGroup(PermissionGroup rg) => mockRoleGroupRepo.Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                .ReturnsAsync(rg);

            #endregion
        }

        [Test]
        public async Task DeleteRoleGroupAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo)
                .GetRequiredService<IPermissionGroupService>();

            var corp1 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeleteGroupAsync(null));

            // if role group does not exists, so throwing EntityNotExistsException
            SetupFindSingleRoleGroup(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeleteGroupAsync(rg1));

            // Normal
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            });
            mockRoleGroupRepo.Setup(x => x.DeleteManyAsync(It.IsAny<IEnumerable<PermissionGroup>>())).ReturnsAsync(1);
            await svc.DeleteGroupAsync(rg1);
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()));
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(m => m.DeleteManyAsync(It.Is<IEnumerable<PermissionGroup>>(rgs => rgs.Count() == 1 && rgs.Count(x => x.Name == rg1.Name && x.Corp == rg1.Corp && x.App == rg1.App) == 1)));
            // ReSharper restore PossibleMultipleEnumeration
            mockRoleGroupRepo.VerifyNoOtherCalls();

            // if role group contains RoleRecord(s), check if Repository.UpdateRoleRecordsAsync(RoleGroup roleGroup, List<RoleRecord> newRoles) receives an empty `newRoles`
            mockRoleGroupRepo.Reset();
            SetupFindSingleRoleGroup(new PermissionGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                PermissionRecords = new List<PermissionRecord>
                {
                    new PermissionRecord()
                }
            });
            mockRoleGroupRepo.Setup(x => x.UpdatePermissionRecordsAsync(It.IsAny<PermissionGroup>(), It.IsAny<List<PermissionRecord>>()))
                .ReturnsAsync(1);
            await svc.DeleteAllPermissionsFromGroupAsync(rg1);
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()));
            mockRoleGroupRepo.Verify(m => m.UpdatePermissionRecordsAsync(It.Is<PermissionGroup>(rg => rg.Name == roleGroup1), It.Is<List<PermissionRecord>>(rrs => !rrs.IsAny())));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            #region Local methods

            void SetupFindSingleRoleGroup(PermissionGroup rg) => mockRoleGroupRepo.Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<PermissionGroup, bool>>>>()))
                .ReturnsAsync(rg);

            #endregion
        }
    }
}