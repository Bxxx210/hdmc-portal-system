using System.Configuration;
using System.Data.SqlClient;

namespace HDMC.Portal.Helpers
{
    public static class DbConnectionFactory
    {
        public static SqlConnection CreateSharedAuthConnection()
        {
            var connectionString =
                ConfigurationManager.ConnectionStrings["SharedAuthDb"].ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}
