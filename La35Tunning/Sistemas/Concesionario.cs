using System.Collections.Generic;
using System.Linq;
using La35Tunning.Entidades;
using La35Tunning.Modelos;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

        // 1. Comprar un auto del catálogo. Devuelve un mensaje para mostrarle al jugador
        // (así la pantalla no tiene que andar armando los textos de éxito/error, se los pedimos acá).
        public string ComprarAuto(Jugador jugador, int indiceAuto)
        {
            if (indiceAuto < 0 || indiceAuto >= _catalogo.Count)
                return "Ese auto ya no está disponible.";

            Auto autoAComprar = _catalogo[indiceAuto];

            if (jugador.Dinero < autoAComprar.Precio)
                return "No te alcanza la plata para este auto.";

            jugador.RestarDinero(autoAComprar.Precio);
            jugador.AgregarAutoComprado(autoAComprar);

            // Una vez comprado, sale de la vidriera: cada auto del catálogo se vende una sola vez.
            _catalogo.RemoveAt(indiceAuto);

            return $"Compraste el {autoAComprar.Modelo}.";
        }

        // 2. Vender un auto puntual del garage del jugador (no necesariamente el que tiene equipado).
        // NOTA: por ahora solo se paga el 50% del precio base del auto, sin sumar el valor de las
        // piezas instaladas. Es una limitación conocida, documentada en el README/Changelog.
        public string VenderAuto(Jugador jugador, Auto auto)
        {
            if (auto == null || !jugador.AutosComprados.Contains(auto))
                return "Ese auto no está en tu garage.";

            decimal dineroGanado = auto.Precio * 0.5m;

            jugador.SumarDinero(dineroGanado);
            jugador.QuitarAutoComprado(auto);

            return $"Vendiste el {auto.Modelo} por ${dineroGanado}.";
        }
    }
}