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

                string crearTabla = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
                    CREATE TABLE Usuarios (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Usuario NVARCHAR(100) NOT NULL UNIQUE,
                        Clave NVARCHAR(64) NOT NULL,
                        Rol NVARCHAR(50) NOT NULL DEFAULT 'usuario'
                    );";
                using (var cmd = new SqlCommand(crearTabla, con))
                    cmd.ExecuteNonQuery();

                string agregarRol = @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Rol' AND Object_ID = Object_ID('Usuarios'))
                    ALTER TABLE Usuarios ADD Rol NVARCHAR(50) NOT NULL DEFAULT 'usuario';";
                using (var cmd = new SqlCommand(agregarRol, con))
                    cmd.ExecuteNonQuery();

                string hashAdmin = HashSHA256("admin123");
                string crearAdmin = @"IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'admin')
                    INSERT INTO Usuarios (Usuario, Clave, Rol) VALUES ('admin', @hash, 'admin');";
                using (var cmd = new SqlCommand(crearAdmin, con))
                {
                    cmd.Parameters.AddWithValue("@hash", hashAdmin);
                    cmd.ExecuteNonQuery();
                }

                string actualizarAdmin = "UPDATE Usuarios SET Clave = @hash WHERE Usuario = 'admin';";
                using (var cmd = new SqlCommand(actualizarAdmin, con))
                {
                    cmd.Parameters.AddWithValue("@hash", hashAdmin);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Usuario Login(string nombreUsuario, string clave)
        {
            string hash = HashSHA256(clave);
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Id, Usuario, Clave, Rol FROM Usuarios WHERE Usuario = @usuario AND Clave = @clave";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@usuario", nombreUsuario);
                    cmd.Parameters.AddWithValue("@clave", hash);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                Id = (int)reader["Id"],
                                NombreUsuario = reader["Usuario"].ToString(),
                                Clave = reader["Clave"].ToString(),
                                Rol = reader["Rol"].ToString()
                            };
                        }
                    }
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
                string sql = "SELECT Id, Usuario, Clave, Rol FROM Usuarios ORDER BY Id";
                using (var cmd = new SqlCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Id = (int)reader["Id"],
                            NombreUsuario = reader["Usuario"].ToString(),
                            Clave = reader["Clave"].ToString(),
                            Rol = reader["Rol"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public bool Agregar(Usuario usuario)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string sql = "INSERT INTO Usuarios (Usuario, Clave, Rol) VALUES (@usuario, @clave, @rol)";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario.NombreUsuario);
                        cmd.Parameters.AddWithValue("@clave", HashSHA256(usuario.Clave));
                        cmd.Parameters.AddWithValue("@rol", usuario.Rol);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public bool Modificar(Usuario usuario, string nuevaClave)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string sql;
                    if (!string.IsNullOrWhiteSpace(nuevaClave))
                        sql = "UPDATE Usuarios SET Usuario = @usuario, Clave = @clave, Rol = @rol WHERE Id = @id";
                    else
                        sql = "UPDATE Usuarios SET Usuario = @usuario, Rol = @rol WHERE Id = @id";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario.NombreUsuario);
                        cmd.Parameters.AddWithValue("@rol", usuario.Rol);
                        cmd.Parameters.AddWithValue("@id", usuario.Id);
                        if (!string.IsNullOrWhiteSpace(nuevaClave))
                            cmd.Parameters.AddWithValue("@clave", HashSHA256(nuevaClave));
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
}