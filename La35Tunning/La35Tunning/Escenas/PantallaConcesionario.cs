using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Entidades;
using La35Tunning.Modelos;

namespace La35Tunning.Escenas
{
    public class PantallaConcesionario
    {
        private readonly Sistemas.Concesionario _concesionario;
        private readonly Jugador _jugador;
        private readonly Texture2D _texturaPixel;

        private MouseState _mouseAnterior;
        private bool _entradaInicializada;

        // Índice del primer auto del catálogo que se ve en pantalla (esto es "el scroll")
        private int _scrollCatalogo;
        private const int FilasVisibles = 3;

        private string _mensajeFeedback = "";
        private float _tiempoMensajeRestante;

        // En cada Draw anotamos dónde quedó dibujado cada botón, para poder revisar en el
        // próximo Update si el click cayó ahí adentro. Es el mismo approach que un Rectangle
        // de colisión en Java: se recalcula todo el tiempo porque la lista de autos cambia
        // (se van vendiendo/comprando) y las filas se corren de posición.
        private readonly List<(Rectangle rect, int indiceCatalogo)> _botonesComprar = new();
        private readonly List<(Rectangle rect, Auto auto)> _botonesEquipar = new();
        private readonly List<(Rectangle rect, Auto auto)> _botonesVender = new();

        private readonly Rectangle _rectFlechaArriba = new Rectangle(700, 145, 40, 35);
        private readonly Rectangle _rectFlechaAbajo = new Rectangle(700, 565, 40, 35);

        public PantallaConcesionario(Sistemas.Concesionario concesionario, Jugador jugador, GraphicsDevice graphicsDevice)
        {
            _concesionario = concesionario;
            _jugador = jugador;
            _texturaPixel = new Texture2D(graphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });
        }

        // Se llama cada vez que entramos a esta pantalla desde el menú, para no arrastrar
        // el estado de la visita anterior (el mismo bug que ya nos pasó con DebeVolver en Configuración).
        public void Reiniciar()
        {
            _entradaInicializada = false;
            _scrollCatalogo = 0;
            _mensajeFeedback = "";
            _tiempoMensajeRestante = 0f;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();

            // Primer frame en esta pantalla: solo guardamos el estado del mouse, no reaccionamos
            // a nada todavía (evita procesar un click que en realidad era para el menú anterior).
            if (!_entradaInicializada)
            {
                _mouseAnterior = mouseActual;
                _entradaInicializada = true;
                return;
            }

            if (_tiempoMensajeRestante > 0f)
                _tiempoMensajeRestante -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool hizoClic = mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released;

            // ScrollWheelValue es un acumulado total, no un delta. Restando el valor anterior
            // sacamos cuánto giró la rueda desde el último frame. Cada "click" de rueda son 120 unidades.
            int deltaRueda = mouseActual.ScrollWheelValue - _mouseAnterior.ScrollWheelValue;
            if (deltaRueda != 0)
            {
                _scrollCatalogo -= deltaRueda / 120;
            }

            if (hizoClic)
            {
                Point posicion = mouseActual.Position;

                if (_rectFlechaArriba.Contains(posicion)) _scrollCatalogo--;
                if (_rectFlechaAbajo.Contains(posicion)) _scrollCatalogo++;

                foreach (var boton in _botonesComprar)
                {
                    if (boton.rect.Contains(posicion))
                    {
                        MostrarMensaje(_concesionario.ComprarAuto(_jugador, boton.indiceCatalogo));
                        break;
                    }
                }

                foreach (var boton in _botonesEquipar)
                {
                    if (boton.rect.Contains(posicion))
                    {
                        _jugador.AsignarAuto(boton.auto);
                        MostrarMensaje($"Ahora estás usando el {boton.auto.Modelo}.");
                        break;
                    }
                }

                foreach (var boton in _botonesVender)
                {
                    if (boton.rect.Contains(posicion))
                    {
                        MostrarMensaje(_concesionario.VenderAuto(_jugador, boton.auto));
                        break;
                    }
                }
            }

            int totalCatalogo = _concesionario.ObtenerCatalogo().Count;
            int maximoScroll = Math.Max(0, totalCatalogo - FilasVisibles);
            _scrollCatalogo = MathHelper.Clamp(_scrollCatalogo, 0, maximoScroll);

            _mouseAnterior = mouseActual;
        }

        private void MostrarMensaje(string mensaje)
        {
            _mensajeFeedback = mensaje;
            _tiempoMensajeRestante = 3f;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            // Recalculamos los botones desde cero en cada frame: si se compra o vende un auto,
            // la cantidad de filas cambia, así que las posiciones de clickeo de antes ya no sirven.
            _botonesComprar.Clear();
            _botonesEquipar.Clear();
            _botonesVender.Clear();

            spriteBatch.DrawString(fuente, "CONCESIONARIO", new Vector2(60, 40), Color.Gold);
            spriteBatch.DrawString(fuente, $"Dinero: ${_jugador.Dinero}", new Vector2(60, 75), Color.White);

            DibujarCatalogo(spriteBatch, fuente);
            DibujarGarage(spriteBatch, fuente);

            if (_tiempoMensajeRestante > 0f)
            {
                spriteBatch.DrawString(fuente, _mensajeFeedback, new Vector2(60, 660), Color.Cyan);
            }

            spriteBatch.DrawString(fuente, "Presiona [ ESC ] para volver al menu", new Vector2(60, 700), Color.Gray);
        }

        private void DibujarCatalogo(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            spriteBatch.DrawString(fuente, "EN VENTA", new Vector2(60, 115), Color.Yellow);

            List<Auto> catalogo = _concesionario.ObtenerCatalogo();

            if (catalogo.Count == 0)
            {
                spriteBatch.DrawString(fuente, "Ya compraste todos los autos disponibles.", new Vector2(60, 150), Color.Gray);
                return;
            }

            int y = 150;
            int ultimoIndice = Math.Min(catalogo.Count, _scrollCatalogo + FilasVisibles);

            for (int i = _scrollCatalogo; i < ultimoIndice; i++)
            {
                Auto auto = catalogo[i];
                Rectangle fila = new Rectangle(60, y, 610, 120);
                DibujarFilaAuto(spriteBatch, fuente, fila, auto);

                Rectangle botonComprar = new Rectangle(fila.Right - 150, fila.Bottom - 45, 130, 35);
                bool puedeComprar = _jugador.Dinero >= auto.Precio;

                DibujarBoton(spriteBatch, fuente, botonComprar, "COMPRAR",
                    puedeComprar ? Color.DarkGreen : Color.DarkSlateGray,
                    puedeComprar ? Color.White : Color.Gray);

                if (puedeComprar)
                {
                    _botonesComprar.Add((botonComprar, i));
                }

                y += fila.Height + 15;
            }

            // Flechas para moverse por el catálogo cuando hay más autos de los que entran en pantalla.
            bool hayMasArriba = _scrollCatalogo > 0;
            bool hayMasAbajo = _scrollCatalogo + FilasVisibles < catalogo.Count;

            DibujarBoton(spriteBatch, fuente, _rectFlechaArriba, "^",
                hayMasArriba ? Color.DarkSlateGray : Color.Black, hayMasArriba ? Color.White : Color.DarkGray);
            DibujarBoton(spriteBatch, fuente, _rectFlechaAbajo, "v",
                hayMasAbajo ? Color.DarkSlateGray : Color.Black, hayMasAbajo ? Color.White : Color.DarkGray);
        }

        private void DibujarGarage(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            spriteBatch.DrawString(fuente, "TU GARAGE", new Vector2(780, 115), Color.Yellow);

            IReadOnlyList<Auto> autos = _jugador.AutosComprados;

            if (autos.Count == 0)
            {
                spriteBatch.DrawString(fuente, "Todavía no compraste ningún auto.", new Vector2(780, 150), Color.Gray);
                return;
            }

            int y = 150;
            foreach (Auto auto in autos)
            {
                Rectangle fila = new Rectangle(780, y, 610, 120);
                DibujarFilaAuto(spriteBatch, fuente, fila, auto);

                bool esElEquipado = _jugador.AutoActual == auto;

                Rectangle botonEquipar = new Rectangle(fila.Right - 300, fila.Bottom - 45, 130, 35);
                Rectangle botonVender = new Rectangle(fila.Right - 150, fila.Bottom - 45, 130, 35);

                if (esElEquipado)
                {
                    DibujarBoton(spriteBatch, fuente, botonEquipar, "EQUIPADO", Color.DarkSlateGray, Color.Gold);
                }
                else
                {
                    DibujarBoton(spriteBatch, fuente, botonEquipar, "EQUIPAR", Color.DarkBlue, Color.White);
                    _botonesEquipar.Add((botonEquipar, auto));
                }

                DibujarBoton(spriteBatch, fuente, botonVender, "VENDER", Color.DarkRed, Color.White);
                _botonesVender.Add((botonVender, auto));

                y += fila.Height + 15;
            }
        }

        private void DibujarFilaAuto(SpriteBatch spriteBatch, SpriteFont fuente, Rectangle fila, Auto auto)
        {
            spriteBatch.Draw(_texturaPixel, fila, new Color(30, 30, 35));

            if (auto.TexturaAuto != null)
            {
                Rectangle destinoImagen = new Rectangle(fila.X + 10, fila.Y + 10, 150, 100);
                spriteBatch.Draw(auto.TexturaAuto, destinoImagen, Color.White);
            }

            spriteBatch.DrawString(fuente, auto.Modelo, new Vector2(fila.X + 170, fila.Y + 10), Color.White);
            spriteBatch.DrawString(fuente, $"${auto.Precio}", new Vector2(fila.X + 170, fila.Y + 40), Color.LightGreen);
        }

        private void DibujarBoton(SpriteBatch spriteBatch, SpriteFont fuente, Rectangle rectangulo, string texto, Color colorFondo, Color colorTexto)
        {
            spriteBatch.Draw(_texturaPixel, rectangulo, colorFondo);
            Vector2 medida = fuente.MeasureString(texto);
            Vector2 posicionTexto = new Vector2(
                rectangulo.Center.X - medida.X / 2,
                rectangulo.Center.Y - medida.Y / 2);
            spriteBatch.DrawString(fuente, texto, posicionTexto, colorTexto);
        }
    }
}
