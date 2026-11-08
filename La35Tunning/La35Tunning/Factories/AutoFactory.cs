using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using La35Tunning.Entidades;

namespace La35Tunning.Factories
{
    /// <summary>
    /// Los últimos 4 números de cada auto son la posición de las llantas, medidas
    /// sobre la imagen tal como se ve en el juego (dada vuelta, mirando a la derecha):
    ///   llanta izquierda X, llanta derecha X, centro de llantas Y, diámetro de llanta.
    /// X y diámetro son fracción del ANCHO de la imagen; Y es fracción del ALTO.
    /// Se midieron sobre los pasaruedas de cada sprite (ver CAMBIOS.md).
    /// </summary>
    public static class AutoFactory
    {
        public static Auto CrearGol(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaGol = content.Load<Texture2D>("gol");
            Auto auto = new Auto("Volkswagen Gol G3", 8f, 0.15f, 4500000, texturaGol, null, 0.133f, 0.765f, 0.705f, 0.148f);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        public static Auto CrearUno(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaUno = content.Load<Texture2D>("Uno");
            Auto auto = new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, texturaUno, null, 0.180f, 0.778f, 0.746f, 0.143f);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        public static Auto CrearClio(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaClio = content.Load<Texture2D>("clio");
            Auto auto = new Auto("Renault Clio", 8.5f, 0.16f, 5200000, texturaClio, null, 0.164f, 0.785f, 0.872f, 0.158f);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

        public static Auto CrearCorsa(ContentManager content, Texture2D texturaLlantas)
        {
            Texture2D texturaCorsa = content.Load<Texture2D>("corsa");
            Auto auto = new Auto("Chevrolet Corsa", 8f, 0.15f, 4200000, texturaCorsa, null, 0.184f, 0.775f, 0.851f, 0.148f);
            auto.InstalarLlantas(texturaLlantas, texturaLlantas);
            return auto;
        }

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
