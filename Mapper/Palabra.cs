namespace Mapper
{
    /// <summary>
    /// Representa una palabra/clave de traducción.
    /// Tabla: Palabras (Tag NVARCHAR PK)
    /// El Tag es la clave que el código usa para pedir una traducción.
    /// Ej: "btn_login", "lbl_usuario", "titulo_principal"
    /// </summary>
    public class Palabra
    {
        public string Tag { get; set; }   // PK — clave única usada en el código

        public override string ToString() => Tag;
    }
}
