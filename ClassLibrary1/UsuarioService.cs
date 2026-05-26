using System.Collections.Generic;
using DAL;
using Mapper;


namespace BLL
{
    public class UsuarioService
    {
        private UsuarioDAL dao = new UsuarioDAL();

        public List<Usuario> ObtenerTodos() => dao.ObtenerTodos();

        public string Login(string usuario, string clave)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
                return null;

            string rol = dao.Login(usuario, clave);

            if (rol != null)
            {
                // Obtenemos el usuario completo para guardarlo en la sesión
                var usuarioObj = dao.ObtenerPorNombre(usuario);
                SesionManager.Instancia.IniciarSesion(usuarioObj);
            }

            return rol;
        }

        public void Logout()
        {
            SesionManager.Instancia.CerrarSesion();
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