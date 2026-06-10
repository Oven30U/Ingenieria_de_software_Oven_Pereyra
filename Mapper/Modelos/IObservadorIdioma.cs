namespace Mapper
{
    /// <summary>
    /// Interfaz Observer: cualquier form que quiera reaccionar al cambio de idioma
    /// debe implementar esta interfaz.
    /// </summary>
    public interface IObservadorIdioma
    {
        void ActualizarIdioma();
    }
}
