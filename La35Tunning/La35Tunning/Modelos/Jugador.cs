using La35Tunning.Modelos;

namespace La35Tunning.Entidades
{
    public class Jugador
    {
        public string Nombre { get; set; }
        public int Dinero { get; set; }
        
        // El jugador posee un auto asignado
        public Auto AutoActual { get; set; }

        

                public Jugador(string nombre, int dineroInicial)
        {
            Nombre = nombre;
            Dinero = dineroInicial;
            AutoActual = null; // Arranca sin auto
        }

        // Método para comprar una pieza pasando el objeto Componente
        public void ComprarPieza(Componente nuevaPieza)
        {
            if (Dinero >= nuevaPieza.Costo)
            {
                Dinero -= nuevaPieza.Costo;
                AutoActual.InstalarPieza(nuevaPieza);
                System.Console.WriteLine($"¡{nuevaPieza.Nombre} comprado e instalado con éxito!");
            }
            else
            {
                System.Console.WriteLine("¡No te alcanza la plata!");
            }
        }
    }
}