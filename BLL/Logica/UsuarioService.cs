using System.Collections.Generic;
using DAL;
using Mapper;

namespace BLL
{
    public class UsuarioService
    {
        private UsuarioDAL _dao = new UsuarioDAL();

        public string Login(string nombreUsuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(clave))
                return null;

            Usuario usuario = _dao.Login(nombreUsuario, clave);
            if (usuario != null)
            {
                SesionManager.Instancia.IniciarSesion(usuario);
                return usuario.Rol;
            }
            return null;
        }

        public void Logout()
        {
            SesionManager.Instancia.CerrarSesion();
        }

        public List<Usuario> ObtenerTodos()
        {
            return _dao.ObtenerTodos();
        }

        public (bool ok, string mensaje) Agregar(string nombreUsuario, string clave, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return (false, "El nombre de usuario no puede estar vacio.");
            if (string.IsNullOrWhiteSpace(clave))
                return (false, "La clave no puede estar vacia.");
            if (_dao.ExisteUsuario(nombreUsuario))
                return (false, "Ya existe un usuario con ese nombre.");

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Clave = clave,
                Rol = rol
            };

            bool resultado = _dao.Agregar(usuario);
            return resultado
                ? (true, "Usuario agregado correctamente.")
                : (false, "Error al agregar el usuario.");
        }

        public (bool ok, string mensaje) Modificar(int id, string nombreUsuario, string nuevaClave, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return (false, "El nombre de usuario no puede estar vacio.");

            var usuario = new Usuario
            {
                Id = id,
                NombreUsuario = nombreUsuario,
                Rol = rol
            };

            bool resultado = _dao.Modificar(usuario, nuevaClave);
            return resultado
                ? (true, "Usuario modificado correctamente.")
                : (false, "Error al modificar el usuario.");
        }

        public (bool ok, string mensaje) Eliminar(int id, string nombreUsuario)
        {
            if (nombreUsuario == "admin")
                return (false, "No se puede eliminar el usuario admin.");

            bool resultado = _dao.Eliminar(id);
            return resultado
                ? (true, "Usuario eliminado correctamente.")
                : (false, "Error al eliminar el usuario.");
        }


        public GrupoPermiso ObtenerArbolPermisos()
        {
            var raiz = new GrupoPermiso("Raiz");

            // Administrador
            var admin = new GrupoPermiso("Administrador");

            var gestionProductosAdmin = new GrupoPermiso("gestionProductos");
            gestionProductosAdmin.Agregar(new PermisoLeaf("addProducto"));
            gestionProductosAdmin.Agregar(new PermisoLeaf("updateProducto"));
            gestionProductosAdmin.Agregar(new PermisoLeaf("deleteProducto"));

            var gestionUsuarios = new GrupoPermiso("gestionUsuarios");
            gestionUsuarios.Agregar(new PermisoLeaf("addUsuario"));
            gestionUsuarios.Agregar(new PermisoLeaf("updateUsuario"));
            gestionUsuarios.Agregar(new PermisoLeaf("deleteUsuario"));

            admin.Agregar(gestionProductosAdmin);
            admin.Agregar(gestionUsuarios);

            // Vendedor
            var vendedor = new GrupoPermiso("Vendedor");

            var gestionProductosVendedor = new GrupoPermiso("gestionProductos");
            gestionProductosVendedor.Agregar(new PermisoLeaf("addProducto"));
            gestionProductosVendedor.Agregar(new PermisoLeaf("updateProducto"));

            vendedor.Agregar(gestionProductosVendedor);

            raiz.Agregar(admin);
            raiz.Agregar(vendedor);

            return raiz;
        }

    }
}