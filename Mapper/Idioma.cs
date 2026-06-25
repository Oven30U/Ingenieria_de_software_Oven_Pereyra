namespace Mapper
{
    /// <summary>
    /// Representa un idioma disponible en el sistema.
    /// Tabla: Idiomas (Id PK IDENTITY, Nombre NVARCHAR)
    /// </summary>
    public class Idioma
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; }   // Ej: "Español", "Inglés"

        public override string ToString() => Nombre;
    }
}
