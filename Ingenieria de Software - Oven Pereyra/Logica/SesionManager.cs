using Ingenieria_de_Software___Oven_Pereyra.Modelos;

namespace Ingenieria_de_Software___Oven_Pereyra.Logica
{
    public class SesionManager
    {
        // Instancia única — patrón Singleton
        private static SesionManager _instancia;
        public static SesionManager Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new SesionManager();
                return _instancia;
            }
        }

        // Constructor privado — nadie puede crear instancias desde afuera
        private SesionManager() { }

        // Usuario actualmente autenticado
        public Usuario UsuarioActual { get; private set; }

        public void IniciarSesion(Usuario usuario)
        {
            UsuarioActual = usuario;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
        }

        public bool HaySesionActiva()
        {
            return UsuarioActual != null;
        }

        public bool EsAdmin()
        {
            return UsuarioActual != null && UsuarioActual.Rol == "admin";
        }
    }
}