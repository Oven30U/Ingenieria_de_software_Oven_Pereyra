using System.Collections.Generic;

namespace Mapper
{
    public class GrupoPermiso : IComponentePermiso
    {
        private List<IComponentePermiso> _componentes = new List<IComponentePermiso>();
        public string Nombre { get; }

        public GrupoPermiso(string nombre)
        {
            Nombre = nombre;
        }

        public void Agregar(IComponentePermiso componente) => _componentes.Add(componente);

        public IEnumerable<IComponentePermiso> Hijos() => _componentes;

        public int Contar()
        {
            int total = 0;
            foreach (var c in _componentes)
                total += c.Contar();
            return total;
        }
    }
}