using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace CentroCapacitacionEmergencias.Helpers
{
    public static class JsonConfigHelper
    {
        private static Dictionary<string, object> ReadJson()
        {
            var path = HttpContext.Current.Server.MapPath(
                ConfigurationManager.AppSettings["SecuritySettingsPath"]
            );

            var json = File.ReadAllText(path);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<Dictionary<string, object>>(json);
        }

        public static int GetInt(string key)
        {
            var data = ReadJson();
            return Convert.ToInt32(data[key]);
        }
    }
}