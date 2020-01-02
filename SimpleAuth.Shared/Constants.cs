namespace SimpleAuth.Shared
{
    public static class Constants
    {
        public const string SplitterSubModules = "|";
        public const char chSplitterSubModules = '|';
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

        public static class Identity
        {
            public const string Issuer = "SA";
        }

        public static class Test
        {
            public const string Rsa2048PublicKey =
                "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTE2Ij8+CjxSU0FQYXJhbWV0ZXJzIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhtbG5zOnhzZD0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiPgogIDxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD4KICA8TW9kdWx1cz42SkwybzBRT3kySmUzd3BIQVpuaU1OZ2hMQ1VmK3lReHZkMTZWMzBjTWx0UWppekhLcWN2WnV6cmozc2R4cGwvbDdKeG83UWNxSVBQLzIwSHJJalQ1UXgxNm9XZWNydzBKeGFMb2d2YXNKTnJzOW9LV2dObGUrUmtXcXQ4NmtDUmZ6VWd2WTZCZExPdDBYWDNwZzN6eG14TVc5ajRPTzR6UFJZOUlxS0lwY2hzYVNlSU5Tb2p0cndqckk0Qzh5amdTTFRUc2tuU0RvTC9LVVVGVy9lUW1oSkUwUmFjTlluRFU4VkRYZ1NtWCtTYXBxZ1Z3N0Z2a0duZUtDamc3WTZRejBGQ003bWgrYTRkNW9YN2ZPZFB0WjltR3VaK283b3pnd1NwNWMyNEQzVHQ1UjZ3blFIY3V2OEhwRG1CMnNaQ3Y5Z0liZCtuRFk0UmwyNTAvVWF4V3c9PTwvTW9kdWx1cz4KPC9SU0FQYXJhbWV0ZXJzPg==";

            public const string Rsa2048PrivateKey =
                "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTE2Ij8+CjxSU0FQYXJhbWV0ZXJzIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhtbG5zOnhzZD0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiPgogIDxEPldNemNUUVJNWGMybWlhVEQ3ZEdTc3JHaklOYmYraVVjdFBEZXFJOEZlQml6amtWOG1rV1JiTXdqT1BwNktQMHM0WC9wc2Vhd1ZvdFZuS0pGbnRQemkrNFJDOFdPOVNIMUM4T285dXR0eWFQUUtMQ2s2UytJelNBc1RRNVRPS0lFcTd4ZVR1SWRmYkNwT21PL2RmQzMxbGc5WTRoWlptdFV0d3RKdG5LMm1BMVBHWXZQa2xkQXFVNlBWOU9JMk9FTkJuUTNTbmVYdWUzVXpvZUhpZ20vQ1N2MHMxVXZ5WDdrWWExME5IaHB1TTJpTDVtWW85ZHdSYWxQRGNXREk2RUhwYXpTMVU2cERnbHYxYXdMcG9KcVpsNmFsWmhKOFFNWHE1QkhiOGVCcE5xTUdKanlyVWVFQjJuenczMm9uTUJIRDBBT3BxSW1MVmRWN3NRVFZ5VUllUT09PC9EPgogIDxEUD5SSVJkY0lqOXpaUXpRVmEwVE1xZTgrVWNUTmxGSTdpcFlEME9WN0NQU24wWWRYdVRsYm1HTk5ZdkhkajdjN2htZVdEcHhWR3ZMTXUrU1hGV0pMZ0wvNlhxMzlzbkFDWlVUdWY5WkkrbVU4aVExYjFMSVNsWTBNSDBPNTA4MFp6ZXdxeEVwbXVMcTZtdkt3b2FmaW1ZMzE0TWdtL2dPMTRCOWxhYTZOeUNXWXM9PC9EUD4KICA8RFE+eWcwaHJOMU5HRk4rUnN3anh0RlUyb3dJUE02U2o3QTFuQXNYRnM3YXhhWEZPdHh4cGVxMXhQbDRYaTN6T09lQzVnMHpIV0xHZnQ0UWdJOWVKV3lWeHNTa2o0eUx6VnkwLy9kQUxrVUlYSE9WMzRYK0d4MmpyZmdlbi9EK0luUytQeGRhLzRpSld1WXN5S2k0aXpIMzYydzEyZkg0c0lTZFpBelNhNE5wcXFFPTwvRFE+CiAgPEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PgogIDxJbnZlcnNlUT44dzIrOUVBUS9ab3lBV0Nub0RRVkFTSHYrRmxvTXBNNGtGQWtOTUVBV3YyUllCZVQvWnhTcENIZUtsUFVCaVZFbnE2S3QwT0NhUFZYL3Nxb2dld01aMFZEOHJDNzMrYTF0Mll4ajVrZWtONUlaMFNOdlJEV21MNG1tVHVLQmZrMVV1Q1ZLbk5WK0ErbmtqWW1ycnRyc0o2RmZWM3F2a1pUL2NzeGlmTDVMMm89PC9JbnZlcnNlUT4KICA8TW9kdWx1cz42SkwybzBRT3kySmUzd3BIQVpuaU1OZ2hMQ1VmK3lReHZkMTZWMzBjTWx0UWppekhLcWN2WnV6cmozc2R4cGwvbDdKeG83UWNxSVBQLzIwSHJJalQ1UXgxNm9XZWNydzBKeGFMb2d2YXNKTnJzOW9LV2dObGUrUmtXcXQ4NmtDUmZ6VWd2WTZCZExPdDBYWDNwZzN6eG14TVc5ajRPTzR6UFJZOUlxS0lwY2hzYVNlSU5Tb2p0cndqckk0Qzh5amdTTFRUc2tuU0RvTC9LVVVGVy9lUW1oSkUwUmFjTlluRFU4VkRYZ1NtWCtTYXBxZ1Z3N0Z2a0duZUtDamc3WTZRejBGQ003bWgrYTRkNW9YN2ZPZFB0WjltR3VaK283b3pnd1NwNWMyNEQzVHQ1UjZ3blFIY3V2OEhwRG1CMnNaQ3Y5Z0liZCtuRFk0UmwyNTAvVWF4V3c9PTwvTW9kdWx1cz4KICA8UD4vVEhCZ3JaMXZPaEx0OHdBT1VMdUgzQ1VUVWNCR005NHlkVXFoV1BVa1F1NFlobFZmS0dOYzM5NDh2ZUVSbzRLWThyN3ZyOUVjU2NYSis5N0RwRkFyWG0wNzZWVlY2NXI1eTB6RUVoeVFkWFVwSGloRWI5R09mc2lwc3pWeW1udGJpcVNtKzFEMktSQUdjVVpleXhCNU9CQm1UT09XOTI0NWtWa0x3eUE1eWM9PC9QPgogIDxRPjZ5YTJuZWlmcjBPUkh6Y2xsUmxVVWcxUzJoa3FFaC9QZDR3a3F4c2FHTTVWVXZjQVErdWRKZTNnVWpqakhqTFRFczJaSDlsM2VwNjE2UlpKeHhoaHM2eEIvZmZuclkzaXErOUZZV0lMUi9TWFBGRXFGWEF4T1RCa01rS0wwVDRvVUdhSWZMQTEvS2JsSGo5Y0k5V3VUY1o0YUtIYVorNEdPTnJBV3lxdkpLMD08L1E+CjwvUlNBUGFyYW1ldGVycz4=";
        }
    }
}