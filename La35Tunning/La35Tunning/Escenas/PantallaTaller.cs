using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Entidades;
using La35Tunning.Sistemas;
using La35Tunning.Escenas;

namespace La35Tunning.Pantallas
{
    public class PantallaTaller : IPantalla
    {
        private Jugador _jugador;
        private Taller _taller;

        // Posición donde se va a renderizar el auto de frente en el taller
        private Vector2 _posicionAutoTaller = new Vector2(500, 250);

        public PantallaTaller(Jugador jugador, Taller taller)
        {
            _jugador = jugador;
            _taller = taller;
        }

        public void Update(GameTime gameTime)
        {
            // Acá iría la lógica de detección de clicks en los botones de la interfaz
            // (por ejemplo, si hace clic en "Comprar Motor Stage 1", llamás a:
            // _taller.InstalarPieza(_jugador.AutoActual, indicePieza);)
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Nota: el Begin()/End() del SpriteBatch lo maneja quien orquesta
            // las pantallas (por ejemplo Game1), no cada pantalla individualmente.

            // 1. Dibujar fondo del taller (paredes, herramientas, luces)
            // spriteBatch.Draw(_fondoTaller, Vector2.Zero, Color.White);

            // 2. Dibujar el auto de frente con el capót abierto (si tiene la textura asignada)
            if (_jugador.AutoActual != null && _jugador.AutoActual.TexturaTaller != null)
            {
                spriteBatch.Draw(_jugador.AutoActual.TexturaTaller, _posicionAutoTaller, Color.White);
            }

            // 3. Dibujar interfaz de usuario: dinero actual, listado de componentes con sus precios,
            // stages disponibles y selector de llantas.
        }
    }
}
