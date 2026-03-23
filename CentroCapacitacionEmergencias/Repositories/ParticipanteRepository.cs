using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Repositories
{
    public class ParticipanteRepository
    {
        public List<Participante> Buscar(string texto, int? cohorteId, int? cursoId)
        {
            var lista = new List<Participante>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT DISTINCT p.*
                    FROM Participantes p
                    LEFT JOIN ParticipanteAsignacion pa ON pa.ParticipanteId = p.Id AND pa.Activo = 1
                    WHERE p.Activo = 1
                      AND (@Texto IS NULL OR p.NombreCompleto LIKE '%' + @Texto + '%' OR p.Identificacion LIKE '%' + @Texto + '%')
                      AND (@CohorteId IS NULL OR pa.CohorteId = @CohorteId)
                      AND (@CursoId IS NULL OR pa.CursoId = @CursoId)
                    ORDER BY p.NombreCompleto";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto.Trim());
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId.HasValue ? (object)cohorteId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId.HasValue ? (object)cursoId.Value : DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(MapParticipante(dr));
                        }
                    }
                }
            }

            return lista;
        }

        public Participante GetById(int id)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = "SELECT * FROM Participantes WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return null;
                        return MapParticipante(dr);
                    }
                }
            }
        }

        public int Crear(ParticipanteFormViewModel model)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                ValidarDuplicados(cn, model.Identificacion, model.Correo, null);

                string codigoInterno = "P-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                string sql = @"
                    INSERT INTO Participantes
                    (
                        CodigoInterno,
                        TipoIdentificacion,
                        Identificacion,
                        NombreCompleto,
                        FechaNacimiento,
                        Provincia,
                        Canton,
                        Distrito,
                        DetalleDireccion,
                        EstadoCivil,
                        Correo,
                        Telefono,
                        DireccionResidenciaSecundaria,
                        ContactoEmergencia,
                        EstadoCertificacion,
                        Activo
                    )
                    OUTPUT INSERTED.Id
                    VALUES
                    (
                        @CodigoInterno,
                        @TipoIdentificacion,
                        @Identificacion,
                        @NombreCompleto,
                        @FechaNacimiento,
                        @Provincia,
                        @Canton,
                        @Distrito,
                        @DetalleDireccion,
                        @EstadoCivil,
                        @Correo,
                        @Telefono,
                        @DireccionResidenciaSecundaria,
                        @ContactoEmergencia,
                        'En Proceso',
                        1
                    )";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CodigoInterno", codigoInterno);
                    cmd.Parameters.AddWithValue("@TipoIdentificacion", model.TipoIdentificacion.Trim());
                    cmd.Parameters.AddWithValue("@Identificacion", model.Identificacion.Trim());
                    cmd.Parameters.AddWithValue("@NombreCompleto", model.NombreCompleto.Trim());
                    cmd.Parameters.AddWithValue("@FechaNacimiento", model.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@Provincia", model.Provincia.Trim());
                    cmd.Parameters.AddWithValue("@Canton", model.Canton.Trim());
                    cmd.Parameters.AddWithValue("@Distrito", model.Distrito.Trim());
                    cmd.Parameters.AddWithValue("@DetalleDireccion", model.DetalleDireccion.Trim());
                    cmd.Parameters.AddWithValue("@EstadoCivil", model.EstadoCivil.Trim());
                    cmd.Parameters.AddWithValue("@Correo", model.Correo.Trim());
                    cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(model.Telefono) ? (object)DBNull.Value : model.Telefono.Trim());
                    cmd.Parameters.AddWithValue("@DireccionResidenciaSecundaria", string.IsNullOrWhiteSpace(model.DireccionResidenciaSecundaria) ? (object)DBNull.Value : model.DireccionResidenciaSecundaria.Trim());
                    cmd.Parameters.AddWithValue("@ContactoEmergencia", string.IsNullOrWhiteSpace(model.ContactoEmergencia) ? (object)DBNull.Value : model.ContactoEmergencia.Trim());

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Editar(ParticipanteFormViewModel model)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                ValidarDuplicados(cn, null, model.Correo, model.Id);

                string sql = @"
                    UPDATE Participantes
                    SET NombreCompleto = @NombreCompleto,
                        FechaNacimiento = @FechaNacimiento,
                        Provincia = @Provincia,
                        Canton = @Canton,
                        Distrito = @Distrito,
                        DetalleDireccion = @DetalleDireccion,
                        EstadoCivil = @EstadoCivil,
                        Correo = @Correo,
                        Telefono = @Telefono,
                        DireccionResidenciaSecundaria = @DireccionResidenciaSecundaria,
                        ContactoEmergencia = @ContactoEmergencia
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@NombreCompleto", model.NombreCompleto.Trim());
                    cmd.Parameters.AddWithValue("@FechaNacimiento", model.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@Provincia", model.Provincia.Trim());
                    cmd.Parameters.AddWithValue("@Canton", model.Canton.Trim());
                    cmd.Parameters.AddWithValue("@Distrito", model.Distrito.Trim());
                    cmd.Parameters.AddWithValue("@DetalleDireccion", model.DetalleDireccion.Trim());
                    cmd.Parameters.AddWithValue("@EstadoCivil", model.EstadoCivil.Trim());
                    cmd.Parameters.AddWithValue("@Correo", model.Correo.Trim());
                    cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(model.Telefono) ? (object)DBNull.Value : model.Telefono.Trim());
                    cmd.Parameters.AddWithValue("@DireccionResidenciaSecundaria", string.IsNullOrWhiteSpace(model.DireccionResidenciaSecundaria) ? (object)DBNull.Value : model.DireccionResidenciaSecundaria.Trim());
                    cmd.Parameters.AddWithValue("@ContactoEmergencia", string.IsNullOrWhiteSpace(model.ContactoEmergencia) ? (object)DBNull.Value : model.ContactoEmergencia.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Desactivar(int id)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = "UPDATE Participantes SET Activo = 0 WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AsignarCohorteYCursos(int participanteId, int cohorteId, List<int> cursosIds)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    foreach (var cursoId in cursosIds)
                    {
                        string existeSql = @"
                            SELECT COUNT(1)
                            FROM ParticipanteAsignacion
                            WHERE ParticipanteId = @ParticipanteId
                              AND CohorteId = @CohorteId
                              AND CursoId = @CursoId
                              AND Activo = 1";

                        using (var cmdExiste = new SqlCommand(existeSql, cn, tx))
                        {
                            cmdExiste.Parameters.AddWithValue("@ParticipanteId", participanteId);
                            cmdExiste.Parameters.AddWithValue("@CohorteId", cohorteId);
                            cmdExiste.Parameters.AddWithValue("@CursoId", cursoId);

                            int existe = Convert.ToInt32(cmdExiste.ExecuteScalar());
                            if (existe == 0)
                            {
                                string insertSql = @"
                                    INSERT INTO ParticipanteAsignacion
                                    (ParticipanteId, CohorteId, CursoId, Activo, FechaAsignacion)
                                    VALUES
                                    (@ParticipanteId, @CohorteId, @CursoId, 1, GETDATE())";

                                using (var cmdInsert = new SqlCommand(insertSql, cn, tx))
                                {
                                    cmdInsert.Parameters.AddWithValue("@ParticipanteId", participanteId);
                                    cmdInsert.Parameters.AddWithValue("@CohorteId", cohorteId);
                                    cmdInsert.Parameters.AddWithValue("@CursoId", cursoId);
                                    cmdInsert.ExecuteNonQuery();
                                }

                                string horasSql = @"
                                    IF NOT EXISTS (
                                        SELECT 1 FROM HorasPracticas
                                        WHERE ParticipanteId = @ParticipanteId AND CursoId = @CursoId
                                    )
                                    INSERT INTO HorasPracticas
                                    (ParticipanteId, CursoId, HorasCompletadas, FechaActualizacion)
                                    VALUES
                                    (@ParticipanteId, @CursoId, 0, GETDATE())";

                                using (var cmdHoras = new SqlCommand(horasSql, cn, tx))
                                {
                                    cmdHoras.Parameters.AddWithValue("@ParticipanteId", participanteId);
                                    cmdHoras.Parameters.AddWithValue("@CursoId", cursoId);
                                    cmdHoras.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    tx.Commit();
                }
            }
        }

        public void RemoverCurso(int participanteId, int cursoId, string motivoCambio)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    UPDATE ParticipanteAsignacion
                    SET Activo = 0,
                        FechaFinAsignacion = GETDATE(),
                        MotivoCambio = @MotivoCambio
                    WHERE ParticipanteId = @ParticipanteId
                      AND CursoId = @CursoId
                      AND Activo = 1";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    cmd.Parameters.AddWithValue("@MotivoCambio", string.IsNullOrWhiteSpace(motivoCambio) ? (object)DBNull.Value : motivoCambio.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CambiarCohorte(int participanteId, int nuevaCohorteId, string motivoCambio)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    var cursosActivos = new List<int>();

                    string cursosSql = @"
                        SELECT CursoId
                        FROM ParticipanteAsignacion
                        WHERE ParticipanteId = @ParticipanteId AND Activo = 1";

                    using (var cmdCursos = new SqlCommand(cursosSql, cn, tx))
                    {
                        cmdCursos.Parameters.AddWithValue("@ParticipanteId", participanteId);
                        using (var dr = cmdCursos.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                cursosActivos.Add(Convert.ToInt32(dr["CursoId"]));
                            }
                        }
                    }

                    string desactivarSql = @"
                        UPDATE ParticipanteAsignacion
                        SET Activo = 0,
                            FechaFinAsignacion = GETDATE(),
                            MotivoCambio = @MotivoCambio
                        WHERE ParticipanteId = @ParticipanteId
                          AND Activo = 1";

                    using (var cmdDesactivar = new SqlCommand(desactivarSql, cn, tx))
                    {
                        cmdDesactivar.Parameters.AddWithValue("@ParticipanteId", participanteId);
                        cmdDesactivar.Parameters.AddWithValue("@MotivoCambio", string.IsNullOrWhiteSpace(motivoCambio) ? (object)DBNull.Value : motivoCambio.Trim());
                        cmdDesactivar.ExecuteNonQuery();
                    }

                    foreach (var cursoId in cursosActivos)
                    {
                        string insertSql = @"
                            INSERT INTO ParticipanteAsignacion
                            (ParticipanteId, CohorteId, CursoId, Activo, FechaAsignacion)
                            VALUES
                            (@ParticipanteId, @CohorteId, @CursoId, 1, GETDATE())";

                        using (var cmdInsert = new SqlCommand(insertSql, cn, tx))
                        {
                            cmdInsert.Parameters.AddWithValue("@ParticipanteId", participanteId);
                            cmdInsert.Parameters.AddWithValue("@CohorteId", nuevaCohorteId);
                            cmdInsert.Parameters.AddWithValue("@CursoId", cursoId);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        public string GetCohorteActual(int participanteId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT TOP 1 c.Nombre
                    FROM ParticipanteAsignacion pa
                    INNER JOIN Cohortes c ON c.Id = pa.CohorteId
                    WHERE pa.ParticipanteId = @ParticipanteId AND pa.Activo = 1
                    ORDER BY pa.FechaAsignacion DESC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    object result = cmd.ExecuteScalar();
                    return result == null ? "" : result.ToString();
                }
            }
        }

        public List<CursoAsignadoViewModel> GetCursosAsignadosDetalle(int participanteId)
        {
            var lista = new List<CursoAsignadoViewModel>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT c.Id, c.Titulo, pa.Activo
                    FROM ParticipanteAsignacion pa
                    INNER JOIN Cursos c ON c.Id = pa.CursoId
                    WHERE pa.ParticipanteId = @ParticipanteId
                    ORDER BY pa.Activo DESC, c.Titulo";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new CursoAsignadoViewModel
                            {
                                CursoId = Convert.ToInt32(dr["Id"]),
                                CursoTitulo = dr["Titulo"].ToString(),
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public List<string> GetHistorialDestrezas(int participanteId)
        {
            var lista = new List<string>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT d.Nombre, e.EstadoDominio, e.FechaEvaluacion
                    FROM Evaluaciones e
                    INNER JOIN Destrezas d ON d.Id = e.DestrezaId
                    WHERE e.ParticipanteId = @ParticipanteId
                    ORDER BY e.FechaEvaluacion DESC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(
                                dr["Nombre"].ToString() + " - " +
                                dr["EstadoDominio"].ToString() + " - " +
                                Convert.ToDateTime(dr["FechaEvaluacion"]).ToString("yyyy-MM-dd HH:mm")
                            );
                        }
                    }
                }
            }

            return lista;
        }

        public int GetCohorteAsignadaId(int participanteId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT TOP 1 CohorteId
                    FROM ParticipanteAsignacion
                    WHERE ParticipanteId = @ParticipanteId AND Activo = 1
                    ORDER BY FechaAsignacion DESC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public List<int> GetCursosAsignadosIds(int participanteId)
        {
            var lista = new List<int>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT CursoId
                    FROM ParticipanteAsignacion
                    WHERE ParticipanteId = @ParticipanteId AND Activo = 1";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(Convert.ToInt32(dr["CursoId"]));
                        }
                    }
                }
            }

            return lista;
        }

        private void ValidarDuplicados(SqlConnection cn, string identificacion, string correo, int? idExcluir)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Participantes
                WHERE
                    ((@Identificacion IS NOT NULL AND Identificacion = @Identificacion)
                    OR (@Correo IS NOT NULL AND Correo = @Correo))
                    AND (@IdExcluir IS NULL OR Id <> @IdExcluir)";

            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Identificacion", string.IsNullOrWhiteSpace(identificacion) ? (object)DBNull.Value : identificacion.Trim());
                cmd.Parameters.AddWithValue("@Correo", string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo.Trim());
                cmd.Parameters.AddWithValue("@IdExcluir", idExcluir.HasValue ? (object)idExcluir.Value : DBNull.Value);

                int existe = Convert.ToInt32(cmd.ExecuteScalar());
                if (existe > 0)
                    throw new Exception("Ya existe un participante con esa identificación o correo.");
            }
        }

        private Participante MapParticipante(SqlDataReader dr)
        {
            return new Participante
            {
                Id = Convert.ToInt32(dr["Id"]),
                CodigoInterno = dr["CodigoInterno"].ToString(),
                TipoIdentificacion = dr["TipoIdentificacion"].ToString(),
                Identificacion = dr["Identificacion"].ToString(),
                NombreCompleto = dr["NombreCompleto"].ToString(),
                FechaNacimiento = Convert.ToDateTime(dr["FechaNacimiento"]),
                Provincia = dr["Provincia"].ToString(),
                Canton = dr["Canton"].ToString(),
                Distrito = dr["Distrito"].ToString(),
                DetalleDireccion = dr["DetalleDireccion"].ToString(),
                EstadoCivil = dr["EstadoCivil"].ToString(),
                Correo = dr["Correo"].ToString(),
                Telefono = dr["Telefono"] == DBNull.Value ? "" : dr["Telefono"].ToString(),
                DireccionResidenciaSecundaria = dr["DireccionResidenciaSecundaria"] == DBNull.Value ? "" : dr["DireccionResidenciaSecundaria"].ToString(),
                ContactoEmergencia = dr["ContactoEmergencia"] == DBNull.Value ? "" : dr["ContactoEmergencia"].ToString(),
                EstadoCertificacion = dr["EstadoCertificacion"].ToString(),
                Activo = Convert.ToBoolean(dr["Activo"])
            };
        }
    }
}