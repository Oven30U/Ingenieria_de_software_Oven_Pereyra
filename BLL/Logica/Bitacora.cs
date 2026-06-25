using System;
using System.IO;

namespace BLL
{
    /// <summary>
    /// Singleton que maneja el archivo de log de la sesion actual.
    /// Formato de cada linea:
    ///   [yyyy-MM-dd] [HH:mm:ss] [USUARIO      ] [NIVEL] [CATEGORIA  ] Mensaje
    /// </summary>
    public class Bitacora
    {
        private static Bitacora _instancia;
        private string _rutaArchivo;
        private static readonly object _lock = new object();

        // Usuario actualmente logueado (se actualiza al login/logout)
        private string _usuarioActual = "sistema";

        private Bitacora()
        {
            string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            string nombreArchivo = "log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
            _rutaArchivo = Path.Combine(carpeta, nombreArchivo);

            EscribirLinea("=================================================");
            EscribirLinea("  BITACORA - Sistema de Usuarios");
            EscribirLinea("  Inicio de ejecucion: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            EscribirLinea("=================================================");
            EscribirLinea("");
        }

        public static Bitacora Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new Bitacora();
                return _instancia;
            }
        }

        // ── Escritura ──────────────────────────────────────────────────
        private void EscribirLinea(string texto)
        {
            lock (_lock)
            {
                try { File.AppendAllText(_rutaArchivo, texto + Environment.NewLine); }
                catch { }
            }
        }

        private void Registrar(string nivel, string categoria, string mensaje)
        {
            string linea = string.Format("[{0}] [{1}] [{2}] [{3}] [{4}] {5}",
                DateTime.Now.ToString("yyyy-MM-dd"),
                DateTime.Now.ToString("HH:mm:ss"),
                _usuarioActual.PadRight(15),
                nivel.PadRight(5),
                categoria.PadRight(12),
                mensaje);
            EscribirLinea(linea);
        }

        // ── Control del usuario activo ─────────────────────────────────
        public void EstablecerUsuario(string usuario)
        {
            _usuarioActual = string.IsNullOrWhiteSpace(usuario) ? "sistema" : usuario;
        }

        public void LimpiarUsuario()
        {
            _usuarioActual = "sistema";
        }

        // ── Metodos publicos ───────────────────────────────────────────
        public void Info(string categoria, string mensaje)
            => Registrar("INFO", categoria, mensaje);

        public void Error(string categoria, string mensaje)
            => Registrar("ERROR", categoria, mensaje);

        public void Advertencia(string categoria, string mensaje)
            => Registrar("WARN", categoria, mensaje);

        // ── Eventos especificos ────────────────────────────────────────
        public void LoginExitoso(string usuario)
        {
            EstablecerUsuario(usuario);
            Info("LOGIN", "Usuario '" + usuario + "' inicio sesion correctamente.");
        }

        public void LoginFallido(string usuario)
            => Advertencia("LOGIN", "Intento de login fallido para el usuario '" + usuario + "'.");

        public void Logout(string usuario)
        {
            Info("LOGOUT", "Usuario '" + usuario + "' cerro sesion.");
            LimpiarUsuario();
        }

        public void UsuarioAgregado(string operador, string nuevoUsuario, string rol)
            => Info("ABM", "'" + operador + "' agrego el usuario '" + nuevoUsuario + "' con rol '" + rol + "'.");

        public void UsuarioModificado(string operador, string usuarioModificado)
            => Info("ABM", "'" + operador + "' modifico el usuario '" + usuarioModificado + "'.");

        public void UsuarioEliminado(string operador, string usuarioEliminado)
            => Info("ABM", "'" + operador + "' elimino el usuario '" + usuarioEliminado + "'.");

        public void PermisosGuardados(string operador, string usuarioDestino, string tipoPermiso)
            => Info("PERMISOS", "'" + operador + "' guardo permisos para '" + usuarioDestino + "' (tipo: " + (tipoPermiso ?? "sin tipo") + ").");

        public void ErrorBaseDatos(string operacion, string detalle)
            => Error("BD", "Error en operacion '" + operacion + "': " + detalle);

        public void CambioIdioma(string idiomaAnterior, string idiomaActual)
            => Info("IDIOMA", "Idioma cambiado de '" + idiomaAnterior + "' a '" + idiomaActual + "'.");

        public void CierreApp()
        {
            EscribirLinea("");
            EscribirLinea("=================================================");
            EscribirLinea("  Fin de ejecucion: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            EscribirLinea("=================================================");
        }
    }
}
