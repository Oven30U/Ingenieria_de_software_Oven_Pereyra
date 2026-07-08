using System.Collections.Generic;
using DAL;
using Mapper;

namespace BLL
{
    public class UsuarioService
    {
        private UsuarioDAL _dao = new UsuarioDAL();
        private PermisoDAL _permisoDao = new PermisoDAL();
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

        public void Logout() => SesionManager.Instancia.CerrarSesion();

        public List<Usuario> ObtenerTodos() => _dao.ObtenerTodos();

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

            int nuevoId = _dao.Agregar(usuario);
            return nuevoId >= 0
                ? new Resultado(true, "Usuario agregado correctamente.")
                : new Resultado(false, "Error al agregar el usuario.");
        }

        public Resultado Modificar(int id, string nombreUsuario, string nuevaClave, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return new Resultado(false, "El nombre de usuario no puede estar vacio.");
            var usuario = new Usuario { Id = id, NombreUsuario = nombreUsuario, Rol = rol };
            bool ok = _dao.Modificar(usuario, nuevaClave);
            return ok
                ? new Resultado(true, "Usuario modificado correctamente.")
                : new Resultado(false, "Error al modificar el usuario.");
        }

        public Resultado Eliminar(int id, string nombreUsuario)
        {
            if (nombreUsuario == "admin")
                return new Resultado(false, "No se puede eliminar el usuario admin.");
            bool ok = _dao.Eliminar(id);
            return ok
                ? new Resultado(true, "Usuario eliminado correctamente.")
                : new Resultado(false, "Error al eliminar el usuario.");
        }

        public GrupoPermiso CargarArbolGlobal()
        {
            _parientesDisponibles = new List<string>();
            _arbol = _permisoDao.CargarArbolGlobal();
            if (_arbol == null || _arbol.Hijos().Count == 0)
                _arbol = ConstruirArbolDefault();
            return _arbol;
        }

        public Resultado GuardarArbolGlobal()
        {
            if (_arbol == null)
                return new Resultado(false, "No hay un catálogo cargado para guardar.");

            bool ok = _permisoDao.GuardarArbolGlobal(_arbol);
            return ok
                ? new Resultado(true, "Catálogo de permisos guardado correctamente.")
                : new Resultado(false, "Error al guardar el catálogo de permisos.");
        }

        public Resultado EnlazarFamiliaConUsuario(string nombreFamilia, int idUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreFamilia))
                return new Resultado(false, "Seleccioná una familia.");

            bool ok = _dao.ActualizarTipoPermiso(idUsuario, nombreFamilia);
            return ok
                ? new Resultado(true, "Familia enlazada al usuario.")
                : new Resultado(false, "Error al enlazar la familia con el usuario.");
        }

        public Resultado EliminarPermisosDeUsuario(int idUsuario)
        {
            bool ok = _dao.ActualizarTipoPermiso(idUsuario, null);
            return ok
                ? new Resultado(true, "Permisos eliminados del usuario.")
                : new Resultado(false, "Error al eliminar los permisos del usuario.");
        }

        public GrupoPermiso ObtenerArbol()
        {
            if (_arbol == null) _arbol = ConstruirArbolDefault();
            return _arbol;
        }

        private GrupoPermiso ConstruirArbolDefault()
        {
            var raiz = new GrupoPermiso("Raiz");
            var admin = new GrupoPermiso("Administrador");
            var gu = new GrupoPermiso("gestionUser");
            gu.Agregar(new PermisoLeaf("addUser"));
            gu.Agregar(new PermisoLeaf("updateUser"));
            gu.Agregar(new PermisoLeaf("deleteUser"));
            var gp = new GrupoPermiso("gestionProducto");
            gp.Agregar(new PermisoLeaf("addProducto"));
            gp.Agregar(new PermisoLeaf("updateProducto"));
            gp.Agregar(new PermisoLeaf("deleteProducto"));
            admin.Agregar(gu);
            admin.Agregar(gp);
            raiz.Agregar(admin);
            return raiz;
        }

        public Resultado AgregarFamilia(string nombre, string padreNombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            var raiz = ObtenerArbol();

            GrupoPermiso padre;
            if (string.IsNullOrWhiteSpace(padreNombre) || padreNombre == raiz.Nombre)
                padre = raiz;
            else
            {
                padre = BuscarGrupo(raiz, padreNombre);
                if (padre == null)
                    return new Resultado(false, "No se encontro el grupo padre.");
            }

            padre.Agregar(new GrupoPermiso(nombre));
            return new Resultado(true, "Familia agregada.");
        }

        public Resultado AgregarPariente(string nombre, string padreNombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            var raiz = ObtenerArbol();
            var padre = padreNombre == raiz.Nombre ? raiz : BuscarGrupo(raiz, padreNombre);
            if (padre == null)
                return new Resultado(false, "No se encontro el grupo padre.");

            bool yaEnlazado = padre.Hijos().Exists(h => h.Nombre == nombre);
            if (yaEnlazado)
                return new Resultado(false, "El pariente ya esta enlazado a esa familia.");

            padre.Agregar(new PermisoLeaf(nombre));
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

        public Resultado EliminarPariente(string nombre)
        {
            var raiz = ObtenerArbol();
            bool eliminado = false;
            while (EliminarDeGrupo(raiz, nombre)) eliminado = true;
            return eliminado
                ? new Resultado(true, "Pariente eliminado.")
                : new Resultado(false, "No se encontro el pariente.");
        }

        private GrupoPermiso BuscarGrupo(GrupoPermiso actual, string nombre)
        {
            foreach (var hijo in actual.Hijos())
            {
                GrupoPermiso g = hijo as GrupoPermiso;
                if (g != null)
                {
                    if (g.Nombre == nombre) return g;
                    var enc = BuscarGrupo(g, nombre);
                    if (enc != null) return enc;
                }
            }
            return null;
        }

        private bool EliminarDeGrupo(GrupoPermiso actual, string nombre)
        {
            var hijos = actual.Hijos();
            for (int i = 0; i < hijos.Count; i++)
            {
                if (hijos[i].Nombre == nombre) { actual.Quitar(hijos[i]); return true; }
                GrupoPermiso g = hijos[i] as GrupoPermiso;
                if (g != null && EliminarDeGrupo(g, nombre)) return true;
            }
            return false;
        }

        private List<string> _parientesDisponibles = new List<string>();
        public List<string> ObtenerParientesDisponibles() => _parientesDisponibles;

        public Resultado AgregarParienteLibre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            if (_parientesDisponibles.Contains(nombre))
                return new Resultado(false, "Ya existe un pariente con ese nombre.");
            _parientesDisponibles.Add(nombre);
            return new Resultado(true, "Pariente agregado al listado.");
        }
    }
}
