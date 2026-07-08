namespace Mapper
{

    public class Idioma
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }

        public override string ToString() => Nombre;
    }
}
