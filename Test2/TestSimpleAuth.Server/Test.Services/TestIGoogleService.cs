using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Moq;
using Moq.Contrib.HttpClient;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Server.Services;
using SimpleAuth.Services;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using Test.Shared.Utils;

namespace Test.SimpleAuth.Server.Test.Services
{
    public class TestIGoogleService : BaseTestServer
    {
        private IGoogleService GetService(out Mock<IUserService> mockUsrSvc, out Mock<IHttpService> mockHttpSvc)
        {
            mockUsrSvc = Mu.Of<IUserService>();
            mockHttpSvc = Mu.Of<IHttpService>();

            return new DefaultGoogleService(mockUsrSvc.Object, mockHttpSvc.Object);
        }

        [Test]
        public async Task GetInfoAsync()
        {
            // All requests made with HttpClient go through its handler's SendAsync() which we mock
            var handler = new Mock<HttpMessageHandler>();
            var client = handler.CreateClient();

            //
            var ggSvc = GetService(out _, out var mockHttpSvc);
            mockHttpSvc.Setup(x => x.GetClient()).Returns(client);

            var token = RandomText(10);
            var expectedUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";

            handler.SetupRequest(HttpMethod.Get, expectedUrl)
                .ReturnsResponse(GgAuthResponseToken.Replace("\n", ""), "application/json");

            var googleTokenResponseResult = await ggSvc.GetInfoAsync(token);
            Assert.NotNull(googleTokenResponseResult);

            var properties = googleTokenResponseResult.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var dataMemberAttr = property.GetCustomAttribute<DataMemberAttribute>();
                Assert.NotNull(dataMemberAttr);

                var value = property.GetValue(googleTokenResponseResult);
                if (!(value is string strVal))
                    continue;

                if (dataMemberAttr.IsRequired)
                    Assert.IsFalse(strVal.IsBlank());
            }
        }

        [Test]
        public void VerifyRequestAsync_ValidateArguments()
        {
            var svc = Svc<IGoogleService>();
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.VerifyRequestAsync(null, new LoginByGoogleRequest(), new GoogleTokenResponseResult()));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.VerifyRequestAsync(RandomCorp(), null, new GoogleTokenResponseResult()));
            Assert.CatchAsync<ArgumentNullException>(async () =>
                await svc.VerifyRequestAsync(RandomCorp(), new LoginByGoogleRequest(), null));
        }

        [Test]
        public async Task VerifyRequestAsync()
        {
            // All requests made with HttpClient go through its handler's SendAsync() which we mock
            var handler = new Mock<HttpMessageHandler>();
            var client = handler.CreateClient();

            //
            var ggSvc = GetService(out var mockUsrSvc, out var mockHttpSvc);
            mockHttpSvc.Setup(x => x.GetClient()).Returns(client);

            // setup querying token
            var token = RandomText(10);
            var expectedUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";

            handler.SetupRequest(HttpMethod.Get, expectedUrl)
                .ReturnsResponse(GgAuthResponseToken.Replace("\n", ""), "application/json");

            var googleTokenResponseResult = await ggSvc.GetInfoAsync(token);
            Assert.NotNull(googleTokenResponseResult);

            var corp = "standingtrust.com";
            var email = "hungpv@standingtrust.com";
            var clientId = "8621641841-21782164174.apps.googleusercontent.com";

            // user not found
            mockUsrSvc.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns((User)null);
            Assert.CatchAsync<EntityNotExistsException>(async () =>
                await ggSvc.VerifyRequestAsync(corp, new LoginByGoogleRequest
                {
                    Email = email,
                    VerifyWithClientId = clientId,
                    VerifyWithGSuite = corp
                }, googleTokenResponseResult));
            mockUsrSvc.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns(new User
            {
                Id = email
            });
            Assert.CatchAsync<EntityNotExistsException>(async () =>
                await ggSvc.VerifyRequestAsync(corp, new LoginByGoogleRequest
                {
                    Email = email,
                    VerifyWithClientId = clientId,
                    VerifyWithGSuite = corp
                }, googleTokenResponseResult));
            
            // user locked
            mockUsrSvc.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns((string uid, string c) =>
                new User
                {
                    Id = uid,
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = c,
                            Email = uid,
                            Locked = true
                        }
                    }
                });
            Assert.CatchAsync<AccessLockedEntityException>(async () =>
                await ggSvc.VerifyRequestAsync(corp, new LoginByGoogleRequest
                {
                    Email = email,
                    VerifyWithClientId = clientId,
                    VerifyWithGSuite = corp
                }, googleTokenResponseResult));

            // setup return user
            mockUsrSvc.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns((string uid, string c) =>
                new User
                {
                    Id = uid,
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = c,
                            Email = uid,
                        }
                    }
                });

            // normal
            await ggSvc.VerifyRequestAsync(corp, new LoginByGoogleRequest
            {
                Email = email,
                VerifyWithClientId = clientId,
                VerifyWithGSuite = corp
            }, googleTokenResponseResult);

            // mis-match email
            Assert.CatchAsync<DataVerificationMismatchException>(async () =>
                await ggSvc.VerifyRequestAsync(corp,
                    new LoginByGoogleRequest
                    {
                        Email = RandomEmail(),
                        VerifyWithClientId = clientId,
                        VerifyWithGSuite = corp
                    },
                    googleTokenResponseResult)
            );

            // mis-match 
            Assert.CatchAsync<DataVerificationMismatchException>(async () =>
                await ggSvc.VerifyRequestAsync(corp,
                    new LoginByGoogleRequest
                    {
                        Email = email,
                        VerifyWithClientId = RandomText(),
                        VerifyWithGSuite = corp
                    },
                    googleTokenResponseResult)
            );

            // mis-match 
            Assert.CatchAsync<DataVerificationMismatchException>(async () =>
                await ggSvc.VerifyRequestAsync(corp,
                    new LoginByGoogleRequest
                    {
                        Email = email,
                        VerifyWithClientId = clientId,
                        VerifyWithGSuite = RandomCorp()
                    },
                    googleTokenResponseResult)
            );
        }

        [Test]
        public async Task VerifyRequestAsync_WithExpiredToken()
        {
            // All requests made with HttpClient go through its handler's SendAsync() which we mock
            var handler = new Mock<HttpMessageHandler>();
            var client = handler.CreateClient();

            //
            var ggSvc = GetService(out var mockUsrSvc, out var mockHttpSvc);
            mockHttpSvc.Setup(x => x.GetClient()).Returns(client);

            // setup querying token
            var token = RandomText(10);
            var expectedUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";

            handler.SetupRequest(HttpMethod.Get, expectedUrl)
                .ReturnsResponse(GgAuthResponseExpiredToken.Replace("\n", ""), "application/json");

            var googleTokenResponseResult = await ggSvc.GetInfoAsync(token);
            Assert.NotNull(googleTokenResponseResult);

            var corp = "standingtrust.com";
            var email = "hungpv@standingtrust.com";
            var clientId = "8621641841-21782164174.apps.googleusercontent.com";

            // setup return user
            mockUsrSvc.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns((string uid, string c) =>
                new User
                {
                    Id = uid,
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = c,
                            Email = uid,
                        }
                    }
                });

            // expired token
            Assert.CatchAsync<DataVerificationMismatchException>(async () =>
                await ggSvc.VerifyRequestAsync(corp,
                    new LoginByGoogleRequest
                    {
                        Email = email,
                        VerifyWithClientId = clientId,
                        VerifyWithGSuite = corp
                    },
                    googleTokenResponseResult)
            );
        }

        private const string GgAuthResponseToken = @"
{
  ""iss"": ""accounts.google.com"",
  ""azp"": ""8621641841-21782164174.apps.googleusercontent.com"",
  ""aud"": ""8621641841-21782164174.apps.googleusercontent.com"",
  ""sub"": ""64327453264734"",
  ""hd"": ""standingtrust.com"",
  ""email"": ""hungpv@standingtrust.com"",
  ""email_verified"": ""true"",
  ""at_hash"": ""vvsca86JJSVCg32"",
  ""name"": ""Phạm Việt Hùng"",
  ""picture"": ""https://lh3.googleusercontent.com/a-/ajcaA75a-bsac36VGCGSV63vca=y96-I"",
  ""given_name"": ""Phạm "",
  ""family_name"": ""Việt Hùng"",
  ""locale"": ""en"",
  ""iat"": ""1581045317"",
  ""exp"": ""2147483647"",
  ""jti"": ""86128418274141"",
  ""alg"": ""RS256"",
  ""kid"": ""12517536127421"",
  ""typ"": ""JWT""
}
";

        private const string GgAuthResponseExpiredToken = @"
{
  ""iss"": ""accounts.google.com"",
  ""azp"": ""8621641841-21782164174.apps.googleusercontent.com"",
  ""aud"": ""8621641841-21782164174.apps.googleusercontent.com"",
  ""sub"": ""64327453264734"",
  ""hd"": ""standingtrust.com"",
  ""email"": ""hungpv@standingtrust.com"",
  ""email_verified"": ""true"",
  ""at_hash"": ""vvsca86JJSVCg32"",
  ""name"": ""Phạm Việt Hùng"",
  ""picture"": ""https://lh3.googleusercontent.com/a-/ajcaA75a-bsac36VGCGSV63vca=y96-I"",
  ""given_name"": ""Phạm "",
  ""family_name"": ""Việt Hùng"",
  ""locale"": ""en"",
  ""iat"": ""1581045317"",
  ""exp"": ""0"",
  ""jti"": ""86128418274141"",
  ""alg"": ""RS256"",
  ""kid"": ""12517536127421"",
  ""typ"": ""JWT""
}
";
    }
}