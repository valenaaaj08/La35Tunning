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

        // NUEVO: para poder volver a fabricar un auto cuando lo vendés (ver DevolverAlCatalogo).
        private readonly ContentManager _content;
        private readonly Texture2D _texturaLlantaDefault;
        private readonly List<string> _ordenOriginal = new List<string>();

        public Concesionario(ContentManager content, Texture2D texturaLlantaDefault)
        {
            _content = content;
            _texturaLlantaDefault = texturaLlantaDefault;
            _catalogo = new List<Auto>();

            Auto autoGol = AutoFactory.CrearGol(content, texturaLlantaDefault);
            Auto autoUno = AutoFactory.CrearUno(content, texturaLlantaDefault);
            Auto autoClio = AutoFactory.CrearClio(content, texturaLlantaDefault);
            Auto autoCorsa = AutoFactory.CrearCorsa(content, texturaLlantaDefault);

            _catalogo.Add(autoGol);
            _catalogo.Add(autoUno);
            _catalogo.Add(autoClio);
            _catalogo.Add(autoCorsa);

            // NUEVO: anotamos el orden de la vidriera para devolver cada auto a su lugar.
            foreach (Auto auto in _catalogo)
                _ordenOriginal.Add(auto.Modelo);
        }

        public List<Auto> ObtenerCatalogo()
        {
            return _catalogo;
        }

        public string ComprarAuto(Jugador jugador, int indiceAuto)
        {
            if (indiceAuto < 0 || indiceAuto >= _catalogo.Count)
                return "Ese auto ya no está disponible.";

            Auto autoAComprar = _catalogo[indiceAuto];

            if (jugador.Dinero < autoAComprar.Precio)
                return "No te alcanza la plata para este auto.";

            jugador.RestarDinero(autoAComprar.Precio);
            jugador.AgregarAutoComprado(autoAComprar);

            _catalogo.RemoveAt(indiceAuto);

            return $"Compraste el {autoAComprar.Modelo}.";
        }

        public string VenderAuto(Jugador jugador, Auto auto)
        {
            if (auto == null || !jugador.AutosComprados.Contains(auto))
                return "Ese auto no está en tu garage.";

            decimal dineroGanado = auto.Precio * 0.5m;

            jugador.SumarDinero(dineroGanado);
            jugador.QuitarAutoComprado(auto);

            // ARREGLO: antes el auto vendido desaparecía para siempre (no volvía a la venta).
            DevolverAlCatalogo(auto.Modelo);

            // CAMBIO: UI.Dinero -> "$ 1.900.000" (antes salía "$1900000,0").
            return $"Vendiste el {auto.Modelo} por {UI.Dinero(dineroGanado)}.";
        }

        /// <summary>
        /// NUEVO: pone a la venta un auto NUEVO del mismo modelo, en el mismo lugar de la
        /// vidriera donde estaba al principio. Se fabrica de nuevo con AutoFactory (y no se
        /// devuelve el mismo objeto) para que vuelva de fábrica: sin las piezas ni las
        /// llantas que le pusiste en el taller.
        /// </summary>
        private void DevolverAlCatalogo(string modelo)
        {
            // Por las dudas: que no quede repetido en la vidriera.
            if (_catalogo.Any(a => a.Modelo == modelo))
                return;

            Auto autoNuevo = AutoFactory.CrearAuto(modelo, _content, _texturaLlantaDefault);

            // Buscamos la posición: después de todos los que estaban antes en el orden original.
            int orden = _ordenOriginal.IndexOf(modelo);
            int posicion = 0;
            while (posicion < _catalogo.Count && _ordenOriginal.IndexOf(_catalogo[posicion].Modelo) < orden)
                posicion++;

            _catalogo.Insert(posicion, autoNuevo);
        }
    }
}