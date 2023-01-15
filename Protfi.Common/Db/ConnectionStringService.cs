using System.Reflection;
using Protfi.Common.Helpers;

namespace Protfi.Common.Db
{
    public static class ConnectionStringService
    {
        private const int MaxPoolSize = 50;
        public const int ThreeHoursInSeconds = 10800;
        
        private static readonly string ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name;

        public static string GetConnectionString(string dbName) => GetConnectionStringFromEnvironment(dbName);

        public static IEnumerable<string> GetConnectionStrings(IEnumerable<string> dbNames) => dbNames.Select(GetConnectionStringFromEnvironment);
        

        private static string GetConnectionStringFromEnvironment(string dbName)
        {
            try
            {
                var dbIp = Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_IP);
                var dbPort = Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_PORT);
                var dbUsername = Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_USERNAME);
                var dbPasswordEnc = Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_PASSWORD);
                var dbMaxPoolSizeEnv = Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_MAX_POOL);
                int dbMaxPoolSize = String.IsNullOrEmpty(dbMaxPoolSizeEnv) ? MaxPoolSize : int.Parse(dbMaxPoolSizeEnv);
                var dbPooling = bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_POOLING) ?? String.Empty, out var pooling) ? pooling : default(bool?);
                var dbSslMode = Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_SSL_MODE);
                var validateServerCert = bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableHelper.DB_VALIDATE_CERTIFICATE) ?? string.Empty, out var validateCert) ? validateCert : default(bool?);
                
                var dbPassword = dbPasswordEnc;
                
                // if (CertificateHelper.HasCertificate())
                // {
                //     dbPassword = dbPasswordEnc.DecryptOrDefault();
                //     if (dbPassword == dbPasswordEnc)
                //     {
                //         var encryptedPassword = dbPassword.EncryptOrDefault();
                //         Environment.SetEnvironmentVariable(EnvironmentVariableHelper.DB_PASSWORD, encryptedPassword);
                //     }
                // }

                return BuildConnectionString("localhost", "5432", "sa", "Yeswecan2015", dbName, dbPooling, dbMaxPoolSize, dbSslMode, validateCert);
                
                    //? OnInvalidConnection()
                    //: BuildConnectionString(dbIp, dbPort, dbUsername, dbPassword, dbName, dbPooling, dbMaxPoolSize, dbSslMode, validateCert);
            }
            catch (Exception e)
            {
                //SystemLogger.Instance.Error("Failed to get connection string from environment vars : " + e, LoggerConsts.CommonGeneral, e);
                throw;
            }
        }

        private static string BuildConnectionString(string host = "localhost", string port = "5432", string user = "protfiAdmin", string password = "Gobigorgohome", string databaseName = "database", bool? pooling = null, int poolSize = MaxPoolSize, string sslMode = "Prefer", bool validateServerCert = false)
        {
            sslMode = sslMode ?? "Prefer";
            var serverCert = validateServerCert ? string.Empty : "Trust Server Certificate=true";
            var poolingParameter = pooling.HasValue ? $"Pooling={pooling}" : string.Empty;
            const string connectionStringTemplate = "Host={0};Port={1};Username={2};Password={3};Database={4}; Maximum Pool Size={5};Application Name={6};SSL Mode={7};{8};{9}";
            return string.Format(connectionStringTemplate, host, port, user, password, databaseName, poolSize, ApplicationName, sslMode, serverCert, poolingParameter);
        }

        #region Invalid connection string behavior 

        private static Func<string> OnInvalidConnection { get; set; } = DefaultConnectionStringOnInvalidConnection;

        public static void ThrowAnErrorOnInvalidConnection()
        {
            OnInvalidConnection = ErrorOnInvalidConnection;
        }

        private static string ErrorOnInvalidConnection()
        {
            throw new Exception("Not found connection string params in the Environment vars.");
        }

        private static string DefaultConnectionStringOnInvalidConnection() => BuildConnectionString();

        #endregion Invalid connection string behavior 
    }
}