using System.Configuration;
using System.Data.SqlClient;

namespace HDMC.HardwareMinAlarm.Helpers
{
    public static class DbConnectionFactory
    {
        public static SqlConnection CreateSharedAuthConnection()
        {
            var connectionString =
                ConfigurationManager
                    .ConnectionStrings["SharedAuthDB"]
                    .ConnectionString;

            return new SqlConnection(connectionString);
        }

        public static SqlConnection CreateHardwareConnection()
        {
            var connectionString =
                ConfigurationManager
                    .ConnectionStrings["HardwareDb"]
                    .ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}
