namespace Mapper
{
    /// <summary>
    /// Representa una traducción concreta de una Palabra en un Idioma.
    /// Tabla: Traducciones (IdIdioma FK → Idiomas.Id, Tag FK → Palabras.Tag, Traduccion NVARCHAR)
    /// PK compuesta: (IdIdioma, Tag)
    ///
    /// Ejemplo de fila: IdIdioma=1, Tag="btn_login", Traduccion="Iniciar sesión"
    ///                  IdIdioma=2, Tag="btn_login", Traduccion="Log in"
    /// </summary>
    public class Traduccion
    {
        public int    IdIdioma   { get; set; }   // FK → Idiomas.Id
        public string Tag        { get; set; }   // FK → Palabras.Tag
        public string Texto      { get; set; }   // El texto traducido
    }
}
