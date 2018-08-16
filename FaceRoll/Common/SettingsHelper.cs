using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FaceRoll.Common
{
    public class SettingsHelper
    {
        public const string FaceApiSubscriptionKey = "FaceApiSubscriptionKey";
        public const string FaceApiRoot = "FaceApiRoot";
        public const string FaceApiPersonGroup = "FaceApiPersonGroup";
        public const string TimeBetweenPhotos = "TimeBetweenPhotos";
        public const string MatchFoundPause = "MatchFoundPause";
        public const string FaceBoxSize = "FaceBoxSize";

        public static void SaveSetting(string name, string value)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values[name] = value;
        }

        public static string ReadSettings(string name)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey(name))
            {
                return localSettings.Values[name].ToString();
            }

            return string.Empty;
        }
    }
}
