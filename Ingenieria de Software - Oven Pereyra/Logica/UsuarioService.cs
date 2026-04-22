using System.Collections.Generic;
using Ingenieria_de_Software___Oven_Pereyra.Datos;
using Ingenieria_de_Software___Oven_Pereyra.Modelos;

namespace Ingenieria_de_Software___Oven_Pereyra.Logica
{
    public class UsuarioService
    {
        private UsuarioDAO dao = new UsuarioDAO();

        public List<Usuario> ObtenerTodos() => dao.ObtenerTodos();

        public string Login(string usuario, string clave)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
                return null;
            return dao.Login(usuario, clave);
        }

        public (bool ok, string mensaje) Agregar(string nombreUsuario, string clave, string rol = "usuario")
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(clave))
                return (false, "Completá todos los campos.");
            if (dao.ExisteUsuario(nombreUsuario))
                return (false, "El usuario ya existe.");
            dao.Agregar(new Usuario { NombreUsuario = nombreUsuario, Clave = clave, Rol = rol });
            return (true, "Usuario agregado correctamente.");
        }

        public (bool ok, string mensaje) Modificar(int id, string nombreUsuario, string clave, string rol = "usuario")
        {
            if (string.IsNullOrEmpty(nombreUsuario))
                return (false, "El usuario no puede estar vacío.");

            if (string.IsNullOrEmpty(clave))
                dao.ModificarSinClave(new Usuario { Id = id, NombreUsuario = nombreUsuario, Rol = rol });
            else
                dao.Modificar(new Usuario { Id = id, NombreUsuario = nombreUsuario, Clave = clave, Rol = rol });

            return (true, "Usuario modificado correctamente.");
        }

        public (bool ok, string mensaje) Eliminar(int id, string nombreUsuario)
        {
            if (nombreUsuario == "admin")
                return (false, "No se puede eliminar el usuario admin.");
            dao.Eliminar(id);
            return (true, "Usuario eliminado correctamente.");
        }
    }
}