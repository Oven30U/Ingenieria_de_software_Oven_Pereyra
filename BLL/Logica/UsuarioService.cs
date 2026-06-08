using System.Collections.Generic;
using DAL;
using Mapper;

namespace BLL
{
    public class UsuarioService
    {
        private UsuarioDAL _dao = new UsuarioDAL();
        private GrupoPermiso _arbol = null;

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

        public Resultado Agregar(string nombreUsuario, string clave, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return new Resultado(false, "El nombre de usuario no puede estar vacio.");
            if (string.IsNullOrWhiteSpace(clave))
                return new Resultado(false, "La clave no puede estar vacia.");
            if (_dao.ExisteUsuario(nombreUsuario))
                return new Resultado(false, "Ya existe un usuario con ese nombre.");

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Clave = clave,
                Rol = rol
            };

            bool resultado = _dao.Agregar(usuario);
            return resultado
                ? new Resultado(true, "Usuario agregado correctamente.")
                : new Resultado(false, "Error al agregar el usuario.");
        }

        public Resultado Modificar(int id, string nombreUsuario, string nuevaClave, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return new Resultado(false, "El nombre de usuario no puede estar vacio.");

            var usuario = new Usuario
            {
                Id = id,
                NombreUsuario = nombreUsuario,
                Rol = rol
            };

            bool resultado = _dao.Modificar(usuario, nuevaClave);
            return resultado
                ? new Resultado(true, "Usuario modificado correctamente.")
                : new Resultado(false, "Error al modificar el usuario.");
        }

        public Resultado Eliminar(int id, string nombreUsuario)
        {
            if (nombreUsuario == "admin")
                return new Resultado(false, "No se puede eliminar el usuario admin.");

            bool resultado = _dao.Eliminar(id);
            return resultado
                ? new Resultado(true, "Usuario eliminado correctamente.")
                : new Resultado(false, "Error al eliminar el usuario.");
        }

        // ── Composite ────────────────────────────────────────────────────────

        public GrupoPermiso ObtenerArbol()
        {
            if (_arbol == null)
                _arbol = ObtenerArbolPermisos();
            return _arbol;
        }

        private GrupoPermiso ObtenerArbolPermisos()
        {
            var raiz = new GrupoPermiso("Raiz");

            var admin = new GrupoPermiso("Administrador");
            var gestionUsuarios = new GrupoPermiso("gestionUser");
            gestionUsuarios.Agregar(new PermisoLeaf("addUser"));
            gestionUsuarios.Agregar(new PermisoLeaf("updateUser"));
            gestionUsuarios.Agregar(new PermisoLeaf("deleteUser"));
            var gestionProductos = new GrupoPermiso("gestionProducto");
            gestionProductos.Agregar(new PermisoLeaf("addProducto"));
            gestionProductos.Agregar(new PermisoLeaf("updateProducto"));
            gestionProductos.Agregar(new PermisoLeaf("deleteProducto"));
            admin.Agregar(gestionUsuarios);
            admin.Agregar(gestionProductos);

            raiz.Agregar(admin);
            return raiz;
        }

        public Resultado AgregarFamilia(string nombre, string padreNombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            var raiz = ObtenerArbol();
            var padre = padreNombre == raiz.Nombre ? raiz : BuscarGrupo(raiz, padreNombre);
            if (padre == null)
                return new Resultado(false, "No se encontro el grupo padre.");
            padre.Agregar(new GrupoPermiso(nombre));
            return new Resultado(true, "Familia agregada.");
        }

        public Resultado AgregarPariente(string nombre, string padreNombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            var raiz = ObtenerArbol();
            // Si ya existe en el arbol, eliminarlo primero para no duplicar
            while (EliminarDeGrupo(raiz, nombre)) { }
            var padre = padreNombre == raiz.Nombre ? raiz : BuscarGrupo(raiz, padreNombre);
            if (padre == null)
                return new Resultado(false, "No se encontro el grupo padre.");
            padre.Agregar(new PermisoLeaf(nombre));
            // Quitar de disponibles si estaba ahi
            _parientesDisponibles.Remove(nombre);
            return new Resultado(true, "Pariente enlazado.");
        }

        public Resultado EliminarFamilia(string nombre)
        {
            if (nombre == "Raiz")
                return new Resultado(false, "No se puede eliminar la raiz.");
            var raiz = ObtenerArbol();
            bool eliminado = EliminarDeGrupo(raiz, nombre);
            return eliminado
                ? new Resultado(true, "Familia eliminada.")
                : new Resultado(false, "No se encontro la familia.");
        }

        private GrupoPermiso BuscarGrupo(GrupoPermiso actual, string nombre)
        {
            foreach (var hijo in actual.Hijos())
            {
                GrupoPermiso g = hijo as GrupoPermiso;
                if (g != null)
                {
                    if (g.Nombre == nombre) return g;
                    var encontrado = BuscarGrupo(g, nombre);
                    if (encontrado != null) return encontrado;
                }
            }
            return null;
        }

        private bool EliminarDeGrupo(GrupoPermiso actual, string nombre)
        {
            var hijos = actual.Hijos();
            for (int i = 0; i < hijos.Count; i++)
            {
                if (hijos[i].Nombre == nombre)
                {
                    actual.Quitar(hijos[i]);
                    return true;
                }
                GrupoPermiso g = hijos[i] as GrupoPermiso;
                if (g != null && EliminarDeGrupo(g, nombre))
                    return true;
            }
            return false;
        }
        public Resultado EliminarPariente(string nombre)
        {
            var raiz = ObtenerArbol();
            bool eliminado = false;
            // Eliminar todas las ocurrencias del pariente en todo el arbol
            while (EliminarDeGrupo(raiz, nombre))
                eliminado = true;
            return eliminado
                ? new Resultado(true, "Pariente eliminado.")
                : new Resultado(false, "No se encontro el pariente.");
        }

        // Lista separada de parientes disponibles (no forman parte del arbol hasta enlazarlos)
        private List<string> _parientesDisponibles = new List<string>();

        public List<string> ObtenerParientesDisponibles()
        {
            return _parientesDisponibles;
        }

        public Resultado AgregarParienteLibre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            if (_parientesDisponibles.Contains(nombre))
                return new Resultado(false, "Ya existe un pariente con ese nombre.");
            _parientesDisponibles.Add(nombre);
            return new Resultado(true, "Pariente agregado al listado.");
        }

        private bool ExisteEnArbol(GrupoPermiso actual, string nombre)
        {
            foreach (var hijo in actual.Hijos())
            {
                if (hijo.Nombre == nombre) return true;
                GrupoPermiso g = hijo as GrupoPermiso;
                if (g != null && ExisteEnArbol(g, nombre)) return true;
            }
            return false;
        }

    }
}