namespace SimpleAuth.Server.Models
{
    public class SecretConstants
    {
        public string MasterTokenValue { get; }

        public SecretConstants(string masterTokenValue)
        {
            MasterTokenValue = masterTokenValue;
        }
    }
}