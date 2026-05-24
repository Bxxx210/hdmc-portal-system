using System;
using System.Configuration;
using HDMC.HardwareMinAlarm.Repositories;

namespace HDMC.HardwareMinAlarm.Services
{
    public class SystemSettingService
    {
        public const string PortalLoginUrlKey = "PortalLoginUrl";

        private readonly SystemSettingRepository _repository;

        public SystemSettingService()
            : this(new SystemSettingRepository())
        {
        }

        public SystemSettingService(SystemSettingRepository repository)
        {
            _repository = repository;
        }

        public string GetValue(
            string settingKey,
            string appSettingKey,
            string defaultValue)
        {
            var dbValue = GetDatabaseValue(settingKey);

            if (!string.IsNullOrWhiteSpace(dbValue))
            {
                return dbValue;
            }

            var appSettingValue =
                ConfigurationManager.AppSettings[appSettingKey];

            if (!string.IsNullOrWhiteSpace(appSettingValue))
            {
                return appSettingValue;
            }

            return defaultValue;
        }

        private string GetDatabaseValue(string settingKey)
        {
            try
            {
                return _repository.GetValue(settingKey);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
