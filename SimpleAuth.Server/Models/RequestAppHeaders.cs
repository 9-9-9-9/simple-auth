namespace SimpleAuth.Server.Models
{
    /// <summary>
    /// Model which will be encrypted to deliver to client side, client sends the encrypted string to server and then be decrypted to determine if requester has required permission
    /// </summary>
    public class RequestAppHeaders
    {
        /// <summary>
        /// This must be x-app-token
        /// </summary>
        public string Header { get; set; }
        
        /// <summary>
        /// Corp name, combine with App to determine which app does requester has management permission
        /// </summary>
        public string Corp { get; set; }
        
        /// <summary>
        /// App which requester has management permission
        /// </summary>
        public string App { get; set; }
        
        /// <summary>
        /// Version of token
        /// </summary>
        public int Version { get; set; }
    }
}