using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Mapper;

namespace DAL
{
    public class UsuarioDAL
    {
        private string connectionString;

        public UsuarioDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["BaseDatos"].ConnectionString;
        }

        public void InicializarBaseDatos()
        {
            string masterConnStr = connectionString
                .Replace("Database=IngenieriaSoftware;", "Database=master;")
                .Replace("database=IngenieriaSoftware;", "Database=master;");

            using (var con = new SqlConnection(masterConnStr))
            {
                con.Open();
                string crearDB = @"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IngenieriaSoftware')
                                   CREATE DATABASE IngenieriaSoftware;";
                using (var cmd = new SqlCommand(crearDB, con))
                    cmd.ExecuteNonQuery();
            }

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                // Tabla Usuarios
                string crearTabla = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
                    CREATE TABLE Usuarios (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Usuario NVARCHAR(100) NOT NULL UNIQUE,
                        Clave NVARCHAR(64) NOT NULL,
                        Rol NVARCHAR(50) NOT NULL DEFAULT 'usuario',
                        Permisos NVARCHAR(MAX) NULL,
                        TipoPermiso NVARCHAR(100) NULL
                    );";
                using (var cmd = new SqlCommand(crearTabla, con))
                    cmd.ExecuteNonQuery();

                EjecutarSiNoExisteColumna(con, "Rol",        "ALTER TABLE Usuarios ADD Rol NVARCHAR(50) NOT NULL DEFAULT 'usuario';");
                EjecutarSiNoExisteColumna(con, "Permisos",   "ALTER TABLE Usuarios ADD Permisos NVARCHAR(MAX) NULL;");
                EjecutarSiNoExisteColumna(con, "TipoPermiso","ALTER TABLE Usuarios ADD TipoPermiso NVARCHAR(100) NULL;");

                // Tabla HistorialClaves
                string crearHistorial = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialClaves' AND xtype='U')
                    CREATE TABLE HistorialClaves (
                        Id          INT IDENTITY(1,1) PRIMARY KEY,
                        IdUsuario   INT           NOT NULL,
                        NombreUsuario NVARCHAR(100) NOT NULL,
                        Operador    NVARCHAR(100) NOT NULL,
                        Evento      NVARCHAR(50)  NOT NULL,
                        ClaveHash   NVARCHAR(64)  NOT NULL,
                        Fecha       DATETIME      NOT NULL DEFAULT GETDATE(),
                        FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id) ON DELETE CASCADE
                    );";
                using (var cmd = new SqlCommand(crearHistorial, con))
                    cmd.ExecuteNonQuery();

                // Usuario admin por defecto
                string hashAdmin = HashSHA256("admin123");
                string crearAdmin = @"IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'admin')
                    INSERT INTO Usuarios (Usuario, Clave, Rol) VALUES ('admin', @hash, 'admin');";
                using (var cmd = new SqlCommand(crearAdmin, con))
                {
                    cmd.Parameters.AddWithValue("@hash", hashAdmin);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("UPDATE Usuarios SET Clave = @hash WHERE Usuario = 'admin';", con))
                {
                    cmd.Parameters.AddWithValue("@hash", hashAdmin);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void EjecutarSiNoExisteColumna(SqlConnection con, string columna, string sql)
        {
            string check = @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = @col AND Object_ID = Object_ID('Usuarios'))
                             EXEC(@sql)";
            using (var cmd = new SqlCommand(check, con))
            {
                cmd.Parameters.AddWithValue("@col", columna);
                cmd.Parameters.AddWithValue("@sql", sql);
                cmd.ExecuteNonQuery();
            }
        }

        private Usuario LeerUsuario(SqlDataReader reader)
        {
            return new Usuario
            {
                Id            = (int)reader["Id"],
                NombreUsuario = reader["Usuario"].ToString(),
                Clave         = reader["Clave"].ToString(),
                Rol           = reader["Rol"].ToString(),
                Permisos      = reader["Permisos"]    == DBNull.Value ? null : reader["Permisos"].ToString(),
                TipoPermiso   = reader["TipoPermiso"] == DBNull.Value ? null : reader["TipoPermiso"].ToString()
            };
        }

        // ── Login / listado ────────────────────────────────────────────
        public Usuario Login(string nombreUsuario, string clave)
        {
            string hash = HashSHA256(clave);
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Id, Usuario, Clave, Rol, Permisos, TipoPermiso FROM Usuarios WHERE Usuario = @usuario AND Clave = @clave";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@usuario", nombreUsuario);
                    cmd.Parameters.AddWithValue("@clave",   hash);
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read()) return LeerUsuario(reader);
                }
            }
            return null;
        }

        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Id, Usuario, Clave, Rol, Permisos, TipoPermiso FROM Usuarios ORDER BY Id";
                using (var cmd = new SqlCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        lista.Add(LeerUsuario(reader));
            }
            return lista;
        }

        public Usuario ObtenerPorId(int id)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Id, Usuario, Clave, Rol, Permisos, TipoPermiso FROM Usuarios WHERE Id = @id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read()) return LeerUsuario(reader);
                }
            }
            return null;
        }

        // ── CRUD con historial ─────────────────────────────────────────
        public bool Agregar(Usuario usuario)
        {
            return Agregar(usuario, "sistema");
        }

        public bool Agregar(Usuario usuario, string operador)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string hashClave = HashSHA256(usuario.Clave);

                    string sql = "INSERT INTO Usuarios (Usuario, Clave, Rol, Permisos, TipoPermiso) " +
                                 "OUTPUT INSERTED.Id " +
                                 "VALUES (@usuario, @clave, @rol, @permisos, @tipo)";
                    int nuevoId;
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@usuario",  usuario.NombreUsuario);
                        cmd.Parameters.AddWithValue("@clave",    hashClave);
                        cmd.Parameters.AddWithValue("@rol",      usuario.Rol);
                        cmd.Parameters.AddWithValue("@permisos", string.IsNullOrEmpty(usuario.Permisos)    ? (object)DBNull.Value : usuario.Permisos);
                        cmd.Parameters.AddWithValue("@tipo",     string.IsNullOrEmpty(usuario.TipoPermiso) ? (object)DBNull.Value : usuario.TipoPermiso);
                        nuevoId = (int)cmd.ExecuteScalar();
                    }

                    RegistrarHistorial(con, nuevoId, usuario.NombreUsuario, operador, "ALTA", hashClave);
                }
                return true;
            }
            catch { return false; }
        }

        public bool Modificar(Usuario usuario, string nuevaClave)
        {
            return Modificar(usuario, nuevaClave, "sistema");
        }

        public bool Modificar(Usuario usuario, string nuevaClave, string operador)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    if (!string.IsNullOrWhiteSpace(nuevaClave))
                    {
                        string hashNuevo = HashSHA256(nuevaClave);
                        string sql = "UPDATE Usuarios SET Usuario = @usuario, Clave = @clave, Rol = @rol WHERE Id = @id";
                        using (var cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@usuario", usuario.NombreUsuario);
                            cmd.Parameters.AddWithValue("@rol",     usuario.Rol);
                            cmd.Parameters.AddWithValue("@id",      usuario.Id);
                            cmd.Parameters.AddWithValue("@clave",   hashNuevo);
                            cmd.ExecuteNonQuery();
                        }
                        RegistrarHistorial(con, usuario.Id, usuario.NombreUsuario, operador, "MODIFICACION", hashNuevo);
                    }
                    else
                    {
                        string sql = "UPDATE Usuarios SET Usuario = @usuario, Rol = @rol WHERE Id = @id";
                        using (var cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@usuario", usuario.NombreUsuario);
                            cmd.Parameters.AddWithValue("@rol",     usuario.Rol);
                            cmd.Parameters.AddWithValue("@id",      usuario.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public bool ActualizarPermisos(int idUsuario, string permisos, string tipoPermiso)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string sql = "UPDATE Usuarios SET Permisos = @permisos, TipoPermiso = @tipo WHERE Id = @id";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id",       idUsuario);
                        cmd.Parameters.AddWithValue("@permisos", string.IsNullOrEmpty(permisos)    ? (object)DBNull.Value : permisos);
                        cmd.Parameters.AddWithValue("@tipo",     string.IsNullOrEmpty(tipoPermiso) ? (object)DBNull.Value : tipoPermiso);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public bool Eliminar(int id)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string sql = "DELETE FROM Usuarios WHERE Id = @id";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public bool ExisteUsuario(string nombreUsuario)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @usuario";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@usuario", nombreUsuario);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // ── Historial de claves ────────────────────────────────────────

        private void RegistrarHistorial(SqlConnection con, int idUsuario, string nombreUsuario,
            string operador, string evento, string claveHash)
        {
            string sql = "INSERT INTO HistorialClaves (IdUsuario, NombreUsuario, Operador, Evento, ClaveHash, Fecha) " +
                         "VALUES (@id, @nombre, @op, @ev, @hash, GETDATE())";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id",     idUsuario);
                cmd.Parameters.AddWithValue("@nombre", nombreUsuario);
                cmd.Parameters.AddWithValue("@op",     operador);
                cmd.Parameters.AddWithValue("@ev",     evento);
                cmd.Parameters.AddWithValue("@hash",   claveHash);
                cmd.ExecuteNonQuery();
            }
        }

        public List<RegistroClave> ObtenerHistorialClaves()
        {
            var lista = new List<RegistroClave>();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Id, IdUsuario, NombreUsuario, Operador, Evento, ClaveHash, Fecha " +
                             "FROM HistorialClaves ORDER BY Fecha DESC";
                using (var cmd = new SqlCommand(sql, con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new RegistroClave
                        {
                            Id            = (int)r["Id"],
                            IdUsuario     = (int)r["IdUsuario"],
                            NombreUsuario = r["NombreUsuario"].ToString(),
                            Operador      = r["Operador"].ToString(),
                            Evento        = r["Evento"].ToString(),
                            ClaveHash     = r["ClaveHash"].ToString(),
                            Fecha         = (DateTime)r["Fecha"]
                        });
            }
            return lista;
        }

        /// <summary>Restaura la clave de un usuario al hash indicado y registra el evento.</summary>
        public bool RestaurarClave(int idUsuario, string nombreUsuario, string claveHash, string operador)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("UPDATE Usuarios SET Clave = @hash WHERE Id = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@hash", claveHash);
                        cmd.Parameters.AddWithValue("@id",   idUsuario);
                        cmd.ExecuteNonQuery();
                    }
                    RegistrarHistorial(con, idUsuario, nombreUsuario, operador, "RESTAURACION", claveHash);
                }
                return true;
            }
            catch { return false; }
        }

        public static string HashSHA256(string texto)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }

    // ── Modelo para el historial ───────────────────────────────────────
    public class RegistroClave
    {
        public int      Id            { get; set; }
        public int      IdUsuario     { get; set; }
        public string   NombreUsuario { get; set; }
        public string   Operador      { get; set; }
        public string   Evento        { get; set; }
        public string   ClaveHash     { get; set; }
        public DateTime Fecha         { get; set; }
    }
}
