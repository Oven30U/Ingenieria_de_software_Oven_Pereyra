using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Ingenieria_de_Software___Oven_Pereyra.Modelos;

#HOLA
namespace Ingenieria_de_Software___Oven_Pereyra.Datos
{
    public class UsuarioDAO
    {
        private string connectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["BaseDatos"].ConnectionString;

        private string Encriptar(string texto)
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

        public void InicializarBaseDatos()
        {
            string masterConnection = connectionString
                .Replace("Database=IngenieriaSoftware", "Database=master")
                .Replace("Initial Catalog=IngenieriaSoftware", "Initial Catalog=master");

            using (var con = new SqlConnection(masterConnection))
            {
                con.Open();
                new SqlCommand(@"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IngenieriaSoftware')
                    BEGIN
                        CREATE DATABASE IngenieriaSoftware
                    END", con).ExecuteNonQuery();
            }

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
                    BEGIN
                        CREATE TABLE Usuarios (
                            Id INT PRIMARY KEY IDENTITY,
                            Usuario NVARCHAR(50) NOT NULL,
                            Clave NVARCHAR(64) NOT NULL,
                            FechaRegistro DATETIME DEFAULT GETDATE(),
                            Rol NVARCHAR(20) NOT NULL DEFAULT 'usuario'
                        )
                    END", con).ExecuteNonQuery();

                new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns 
                                   WHERE object_id = OBJECT_ID('Usuarios') AND name = 'Rol')
                    BEGIN
                        EXEC('ALTER TABLE Usuarios ADD Rol NVARCHAR(20) NOT NULL DEFAULT ''usuario''')
                    END", con).ExecuteNonQuery();

                // Admin con clave encriptada por el propio método Encriptar()
                string claveAdminEncriptada = Encriptar("admin123");
                var cmdAdmin = new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'admin')
                    BEGIN
                        INSERT INTO Usuarios (Usuario, Clave, Rol) 
                        VALUES ('admin', @clave, 'admin')
                    END", con);
                cmdAdmin.Parameters.AddWithValue("@clave", claveAdminEncriptada);
                cmdAdmin.ExecuteNonQuery();

                // Actualizar clave del admin existente al hash correcto
                var cmdUpdate = new SqlCommand(@"
                    UPDATE Usuarios SET Clave = @clave, Rol = 'admin'
                    WHERE Usuario = 'admin'", con);
                cmdUpdate.Parameters.AddWithValue("@clave", claveAdminEncriptada);
                cmdUpdate.ExecuteNonQuery();
            }
        }

        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT Id, Usuario, Clave, Rol FROM Usuarios", con);
                var reader = cmd.ExecuteReader();
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
            return lista;
        }

        public bool ExisteUsuario(string nombreUsuario)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE Usuario = @u", con);
                cmd.Parameters.AddWithValue("@u", nombreUsuario);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public string Login(string nombreUsuario, string clave)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    "SELECT Rol FROM Usuarios WHERE Usuario = @u AND Clave = @c", con);
                cmd.Parameters.AddWithValue("@u", nombreUsuario);
                cmd.Parameters.AddWithValue("@c", Encriptar(clave));
                var resultado = cmd.ExecuteScalar();
                return resultado != null ? resultado.ToString() : null;
            }
        }

        public void Agregar(Usuario u)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    "INSERT INTO Usuarios (Usuario, Clave, Rol) VALUES (@u, @c, @r)", con);
                cmd.Parameters.AddWithValue("@u", u.NombreUsuario);
                cmd.Parameters.AddWithValue("@c", Encriptar(u.Clave));
                cmd.Parameters.AddWithValue("@r", u.Rol ?? "usuario");
                cmd.ExecuteNonQuery();
            }
        }

        public void Modificar(Usuario u)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    "UPDATE Usuarios SET Usuario = @u, Clave = @c, Rol = @r WHERE Id = @id", con);
                cmd.Parameters.AddWithValue("@u", u.NombreUsuario);
                cmd.Parameters.AddWithValue("@c", Encriptar(u.Clave));
                cmd.Parameters.AddWithValue("@r", u.Rol ?? "usuario");
                cmd.Parameters.AddWithValue("@id", u.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void ModificarSinClave(Usuario u)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    "UPDATE Usuarios SET Usuario = @u, Rol = @r WHERE Id = @id", con);
                cmd.Parameters.AddWithValue("@u", u.NombreUsuario);
                cmd.Parameters.AddWithValue("@r", u.Rol ?? "usuario");
                cmd.Parameters.AddWithValue("@id", u.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("DELETE FROM Usuarios WHERE Id = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}