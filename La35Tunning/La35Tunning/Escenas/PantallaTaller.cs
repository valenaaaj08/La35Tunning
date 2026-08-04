using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using La35Tunning.Entidades;
using La35Tunning.Sistemas;
using La35Tunning.Modelos;

namespace La35Tunning.Escenas
{
    public class PantallaTaller
    {
        private Taller _taller;
        private Jugador _jugador;
        
        // Textura de fondo del taller
        private Texture2D _fondoTaller;

        // Posición en pantalla donde se dibuja el auto de frente en el taller
        private Vector2 _posicionDibujoAuto = new Vector2(400, 150);

        public PantallaTaller(ContentManager content, Jugador jugador)
        {
            _taller = new Taller(content);
            _jugador = jugador;

            // Cargamos el fondo del taller desde el ContentPipeline (asumiendo que se llama "FondoTaller")
            _fondoTaller = content.Load<Texture2D>("FondoTaller");
        }

        public void Update(GameTime gameTime)
        {
            var teclado = Keyboard.GetState();

            if (_jugador.AutoActual != null)
            {
                // Ejemplo: Presionando Enter instala la primera pieza al auto actual
                if (teclado.IsKeyDown(Keys.Enter))
                {
                    _taller.InstalarPieza(_jugador.AutoActual, 0); 
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            if (_jugador == null) return;

            // 1. Dibujar primero el fondo del taller (ocupa toda la pantalla o la posición inicial)
            if (_fondoTaller != null)
            {
                spriteBatch.Draw(_fondoTaller, Vector2.Zero, Color.White);
            }

            // 2. Dibujar el auto actual de frente (usando TexturaTaller)
            if (_jugador.AutoActual != null && _jugador.AutoActual.TexturaTaller != null)
            {
                spriteBatch.Draw(_jugador.AutoActual.TexturaTaller, _posicionDibujoAuto, Color.White);
                
                // Mostrar información del auto y dinero actual
                spriteBatch.DrawString(fuente, $"Auto: {_jugador.AutoActual.Modelo}", new Vector2(50, 50), Color.White);
                spriteBatch.DrawString(fuente, $"Dinero: ${_jugador.Dinero}", new Vector2(50, 80), Color.Green);
            }
            else
            {
                spriteBatch.DrawString(fuente, "No hay ningún auto en el taller.", new Vector2(50, 50), Color.Red);
            }

            // 3. Listar las piezas disponibles en el taller para comprar
            Vector2 posicionTexto = new Vector2(50, 200);
            spriteBatch.DrawString(fuente, "--- PIEZAS DISPONIBLES EN TALLER ---", posicionTexto, Color.Yellow);
            posicionTexto.Y += 30;

            for (int i = 0; i < _taller.CatalogoPiezas.Count; i++)
            {
                Componente pieza = _taller.CatalogoPiezas[i];
                string textoPieza = $"{i + 1}. {pieza.Nombre} - ${pieza.Costo}";
                spriteBatch.DrawString(fuente, textoPieza, posicionTexto, Color.White);
                posicionTexto.Y += 25;
            }
        }
    }
}