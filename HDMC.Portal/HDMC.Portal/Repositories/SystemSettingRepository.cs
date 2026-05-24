using Dapper;
using HDMC.Portal.Helpers;

namespace HDMC.Portal.Repositories
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

        public void SaveValue(
            string settingKey,
            string settingValue)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    IF EXISTS
                    (
                        SELECT 1
                        FROM System_Settings
                        WHERE setting_key = @SettingKey
                    )
                    BEGIN
                        UPDATE System_Settings
                        SET
                            setting_value = @SettingValue,
                            updated_date = GETDATE()
                        WHERE setting_key = @SettingKey
                    END
                    ELSE
                    BEGIN
                        INSERT INTO System_Settings
                        (
                            setting_key,
                            setting_value,
                            updated_date
                        )
                        VALUES
                        (
                            @SettingKey,
                            @SettingValue,
                            GETDATE()
                        )
                    END";

                connection.Execute(
                    sql,
                    new
                    {
                        SettingKey = settingKey,
                        SettingValue = settingValue
                    });
            }
        }
    }
}
