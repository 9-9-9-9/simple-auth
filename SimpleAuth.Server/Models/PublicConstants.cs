namespace SimpleAuth.Server.Models
{
    /// <summary>
    /// Contains public information that could be downloaded by clients/users
    /// </summary>
    public class PublicConstants
    {
        /// <summary>
        /// Public Sign In Client Id provided by Google, user can download this client id and use it to perform google sign-in
        /// </summary>
        public string GoogleSignInClientId { get; set; }
    }
}