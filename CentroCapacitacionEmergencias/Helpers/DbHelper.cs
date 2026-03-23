using System.Configuration;
using System.Data.SqlClient;

namespace CentroCapacitacionEmergencias.Helpers
{
    public static class DbHelper
    {
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString
            );
        }
    }
}