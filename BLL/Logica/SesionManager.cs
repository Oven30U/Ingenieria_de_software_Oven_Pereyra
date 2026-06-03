using Mapper;

namespace BLL
{
    public class SesionManager
    {
        private static SesionManager _instancia;
        private Usuario _usuarioActual;

        private SesionManager() { }

        public static SesionManager Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new SesionManager();
                return _instancia;
            }
        }

        public void IniciarSesion(Usuario usuario)
        {
            _usuarioActual = usuario;
        }

        public void CerrarSesion()
        {
            _usuarioActual = null;
        }

        public bool EstaLogueado()
        {
            return _usuarioActual != null;
        }

        public bool EsAdmin()
        {
            return _usuarioActual != null && _usuarioActual.Rol == "admin";
        }

        public Usuario ObtenerUsuarioActual()
        {
            return _usuarioActual;
        }
    }
}