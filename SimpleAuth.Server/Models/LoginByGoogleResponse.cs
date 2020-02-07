using System.Text.Json.Serialization;
#pragma warning disable 1591

namespace SimpleAuth.Server.Models
{
    /// <summary>
    /// Response by google
    /// </summary>
    public class GoogleTokenResponse
    {
        [JsonPropertyName("iss")] public string Iss { get; set; }
        [JsonPropertyName("azp")] public string Azp { get; set; }
        [JsonPropertyName("aud")] public string Aud { get; set; }
        [JsonPropertyName("sub")] public string Sub { get; set; }
        [JsonPropertyName("hd")] public string Hd { get; set; }
        [JsonPropertyName("email")] public string Email { get; set; }
        [JsonPropertyName("emailVerified")] public bool EmailVerified { get; set; }
        [JsonPropertyName("at_hash")] public string AtHash { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("picture")] public string Picture { get; set; }
        [JsonPropertyName("given_name")] public string GivenName { get; set; }
        [JsonPropertyName("family_name")] public string FamilyName { get; set; }
        [JsonPropertyName("locale")] public string Local { get; set; }
        [JsonPropertyName("iat")] public string Iat { get; set; }
        [JsonPropertyName("exp")] public string Exp { get; set; }
        [JsonPropertyName("jti")] public string Jti { get; set; }
        [JsonPropertyName("alg")] public string Alg { get; set; }
        [JsonPropertyName("kid")] public string Kid { get; set; }
        [JsonPropertyName("typ")] public string Typ { get; set; }
    }
}