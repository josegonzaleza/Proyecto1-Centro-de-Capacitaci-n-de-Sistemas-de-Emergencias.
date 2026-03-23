using System;
using System.Data.SqlClient;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Repositories
{
    public class AccountRepository
    {
        public Usuario GetByUsername(string username)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT u.Id,
                           u.NombreCompleto,
                           u.Correo,
                           u.Username,
                           u.PasswordHash,
                           u.RolId,
                           r.Nombre AS RolNombre,
                           u.Activo,
                           u.IntentosFallidos,
                           u.BloqueadoHasta
                    FROM Usuarios u
                    INNER JOIN Roles r ON r.Id = u.RolId
                    WHERE u.Username = @Username";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                            return null;

                        return new Usuario
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            NombreCompleto = dr["NombreCompleto"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Username = dr["Username"].ToString(),
                            PasswordHash = dr["PasswordHash"].ToString(),
                            RolId = Convert.ToInt32(dr["RolId"]),
                            RolNombre = dr["RolNombre"].ToString(),
                            Activo = Convert.ToBoolean(dr["Activo"]),
                            IntentosFallidos = Convert.ToInt32(dr["IntentosFallidos"]),
                            BloqueadoHasta = dr["BloqueadoHasta"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(dr["BloqueadoHasta"])
                        };
                    }
                }
            }
        }

        public void RegisterFailedAttempt(int userId, int maxAttempts, int lockMinutes)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    UPDATE Usuarios
                    SET IntentosFallidos = IntentosFallidos + 1,
                        BloqueadoHasta = CASE
                            WHEN IntentosFallidos + 1 >= @MaxAttempts
                                THEN DATEADD(MINUTE, @LockMinutes, GETDATE())
                            ELSE BloqueadoHasta
                        END
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    cmd.Parameters.AddWithValue("@MaxAttempts", maxAttempts);
                    cmd.Parameters.AddWithValue("@LockMinutes", lockMinutes);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ResetFailedAttempts(int userId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    UPDATE Usuarios
                    SET IntentosFallidos = 0,
                        BloqueadoHasta = NULL
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}