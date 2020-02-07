using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Server.Services
{
    /// <summary>
    /// Services for communicating with Google
    /// </summary>
    public interface IGoogleService
    {
        /// <summary>
        /// Connect to tokeninfo endpoint of Google OAuth2 api to retrieve information
        /// </summary>
        /// <param name="token">Token string to forward to Google OAuth</param>
        /// <returns>Model response by Google OAuth</returns>
        Task<GoogleTokenResponseResult> GetInfoAsync(string token);

        /// <summary>
        /// Validate based on condition provided in request
        /// </summary>
        /// <param name="form">Request condition</param>
        /// <param name="corp">Corp ID</param>
        /// <param name="ggToken">Token object responded by GG</param>
        /// <returns>any Exception when false</returns>
        Task VerifyRequestAsync(string corp, LoginByGoogleRequest form, GoogleTokenResponseResult ggToken);
    }

    /// <summary>
    /// The first and default implementation
    /// </summary>
    public class DefaultGoogleService : IGoogleService
    {
        private readonly IUserService _userService;
        private readonly IHttpService _httpService;

        /// <inheritdoc />
        public DefaultGoogleService(IUserService userService, IHttpService httpService)
        {
            _userService = userService;
            _httpService = httpService;
        }

        /// <inheritdoc />
        public async Task<GoogleTokenResponseResult> GetInfoAsync(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var httpResponse = await _httpService.GetClient().GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={token}");

            if (!httpResponse.IsSuccessStatusCode)
                throw new SimpleAuthException(httpResponse.StatusCode.ToString());

            var response = await httpResponse.Content.ReadAsStringAsync();
            return CastToSharedModel(JsonSerializer.Deserialize<GoogleTokenResponse>(response));
        }

        /// <inheritdoc />
        public Task VerifyRequestAsync(string corp, LoginByGoogleRequest form, GoogleTokenResponseResult ggToken)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (form == null)
                throw new ArgumentNullException(nameof(form));
            if (ggToken == null)
                throw new ArgumentNullException(nameof(ggToken));

            var user = _userService.GetUser(form.Email, corp);
            var localUserInfo = user?.LocalUserInfos?.FirstOrDefault(x => x.Corp == corp);
            if (localUserInfo == null)
                throw new EntityNotExistsException($"{form.Email} at {corp}");

            if (localUserInfo.Locked)
                throw new AccessLockedEntityException($"{user.Id} at {localUserInfo.Corp}");


            if (!form.Email.EqualsIgnoreCase(ggToken.Email))
                throw new DataVerificationMismatchException(
                    $"This token belong to {ggToken.Email} which is different than email provided in payload {form.Email}"
                );

            if (!form.VerifyWithClientId.IsBlank())
                if (!form.VerifyWithClientId.EqualsIgnoreCase(ggToken.Aud))
                    throw new DataVerificationMismatchException(
                        $"This token is rejected, it's not belong to client id {form.VerifyWithClientId}"
                    );

            if (!form.VerifyWithGSuite.IsBlank())
                if (!form.VerifyWithGSuite.EqualsIgnoreCase(ggToken.Hd))
                    throw new DataVerificationMismatchException(
                        $"This token is rejected, it's not belong to GSuite domain {form.VerifyWithGSuite}"
                    );

            var expiryDate = new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            expiryDate = expiryDate.AddSeconds(int.Parse(ggToken.Exp));

            if (DateTime.UtcNow > expiryDate)
                throw new DataVerificationMismatchException(
                    "Token already expired"
                );

            return Task.CompletedTask;
        }

        private GoogleTokenResponseResult CastToSharedModel(GoogleTokenResponse model)
        {
            var res = new GoogleTokenResponseResult
            {
                Iss = model.Iss,
                Azp = model.Azp,
                Aud = model.Aud,
                Sub = model.Sub,
                Hd = model.Hd,
                Email = model.Email,
                EmailVerified = model.EmailVerified,
                AtHash = model.AtHash,
                Name = model.Name,
                Picture = model.Picture,
                GivenName = model.GivenName,
                FamilyName = model.FamilyName,
                Local = model.Local,
                Iat = model.Iat,
                Exp = model.Exp,
                Jti = model.Jti,
                Alg = model.Alg,
                Kid = model.Kid,
                Typ = model.Typ
            };
            return res;
        }
    }
}