using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;

namespace SimpleAuth.Shared.Models
{
    public class CreateUserModel : BaseModel
    {
        [Required]
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
    
    public class ResponseUserModel : BaseModel
    {
        public string Id { get; set; }
        public string Corp { get; set; }
        public string Email { get; set; }
        public bool Locked { get; set; }
        public RoleModel[] ActiveRoles { get; set; }
        
        public GoogleTokenResponseResult GoogleToken { get; set; }
        public long? ExpiryDate { get; set; }

        public void ExpireAt(DateTime expiryDate)
        {
            if (expiryDate.Kind != DateTimeKind.Utc)
                throw new InvalidDataException("Only accept UTC");
            ExpiryDate = (long) (expiryDate - new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc)).TotalSeconds;
        }
    }

    public class LoginByGoogleRequest : BaseModel
    {
        [Required] public string Email { get; set; }
        [Required] public string GoogleToken { get; set; }
        public string VerifyWithClientId { get; set; }
        public string VerifyWithGSuite { get; set; }
    }

    [DataContract]
    public class GoogleTokenResponseResult
    {
        [DataMember(Name = "iss")] public string Iss { get; set; }
        [DataMember(Name = "azp", IsRequired = false)] public string Azp { get; set; }
        [DataMember(Name = "aud")] public string Aud { get; set; }
        [DataMember(Name = "sub", IsRequired = false)] public string Sub { get; set; }
        [DataMember(Name = "hd")] public string Hd { get; set; }
        [DataMember(Name = "email")] public string Email { get; set; }
        [DataMember(Name = "emailVerified", IsRequired = false)] public bool EmailVerified { get; set; }
        [DataMember(Name = "at_hash", IsRequired = false)] public string AtHash { get; set; }
        [DataMember(Name = "name", IsRequired = false)] public string Name { get; set; }
        [DataMember(Name = "picture", IsRequired = false)] public string Picture { get; set; }
        [DataMember(Name = "given_name", IsRequired = false)] public string GivenName { get; set; }
        [DataMember(Name = "family_name", IsRequired = false)] public string FamilyName { get; set; }
        [DataMember(Name = "locale", IsRequired = false)] public string Local { get; set; }
        [DataMember(Name = "iat", IsRequired = false)] public string Iat { get; set; }
        [DataMember(Name = "exp")] public string Exp { get; set; }
        [DataMember(Name = "jti", IsRequired = false)] public string Jti { get; set; }
        [DataMember(Name = "alg", IsRequired = false)] public string Alg { get; set; }
        [DataMember(Name = "kid", IsRequired = false)] public string Kid { get; set; }
        [DataMember(Name = "typ", IsRequired = false)] public string Typ { get; set; }
    }

    public class ModifyUserRoleGroupsModel
    {
        public string[] RoleGroups { get; set; }
    }
}