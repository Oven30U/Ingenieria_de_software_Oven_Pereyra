using System;

namespace Mapper
{
    // LEAF: representa un usuario individual
    public class UsuarioLeaf : IComponenteUsuario
    {
        private Usuario _usuario;

        public UsuarioLeaf(Usuario usuario)
        {
            _usuario = usuario;
        }

        public string Nombre => _usuario.NombreUsuario;

        public void Mostrar(int nivel = 0)
        {
            Console.WriteLine(new string('-', nivel * 2) + $" Usuario: {_usuario.NombreUsuario} | Rol: {_usuario.Rol}");
        }

        public int Contar() => 1;
    }
}