using La35Tunning.Entidades;

namespace La35Tunning.Modelos
{
    public class Jugador
    {
        // Propiedades del jugador (usamos private set para que solo esta clase pueda modificar los valores directamente)
        public string Nombre { get; private set; }
        public decimal Dinero { get; private set; }
        public Auto AutoActual { get; private set; }

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