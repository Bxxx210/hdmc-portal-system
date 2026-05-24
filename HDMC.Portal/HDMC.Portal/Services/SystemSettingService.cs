using System;
using System.Configuration;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Services
{
    public class SystemSettingService
    {
        public const string HardwareEntryUrlKey = "HardwareEntryUrl";
        public const string PortalLoginUrlKey = "PortalLoginUrl";

        private const string DefaultHardwareEntryUrl =
            "https://localhost:44316/Home/Entry";

        private const string DefaultPortalLoginUrl =
            "https://localhost:44370/Login";

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

        public SystemSettingModel GetSettings()
        {
            return new SystemSettingModel
            {
                HardwareEntryUrl = GetValue(
                    HardwareEntryUrlKey,
                    "HardwareEntryUrl",
                    DefaultHardwareEntryUrl),

                PortalLoginUrl = GetValue(
                    PortalLoginUrlKey,
                    "PortalLoginUrl",
                    DefaultPortalLoginUrl)
            };
        }

        public void SaveSettings(SystemSettingModel model)
        {
            SaveValue(HardwareEntryUrlKey, model.HardwareEntryUrl);
            SaveValue(PortalLoginUrlKey, model.PortalLoginUrl);
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

        private void SaveValue(
            string settingKey,
            string settingValue)
        {
            _repository.SaveValue(
                settingKey,
                settingValue?.Trim());
        }
    }
}
