using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using La35Tunning.Entidades;

namespace La35Tunning.Factories
{
    /// <summary>
    /// Factory para la creación de autos.
    /// Centraliza toda la lógica de instanciación de vehículos.
    /// </summary>
    public static class AutoFactory
    {
        /// <summary>
        /// Crea un auto Volkswagen Gol G3
        /// </summary>
        public static Auto CrearGol(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaGol = content.Load<Texture2D>("gol");
            Auto auto = new Auto("Volkswagen Gol G3", 8f, 0.15f, 4500000, texturaGol);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        /// <summary>
        /// Crea un auto Fiat Uno
        /// </summary>
        public static Auto CrearUno(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaUno = content.Load<Texture2D>("Uno");
            Auto auto = new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, texturaUno);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        /// <summary>
        /// Crea un auto Renault Clio
        /// </summary>
        public static Auto CrearClio(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaClio = content.Load<Texture2D>("clio");
            Auto auto = new Auto("Renault Clio", 8.5f, 0.16f, 5200000, texturaClio);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        /// <summary>
        /// Crea un auto Chevrolet Corsa
        /// </summary>
        public static Auto CrearCorsa(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaCorsa = content.Load<Texture2D>("corsa");
            Auto auto = new Auto("Chevrolet Corsa", 8f, 0.15f, 4200000, texturaCorsa);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        /// <summary>
        /// Crea un auto por nombre (útil para futuras expansiones)
        /// </summary>
        public static Auto CrearAuto(string nombreAuto, ContentManager content, Texture2D texturaLlantas)
        {
            return nombreAuto.ToLower() switch
            {
                "gol" or "volkswagen gol g3" => CrearGol(content, texturaLlantas),
                "uno" or "fiat uno" => CrearUno(content, texturaLlantas),
                "clio" or "renault clio" => CrearClio(content, texturaLlantas),
                "corsa" or "chevrolet corsa" => CrearCorsa(content, texturaLlantas),
                _ => throw new System.ArgumentException($"Auto no reconocido: {nombreAuto}")
            };
        }
    }
}
