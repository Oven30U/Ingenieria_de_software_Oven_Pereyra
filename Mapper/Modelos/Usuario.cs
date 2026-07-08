namespace Mapper
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Clave { get; set; }
        public string Rol { get; set; }
        public bool TienePermisos { get; set; }
        public string TipoPermiso { get; set; }
    }
}
