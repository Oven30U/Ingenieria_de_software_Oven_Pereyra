namespace Mapper
{
    public class PermisoLeaf : IComponentePermiso
    {
        public string Nombre { get; }

        public PermisoLeaf(string nombre)
        {
            Nombre = nombre;
        }

        public string ObtenerInfo() => $"Permiso: {Nombre}";
    }
}
