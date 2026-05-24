using Dapper;
using HDMC.HardwareMinAlarm.Helpers;

namespace HDMC.HardwareMinAlarm.Repositories
{
    public class SystemSettingRepository
    {
        public string GetValue(string settingKey)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT TOP 1 setting_value
                    FROM System_Settings
                    WHERE setting_key = @SettingKey";

                return connection.QueryFirstOrDefault<string>(
                    sql,
                    new
                    {
                        SettingKey = settingKey
                    });
            }
        }
    }
}
