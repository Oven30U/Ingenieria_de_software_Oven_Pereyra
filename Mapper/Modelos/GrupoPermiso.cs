using System.Collections.Generic;
using System.Text;

namespace Mapper
{
    public class GrupoPermiso : IComponentePermiso
    {
        public string Nombre { get; private set; }
        private readonly List<IComponentePermiso> _hijos = new List<IComponentePermiso>();

        public GrupoPermiso(string nombre)
        {
            Nombre = nombre;
        }

        public void SetNombre(string nombre) { Nombre = nombre; }

        public void Agregar(IComponentePermiso componente) => _hijos.Add(componente);
        public void Quitar(IComponentePermiso componente) => _hijos.Remove(componente);
        public List<IComponentePermiso> Hijos() => _hijos;

        public string ObtenerInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Grupo: {Nombre}");
            foreach (var hijo in _hijos)
                sb.AppendLine("  " + hijo.ObtenerInfo());
            return sb.ToString();
        }
    }
}
