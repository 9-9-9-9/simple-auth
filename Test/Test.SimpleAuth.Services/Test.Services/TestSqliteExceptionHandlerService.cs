using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;
using Test.Shared;
using Test.Shared.Extensions;

namespace Test.SimpleAuth.Shared.Test.Services
{
    public class TestSqliteExceptionHandlerService : BaseTestClass
    {
        [Test]
        public void Default()
        {
            var svc = Svc<ISqlExceptionHandlerService>();

            PerformDoubleTest<NullReferenceException, NullReferenceException>();
            PerformDoubleTest<Exception, Exception>();
            PerformDoubleTest<DbUpdateException, HandledSqlException>();
            PerformDoubleTest<DbUpdateConcurrencyException, ConcurrentUpdateException>();

            void PerformDoubleTest<TSrcEx, TExpectEx>()
                where TSrcEx : Exception, new()
                where TExpectEx : Exception
            {
                AssertE.Ex<TExpectEx>(() => throw svc.TryTransform(new TSrcEx()));
                AssertE.Ex<TExpectEx>(() =>
                {
                    svc.RethrowException(new TSrcEx());
                    return null;
                });
            }
        }

        [Test]
        public async Task RethrowException()
        {
            var svc = Svc<ISqlExceptionHandlerService>();

            var userRepo = Svc<IUserRepository>();
            var localInfoRepo = Svc<ILocalUserInfoRepository>();

            var usId1 = RandomUser();
            var unId1 = usId1.NormalizeInput();
            var usId2 = RandomUser();
            var unId2 = usId2.NormalizeInput();
            var usId3 = RandomUser();
            var unId3 = usId3.NormalizeInput();
            var usId4 = RandomUser();
            var unId4 = usId4.NormalizeInput();
            var usId5 = RandomUser();
            var unId5 = usId5.NormalizeInput();

            var corp1 = RandomCorp();
            var corp2 = RandomCorp();

            const string emailA = "example-a-a@gmail.com";
            //const string emailB = "example-b-b@gmail.com";

            await CreateUser(usId1, unId1);
            await CreateUser(usId2, unId2);
            await CreateUser(usId4, unId4);
            await CreateUser(usId5, unId5);

            await CreateLocalUser(usId1, emailA, corp1);
            await CreateLocalUser(usId2, emailA, corp2); // 1 email can be used in 2 different corps

            // primary key test
            await Expect<ConstraintViolationException>(async () => await CreateUser(usId1, unId3));

            // Unique (single) column test
            
            /* In Memory DB cannot test this
            await Expect<ConstraintViolationException>(async () => await CreateUser(usId3, unId1));
            await Expect<ConstraintViolationException>(async () => await CreateUser(usId3, unId2));
            await CreateUser(usId3, unId3);

            // Unique (combination) column test
            await Expect<ConstraintViolationException>(async () => await CreateLocalUser(usId3, emailA, corp1));
            await CreateLocalUser(usId3, emailB, corp1);
            // Unique (combination) column test, NULL should not be counted as constraint violation
            await CreateLocalUser(usId4, null, corp1);
            await CreateLocalUser(usId5, null, corp1);
            */

            async Task Expect<TEx>(Func<Task> act) where TEx : Exception
            {
                try
                {
                    await act();
                    Assert.Fail("Expect error");
                }
                catch (Exception e)
                {
                    AssertE.Ex<TEx>(() =>
                    {
                        svc.RethrowException(e);
                        return null;
                    });
                }
            }

            async Task CreateUser(string userId, string normalizedId)
            {
                await userRepo.CreateAsync(new User
                {
                    Id = userId,
                    NormalizedId = normalizedId
                });
            }

            async Task CreateLocalUser(string userId, string email, string corp)
            {
                await localInfoRepo.CreateAsync(new LocalUserInfo
                {
                    Email = email,
                    NormalizedEmail = email?.NormalizeInput(),
                    UserId = userId,
                    Corp = corp
                }.WithRandomId());
            }
        }
    }
}