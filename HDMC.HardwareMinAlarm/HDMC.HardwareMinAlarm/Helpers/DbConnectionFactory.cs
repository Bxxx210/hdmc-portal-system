using System.Configuration;
using System.Data.SqlClient;

namespace HDMC.HardwareMinAlarm.Helpers
{
    public static class DbConnectionFactory
    {
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
