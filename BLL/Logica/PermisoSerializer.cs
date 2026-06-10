using System.Collections.Generic;
using System.Text;
using Mapper;

namespace BLL
{
    /// <summary>
    /// Serializa y deserializa el arbol Composite (GrupoPermiso + PermisoLeaf)
    /// a un formato de texto plano simple, linea por linea.
    ///
    /// Formato:
    ///   G|nombre|cantidadHijos   -> grupo
    ///   L|nombre                 -> hoja
    ///
    /// Ejemplo:
    ///   G|Raiz|1
    ///   G|Administrador|2
    ///   G|gestionUser|3
    ///   L|addUser
    ///   L|updateUser
    ///   L|deleteUser
    ///   G|gestionProducto|1
    ///   L|addProducto
    ///
    /// Restriccion: los nombres no pueden contener el caracter '|'.
    /// </summary>
    public static class PermisoSerializer
    {
        private const char SEP = '|';

        // ── SERIALIZAR ───────────────────────────────────────────────────────
        public static string Serializar(GrupoPermiso raiz)
        {
            if (raiz == null) return "";
            var sb = new StringBuilder();
            EscribirNodo(raiz, sb);
            return sb.ToString();
        }

        private static void EscribirNodo(IComponentePermiso nodo, StringBuilder sb)
        {
            GrupoPermiso grupo = nodo as GrupoPermiso;
            if (grupo != null)
            {
                var hijos = grupo.Hijos();
                sb.Append('G').Append(SEP)
                  .Append(grupo.Nombre).Append(SEP)
                  .Append(hijos.Count).Append('\n');
                foreach (var hijo in hijos)
                    EscribirNodo(hijo, sb);
            }
            else
            {
                sb.Append('L').Append(SEP)
                  .Append(nodo.Nombre).Append('\n');
            }
        }

        // ── DESERIALIZAR ─────────────────────────────────────────────────────
        public static GrupoPermiso Deserializar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            var lineas = texto.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            int indice = 0;
            IComponentePermiso raiz = LeerNodo(lineas, ref indice);
            return raiz as GrupoPermiso;
        }

        private static IComponentePermiso LeerNodo(string[] lineas, ref int indice)
        {
            if (indice >= lineas.Length) return null;
            string linea = lineas[indice].TrimEnd('\r');
            indice++;

            var partes = linea.Split(SEP);
            if (partes.Length < 2) return null;

            string tipo = partes[0];
            string nombre = partes[1];

            if (tipo == "L")
                return new PermisoLeaf(nombre);

            if (tipo == "G")
            {
                int cantidadHijos = 0;
                if (partes.Length >= 3) int.TryParse(partes[2], out cantidadHijos);

                var grupo = new GrupoPermiso(nombre);
                for (int i = 0; i < cantidadHijos; i++)
                {
                    var hijo = LeerNodo(lineas, ref indice);
                    if (hijo != null) grupo.Agregar(hijo);
                }
                return grupo;
            }

            return null;
        }
    }
}
