namespace SimpleAuth.Shared
{
    public static class Constants
    {
        public const string SplitterSubModules = "|";
        public const string WildCard = "*";
        public const string SplitterRoleParts = ".";

        public static class Length
        {
            public const int MinTerm = 3;
            public const int MaxSearchResults = 999;
        }

        public static class Headers
        {
            public const string AppPermission = "x-app-token";
            public const string CorpPermission = "x-corp-token";
            public const string MasterToken = "x-master-token";
        }

        public static class Encryption
        {
            public const string Section = "SA:Secret";
            public const string MasterTokenKey = "MasterTokenValue";
            
            public const string PublicKeyName = "Rsa2048PublicKey";
            public const string PrivateKeyName = "Rsa2048PrivateKey";
            public const int KeySize = 2048;
        }
    }
}