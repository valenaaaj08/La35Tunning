using System.Collections.Generic;
using La35Tunning.Entidades;

namespace La35Tunning.Modelos
{
    public class Jugador
    {
        // Propiedades del jugador (usamos private set para que solo esta clase pueda modificar los valores directamente)
        public string Nombre { get; private set; }
        public decimal Dinero { get; private set; }
        public Auto AutoActual { get; private set; }

        // Todos los autos que el jugador compró en el concesionario (su "garage").
        // AutoActual es simplemente cuál de estos tiene equipado para correr.
        private List<Auto> _autosComprados = new List<Auto>();
        public IReadOnlyList<Auto> AutosComprados => _autosComprados;

        // Constructor
        public Jugador(string nombre, decimal dineroInicial)
        {
            Nombre = nombre;
            Dinero = dineroInicial;
            AutoActual = null; // Arranca a pie
        }

        // Método para asignarle un auto (cuando compra uno) o quitárselo (pasando null cuando lo vende)
        public void AsignarAuto(Auto nuevoAuto)
        {
            AutoActual = nuevoAuto;
        }

        // Suma un auto al garage del jugador (se llama cuando lo compra en el concesionario).
        // No lo equipa automáticamente: para eso está AsignarAuto.
        public void AgregarAutoComprado(Auto auto)
        {
            if (auto != null && !_autosComprados.Contains(auto))
            {
                _autosComprados.Add(auto);
            }
        }

        // Saca un auto del garage (se llama al venderlo). Si era el que tenía equipado,
        // se queda sin auto actual hasta que elija otro.
        public void QuitarAutoComprado(Auto auto)
        {
            if (auto == null) return;

            _autosComprados.Remove(auto);
            if (AutoActual == auto)
            {
                AutoActual = null;
            }
        }

        // Método para descontar plata al comprar (devuelve false si no le alcanza)
        public bool RestarDinero(decimal cantidad)
        {
            if (Dinero >= cantidad)
            {
                Dinero -= cantidad;
                return true;
            }
            return false;
        }

        // Método para sumarle plata al vender un auto o ganar una carrera
        public void SumarDinero(decimal cantidad)
        {
            if (cantidad > 0)
            {
                Dinero += cantidad;
            }
        }
    }
}