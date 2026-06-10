using System.Collections.Generic;
using DAL;
using Mapper;

namespace BLL
{
    public class UsuarioService
    {
        private UsuarioDAL _dao = new UsuarioDAL();
        private GrupoPermiso _arbol = null;
        private int _idUsuarioPermisos = -1;

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

            var arbolDefault = ConstruirArbolDefault();
            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Clave = clave,
                Rol = rol,
                Permisos = PermisoSerializer.Serializar(arbolDefault),
                TipoPermiso = ObtenerNombreFamiliaPrincipal(arbolDefault)
            };

            bool ok = _dao.Agregar(usuario);
            return ok
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

        // ── Composite ────────────────────────────────────────────────────────

        public GrupoPermiso CargarPermisosDeUsuario(int idUsuario)
        {
            _idUsuarioPermisos = idUsuario;
            _parientesDisponibles = new List<string>();

            Usuario u = _dao.ObtenerPorId(idUsuario);
            if (u != null && !string.IsNullOrWhiteSpace(u.Permisos))
            {
                _arbol = PermisoSerializer.Deserializar(u.Permisos);
                if (_arbol == null) _arbol = ConstruirArbolDefault();
            }
            else
            {
                _arbol = ConstruirArbolDefault();
            }
            return _arbol;
        }

        /// <summary>
        /// Devuelve el nombre del primer hijo directo de Raiz (la familia principal).
        /// Eso es lo que se guarda en TipoPermiso.
        /// </summary>
        public string ObtenerTipoPermiso()
        {
            var raiz = ObtenerArbol();
            var hijos = raiz.Hijos();
            if (hijos.Count > 0)
            {
                GrupoPermiso primerHijo = hijos[0] as GrupoPermiso;
                if (primerHijo != null) return primerHijo.Nombre;
                return hijos[0].Nombre;
            }
            return null;
        }

        public Resultado GuardarPermisosDeUsuario(string tipoPermiso)
        {
            if (_idUsuarioPermisos < 0 || _arbol == null)
                return new Resultado(false, "No hay un usuario cargado para guardar sus permisos.");

            string serializado = PermisoSerializer.Serializar(_arbol);
            bool ok = _dao.ActualizarPermisos(_idUsuarioPermisos, serializado, tipoPermiso);
            return ok
                ? new Resultado(true, "Permisos guardados correctamente.")
                : new Resultado(false, "Error al guardar los permisos.");
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

        private string ObtenerNombreFamiliaPrincipal(GrupoPermiso raiz)
        {
            var hijos = raiz.Hijos();
            if (hijos.Count > 0) return hijos[0].Nombre;
            return null;
        }

        public Resultado AgregarFamilia(string nombre, string padreNombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new Resultado(false, "El nombre no puede estar vacio.");
            if (nombre.Contains("|"))
                return new Resultado(false, "El nombre no puede contener el caracter '|'.");
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
            if (nombre.Contains("|"))
                return new Resultado(false, "El nombre no puede contener el caracter '|'.");
            var raiz = ObtenerArbol();
            while (EliminarDeGrupo(raiz, nombre)) { }
            var padre = padreNombre == raiz.Nombre ? raiz : BuscarGrupo(raiz, padreNombre);
            if (padre == null)
                return new Resultado(false, "No se encontro el grupo padre.");
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
            if (nombre.Contains("|"))
                return new Resultado(false, "El nombre no puede contener el caracter '|'.");
            if (_parientesDisponibles.Contains(nombre))
                return new Resultado(false, "Ya existe un pariente con ese nombre.");
            _parientesDisponibles.Add(nombre);
            return new Resultado(true, "Pariente agregado al listado.");
        }
    }
}