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
        private int _indiceLlantaSeleccionada = 0;
        private bool _modoLlantas = false;
        private KeyboardState _tecladoAnterior;
        private MouseState _mouseAnterior;

        private Texture2D _fondoTaller;

        private Vector2 _posicionDibujoAuto = new Vector2(700, 700);
        private float _escalaDibujoAuto = 1.0f;
        private float _escalaLlantasTaller = 2.0f;

        public PantallaTaller(ContentManager content, Jugador jugador)
        {
            _taller = new Taller(content);
            _jugador = jugador;
            _fondoTaller = content.Load<Texture2D>("FondoTaller");
        }

        public void Update(GameTime gameTime)
        {
            var teclado = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (_jugador.AutoActual != null)
            {
                if (teclado.IsKeyDown(Keys.P) && _tecladoAnterior.IsKeyUp(Keys.P))
                {
                    _modoLlantas = false;
                }
                if (teclado.IsKeyDown(Keys.L) && _tecladoAnterior.IsKeyUp(Keys.L))
                {
                    _modoLlantas = true;
                }

                if (_modoLlantas)
                {
                    if ((teclado.IsKeyDown(Keys.Right) || teclado.IsKeyDown(Keys.Down)) &&
                        ( _tecladoAnterior.IsKeyUp(Keys.Right) && _tecladoAnterior.IsKeyUp(Keys.Down)))
                    {
                        _indiceLlantaSeleccionada++;
                        if (_indiceLlantaSeleccionada >= _taller.CatalogoLlantas.Count)
                            _indiceLlantaSeleccionada = 0;
                    }
                    else if ((teclado.IsKeyDown(Keys.Left) || teclado.IsKeyDown(Keys.Up)) &&
                        (_tecladoAnterior.IsKeyUp(Keys.Left) && _tecladoAnterior.IsKeyUp(Keys.Up)))
                    {
                        _indiceLlantaSeleccionada--;
                        if (_indiceLlantaSeleccionada < 0)
                            _indiceLlantaSeleccionada = _taller.CatalogoLlantas.Count - 1;
                    }
                    else if (teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter))
                    {
                        _taller.CambiarLlantas(
                            _jugador.AutoActual,
                            _indiceLlantaSeleccionada,
                            _indiceLlantaSeleccionada);
                    }

                    if (mouse.LeftButton == ButtonState.Pressed &&
                        _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        Point posicionMouse = new Point(mouse.X, mouse.Y);

                        for (int i = 0; i < _taller.CatalogoLlantas.Count; i++)
                        {
                            Rectangle botonLlanta = new Rectangle(50, 270 + (i * 25), 250, 22);

                            if (botonLlanta.Contains(posicionMouse))
                            {
                                _indiceLlantaSeleccionada = i;
                                _taller.CambiarLlantas(
                                    _jugador.AutoActual,
                                    _indiceLlantaSeleccionada,
                                    _indiceLlantaSeleccionada);
                            }
                        }
                    }
                }
                else
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

                    if (mouse.LeftButton == ButtonState.Pressed &&
                        _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        Point posicionMouse = new Point(mouse.X, mouse.Y);

                        for (int i = 0; i < _taller.CatalogoPiezas.Count; i++)
                        {
                            Rectangle botonPieza = new Rectangle(50, 270 + (i * 25), 350, 22);

                            if (botonPieza.Contains(posicionMouse))
                            {
                                _indiceSeleccionado = i;
                                _taller.InstalarPieza(_jugador.AutoActual, _indiceSeleccionado, _jugador);
                            }
                        }
                    }
                }
            }

            _tecladoAnterior = teclado;
            _mouseAnterior = mouse;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            if (_jugador == null) return;

            if (_fondoTaller != null)
            {
                Rectangle pantallaCompleta = new Rectangle(
                    0,
                    0,
                    spriteBatch.GraphicsDevice.Viewport.Width,
                    spriteBatch.GraphicsDevice.Viewport.Height);
                spriteBatch.Draw(_fondoTaller, pantallaCompleta, Color.White);
            }

            if (_jugador.AutoActual != null)
            {
                if (_jugador.AutoActual.TexturaTaller != null)
                {
                    spriteBatch.Draw(
                        _jugador.AutoActual.TexturaTaller,
                        _posicionDibujoAuto,
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        _escalaDibujoAuto,
                        Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                        0f);
                }
                else
                {
                    _jugador.AutoActual.Draw(
                        spriteBatch,
                        _posicionDibujoAuto,
                        _escalaDibujoAuto,
                        _escalaLlantasTaller);
                }

                spriteBatch.DrawString(fuente, $"Auto: {_jugador.AutoActual.Modelo}", new Vector2(50, 50), Color.White);
                spriteBatch.DrawString(fuente, $"Dinero: ${_jugador.Dinero}", new Vector2(50, 80), Color.White);
            }
            else
            {
                spriteBatch.DrawString(fuente, "No hay ningún auto en el taller.", new Vector2(50, 50), Color.Red);
            }

            Vector2 posicionTexto = new Vector2(50, 200);
            spriteBatch.DrawString(fuente, "P = PIEZAS    L = LLANTAS", posicionTexto, Color.Cyan);
            posicionTexto.Y += 40;

            if (_modoLlantas == false)
            {
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
            else
            {
                spriteBatch.DrawString(fuente, "--- LLANTAS DISPONIBLES ---", posicionTexto, Color.Yellow);
                posicionTexto.Y += 30;

                for (int i = 0; i < _taller.CatalogoLlantas.Count; i++)
                {
                    string nombreLlanta = _taller.NombresLlantas[i];

                    Color colorTexto = (i == _indiceLlantaSeleccionada) ? Color.Yellow : Color.White;
                    spriteBatch.DrawString(fuente, nombreLlanta, posicionTexto, colorTexto);
                    posicionTexto.Y += 25;
                }

                Texture2D llantaSeleccionada = _taller.CatalogoLlantas[_indiceLlantaSeleccionada];
                spriteBatch.Draw(llantaSeleccionada, new Rectangle(50, 450, 150, 150), Color.White);

                posicionTexto.Y += 15;
                spriteBatch.DrawString(fuente, "Flechas para moverse, ENTER para instalar", posicionTexto, Color.Gray);
            }
        }
    }
}