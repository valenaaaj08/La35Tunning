using System.Collections.Generic;
using System.Linq;
using La35Tunning.Entidades;
using La35Tunning.Factories;
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

            // Usamos el Factory para crear los autos del catálogo
            Auto autoGol = AutoFactory.CrearGol(content, texturaLlantaDefault);
            Auto autoUno = AutoFactory.CrearUno(content, texturaLlantaDefault);
            Auto autoClio = AutoFactory.CrearClio(content, texturaLlantaDefault);
            Auto autoCorsa = AutoFactory.CrearCorsa(content, texturaLlantaDefault);

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