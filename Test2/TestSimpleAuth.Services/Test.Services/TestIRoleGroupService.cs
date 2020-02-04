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
    public class TestIRoleGroupService : BaseTestService<IRoleGroupRepository, RoleGroup, Guid>
    {
        [Test]
        public void SearchRoleGroups()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IRoleGroupService>();

            var roleGroups = SetupSearchReturns(null).ToList();
            Assert.NotNull(roleGroups);
            Assert.IsEmpty(roleGroups);

            roleGroups = SetupSearchReturns(new[]
            {
                new RoleGroup
                {
                    Id = Guid.NewGuid(),
                    Name = RandomRoleGroup(),
                    Corp = RandomCorp(),
                    App = RandomApp(),
                    Locked = true,
                    RoleRecords = new List<RoleRecord>
                    {
                        new RoleRecord(),
                        new RoleRecord(),
                        new RoleRecord()
                    }
                },
                new RoleGroup
                {
                    Id = Guid.NewGuid(),
                    Name = RandomRoleGroup(),
                    Corp = RandomCorp(),
                    App = RandomApp(),
                    Locked = true,
                    RoleRecords = new List<RoleRecord>
                    {
                        new RoleRecord(),
                    }
                },
            }).ToList();

            Assert.AreEqual(2, roleGroups.Count);
            VerifyObject(roleGroups.Skip(0).First(), 3);
            VerifyObject(roleGroups.Skip(1).First(), 1);

            void VerifyObject(global::SimpleAuth.Shared.Domains.RoleGroup rg, int noOfRoles)
            {
                Assert.NotNull(rg);
                Assert.IsFalse(rg.Name.IsBlank());
                Assert.IsFalse(rg.Corp.IsBlank());
                Assert.IsFalse(rg.App.IsBlank());
                Assert.IsTrue(rg.Locked);
                Assert.AreEqual(noOfRoles, rg.Roles.Length);
            }

            IEnumerable<global::SimpleAuth.Shared.Domains.RoleGroup> SetupSearchReturns(IEnumerable<RoleGroup> rg)
            {
                mockRoleGroupRepo.Setup(x =>
                        x.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FindOptions>()))
                    .Returns(rg);
                return svc.SearchRoleGroups(null, null, null);
            }
        }

        [Test]
        public async Task GetRoleGroupByNameAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IRoleGroupService>();

            var roleGroup = SetupReturns(null).Result;
            Assert.IsNull(roleGroup);

            roleGroup = SetupReturns(
                new RoleGroup
                {
                    Id = Guid.NewGuid(),
                    Name = RandomRoleGroup(),
                    Corp = RandomCorp(),
                    App = RandomApp(),
                    Locked = true,
                    RoleRecords = new List<RoleRecord>
                    {
                        new RoleRecord(),
                        new RoleRecord(),
                        new RoleRecord()
                    }
                }).Result;

            VerifyObject(roleGroup, 3);

            void VerifyObject(global::SimpleAuth.Shared.Domains.RoleGroup rg, int noOfRoles)
            {
                Assert.NotNull(rg);
                Assert.IsFalse(rg.Name.IsBlank());
                Assert.IsFalse(rg.Corp.IsBlank());
                Assert.IsFalse(rg.App.IsBlank());
                Assert.IsTrue(rg.Locked);
                Assert.AreEqual(noOfRoles, rg.Roles.Length);
            }

            Task<global::SimpleAuth.Shared.Domains.RoleGroup> SetupReturns(RoleGroup rg)
            {
                mockRoleGroupRepo.Setup(x =>
                        x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                    .ReturnsAsync(rg);
                return svc.GetRoleGroupByNameAsync(null, null, null);
            }

            await Task.CompletedTask;
        }

        [TestCase("c", "a", "g1")]
        [TestCase("c", "a", "g1", "g2")]
        [TestCase("c2", "a2", "g1", "g2", "g3")]
        public void FindByName(string corp, string app, params string[] nameList)
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IRoleGroupService>();

            mockRoleGroupRepo.Setup(x =>
                x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>())
            ).Returns(nameList.Select(x => new RoleGroup
            {
                Name = x,
                Corp = corp,
                App = app,
            }));

            var roleGroups = svc.FindByName(nameList, corp, app)?.ToList();

            mockRoleGroupRepo.Verify(m =>
                m.FindMany(It.Is<IEnumerable<Expression<Func<RoleGroup, bool>>>>(args => args.ToArray().Length == 2),
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

            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IRoleGroupService>();

            mockRoleGroupRepo.Setup(x =>
                x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>())
            ).Returns(new[] {"1", "2", "3"}.Select(x => new RoleGroup
            {
                Name = x,
                Corp = corp,
                App = app,
            }));

            var roleGroups = svc.FindByName(null, corp, app)?.ToList();

            mockRoleGroupRepo.Verify(m =>
                m.FindMany(It.Is<IEnumerable<Expression<Func<RoleGroup, bool>>>>(args => args.ToArray().Length == 1),
                    It.Is<FindOptions>(opt => true)));

            Assert.NotNull(roleGroups);
            Assert.AreEqual(3, roleGroups.Count);
            Assert.IsTrue(roleGroups.All(x => x.Corp == corp));
            Assert.IsTrue(roleGroups.All(x => x.App == app));
        }

        [Test]
        public async Task AddRoleGroupAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IRoleGroupService>();

            var roleGroup = RandomRoleGroup();
            var corp = RandomCorp();
            var app = RandomApp();

            // If role group already exists, then throwing EntityAlreadyExistsException
            SetupFindReturns(new RoleGroup());
            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await PerformAdd());

            // If role group is not exists then new group would be created correctly
            // WITHOUT option copy from group
            SetupFindReturns(null);
            mockRoleGroupRepo.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<RoleGroup>>())).ReturnsAsync(1);
            await PerformAdd();
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(m => m.CreateManyAsync(It.Is<IEnumerable<RoleGroup>>(rgs =>
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
                new RoleGroup
                {
                    Name = "gr1",
                    RoleRecords = new List<RoleRecord>
                    {
                        new RoleRecord
                        {
                            RoleId = "c.a.e.t11.m"
                        },
                        new RoleRecord
                        {
                            RoleId = "c.a.e.t12.m"
                        },
                        new RoleRecord
                        {
                            RoleId = "c.a.e.t13.m"
                        }
                    }
                },
                new RoleGroup
                {
                    Name = "gr2",
                    RoleRecords = new List<RoleRecord>
                    {
                        new RoleRecord
                        {
                            RoleId = "c.a.e.t21.m"
                        },
                        new RoleRecord
                        {
                            RoleId = "c.a.e.t22.m"
                        }
                    }
                }
            });
            await PerformAdd(true);
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(m => m.CreateManyAsync(It.Is<IEnumerable<RoleGroup>>(rgs =>
                rgs.Count() == 1 && rgs.First().RoleRecords.Count == 5 &&
                rgs.SelectMany(x => x.RoleRecords).All(x => x.RoleId.EndsWith(".m")))));
            // ReSharper restore PossibleMultipleEnumeration


            #region Local functions

            void SetupFindReturns(RoleGroup rg) => mockRoleGroupRepo
                .Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>())
                )
                .ReturnsAsync(rg);

            void SetupFindManyReturns(IEnumerable<RoleGroup> rgs2) => mockRoleGroupRepo
                .Setup(x =>
                    x.FindMany(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>(), It.IsAny<FindOptions>())
                )
                .Returns(rgs2);

            Task PerformAdd(bool specificGroupToBeCopiedFrom = false) => svc.AddRoleGroupAsync(new CreateRoleGroupModel
            {
                Name = roleGroup,
                Corp = corp,
                App = app,
                CopyFromRoleGroups = specificGroupToBeCopiedFrom ? new[] {"gr1", "gr2"} : null
            });

            #endregion
        }

        [Test]
        public async Task UpdateLockStatusAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo).GetRequiredService<IRoleGroupService>();

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
            SetupFindSingleReturns(new RoleGroup
            {
                Name = roleGroup,
                Corp = corp,
                App = app,
                Locked = !@lock
            });
            mockRoleGroupRepo.Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<RoleGroup>>())).ReturnsAsync(1);
            await PerformUpdate();
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(
                m => m.UpdateManyAsync(
                    It.Is<IEnumerable<RoleGroup>>(rgs => rgs.Count() == 1 && rgs.First().Locked == @lock)
                )
            );
            // ReSharper restore PossibleMultipleEnumeration

            // if entity found, but lock status already the same then don't perform any other execution
            SetupFindSingleReturns(new RoleGroup
            {
                Name = roleGroup,
                Corp = corp,
                App = app,
                Locked = @lock
            });
            await PerformUpdate();
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.Is<IEnumerable<Expression<Func<RoleGroup, bool>>>>(args => true)));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            Task PerformUpdate() => svc.UpdateLockStatusAsync(
                new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Name = roleGroup,
                    Corp = corp,
                    App = app,
                    Locked = @lock
                });

            void SetupFindSingleReturns(RoleGroup rg) => mockRoleGroupRepo
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                .ReturnsAsync(rg);
        }

        [Test]
        public async Task AddRolesToGroupAsync()
        {
            var svc = Prepare<IRoleRepository, Role, string>(out var mockRoleGroupRepo, out var mockRoleRepo)
                .GetRequiredService<IRoleGroupService>();

            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.AddRolesToGroupAsync(null, new[]
            {
                new RoleModel()
            }));
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.AddRolesToGroupAsync(rg1, null));
            Assert.CatchAsync<ArgumentException>(async () => await svc.AddRolesToGroupAsync(rg1, new RoleModel[0]));
            Assert.CatchAsync<ArgumentException>(async () => await svc.AddRolesToGroupAsync(rg1, new[]
            {
                new RoleModel(), null
            }));
            // RoleIds must from corp and app of the provided domain role group
            Assert.CatchAsync<SimpleAuthSecurityException>(async () => await svc.AddRolesToGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp2}.{app1}.e.t.m"
                }
            }));
            // Domain object should not store value in property Roles to prevent un-expected behavior
            rg1.Roles = new[] {new global::SimpleAuth.Shared.Domains.Role()};
            Assert.CatchAsync<InvalidOperationException>(async () => await svc.AddRolesToGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m"
                }
            }));
            rg1.Roles = null;

            // if role not found so throw EntityNotExistsException
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync((Role) null);
            mockRoleGroupRepo
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                .ReturnsAsync(new RoleGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1,
                });
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.AddRolesToGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Permission = Permission.Add.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m2",
                    Permission = Permission.Add.Serialize()
                }
            }));

            // normal without any existing role
            mockRoleGroupRepo.Setup(x => x.UpdateRoleRecordsAsync(It.IsAny<RoleGroup>(), It.IsAny<List<RoleRecord>>()))
                .ReturnsAsync(1);
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync(new Role
                {
                    Env = "e",
                    Tenant = "t"
                });
            await SetupFindSingleReturnsAndThenUpdate();
            mockRoleGroupRepo.Verify(m => m.UpdateRoleRecordsAsync(It.Is<RoleGroup>(rg => true),
                It.Is<List<RoleRecord>>(rrs => rrs.Count == 2 && rrs.All(x =>
                                                   x.Permission != Permission.None && x.Id != Guid.Empty &&
                                                   !x.RoleId.IsBlank() && !x.Env.IsBlank() && !x.Tenant.IsBlank()))));

            // normal with some existing roles
            mockRoleGroupRepo.Setup(x => x.UpdateRoleRecordsAsync(It.IsAny<RoleGroup>(), It.IsAny<List<RoleRecord>>()))
                .ReturnsAsync(1);
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync(new Role
                {
                    Env = "e",
                    Tenant = "t"
                });
            await SetupFindSingleReturnsAndThenUpdate(new List<RoleRecord>
            {
                new RoleRecord
                {
                    RoleId = $"{corp1}.{app1}.e.t.m3",
                    Permission = Permission.Edit
                }
            });
            mockRoleGroupRepo.Verify(m => m.UpdateRoleRecordsAsync(It.Is<RoleGroup>(rg => true),
                It.Is<List<RoleRecord>>(rrs => rrs.Count == 2 /*still 2*/)));

            Task SetupFindSingleReturnsAndThenUpdate(
                ICollection<RoleRecord> existingRoleRecords = null)
            {
                mockRoleGroupRepo
                    .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                    .ReturnsAsync(new RoleGroup
                    {
                        Name = roleGroup1,
                        Corp = corp1,
                        App = app1,
                        RoleRecords = existingRoleRecords
                    });

                return svc.AddRolesToGroupAsync(new global::SimpleAuth.Shared.Domains.RoleGroup
                {
                    Name = roleGroup1,
                    Corp = corp1,
                    App = app1
                }, new[]
                {
                    new RoleModel
                    {
                        Role = $"{corp1}.{app1}.e.t.m1",
                        Permission = Permission.Add.Serialize()
                    },
                    new RoleModel
                    {
                        Role = $"{corp1}.{app1}.e.t.m2",
                        Permission = Permission.Add.Serialize()
                    }
                });
            }
        }

        [Test]
        public async Task DeleteRolesFromGroupAsync()
        {
            var svc = Prepare<IRoleRepository, Role, string>(out var mockRoleGroupRepo, out var mockRoleRepo)
                .GetRequiredService<IRoleGroupService>();

            var corp1 = RandomCorp();
            var corp2 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeleteRolesFromGroupAsync(null, new[]
            {
                new RoleModel()
            }));
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeleteRolesFromGroupAsync(rg1, null));
            Assert.CatchAsync<ArgumentException>(async () =>
                await svc.DeleteRolesFromGroupAsync(rg1, new RoleModel[0]));
            Assert.CatchAsync<ArgumentException>(async () => await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel(), null
            }));
            // RoleIds must from corp and app of the provided domain role group
            Assert.CatchAsync<SimpleAuthSecurityException>(async () => await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp2}.{app1}.e.t.m"
                }
            }));
            // Domain object should not store value in property Roles to prevent un-expected behavior
            rg1.Roles = new[] {new global::SimpleAuth.Shared.Domains.Role()};
            Assert.CatchAsync<InvalidOperationException>(async () => await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m"
                }
            }));
            rg1.Roles = null;

            // if role group does not exists, so throwing EntityNotExistsException
//TODO

            // if role not found so throw EntityNotExistsException
            mockRoleRepo.Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()))
                .ReturnsAsync((Role) null);
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                RoleRecords = new List<RoleRecord>
                {
                    new RoleRecord
                    {
                        RoleId = $"{corp1}.{app1}.e.t.m1"
                    }
                }
            });
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Permission = Permission.Add.Serialize()
                },
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m2",
                    Permission = Permission.Add.Serialize()
                }
            }));

            // if role group not found so throw EntityNotExistsException
            SetupFindSingleRoleGroup(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Permission = Permission.Add.Serialize()
                }
            }));

            // if role group does not contains any roles, so stop execution immediately
            ResetMocks();
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Id = Guid.NewGuid(),
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                RoleRecords = null
            });
            await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Permission = Permission.Add.Serialize()
                }
            });
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()));
            mockRoleGroupRepo.VerifyNoOtherCalls();
            mockRoleRepo.VerifyNoOtherCalls();

            // Normal
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Id = Guid.NewGuid(),
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                RoleRecords = new List<RoleRecord>
                {
                    RoleRecord(1, corp1, app1, Permission.Add),
                    RoleRecord(2, corp1, app1, Permission.Add, Permission.Edit),
                    RoleRecord(3, corp1, app1, Permission.Crud),
                }
            });
            SetupFindSingleRoleAsyncReturns(1, out var expectedEnv, out var expectedTenant);
            mockRoleGroupRepo.Setup(x => x.UpdateRoleRecordsAsync(It.IsAny<RoleGroup>(), It.IsAny<List<RoleRecord>>()))
                .ReturnsAsync(1);
            await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m1",
                    Permission = Permission.Add.Serialize()
                }
            });
            mockRoleGroupRepo.Verify(m =>
                m.UpdateRoleRecordsAsync(It.Is<RoleGroup>(rg => rg.Name == roleGroup1),
                    It.Is<List<RoleRecord>>(rrs =>
                        rrs.Count == 3
                        && 1 == rrs.Count(x =>
                            x.RoleId.EndsWith(".m1") && x.Permission == Permission.None && x.Env == expectedEnv &&
                            x.Tenant == expectedTenant
                        )
                    )
                )
            );
            //
            SetupFindSingleRoleAsyncReturns(2, out expectedEnv, out expectedTenant);
            await svc.DeleteRolesFromGroupAsync(rg1, new[]
            {
                new RoleModel
                {
                    Role = $"{corp1}.{app1}.e.t.m2",
                    Permission = (Permission.Edit | Permission.Delete).Serialize()
                }
            });
            mockRoleGroupRepo.Verify(m =>
                m.UpdateRoleRecordsAsync(It.Is<RoleGroup>(rg => rg.Name == roleGroup1),
                    It.Is<List<RoleRecord>>(rrs =>
                        rrs.Count == 3
                        && 1 == rrs.Count(x =>
                            x.RoleId.EndsWith(".m2") && x.Permission == Permission.Add && x.Env == expectedEnv &&
                            x.Tenant == expectedTenant
                        )
                    )
                )
            );

            #region Local methods

            void SetupFindSingleRoleGroup(RoleGroup rg) => mockRoleGroupRepo.Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                .ReturnsAsync(rg);

            void ResetMocks()
            {
                mockRoleGroupRepo.Reset();
                mockRoleRepo.Reset();
            }

            RoleRecord RoleRecord(int moduleNo, string corp, string app, params Permission[] permissions)
            {
                return new RoleRecord
                {
                    Id = Guid.NewGuid(),
                    RoleId = $"{corp}.{app}.e.t.m{moduleNo}",
                    Permission = Permission.None.Grant(permissions),
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
                .GetRequiredService<IRoleGroupService>();

            var corp1 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeleteAllRolesFromGroupAsync(null));

            // if role group does not exists, so throwing EntityNotExistsException
            SetupFindSingleRoleGroup(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeleteAllRolesFromGroupAsync(rg1));

            // if role group does not have any RoleRecord, so stop execution, nothing more to do
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                RoleRecords = null
            });
            await svc.DeleteAllRolesFromGroupAsync(rg1);
            mockRoleGroupRepo.Verify(x =>
                x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            // if role group contains RoleRecord(s), check if Repository.UpdateRoleRecordsAsync(RoleGroup roleGroup, List<RoleRecord> newRoles) receives an empty `newRoles`
            mockRoleGroupRepo.Reset();
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                RoleRecords = new List<RoleRecord>
                {
                    new RoleRecord()
                }
            });
            mockRoleGroupRepo.Setup(x => x.UpdateRoleRecordsAsync(It.IsAny<RoleGroup>(), It.IsAny<List<RoleRecord>>()))
                .ReturnsAsync(1);
            await svc.DeleteAllRolesFromGroupAsync(rg1);
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()));
            mockRoleGroupRepo.Verify(m => m.UpdateRoleRecordsAsync(It.Is<RoleGroup>(rg => rg.Name == roleGroup1), It.Is<List<RoleRecord>>(rrs => !rrs.IsAny())));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            #region Local methods

            void SetupFindSingleRoleGroup(RoleGroup rg) => mockRoleGroupRepo.Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                .ReturnsAsync(rg);

            #endregion
        }

        [Test]
        public async Task DeleteRoleGroupAsync()
        {
            var svc = Prepare(out var mockRoleGroupRepo)
                .GetRequiredService<IRoleGroupService>();

            var corp1 = RandomCorp();
            var app1 = RandomApp();
            var roleGroup1 = RandomRoleGroup();

            var rg1 = new global::SimpleAuth.Shared.Domains.RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            };

            // Argument verification
            Assert.CatchAsync<ArgumentNullException>(async () => await svc.DeleteRoleGroupAsync(null));

            // if role group does not exists, so throwing EntityNotExistsException
            SetupFindSingleRoleGroup(null);
            Assert.CatchAsync<EntityNotExistsException>(async () => await svc.DeleteRoleGroupAsync(rg1));

            // Normal
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
            });
            mockRoleGroupRepo.Setup(x => x.DeleteManyAsync(It.IsAny<IEnumerable<RoleGroup>>())).ReturnsAsync(1);
            await svc.DeleteRoleGroupAsync(rg1);
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()));
            // ReSharper disable PossibleMultipleEnumeration
            mockRoleGroupRepo.Verify(m => m.DeleteManyAsync(It.Is<IEnumerable<RoleGroup>>(rgs => rgs.Count() == 1 && rgs.Count(x => x.Name == rg1.Name && x.Corp == rg1.Corp && x.App == rg1.App) == 1)));
            // ReSharper restore PossibleMultipleEnumeration
            mockRoleGroupRepo.VerifyNoOtherCalls();

            // if role group contains RoleRecord(s), check if Repository.UpdateRoleRecordsAsync(RoleGroup roleGroup, List<RoleRecord> newRoles) receives an empty `newRoles`
            mockRoleGroupRepo.Reset();
            SetupFindSingleRoleGroup(new RoleGroup
            {
                Name = roleGroup1,
                Corp = corp1,
                App = app1,
                RoleRecords = new List<RoleRecord>
                {
                    new RoleRecord()
                }
            });
            mockRoleGroupRepo.Setup(x => x.UpdateRoleRecordsAsync(It.IsAny<RoleGroup>(), It.IsAny<List<RoleRecord>>()))
                .ReturnsAsync(1);
            await svc.DeleteAllRolesFromGroupAsync(rg1);
            mockRoleGroupRepo.Verify(m =>
                m.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()));
            mockRoleGroupRepo.Verify(m => m.UpdateRoleRecordsAsync(It.Is<RoleGroup>(rg => rg.Name == roleGroup1), It.Is<List<RoleRecord>>(rrs => !rrs.IsAny())));
            mockRoleGroupRepo.VerifyNoOtherCalls();

            #region Local methods

            void SetupFindSingleRoleGroup(RoleGroup rg) => mockRoleGroupRepo.Setup(x =>
                    x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<RoleGroup, bool>>>>()))
                .ReturnsAsync(rg);

            #endregion
        }
    }
}