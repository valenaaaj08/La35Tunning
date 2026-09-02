using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using La35Tunning.Entidades;
using La35Tunning.Modelos;

namespace La35Tunning.Sistemas
{
    public class Concesionario
    {
        private List<Auto> _catalogo;

        public Concesionario(ContentManager content, Texture2D texturaLlantaDefault)
        {
            _catalogo = new List<Auto>();

            // Cargamos las texturas de los autos desde el contenido
            Texture2D texturaUno = content.Load<Texture2D>("Uno");
            Texture2D texturaGol = content.Load<Texture2D>("gol");
            Texture2D texturaClio = content.Load<Texture2D>("clio");
            Texture2D texturaCorsa = content.Load<Texture2D>("corsa");

            // Creamos e instanciamos todos los vehículos del catálogo
            Auto autoGol = new Auto("Volkswagen Gol G3", 8f, 0.15f, 4500000, texturaGol);
            Auto autoUno = new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, texturaUno);
            Auto autoClio = new Auto("Renault Clio", 8.5f, 0.16f, 5200000, texturaClio);
            Auto autoCorsa = new Auto("Chevrolet Corsa", 8f, 0.15f, 4200000, texturaCorsa);

            // Les instalamos las llantas por defecto
            autoGol.InstalarLlantas(texturaLlantaDefault, texturaLlantaDefault);
            autoUno.InstalarLlantas(texturaLlantaDefault, texturaLlantaDefault);
            autoClio.InstalarLlantas(texturaLlantaDefault, texturaLlantaDefault);
            autoCorsa.InstalarLlantas(texturaLlantaDefault, texturaLlantaDefault);

            // Los agregamos al catálogo
            _catalogo.Add(autoGol);
            _catalogo.Add(autoUno);
            _catalogo.Add(autoClio);
            _catalogo.Add(autoCorsa);
        }

        public List<Auto> ObtenerCatalogo()
        {
            return _catalogo;
        }

        // 1. Comprar un auto nuevo del catálogo
        public bool ComprarAuto(Jugador jugador, int indiceAuto)
        {
            if (indiceAuto < 0 || indiceAuto >= _catalogo.Count)
                return false;

            Auto autoAComprar = _catalogo[indiceAuto];

            if (jugador.Dinero >= autoAComprar.Precio)
            {
                jugador.RestarDinero(autoAComprar.Precio);
                jugador.AsignarAuto(autoAComprar);
                return true;
            }

            return false;
        }

        // 2. Vender el auto actual del jugador al concesionario (50% del valor base + modificaciones)
        public bool VenderAutoAlConcesionario(Jugador jugador)
        {
            if (jugador.AutoActual == null)
                return false;

            // Calculamos el valor de tasación: 50% del precio base del auto actual
            decimal valorBaseReventa = jugador.AutoActual.Precio * 0.5m;

            // Si querés sumar el valor estimado de las modificaciones (por ejemplo, si cada mejora suma un extra)
            // Podrías calcularlo o sumarlo acá. Por ahora toma la base del vehículo.
            decimal dineroGanado = valorBaseReventa;

            // Sumamos el dinero al jugador y le quitamos el auto actual
            jugador.SumarDinero(dineroGanado);
            jugador.AsignarAuto(null); // O el método que uses para desvincular el auto

            return true;
        }
    }
}