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
        public string Corp { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
    
    public class ResponseUserModel : BaseModel
    {
        public string Id { get; set; }
        public string Corp { get; set; }
        public string Email { get; set; }
        public RoleModel[] ActiveRoles { get; set; }
        
        public GoogleTokenResponseResult GoogleToken { get; set; }
        public long? TokenExpireAfterSeconds { get; set; }

        public void ExpireAt(DateTime expiryDate)
        {
            if (expiryDate.Kind != DateTimeKind.Utc)
                throw new InvalidDataException("Only accept UTC");
            TokenExpireAfterSeconds = (long) (expiryDate - DateTime.UtcNow).TotalSeconds;
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

        [DataMember(Name = "aud")] public string Aud { get; set; }

        [DataMember(Name = "exp")] public int Exp { get; set; }

        [DataMember(Name = "email")] public string Email { get; set; }

        [DataMember(Name = "hd")] public string Hd { get; set; }
    }
}