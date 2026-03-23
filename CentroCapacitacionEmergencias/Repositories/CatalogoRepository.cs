using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Repositories
{
    public class CatalogoRepository
    {
        public List<Cohorte> GetCohortes(bool incluirArchivadas = false)
        {
            var lista = new List<Cohorte>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT Id, Nombre, FechaInicio, FechaFin, Archivado, Activo
                    FROM Cohortes
                    WHERE (@IncluirArchivadas = 1 OR Archivado = 0)
                    ORDER BY Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IncluirArchivadas", incluirArchivadas);
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

        public Cohorte GetCohorteById(int id)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT Id, Nombre, FechaInicio, FechaFin, Archivado, Activo FROM Cohortes WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return null;

                        return new Cohorte
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Nombre = dr["Nombre"].ToString(),
                            FechaInicio = Convert.ToDateTime(dr["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(dr["FechaFin"]),
                            Archivado = Convert.ToBoolean(dr["Archivado"]),
                            Activo = Convert.ToBoolean(dr["Activo"])
                        };
                    }
                }
            }
        }

        public void CrearCohorte(Cohorte model)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                using (var cmdVal = new SqlCommand("SELECT COUNT(1) FROM Cohortes WHERE Nombre = @Nombre", cn))
                {
                    cmdVal.Parameters.AddWithValue("@Nombre", model.Nombre);
                    if (Convert.ToInt32(cmdVal.ExecuteScalar()) > 0)
                        throw new Exception("Ya existe una cohorte con ese nombre.");
                }

                using (var cmd = new SqlCommand("INSERT INTO Cohortes (Nombre, FechaInicio, FechaFin, Archivado, Activo) VALUES (@Nombre, @FechaInicio, @FechaFin, 0, 1)", cn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@FechaInicio", model.FechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", model.FechaFin);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void EditarCohorte(Cohorte model)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                using (var cmdFecha = new SqlCommand("SELECT FechaFin FROM Cohortes WHERE Id = @Id", cn))
                {
                    cmdFecha.Parameters.AddWithValue("@Id", model.Id);
                    DateTime fechaFinActual = Convert.ToDateTime(cmdFecha.ExecuteScalar());
                    if (fechaFinActual.Date < DateTime.Today)
                        throw new Exception("No se puede editar una cohorte que ya finalizó.");
                }

                using (var cmdVal = new SqlCommand("SELECT COUNT(1) FROM Cohortes WHERE Nombre = @Nombre AND Id <> @Id", cn))
                {
                    cmdVal.Parameters.AddWithValue("@Nombre", model.Nombre);
                    cmdVal.Parameters.AddWithValue("@Id", model.Id);
                    if (Convert.ToInt32(cmdVal.ExecuteScalar()) > 0)
                        throw new Exception("Ya existe otra cohorte con ese nombre.");
                }

                using (var cmd = new SqlCommand("UPDATE Cohortes SET Nombre = @Nombre, FechaInicio = @FechaInicio, FechaFin = @FechaFin WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@FechaInicio", model.FechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", model.FechaFin);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ToggleArchivadoCohorte(int id)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                using (var cmdVal = new SqlCommand("SELECT COUNT(1) FROM ParticipanteAsignacion WHERE CohorteId = @Id AND Activo = 1", cn))
                {
                    cmdVal.Parameters.AddWithValue("@Id", id);
                    if (Convert.ToInt32(cmdVal.ExecuteScalar()) > 0)
                        throw new Exception("La cohorte tiene participantes activos asignados. Debe archivarse solo cuando no existan asignaciones activas.");
                }

                using (var cmd = new SqlCommand("UPDATE Cohortes SET Archivado = CASE WHEN Archivado = 1 THEN 0 ELSE 1 END WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Curso> GetCursos(bool incluirArchivados = false)
        {
            var lista = new List<Curso>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
                    SELECT Id, Codigo, Titulo, TotalHorasPracticas, Archivado, Activo
                    FROM Cursos
                    WHERE (@IncluirArchivados = 1 OR Archivado = 0)
                    ORDER BY Titulo";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IncluirArchivados", incluirArchivados);
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

        public Curso GetCursoById(int id)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT Id, Codigo, Titulo, TotalHorasPracticas, Archivado, Activo FROM Cursos WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return null;

                        return new Curso
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Codigo = dr["Codigo"].ToString(),
                            Titulo = dr["Titulo"].ToString(),
                            TotalHorasPracticas = Convert.ToInt32(dr["TotalHorasPracticas"]),
                            Archivado = Convert.ToBoolean(dr["Archivado"]),
                            Activo = Convert.ToBoolean(dr["Activo"])
                        };
                    }
                }
            }
        }

        public List<Usuario> GetInstructores()
        {
            var lista = new List<Usuario>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                string sql = @"
                    SELECT u.Id, u.NombreCompleto, u.Correo, u.Username, r.Nombre AS RolNombre
                    FROM Usuarios u
                    INNER JOIN Roles r ON r.Id = u.RolId
                    WHERE r.Nombre = 'Instructor' AND u.Activo = 1
                    ORDER BY u.NombreCompleto";

                using (var cmd = new SqlCommand(sql, cn))
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            NombreCompleto = dr["NombreCompleto"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Username = dr["Username"].ToString(),
                            RolNombre = dr["RolNombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        public List<int> GetInstructoresPorCurso(int cursoId)
        {
            var lista = new List<int>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT UsuarioId FROM CursoInstructor WHERE CursoId = @CursoId", cn))
                {
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            lista.Add(Convert.ToInt32(dr["UsuarioId"]));
                    }
                }
            }

            return lista;
        }

        public List<int> GetCohortesPorCurso(int cursoId)
        {
            var lista = new List<int>();

            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT CohorteId FROM CursoCohorte WHERE CursoId = @CursoId", cn))
                {
                    cmd.Parameters.AddWithValue("@CursoId", cursoId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            lista.Add(Convert.ToInt32(dr["CohorteId"]));
                    }
                }
            }

            return lista;
        }

        public void CrearCurso(Curso model, List<int> instructoresIds, List<int> cohortesIds)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    using (var cmdVal = new SqlCommand("SELECT COUNT(1) FROM Cursos WHERE Codigo = @Codigo OR Titulo = @Titulo", cn, tx))
                    {
                        cmdVal.Parameters.AddWithValue("@Codigo", model.Codigo);
                        cmdVal.Parameters.AddWithValue("@Titulo", model.Titulo);
                        if (Convert.ToInt32(cmdVal.ExecuteScalar()) > 0)
                            throw new Exception("Ya existe un curso con ese código o título.");
                    }

                    int maximo = JsonConfigHelper.GetInt("MaximoInstructoresPorCurso");
                    if (instructoresIds != null && instructoresIds.Count > maximo)
                        throw new Exception("La cantidad de instructores supera el máximo permitido.");

                    int cursoId;
                    using (var cmd = new SqlCommand("INSERT INTO Cursos (Codigo, Titulo, TotalHorasPracticas, Archivado, Activo) OUTPUT INSERTED.Id VALUES (@Codigo, @Titulo, @TotalHorasPracticas, 0, 1)", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Codigo", model.Codigo);
                        cmd.Parameters.AddWithValue("@Titulo", model.Titulo);
                        cmd.Parameters.AddWithValue("@TotalHorasPracticas", model.TotalHorasPracticas);
                        cursoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    InsertarRelacionesCurso(cn, tx, cursoId, instructoresIds, cohortesIds);
                    tx.Commit();
                }
            }
        }

        public void EditarCurso(Curso model, List<int> instructoresIds, List<int> cohortesIds)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    using (var cmdVal = new SqlCommand("SELECT COUNT(1) FROM Cursos WHERE (Codigo = @Codigo OR Titulo = @Titulo) AND Id <> @Id", cn, tx))
                    {
                        cmdVal.Parameters.AddWithValue("@Codigo", model.Codigo);
                        cmdVal.Parameters.AddWithValue("@Titulo", model.Titulo);
                        cmdVal.Parameters.AddWithValue("@Id", model.Id);
                        if (Convert.ToInt32(cmdVal.ExecuteScalar()) > 0)
                            throw new Exception("Ya existe otro curso con ese código o título.");
                    }

                    int maximo = JsonConfigHelper.GetInt("MaximoInstructoresPorCurso");
                    if (instructoresIds != null && instructoresIds.Count > maximo)
                        throw new Exception("La cantidad de instructores supera el máximo permitido.");

                    using (var cmd = new SqlCommand("UPDATE Cursos SET Codigo = @Codigo, Titulo = @Titulo, TotalHorasPracticas = @TotalHorasPracticas WHERE Id = @Id", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", model.Id);
                        cmd.Parameters.AddWithValue("@Codigo", model.Codigo);
                        cmd.Parameters.AddWithValue("@Titulo", model.Titulo);
                        cmd.Parameters.AddWithValue("@TotalHorasPracticas", model.TotalHorasPracticas);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmdDel1 = new SqlCommand("DELETE FROM CursoInstructor WHERE CursoId = @CursoId", cn, tx))
                    {
                        cmdDel1.Parameters.AddWithValue("@CursoId", model.Id);
                        cmdDel1.ExecuteNonQuery();
                    }

                    using (var cmdDel2 = new SqlCommand("DELETE FROM CursoCohorte WHERE CursoId = @CursoId", cn, tx))
                    {
                        cmdDel2.Parameters.AddWithValue("@CursoId", model.Id);
                        cmdDel2.ExecuteNonQuery();
                    }

                    InsertarRelacionesCurso(cn, tx, model.Id, instructoresIds, cohortesIds);
                    tx.Commit();
                }
            }
        }

        public void ToggleArchivadoCurso(int id)
        {
            using (var cn = DbHelper.GetConnection())
            {
                cn.Open();
                using (var cmd = new SqlCommand("UPDATE Cursos SET Archivado = CASE WHEN Archivado = 1 THEN 0 ELSE 1 END WHERE Id = @Id", cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InsertarRelacionesCurso(SqlConnection cn, SqlTransaction tx, int cursoId, List<int> instructoresIds, List<int> cohortesIds)
        {
            if (instructoresIds != null)
            {
                foreach (var instructorId in instructoresIds)
                {
                    using (var cmd = new SqlCommand("INSERT INTO CursoInstructor (CursoId, UsuarioId) VALUES (@CursoId, @UsuarioId)", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@CursoId", cursoId);
                        cmd.Parameters.AddWithValue("@UsuarioId", instructorId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            if (cohortesIds != null)
            {
                foreach (var cohorteId in cohortesIds)
                {
                    using (var cmd = new SqlCommand("INSERT INTO CursoCohorte (CursoId, CohorteId) VALUES (@CursoId, @CohorteId)", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@CursoId", cursoId);
                        cmd.Parameters.AddWithValue("@CohorteId", cohorteId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}