namespace Mapper
{
    public class Resultado
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }

        public Resultado(bool ok, string mensaje)
        {
            Ok = ok;
            Mensaje = mensaje;
        }
    }
}
