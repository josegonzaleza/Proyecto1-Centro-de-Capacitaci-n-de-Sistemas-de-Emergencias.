using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Repositories
{
    public class RiesgoDetalleRepository
    {
        public RiesgoDetalleViewModel GetRiesgoDetalle(int destrezaId)
        {
            var vm = new RiesgoDetalleViewModel
            {
                DestrezaId = destrezaId,
                DestrezaNombre = "",
                Participantes = new List<RiesgoParticipanteItem>()
            };

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                using (var cmdNombre = new SqlCommand("SELECT Nombre FROM Destrezas WHERE Id = @DestrezaId", cn))
                {
                    cmdNombre.Parameters.AddWithValue("@DestrezaId", destrezaId);
                    object result = cmdNombre.ExecuteScalar();
                    vm.DestrezaNombre = result == null ? "Destreza no encontrada" : result.ToString();
                }

                string sql = @"
                    ;WITH UltimaEvaluacion AS
                    (
                        SELECT
                            e.ParticipanteId,
                            e.CursoId,
                            e.DestrezaId,
                            e.PuntajeFinal,
                            e.EstadoDominio,
                            e.Comentarios,
                            e.FechaEvaluacion,
                            ROW_NUMBER() OVER (
                                PARTITION BY e.ParticipanteId, e.DestrezaId
                                ORDER BY e.FechaEvaluacion DESC, e.Id DESC
                            ) AS RN
                        FROM Evaluaciones e
                        INNER JOIN Destrezas d ON d.Id = e.DestrezaId
                        WHERE e.DestrezaId = @DestrezaId
                          AND e.PuntajeFinal < d.NotaMinimaAprobacion
                    )
                    SELECT
                        p.Id AS ParticipanteId,
                        p.CodigoInterno,
                        p.NombreCompleto,
                        p.Identificacion,
                        ue.PuntajeFinal,
                        ue.EstadoDominio,
                        ue.Comentarios,
                        ue.FechaEvaluacion,
                        c.Titulo AS CursoTitulo,
                        co.Nombre AS CohorteNombre
                    FROM UltimaEvaluacion ue
                    INNER JOIN Participantes p ON p.Id = ue.ParticipanteId
                    INNER JOIN Cursos c ON c.Id = ue.CursoId
                    LEFT JOIN ParticipanteAsignacion pa
                        ON pa.ParticipanteId = p.Id
                       AND pa.CursoId = ue.CursoId
                       AND pa.Activo = 1
                    LEFT JOIN Cohortes co ON co.Id = pa.CohorteId
                    WHERE ue.RN = 1
                    ORDER BY ue.PuntajeFinal ASC, p.NombreCompleto ASC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@DestrezaId", destrezaId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            vm.Participantes.Add(new RiesgoParticipanteItem
                            {
                                ParticipanteId = Convert.ToInt32(dr["ParticipanteId"]),
                                CodigoInterno = dr["CodigoInterno"].ToString(),
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                Identificacion = dr["Identificacion"].ToString(),
                                UltimoPuntajeFinal = Convert.ToDecimal(dr["PuntajeFinal"]),
                                EstadoDominio = dr["EstadoDominio"].ToString(),
                                CursoTitulo = dr["CursoTitulo"].ToString(),
                                CohorteNombre = dr["CohorteNombre"] == DBNull.Value ? "" : dr["CohorteNombre"].ToString(),
                                FechaEvaluacionTexto = Convert.ToDateTime(dr["FechaEvaluacion"]).ToString("yyyy-MM-dd HH:mm"),
                                Comentarios = dr["Comentarios"] == DBNull.Value ? "" : dr["Comentarios"].ToString()
                            });
                        }
                    }
                }
            }

            return vm;
        }
    }
}