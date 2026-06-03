namespace Mapper
{
    public interface IComponenteUsuario
    {
        string Nombre { get; }
        void Mostrar(int nivel = 0);
        int Contar();
    }
}