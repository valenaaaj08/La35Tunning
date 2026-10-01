using System.Collections.Generic;
using La35Tunning.Entidades;

namespace La35Tunning.Modelos
{
    public class Jugador
    {
        public string Nombre { get; private set; }
        public decimal Dinero { get; private set; }
        public Auto AutoActual { get; private set; }

        private List<Auto> _autosComprados = new List<Auto>();
        public IReadOnlyList<Auto> AutosComprados => _autosComprados;

        public Jugador(string nombre, decimal dineroInicial)
        {
            Nombre = nombre;
            Dinero = dineroInicial;
            AutoActual = null;
        }

        public void AsignarAuto(Auto nuevoAuto)
        {
            AutoActual = nuevoAuto;
        }

        public void AgregarAutoComprado(Auto auto)
        {
            if (auto != null && !_autosComprados.Contains(auto))
            {
                _autosComprados.Add(auto);
            }
        }

        public void QuitarAutoComprado(Auto auto)
        {
            if (auto == null) return;

            _autosComprados.Remove(auto);
            if (AutoActual == auto)
            {
                AutoActual = null;
            }
        }

        public bool RestarDinero(decimal cantidad)
        {
            if (Dinero >= cantidad)
            {
                Dinero -= cantidad;
                return true;
            }
            return false;
        }

        public void SumarDinero(decimal cantidad)
        {
            if (cantidad > 0)
            {
                Dinero += cantidad;
            }
        }
    }
}