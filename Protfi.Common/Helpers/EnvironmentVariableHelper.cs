using Protfi.Common.Extensions;

namespace Protfi.Common.Helpers
{
    public static class EnvironmentVariableHelper
    {
        public static readonly string DB_IP = "DB_IP";
        public static readonly string DB_PORT = "DB_PORT";
        public static readonly string DB_USERNAME = "DB_USERNAME";
        public static readonly string DB_LOGIN_USERNAME = "DB_LOGIN_USERNAME";
        public static readonly string DB_PASSWORD = "DB_PASSWORD";
        public static readonly string DB_POOLING = "DB_POOLING";
        public static readonly string DB_MAX_POOL = "DB_MAX_POOL";
        public static readonly string DB_SSL_MODE = "DB_SSL_MODE";
        public static readonly string DB_VALIDATE_CERTIFICATE = "DB_VALIDATE_CERTIFICATE";
        private static readonly string DEFAULT_SERVICES_IP = "localhost";
        private static readonly string DEFAULT_BINDIND_SERVICES_IP = "*";
        public const string PROTFI_CLIENT_ADDRESS = "CLIENT_ADDRESS";
        
        public static string GetClientAddress()
        {
            return Environment
                .GetEnvironmentVariable(PROTFI_CLIENT_ADDRESS)
                .Or("localhost");
        }
    }
}