using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Mapper;

namespace DAL
{

    public class PermisoDAL
    {
        private string connectionString;

        public PermisoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["BaseDatos"].ConnectionString;
        }

        public void CrearTablasSiNoExisten()
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                string crearPermiso = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permiso' AND xtype='U')
                    CREATE TABLE Permiso (
                        Id      INT IDENTITY(1,1) PRIMARY KEY,
                        Desc_   NVARCHAR(200) NOT NULL,
                        EsPadre BIT NOT NULL DEFAULT 0
                    );";
                using (var cmd = new SqlCommand(crearPermiso, con)) cmd.ExecuteNonQuery();

                string crearPP = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permiso_Permiso' AND xtype='U')
                    CREATE TABLE Permiso_Permiso (
                        IdPadre INT NOT NULL,
                        IdHijo  INT NOT NULL,
                        PRIMARY KEY (IdPadre, IdHijo),
                        FOREIGN KEY (IdPadre) REFERENCES Permiso(Id),
                        FOREIGN KEY (IdHijo)  REFERENCES Permiso(Id)
                    );";
                using (var cmd = new SqlCommand(crearPP, con)) cmd.ExecuteNonQuery();

                string crearUP = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuario_Permiso' AND xtype='U')
                    CREATE TABLE Usuario_Permiso (
                        IdUsuario INT NOT NULL,
                        IdPermiso INT NOT NULL,
                        PRIMARY KEY (IdUsuario, IdPermiso),
                        FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id) ON DELETE CASCADE,
                        FOREIGN KEY (IdPermiso) REFERENCES Permiso(Id) ON DELETE CASCADE
                    );";
                using (var cmd = new SqlCommand(crearUP, con)) cmd.ExecuteNonQuery();
            }
        }

        public GrupoPermiso CargarArbolDeUsuario(int idUsuario)
        {
            var raiz = new GrupoPermiso("Raiz");
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                foreach (var top in ObtenerTopNivelDeUsuario(con, idUsuario))
                    raiz.Agregar(ConstruirNodoRecursivo(con, top.Id, top.Desc, top.EsPadre));
            }
            return raiz;
        }

        public GrupoPermiso CargarArbolGlobal()
        {
            var raiz = new GrupoPermiso("Raiz");
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                foreach (var top in ObtenerRaicesGlobales(con))
                    raiz.Agregar(ConstruirNodoRecursivo(con, top.Id, top.Desc, top.EsPadre));
            }
            return raiz;
        }

        public bool GuardarArbolGlobal(GrupoPermiso raiz)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {

                        using (var delPP = new SqlCommand("DELETE FROM Permiso_Permiso", con, tx))
                            delPP.ExecuteNonQuery();
                        using (var delP = new SqlCommand("DELETE FROM Permiso", con, tx))
                            delP.ExecuteNonQuery();

                        foreach (var hijo in raiz.Hijos())
                            PersistirNodoRecursivo(con, tx, hijo);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }

        private List<PermisoRow> ObtenerRaicesGlobales(SqlConnection con)
        {
            var lista = new List<PermisoRow>();
            string sql = @"SELECT p.Id, p.Desc_, p.EsPadre
                           FROM Permiso p
                           WHERE NOT EXISTS (SELECT 1 FROM Permiso_Permiso pp WHERE pp.IdHijo = p.Id)
                           ORDER BY p.Desc_";
            using (var cmd = new SqlCommand(sql, con))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    lista.Add(new PermisoRow { Id = (int)r["Id"], Desc = r["Desc_"].ToString(), EsPadre = (bool)r["EsPadre"] });
            return lista;
        }

        public bool TienePermisos(int idUsuario)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM Usuario_Permiso WHERE IdUsuario = @id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        private List<PermisoRow> ObtenerTopNivelDeUsuario(SqlConnection con, int idUsuario)
        {
            var lista = new List<PermisoRow>();
            string sql = @"SELECT p.Id, p.Desc_, p.EsPadre
                           FROM Usuario_Permiso up
                           INNER JOIN Permiso p ON p.Id = up.IdPermiso
                           WHERE up.IdUsuario = @id
                           ORDER BY p.Desc_";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idUsuario);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new PermisoRow { Id = (int)r["Id"], Desc = r["Desc_"].ToString(), EsPadre = (bool)r["EsPadre"] });
            }
            return lista;
        }

        private List<PermisoRow> ObtenerHijos(SqlConnection con, int idPadre)
        {
            var lista = new List<PermisoRow>();
            string sql = @"SELECT p.Id, p.Desc_, p.EsPadre
                           FROM Permiso_Permiso pp
                           INNER JOIN Permiso p ON p.Id = pp.IdHijo
                           WHERE pp.IdPadre = @id
                           ORDER BY p.Desc_";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idPadre);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new PermisoRow { Id = (int)r["Id"], Desc = r["Desc_"].ToString(), EsPadre = (bool)r["EsPadre"] });
            }
            return lista;
        }

        private IComponentePermiso ConstruirNodoRecursivo(SqlConnection con, int id, string desc, bool esPadre)
        {
            if (!esPadre) return new PermisoLeaf(desc);
            var grupo = new GrupoPermiso(desc);
            foreach (var hijo in ObtenerHijos(con, id))
                grupo.Agregar(ConstruirNodoRecursivo(con, hijo.Id, hijo.Desc, hijo.EsPadre));
            return grupo;
        }

        public bool GuardarArbolDeUsuario(int idUsuario, GrupoPermiso raiz)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        var idsTopNivel = new List<int>();
                        foreach (var hijo in raiz.Hijos())
                            idsTopNivel.Add(PersistirNodoRecursivo(con, tx, hijo));

                        using (var del = new SqlCommand("DELETE FROM Usuario_Permiso WHERE IdUsuario = @id", con, tx))
                        {
                            del.Parameters.AddWithValue("@id", idUsuario);
                            del.ExecuteNonQuery();
                        }
                        foreach (int idPermiso in idsTopNivel)
                        {
                            using (var ins = new SqlCommand(
                                "INSERT INTO Usuario_Permiso (IdUsuario, IdPermiso) VALUES (@u, @p)", con, tx))
                            {
                                ins.Parameters.AddWithValue("@u", idUsuario);
                                ins.Parameters.AddWithValue("@p", idPermiso);
                                ins.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }

        private int PersistirNodoRecursivo(SqlConnection con, SqlTransaction tx, IComponentePermiso nodo)
        {
            GrupoPermiso grupo = nodo as GrupoPermiso;
            bool esPadre = grupo != null;
            int id = ObtenerOCrearPermiso(con, tx, nodo.Nombre, esPadre);

            if (esPadre)
            {
                foreach (var hijo in grupo.Hijos())
                {
                    int idHijo = PersistirNodoRecursivo(con, tx, hijo);
                    CrearRelacionSiNoExiste(con, tx, id, idHijo);
                }
            }
            return id;
        }

        private int ObtenerOCrearPermiso(SqlConnection con, SqlTransaction tx, string desc, bool esPadre)
        {
            using (var cmd = new SqlCommand(
                "SELECT Id FROM Permiso WHERE Desc_ = @desc AND EsPadre = @esPadre", con, tx))
            {
                cmd.Parameters.AddWithValue("@desc", desc);
                cmd.Parameters.AddWithValue("@esPadre", esPadre);
                object existente = cmd.ExecuteScalar();
                if (existente != null) return (int)existente;
            }
            using (var cmd = new SqlCommand(
                "INSERT INTO Permiso (Desc_, EsPadre) OUTPUT INSERTED.Id VALUES (@desc, @esPadre)", con, tx))
            {
                cmd.Parameters.AddWithValue("@desc", desc);
                cmd.Parameters.AddWithValue("@esPadre", esPadre);
                return (int)cmd.ExecuteScalar();
            }
        }

        private void CrearRelacionSiNoExiste(SqlConnection con, SqlTransaction tx, int idPadre, int idHijo)
        {
            string sql = @"IF NOT EXISTS (SELECT 1 FROM Permiso_Permiso WHERE IdPadre = @padre AND IdHijo = @hijo)
                           INSERT INTO Permiso_Permiso (IdPadre, IdHijo) VALUES (@padre, @hijo)";
            using (var cmd = new SqlCommand(sql, con, tx))
            {
                cmd.Parameters.AddWithValue("@padre", idPadre);
                cmd.Parameters.AddWithValue("@hijo", idHijo);
                cmd.ExecuteNonQuery();
            }
        }

        private class PermisoRow
        {
            public int Id;
            public string Desc;
            public bool EsPadre;
        }
    }
}
