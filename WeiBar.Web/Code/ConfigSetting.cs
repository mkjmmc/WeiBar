using System.Configuration;

namespace WeiBar.Web.Code
{
    public class ConfigSetting
    {
        public static string WeiXinauthUrl = ConfigurationManager.AppSettings["WeiXinauthUrl"];
        public static string WeiXinAppID = ConfigurationManager.AppSettings["WeiXinAppID"];
        public static string WeiXinAppSecret = ConfigurationManager.AppSettings["WeiXinAppSecret"];

        public static bool isDebug = int.Parse(ConfigurationManager.AppSettings["Debug"].ToString()) == 1;
    }
}