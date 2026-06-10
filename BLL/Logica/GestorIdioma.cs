using System.Collections.Generic;
using Mapper;

namespace BLL
{
   
    public class GestorIdioma
    {
        private static GestorIdioma _instancia;
        private List<IObservadorIdioma> _observadores = new List<IObservadorIdioma>();

        public enum Idioma { Espanol, Ingles }
        public Idioma IdiomaActual { get; private set; } = Idioma.Espanol;

        private GestorIdioma() { }

        public static GestorIdioma Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new GestorIdioma();
                return _instancia;
            }
        }

       
        public void Suscribir(IObservadorIdioma observador)
        {
            if (!_observadores.Contains(observador))
                _observadores.Add(observador);
        }

        public void Desuscribir(IObservadorIdioma observador)
        {
            _observadores.Remove(observador);
        }

        private void Notificar()
        {
            foreach (var obs in _observadores)
                obs.ActualizarIdioma();
        }

        
        public void CambiarIdioma()
        {
            IdiomaActual = IdiomaActual == Idioma.Espanol ? Idioma.Ingles : Idioma.Espanol;
            Notificar();
        }

        
        public bool EsEspanol => IdiomaActual == Idioma.Espanol;

        public string T(string espanol, string ingles)
        {
            return EsEspanol ? espanol : ingles;
        }
    }
}
