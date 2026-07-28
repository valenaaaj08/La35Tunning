using La35Tunning.Modelos;

namespace La35Tunning.Entidades
{
    public class Jugador
    {
        public string Nombre { get; set; }
        public int Dinero { get; set; }


        public Auto AutoActual { get; set; }



        public Jugador(string nombre, int dineroInicial)
        {
            Nombre = nombre;
            Dinero = dineroInicial;
            AutoActual = null; // Arranca sin auto
        }


    }
}