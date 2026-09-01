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

        private int _indiceSeleccionado = 0;
        private KeyboardState _tecladoAnterior;

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
                if (teclado.IsKeyDown(Keys.Down) && _tecladoAnterior.IsKeyUp(Keys.Down))
                {
                    _indiceSeleccionado = (int)MathHelper.Clamp(_indiceSeleccionado + 1, 0, _taller.CatalogoPiezas.Count - 1);
                }
                else if (teclado.IsKeyDown(Keys.Up) && _tecladoAnterior.IsKeyUp(Keys.Up))
                {
                    _indiceSeleccionado = (int)MathHelper.Clamp(_indiceSeleccionado - 1, 0, _taller.CatalogoPiezas.Count - 1);
                }
                else if (teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter))
                {
                    _taller.InstalarPieza(_jugador.AutoActual, _indiceSeleccionado, _jugador);
                }
            }

            _tecladoAnterior = teclado;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            if (_jugador == null) return;

            // 1. Dibujar primero el fondo del taller (ocupa toda la pantalla o la posición inicial)
            if (_fondoTaller != null)
            {
                spriteBatch.Draw(_fondoTaller, Vector2.Zero, Color.White);
            }

            // 2. Dibujar el auto actual de frente (usando TexturaTaller), si existe una imagen para eso
            if (_jugador.AutoActual != null)
            {
                if (_jugador.AutoActual.TexturaTaller != null)
                {
                    spriteBatch.Draw(_jugador.AutoActual.TexturaTaller, _posicionDibujoAuto, Color.White);
                }

                // La info del auto y la plata se muestran SIEMPRE que haya un auto asignado,
                // tenga o no imagen de "vista de frente" cargada.
                spriteBatch.DrawString(fuente, $"Auto: {_jugador.AutoActual.Modelo}", new Vector2(50, 50), Color.White);
                spriteBatch.DrawString(fuente, $"Dinero: ${_jugador.Dinero}", new Vector2(50, 80), Color.White);
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
                Color colorTexto = (i == _indiceSeleccionado) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(fuente, textoPieza, posicionTexto, colorTexto);
                posicionTexto.Y += 25;
            }

            posicionTexto.Y += 15;
            spriteBatch.DrawString(fuente, "Flechas ARRIBA/ABAJO para elegir, ENTER para instalar", posicionTexto, Color.Gray);

        }
    }
}