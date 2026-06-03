using System;
using System.Collections.Generic;

namespace Mapper
{
    // COMPOSITE: representa un grupo (ej: "admin", "usuario")
    public class GrupoUsuarios : IComponenteUsuario
    {
        private List<IComponenteUsuario> _componentes = new List<IComponenteUsuario>();
        public string Nombre { get; }

        public GrupoUsuarios(string nombre)
        {
            Nombre = nombre;
        }

        public void Agregar(IComponenteUsuario componente)
        {
            _componentes.Add(componente);
        }

        public void Eliminar(IComponenteUsuario componente)
        {
            _componentes.Remove(componente);
        }

        public void Mostrar(int nivel = 0)
        {
            Console.WriteLine(new string('-', nivel * 2) + $" Grupo: [{Nombre}] ({Contar()} usuarios)");
            foreach (var c in _componentes)
                c.Mostrar(nivel + 1);
        }

        public int Contar()
        {
            int total = 0;
            foreach (var c in _componentes)
                total += c.Contar();
            return total;
        }

        public IEnumerable<IComponenteUsuario> Hijos()
        {
            return _componentes;
        }
    }
}