namespace Mapper
{
    public class PermisoLeaf : IComponentePermiso
    {
        public string Nombre { get; }

        public PermisoLeaf(string nombre)
        {
            Nombre = nombre;
        }

        public int Contar() => 1;
    }
}