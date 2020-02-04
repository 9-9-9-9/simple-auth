using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
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
            SetupFindManyReturns(new []
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
                rgs.Count() == 1 && rgs.First().RoleRecords.Count == 5 && rgs.SelectMany(x => x.RoleRecords).All(x => x.RoleId.EndsWith(".m")))));
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
                CopyFromRoleGroups = specificGroupToBeCopiedFrom ? new []{"gr1", "gr2"} : null
            });

            #endregion
        }
    }
}