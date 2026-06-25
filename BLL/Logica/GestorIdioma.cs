using System;
using System.Collections.Generic;
using DAL;
using Mapper;

namespace BLL
{
    /// <summary>
    /// GestorIdioma — Singleton + Observer.
    /// Compatible con .NET Framework 4.7.2.
    ///
    /// T(tag)          → traduce por tag de BD (nuevo sistema)
    /// T(esp, ing)     → compatibilidad con FormABM existente (hardcodeado)
    /// </summary>
    public class GestorIdioma
    {
        private static GestorIdioma _instancia;
        public static GestorIdioma Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new GestorIdioma();
                return _instancia;
            }
        }

        private readonly IdiomaDAL               _dal          = new IdiomaDAL();
        private readonly List<IObservadorIdioma> _observadores = new List<IObservadorIdioma>();
        private List<Idioma>              _idiomas      = new List<Idioma>();
        private Dictionary<string,string> _traducciones = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

        public Idioma IdiomaActual { get; private set; }

        private GestorIdioma()
        {
            CargarIdiomas();
        }

        public IdiomaDAL ObtenerIdiomaDAL()
        {
            return _dal;
        }

        // ── Carga ──────────────────────────────────────────────────────
        public void CargarIdiomas()
        {
            _idiomas = _dal.ObtenerIdiomas();
            if (_idiomas.Count > 0 && IdiomaActual == null)
                CambiarIdioma(_idiomas[0].Id);
        }

        public List<Idioma> ObtenerIdiomas()
        {
            return new List<Idioma>(_idiomas);
        }

        // ── Cambio de idioma ───────────────────────────────────────────
        public void CambiarIdioma(int idIdioma)
        {
            Idioma encontrado = null;
            for (int i = 0; i < _idiomas.Count; i++)
                if (_idiomas[i].Id == idIdioma) { encontrado = _idiomas[i]; break; }
            if (encontrado == null) return;

            IdiomaActual  = encontrado;
            _traducciones = _dal.ObtenerTraducciones(idIdioma);
            Notificar();
        }

        // ── Traducción por tag (sistema nuevo) ─────────────────────────
        public string T(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return string.Empty;
            string texto;
            if (_traducciones.TryGetValue(tag, out texto) && !string.IsNullOrEmpty(texto))
                return texto;
            return "[" + tag + "]";
        }

        // ── Traducción con parámetros — doble uso:
        //    a) T("msg_bienvenido", nombre)     → formato con tag de BD
        //    b) T("Texto español", "Text eng")  → fallback hardcodeado (FormABM)
        public string T(string primerArg, params object[] args)
        {
            // Detectar firma vieja: exactamente 1 argumento string y primerArg parece texto (no tag)
            if (args.Length == 1 && args[0] is string && EsTextoHardcodeado(primerArg))
            {
                return EsEspanol() ? primerArg : (string)args[0];
            }

            // Firma nueva: tag + parámetros de formato
            string plantilla = T(primerArg);
            try
            {
                return string.Format(plantilla, args);
            }
            catch
            {
                return plantilla;
            }
        }

        // Devuelve true si el string parece texto natural (firma vieja) en vez de un tag de BD
        private static bool EsTextoHardcodeado(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            // Tags de BD son snake_case sin espacios ni mayúsculas al inicio
            // Textos hardcodeados tienen espacios, mayúsculas, signos, acentos, etc.
            if (s.Contains(" ") || s.Contains(".") || s.Contains("!") ||
                s.Contains("?") || s.Contains(",") || s.Contains(":") ||
                s.Contains("{") || s.Contains("¿") || s.Contains("¡"))
                return true;
            if (s.Length > 0 && char.IsUpper(s[0]))
                return true;
            return false;
        }

        private bool EsEspanol()
        {
            if (IdiomaActual == null) return true;
            string n = IdiomaActual.Nombre.ToLowerInvariant();
            return n.Contains("espa") || n.Contains("esp");
        }

        // ── Observer ───────────────────────────────────────────────────
        public void Suscribir(IObservadorIdioma obs)
        {
            if (!_observadores.Contains(obs)) _observadores.Add(obs);
        }

        public void Desuscribir(IObservadorIdioma obs)
        {
            _observadores.Remove(obs);
        }

        private void Notificar()
        {
            foreach (var obs in _observadores)
                obs.ActualizarIdioma();
        }

        // ── ABM de idiomas ─────────────────────────────────────────────
        public bool AgregarIdioma(string nombre, out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensaje = "El nombre del idioma no puede estar vacio.";
                return false;
            }
            int nuevoId;
            bool ok = _dal.AgregarIdioma(nombre.Trim(), out nuevoId);
            if (ok)
            {
                CargarIdiomas();
                mensaje = "Idioma '" + nombre + "' agregado correctamente.";
            }
            else
            {
                mensaje = "No se pudo agregar '" + nombre + "'. Puede que ya exista.";
            }
            return ok;
        }

        public bool EliminarIdioma(int id, out string mensaje)
        {
            if (_idiomas.Count <= 1)
            {
                mensaje = "Debe haber al menos un idioma en el sistema.";
                return false;
            }
            bool eraActivo = IdiomaActual != null && IdiomaActual.Id == id;
            bool ok = _dal.EliminarIdioma(id);
            if (ok)
            {
                CargarIdiomas();
                if (eraActivo && _idiomas.Count > 0)
                    CambiarIdioma(_idiomas[0].Id);
                mensaje = "Idioma eliminado correctamente.";
            }
            else
            {
                mensaje = "No se pudo eliminar el idioma.";
            }
            return ok;
        }

        public bool RenombrarIdioma(int id, string nuevoNombre, out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                mensaje = "El nombre no puede estar vacio.";
                return false;
            }
            bool ok = _dal.RenombrarIdioma(id, nuevoNombre.Trim());
            if (ok)
            {
                CargarIdiomas();
                if (IdiomaActual != null && IdiomaActual.Id == id)
                    IdiomaActual.Nombre = nuevoNombre.Trim();
                Notificar();
                mensaje = "Idioma renombrado correctamente.";
            }
            else
            {
                mensaje = "No se pudo renombrar el idioma.";
            }
            return ok;
        }

        public bool GuardarTraduccion(int idIdioma, string tag, string texto)
        {
            return _dal.GuardarTraduccion(idIdioma, tag, texto);
        }
    }
}
