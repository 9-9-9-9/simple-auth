namespace SimpleAuth.Server.Models
{
    /// <summary>
    /// Store the top secret constants
    /// </summary>
    public class SecretConstants
    {
        /// <summary>
        /// A plain-text master token, to be used to check if requester has administrator permission
        /// </summary>
        public string MasterTokenValue { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SecretConstants(string masterTokenValue)
        {
            MasterTokenValue = masterTokenValue;
        }
    }
}