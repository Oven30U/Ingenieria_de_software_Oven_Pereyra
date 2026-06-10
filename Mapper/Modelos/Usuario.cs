namespace Mapper
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Clave { get; set; }
        public string Rol { get; set; }
        public string Permisos { get; set; }      // arbol completo serializado
        public string TipoPermiso { get; set; }   // nombre de la familia principal (ej: "Administrador")
    }
}