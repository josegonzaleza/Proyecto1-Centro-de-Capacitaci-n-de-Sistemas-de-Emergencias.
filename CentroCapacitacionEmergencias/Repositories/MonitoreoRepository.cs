using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Repositories
{
    public class MonitoreoRepository
    {
        public List<Cohorte> GetCohortesDelInstructor(int instructorId)
        {
            var lista = new List<Cohorte>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT DISTINCT co.Id, co.Nombre, co.FechaInicio, co.FechaFin, co.Archivado, co.Activo
                    FROM CursoInstructor ci
                    INNER JOIN CursoCohorte cc ON cc.CursoId = ci.CursoId
                    INNER JOIN Cohortes co ON co.Id = cc.CohorteId
                    WHERE ci.UsuarioId = @InstructorId
                      AND co.Archivado = 0
                    ORDER BY co.Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@InstructorId", instructorId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Cohorte
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Nombre = dr["Nombre"].ToString(),
                                FechaInicio = Convert.ToDateTime(dr["FechaInicio"]),
                                FechaFin = Convert.ToDateTime(dr["FechaFin"]),
                                Archivado = Convert.ToBoolean(dr["Archivado"]),
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public List<Curso> GetCursosDelInstructor(int instructorId)
        {
            var lista = new List<Curso>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT DISTINCT c.Id, c.Codigo, c.Titulo, c.TotalHorasPracticas, c.Archivado, c.Activo
                    FROM CursoInstructor ci
                    INNER JOIN Cursos c ON c.Id = ci.CursoId
                    WHERE ci.UsuarioId = @InstructorId
                      AND c.Archivado = 0
                    ORDER BY c.Titulo";

                using (var cmd = new SqlCommand(sql, cn))
                {
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

        public bool InstructorTieneAcceso(int instructorId, int cohorteId, int cursoId)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT COUNT(1)
                    FROM CursoInstructor ci
                    INNER JOIN CursoCohorte cc ON cc.CursoId = ci.CursoId
                    WHERE ci.UsuarioId = @InstructorId
                      AND ci.CursoId = @CursoId
                      AND cc.CohorteId = @CohorteId";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@InstructorId", instructorId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public MonitoreoViewModel GetResumen(int cohorteId, int cursoId)
        {
            var vm = new MonitoreoViewModel
            {
                Riesgos = new List<RiesgoItem>()
            };

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sqlAprobacion = @"
                    SELECT ISNULL(
                        CAST(SUM(CASE WHEN e.PuntajeFinal >= d.NotaMinimaAprobacion THEN 1 ELSE 0 END) * 100.0
                        / NULLIF(COUNT(*), 0) AS DECIMAL(10,2)), 0)
                    FROM Evaluaciones e
                    INNER JOIN Destrezas d ON d.Id = e.DestrezaId
                    INNER JOIN ParticipanteAsignacion pa ON pa.ParticipanteId = e.ParticipanteId AND pa.CursoId = e.CursoId
                    WHERE pa.CohorteId = @CohorteId
                      AND e.CursoId = @CursoId
                      AND pa.Activo = 1";

                using (var cmd = new SqlCommand(sqlAprobacion, cn))
                {
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    vm.TasaGlobalAprobacion = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                string sqlCert = @"
                    SELECT COUNT(DISTINCT p.Id)
                    FROM Participantes p
                    INNER JOIN ParticipanteAsignacion pa ON pa.ParticipanteId = p.Id AND pa.Activo = 1
                    INNER JOIN HorasPracticas hp ON hp.ParticipanteId = p.Id AND hp.CursoId = pa.CursoId
                    INNER JOIN Cursos c ON c.Id = pa.CursoId
                    WHERE pa.CohorteId = @CohorteId
                      AND pa.CursoId = @CursoId
                      AND p.EstadoCertificacion = 'Certificado'
                      AND hp.HorasCompletadas >= c.TotalHorasPracticas";

                using (var cmd = new SqlCommand(sqlCert, cn))
                {
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    vm.EstadoCertificacion = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int porcentajePendiente = JsonConfigHelper.GetInt("HorasPracticasPendientesPorcentaje");

                string sqlIntervencion = @"
                    SELECT COUNT(*)
                    FROM HorasPracticas hp
                    INNER JOIN ParticipanteAsignacion pa ON pa.ParticipanteId = hp.ParticipanteId AND pa.CursoId = hp.CursoId AND pa.Activo = 1
                    INNER JOIN Cursos c ON c.Id = hp.CursoId
                    WHERE pa.CohorteId = @CohorteId
                      AND hp.CursoId = @CursoId
                      AND (hp.HorasCompletadas * 100.0 / NULLIF(c.TotalHorasPracticas, 0)) < @Porcentaje";

                using (var cmd = new SqlCommand(sqlIntervencion, cn))
                {
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    cmd.Parameters.AddWithValue("@Porcentaje", porcentajePendiente);
                    vm.IntervencionPendiente = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sqlRiesgos = @"
                    SELECT TOP 10 d.Id, d.Nombre, COUNT(*) AS Cantidad
                    FROM Evaluaciones e
                    INNER JOIN Destrezas d ON d.Id = e.DestrezaId
                    INNER JOIN ParticipanteAsignacion pa ON pa.ParticipanteId = e.ParticipanteId AND pa.CursoId = e.CursoId AND pa.Activo = 1
                    WHERE pa.CohorteId = @CohorteId
                      AND e.CursoId = @CursoId
                      AND e.PuntajeFinal < d.NotaMinimaAprobacion
                    GROUP BY d.Id, d.Nombre
                    ORDER BY Cantidad DESC";

                using (var cmd = new SqlCommand(sqlRiesgos, cn))
                {
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            vm.Riesgos.Add(new RiesgoItem
                            {
                                DestrezaId = Convert.ToInt32(dr["Id"]),
                                Destreza = dr["Nombre"].ToString(),
                                Cantidad = Convert.ToInt32(dr["Cantidad"])
                            });
                        }
                    }
                }
            }

            return vm;
        }

        public List<IntervencionPendienteItem> GetIntervencionPendiente(int cohorteId, int cursoId)
        {
            var lista = new List<IntervencionPendienteItem>();
            int porcentajePendiente = JsonConfigHelper.GetInt("HorasPracticasPendientesPorcentaje");

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT
                        p.Id AS ParticipanteId,
                        p.CodigoInterno,
                        p.NombreCompleto,
                        c.Titulo AS CursoTitulo,
                        co.Nombre AS CohorteNombre,
                        hp.HorasCompletadas,
                        c.TotalHorasPracticas,
                        CAST((hp.HorasCompletadas * 100.0 / NULLIF(c.TotalHorasPracticas, 0)) AS DECIMAL(10,2)) AS PorcentajeCumplimiento
                    FROM HorasPracticas hp
                    INNER JOIN Participantes p ON p.Id = hp.ParticipanteId
                    INNER JOIN Cursos c ON c.Id = hp.CursoId
                    INNER JOIN ParticipanteAsignacion pa ON pa.ParticipanteId = hp.ParticipanteId AND pa.CursoId = hp.CursoId AND pa.Activo = 1
                    INNER JOIN Cohortes co ON co.Id = pa.CohorteId
                    WHERE pa.CohorteId = @CohorteId
                      AND hp.CursoId = @CursoId
                      AND (hp.HorasCompletadas * 100.0 / NULLIF(c.TotalHorasPracticas, 0)) < @Porcentaje
                    ORDER BY PorcentajeCumplimiento ASC, p.NombreCompleto ASC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    cmd.Parameters.AddWithValue("@Porcentaje", porcentajePendiente);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new IntervencionPendienteItem
                            {
                                ParticipanteId = Convert.ToInt32(dr["ParticipanteId"]),
                                CodigoInterno = dr["CodigoInterno"].ToString(),
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                CursoTitulo = dr["CursoTitulo"].ToString(),
                                CohorteNombre = dr["CohorteNombre"].ToString(),
                                HorasCompletadas = Convert.ToDecimal(dr["HorasCompletadas"]),
                                HorasRequeridas = Convert.ToInt32(dr["TotalHorasPracticas"]),
                                PorcentajeCumplimiento = Convert.ToDecimal(dr["PorcentajeCumplimiento"])
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}