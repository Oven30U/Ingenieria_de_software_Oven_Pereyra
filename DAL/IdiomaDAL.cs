using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Mapper;

namespace DAL
{
    public class IdiomaDAL
    {
        private readonly string _conn;

        public IdiomaDAL()
        {
            _conn = ConfigurationManager.ConnectionStrings["BaseDatos"].ConnectionString;
        }

        // ── Inicialización ─────────────────────────────────────────────
        public void InicializarTablas()
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                Ejecutar(con,
                    "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Idiomas' AND xtype='U') " +
                    "CREATE TABLE Idiomas (Id INT IDENTITY(1,1) PRIMARY KEY, Nombre NVARCHAR(100) NOT NULL UNIQUE)");

                Ejecutar(con,
                    "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Palabras' AND xtype='U') " +
                    "CREATE TABLE Palabras (Tag NVARCHAR(100) PRIMARY KEY)");

                Ejecutar(con,
                    "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Traducciones' AND xtype='U') " +
                    "CREATE TABLE Traducciones (" +
                    "  IdIdioma   INT           NOT NULL," +
                    "  Tag        NVARCHAR(100) NOT NULL," +
                    "  Traduccion NVARCHAR(500) NOT NULL DEFAULT ''," +
                    "  PRIMARY KEY (IdIdioma, Tag)," +
                    "  CONSTRAINT FK_Trad_Idioma  FOREIGN KEY (IdIdioma) REFERENCES Idiomas(Id)   ON DELETE CASCADE," +
                    "  CONSTRAINT FK_Trad_Palabra FOREIGN KEY (Tag)      REFERENCES Palabras(Tag) ON DELETE CASCADE)");

                // Idiomas semilla
                Ejecutar(con, "IF NOT EXISTS (SELECT 1 FROM Idiomas WHERE Nombre='Espanol') INSERT INTO Idiomas(Nombre) VALUES('Espanol')");
                Ejecutar(con, "IF NOT EXISTS (SELECT 1 FROM Idiomas WHERE Nombre='Ingles')  INSERT INTO Idiomas(Nombre) VALUES('Ingles')");

                // Tags semilla
                string[] tags = {
                    "btn_login", "btn_usuarios", "btn_logout", "btn_composite",
                    "lbl_usuario", "lbl_clave", "msg_bienvenido", "msg_acceso_ok",
                    "msg_cred_error", "titulo_principal", "abm_titulo",
                    "abm_agregar", "abm_modificar", "abm_eliminar",
                    "abm_limpiar", "abm_permisos", "composite_titulo"
                };
                foreach (var tag in tags)
                    Ejecutar(con, "IF NOT EXISTS (SELECT 1 FROM Palabras WHERE Tag='" + tag + "') INSERT INTO Palabras(Tag) VALUES('" + tag + "')");

                // Traducciones semilla — Espanol
                var esp = new Dictionary<string, string>();
                esp.Add("btn_login",        "Iniciar sesion");
                esp.Add("btn_usuarios",     "Administrar Usuarios");
                esp.Add("btn_logout",       "Cerrar sesion");
                esp.Add("btn_composite",    "Administrar Composite");
                esp.Add("lbl_usuario",      "Usuario:");
                esp.Add("lbl_clave",        "Clave:");
                esp.Add("msg_bienvenido",   "Bienvenido, {0}!");
                esp.Add("msg_acceso_ok",    "Acceso correcto");
                esp.Add("msg_cred_error",   "Usuario o clave incorrectos.");
                esp.Add("titulo_principal", "Sistema de Usuarios");
                esp.Add("abm_titulo",       "Administrar Usuarios");
                esp.Add("abm_agregar",      "Agregar");
                esp.Add("abm_modificar",    "Modificar");
                esp.Add("abm_eliminar",     "Eliminar");
                esp.Add("abm_limpiar",      "Limpiar");
                esp.Add("abm_permisos",     "Editar Permisos");
                esp.Add("composite_titulo", "Administrar Composite");
                SembrarIdioma(con, "Espanol", esp);

                // Traducciones semilla — Ingles
                var eng = new Dictionary<string, string>();
                eng.Add("btn_login",        "Log in");
                eng.Add("btn_usuarios",     "Manage Users");
                eng.Add("btn_logout",       "Log out");
                eng.Add("btn_composite",    "Manage Composite");
                eng.Add("lbl_usuario",      "Username:");
                eng.Add("lbl_clave",        "Password:");
                eng.Add("msg_bienvenido",   "Welcome, {0}!");
                eng.Add("msg_acceso_ok",    "Access granted");
                eng.Add("msg_cred_error",   "Incorrect username or password.");
                eng.Add("titulo_principal", "User System");
                eng.Add("abm_titulo",       "Manage Users");
                eng.Add("abm_agregar",      "Add");
                eng.Add("abm_modificar",    "Modify");
                eng.Add("abm_eliminar",     "Delete");
                eng.Add("abm_limpiar",      "Clear");
                eng.Add("abm_permisos",     "Edit Permissions");
                eng.Add("composite_titulo", "Manage Composite");
                SembrarIdioma(con, "Ingles", eng);
            }
        }

        // ── Idiomas CRUD ───────────────────────────────────────────────
        public List<Idioma> ObtenerIdiomas()
        {
            var lista = new List<Idioma>();
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT Id, Nombre FROM Idiomas ORDER BY Id", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new Idioma { Id = (int)r["Id"], Nombre = r["Nombre"].ToString() });
            }
            return lista;
        }

        public bool AgregarIdioma(string nombre, out int nuevoId)
        {
            nuevoId = -1;
            try
            {
                using (var con = new SqlConnection(_conn))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("INSERT INTO Idiomas(Nombre) OUTPUT INSERTED.Id VALUES(@n)", con))
                    {
                        cmd.Parameters.AddWithValue("@n", nombre);
                        nuevoId = (int)cmd.ExecuteScalar();
                    }
                    var tags = new List<string>();
                    using (var cmd = new SqlCommand("SELECT Tag FROM Palabras", con))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) tags.Add(r["Tag"].ToString());

                    foreach (var tag in tags)
                    {
                        using (var cmd = new SqlCommand(
                            "IF NOT EXISTS (SELECT 1 FROM Traducciones WHERE IdIdioma=@i AND Tag=@t) " +
                            "INSERT INTO Traducciones(IdIdioma,Tag,Traduccion) VALUES(@i,@t,'')", con))
                        {
                            cmd.Parameters.AddWithValue("@i", nuevoId);
                            cmd.Parameters.AddWithValue("@t", tag);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public bool EliminarIdioma(int id)
        {
            try
            {
                using (var con = new SqlConnection(_conn))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("DELETE FROM Idiomas WHERE Id=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public bool RenombrarIdioma(int id, string nuevoNombre)
        {
            try
            {
                using (var con = new SqlConnection(_conn))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("UPDATE Idiomas SET Nombre=@n WHERE Id=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@n", nuevoNombre);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        // ── Traducciones ───────────────────────────────────────────────
        public Dictionary<string, string> ObtenerTraducciones(int idIdioma)
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT Tag, Traduccion FROM Traducciones WHERE IdIdioma=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", idIdioma);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            dic[r["Tag"].ToString()] = r["Traduccion"].ToString();
                }
            }
            return dic;
        }

        public bool GuardarTraduccion(int idIdioma, string tag, string texto)
        {
            try
            {
                using (var con = new SqlConnection(_conn))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(
                        "IF NOT EXISTS (SELECT 1 FROM Palabras WHERE Tag=@t) INSERT INTO Palabras(Tag) VALUES(@t)", con))
                    {
                        cmd.Parameters.AddWithValue("@t", tag);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand(
                        "IF EXISTS (SELECT 1 FROM Traducciones WHERE IdIdioma=@i AND Tag=@t) " +
                        "    UPDATE Traducciones SET Traduccion=@tx WHERE IdIdioma=@i AND Tag=@t " +
                        "ELSE " +
                        "    INSERT INTO Traducciones(IdIdioma,Tag,Traduccion) VALUES(@i,@t,@tx)", con))
                    {
                        cmd.Parameters.AddWithValue("@i",  idIdioma);
                        cmd.Parameters.AddWithValue("@t",  tag);
                        cmd.Parameters.AddWithValue("@tx", texto);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        // ── Helpers ────────────────────────────────────────────────────
        private static void Ejecutar(SqlConnection con, string sql)
        {
            using (var cmd = new SqlCommand(sql, con))
                cmd.ExecuteNonQuery();
        }

        private void SembrarIdioma(SqlConnection con, string nombreIdioma, Dictionary<string, string> traducciones)
        {
            object res;
            using (var cmd = new SqlCommand("SELECT Id FROM Idiomas WHERE Nombre=@n", con))
            {
                cmd.Parameters.AddWithValue("@n", nombreIdioma);
                res = cmd.ExecuteScalar();
            }
            if (res == null) return;
            int id = (int)res;

            foreach (var kv in traducciones)
            {
                using (var cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT 1 FROM Traducciones WHERE IdIdioma=@i AND Tag=@t) " +
                    "INSERT INTO Traducciones(IdIdioma,Tag,Traduccion) VALUES(@i,@t,@tx)", con))
                {
                    cmd.Parameters.AddWithValue("@i",  id);
                    cmd.Parameters.AddWithValue("@t",  kv.Key);
                    cmd.Parameters.AddWithValue("@tx", kv.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
