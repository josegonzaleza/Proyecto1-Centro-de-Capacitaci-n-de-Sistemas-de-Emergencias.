using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Repositories
{
    public class EvaluacionRepository
    {
        public Participante GetParticipante(int participanteId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = "SELECT * FROM Participantes WHERE Id = @Id AND Activo = 1";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", participanteId);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return null;

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
        }

        public List<Curso> GetCursosDelParticipante(int participanteId, int instructorId)
        {
            var lista = new List<Curso>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT DISTINCT c.Id, c.Codigo, c.Titulo, c.TotalHorasPracticas, c.Archivado, c.Activo
                    FROM ParticipanteAsignacion pa
                    INNER JOIN Cursos c ON c.Id = pa.CursoId
                    INNER JOIN CursoInstructor ci ON ci.CursoId = c.Id
                    WHERE pa.ParticipanteId = @ParticipanteId
                      AND pa.Activo = 1
                      AND ci.UsuarioId = @InstructorId
                      AND c.Archivado = 0
                      AND c.Activo = 1
                    ORDER BY c.Titulo";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    cmd.Parameters.AddWithValue("@InstructorId", instructorId);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Curso
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Codigo = dr["Codigo"].ToString(),
                                Titulo = dr["Titulo"].ToString(),
                                TotalHorasPracticas = Convert.ToInt32(dr["TotalHorasPracticas"]),
                                Archivado = Convert.ToBoolean(dr["Archivado"]),
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public List<DestrezaAjaxItem> GetDestrezasPorCurso(int cursoId)
        {
            var lista = new List<DestrezaAjaxItem>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT Id, Nombre
                    FROM Destrezas
                    WHERE CursoId = @CursoId AND Activa = 1
                    ORDER BY Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new DestrezaAjaxItem
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Nombre = dr["Nombre"].ToString()
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public List<PuntoControlAjaxItem> GetPuntosControlAjax(int destrezaId)
        {
            var lista = new List<PuntoControlAjaxItem>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT Id, Descripcion
                    FROM PuntoControlCritico
                    WHERE DestrezaId = @DestrezaId AND Activo = 1
                    ORDER BY Id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@DestrezaId", destrezaId);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new PuntoControlAjaxItem
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Descripcion = dr["Descripcion"].ToString()
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public string GetCursoNombre(int cursoId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT Titulo FROM Cursos WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", cursoId);
                    object result = cmd.ExecuteScalar();
                    return result == null ? "" : result.ToString();
                }
            }
        }

        public string GetDestrezaNombre(int destrezaId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT Nombre FROM Destrezas WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", destrezaId);
                    object result = cmd.ExecuteScalar();
                    return result == null ? "" : result.ToString();
                }
            }
        }

        public bool CursoPerteneceAInstructor(int cursoId, int instructorId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM CursoInstructor WHERE CursoId = @CursoId AND UsuarioId = @InstructorId", cn))
                {
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    cmd.Parameters.AddWithValue("@InstructorId", instructorId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool ParticipanteTieneCursoActivo(int participanteId, int cursoId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM ParticipanteAsignacion WHERE ParticipanteId = @ParticipanteId AND CursoId = @CursoId AND Activo = 1", cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool YaCertificadaConMaxima(int participanteId, int destrezaId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT COUNT(1)
                    FROM Evaluaciones
                    WHERE ParticipanteId = @ParticipanteId
                      AND DestrezaId = @DestrezaId
                      AND PuntajeFinal = 100";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                    cmd.Parameters.AddWithValue("@DestrezaId", destrezaId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public decimal CalcularPuntajeFinal(decimal puntajeOriginal, List<PuntoControlItem> puntosControl)
        {
            bool falloPuntoCritico = puntosControl.Exists(x => x.Cumplido.HasValue && x.Cumplido.Value == false);
            int notaMaximaPorFallo = JsonConfigHelper.GetInt("NotaMaximaSiFallaPuntoCritico");

            if (falloPuntoCritico && puntajeOriginal > notaMaximaPorFallo)
                return notaMaximaPorFallo;

            return puntajeOriginal;
        }

        public string CalcularEstadoDominio(decimal puntajeFinal)
        {
            if (puntajeFinal >= 90) return "Dominada";
            if (puntajeFinal >= 70) return "En Progreso";
            return "Requiere Refuerzo";
        }

        public void Guardar(EvaluacionViewModel model, int instructorId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                using (var tx = cn.BeginTransaction())
                {
                    decimal puntajeFinal = CalcularPuntajeFinal(model.PuntajeOriginal, model.PuntosControl);
                    string estadoDominio = CalcularEstadoDominio(puntajeFinal);
                    int tiempoTotalSegundos = (model.TiempoMinutos * 60) + model.TiempoSegundos;

                    string sqlEval = @"
                        INSERT INTO Evaluaciones
                        (
                            ParticipanteId,
                            CursoId,
                            DestrezaId,
                            InstructorId,
                            TiempoRespuestaSegundos,
                            TiempoMinutos,
                            TiempoSegundos,
                            PuntajeOriginal,
                            PuntajeFinal,
                            AceptacionInstructor,
                            Comentarios,
                            FechaEvaluacion,
                            EstadoDominio
                        )
                        OUTPUT INSERTED.Id
                        VALUES
                        (
                            @ParticipanteId,
                            @CursoId,
                            @DestrezaId,
                            @InstructorId,
                            @TiempoRespuestaSegundos,
                            @TiempoMinutos,
                            @TiempoSegundos,
                            @PuntajeOriginal,
                            @PuntajeFinal,
                            @AceptacionInstructor,
                            @Comentarios,
                            GETDATE(),
                            @EstadoDominio
                        )";

                    int evaluacionId;

                    using (var cmd = new SqlCommand(sqlEval, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@ParticipanteId", model.ParticipanteId);
                        cmd.Parameters.AddWithValue("@CursoId", model.CursoId);
                        cmd.Parameters.AddWithValue("@DestrezaId", model.DestrezaId);
                        cmd.Parameters.AddWithValue("@InstructorId", instructorId);
                        cmd.Parameters.AddWithValue("@TiempoRespuestaSegundos", tiempoTotalSegundos);
                        cmd.Parameters.AddWithValue("@TiempoMinutos", model.TiempoMinutos);
                        cmd.Parameters.AddWithValue("@TiempoSegundos", model.TiempoSegundos);
                        cmd.Parameters.AddWithValue("@PuntajeOriginal", model.PuntajeOriginal);
                        cmd.Parameters.AddWithValue("@PuntajeFinal", puntajeFinal);
                        cmd.Parameters.AddWithValue("@AceptacionInstructor", model.AceptacionInstructor);
                        cmd.Parameters.AddWithValue("@Comentarios", string.IsNullOrWhiteSpace(model.Comentarios) ? (object)DBNull.Value : model.Comentarios.Trim());
                        cmd.Parameters.AddWithValue("@EstadoDominio", estadoDominio);

                        evaluacionId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (var item in model.PuntosControl)
                    {
                        using (var cmdPunto = new SqlCommand("INSERT INTO EvaluacionPuntoControl (EvaluacionId, PuntoControlId, Cumplido) VALUES (@EvaluacionId, @PuntoControlId, @Cumplido)", cn, tx))
                        {
                            cmdPunto.Parameters.AddWithValue("@EvaluacionId", evaluacionId);
                            cmdPunto.Parameters.AddWithValue("@PuntoControlId", item.PuntoControlId);
                            cmdPunto.Parameters.AddWithValue("@Cumplido", item.Cumplido.HasValue && item.Cumplido.Value);
                            cmdPunto.ExecuteNonQuery();
                        }
                    }

                    ActualizarEstadoCertificacion(cn, tx, model.ParticipanteId, model.CursoId);

                    tx.Commit();
                }
            }
        }

        private void ActualizarEstadoCertificacion(SqlConnection cn, SqlTransaction tx, int participanteId, int cursoId)
        {
            string sql = @"
                DECLARE @HorasOk BIT = 0;
                DECLARE @DestrezasPendientes INT = 0;

                IF EXISTS (
                    SELECT 1
                    FROM HorasPracticas hp
                    INNER JOIN Cursos c ON c.Id = hp.CursoId
                    WHERE hp.ParticipanteId = @ParticipanteId
                      AND hp.CursoId = @CursoId
                      AND hp.HorasCompletadas >= c.TotalHorasPracticas
                )
                    SET @HorasOk = 1;

                SELECT @DestrezasPendientes = COUNT(*)
                FROM Destrezas d
                WHERE d.CursoId = @CursoId
                  AND d.Activa = 1
                  AND NOT EXISTS (
                        SELECT 1
                        FROM Evaluaciones e
                        WHERE e.ParticipanteId = @ParticipanteId
                          AND e.DestrezaId = d.Id
                          AND e.PuntajeFinal >= d.NotaMinimaAprobacion
                  );

                UPDATE Participantes
                SET EstadoCertificacion =
                    CASE
                        WHEN @HorasOk = 1 AND @DestrezasPendientes = 0 THEN 'Certificado'
                        ELSE 'En Proceso'
                    END
                WHERE Id = @ParticipanteId";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
                cmd.Parameters.AddWithValue("@CursoId", cursoId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}