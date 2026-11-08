using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Entidades;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;

namespace La35Tunning.Escenas
{
    public class PantallaConcesionario
    {
        private readonly Sistemas.Concesionario _concesionario;
        private readonly Jugador _jugador;
        private readonly Texture2D _texturaPixel;

        private MouseState _mouseAnterior;
        private bool _entradaInicializada;

        private int _scrollCatalogo;
        private const int FilasVisibles = 3;

        private string _mensajeFeedback = "";
        private float _tiempoMensajeRestante;

        private readonly List<(Rectangle rect, int indiceCatalogo)> _botonesComprar = new();
        private readonly List<(Rectangle rect, Auto auto)> _botonesEquipar = new();
        private readonly List<(Rectangle rect, Auto auto)> _botonesVender = new();

        // ANTES eran "readonly" con posiciones fijas. AHORA se recalculan en Draw()
        // según el tamaño de la pantalla, para que todo quede centrado.
        private Rectangle _rectFlechaArriba = new Rectangle(700, 145, 40, 35);
        private Rectangle _rectFlechaAbajo = new Rectangle(700, 565, 40, 35);

        // ---- NUEVO ----
        private Rectangle _rectVolver;
        private Point _posicionMouse;
        private bool _mensajeEsError;

        /// <summary>NUEVO: true cuando tocan el botón VOLVER (Game1 vuelve al menú).</summary>
        public bool DebeVolver { get; private set; }

        public PantallaConcesionario(Sistemas.Concesionario concesionario, Jugador jugador, GraphicsDevice graphicsDevice)
        {
            _concesionario = concesionario;
            _jugador = jugador;
            _texturaPixel = new Texture2D(graphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });
        }

        public void Reiniciar()
        {
            _entradaInicializada = false;
            _scrollCatalogo = 0;
            _mensajeFeedback = "";
            _tiempoMensajeRestante = 0f;
            DebeVolver = false;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();
            _posicionMouse = mouseActual.Position;

            if (!_entradaInicializada)
            {
                _mouseAnterior = mouseActual;
                _entradaInicializada = true;
                return;
            }

            if (_tiempoMensajeRestante > 0f)
                _tiempoMensajeRestante -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool hizoClic = mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released;

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
                if (_rectVolver.Contains(posicion)) DebeVolver = true;

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
            // Los mensajes de error del Concesionario empiezan con "No" o "Ese" (ver Sistemas/Concesionario.cs).
            _mensajeEsError = mensaje.StartsWith("No") || mensaje.StartsWith("Ese");
        }

        // ==================================================================
        // DIBUJO
        // ==================================================================

        // Medidas calculadas en cada Draw según la pantalla.
        private int _origenX, _origenY, _anchoColumna, _altoTarjeta;
        private const int SeparacionTarjetas = 14;

        private void CalcularLayout(int anchoPantalla, int altoPantalla)
        {
            int anchoLienzo = Math.Min(1400, anchoPantalla - 40);
            _anchoColumna = (anchoLienzo - 40) / 2;
            _altoTarjeta = (int)MathHelper.Clamp((altoPantalla - 330) / 4f - SeparacionTarjetas, 104f, 140f);

            int altoLienzo = 150 + 4 * (_altoTarjeta + SeparacionTarjetas) + 80;
            _origenX = (anchoPantalla - anchoLienzo) / 2;
            _origenY = Math.Max(12, (altoPantalla - altoLienzo) / 2);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            _botonesComprar.Clear();
            _botonesEquipar.Clear();
            _botonesVender.Clear();

            int ancho = graphicsDevice.Viewport.Width;
            int alto = graphicsDevice.Viewport.Height;
            CalcularLayout(ancho, alto);

            DibujarFondoShowroom(spriteBatch, ancho, alto);

            // ---- Encabezado ----
            UI.Texto(spriteBatch, UI.Titulo, "CONCESIONARIO", new Vector2(_origenX, _origenY + 6), UI.TextoClaro);
            UI.Texto(spriteBatch, UI.Fuente, "Comprá autos nuevos, elegí con cuál correr o vendé los que no uses.",
                new Vector2(_origenX + 2, _origenY + 62), UI.TextoSecundario);

            string dinero = UI.Dinero(_jugador.Dinero);
            int anchoLienzo = _anchoColumna * 2 + 40;
            UI.Chip(spriteBatch, new Vector2(_origenX + anchoLienzo - UI.AnchoChip("DINERO", dinero), _origenY + 20), "DINERO", dinero, UI.Verde);

            UI.Rect(spriteBatch, new Rectangle(_origenX, _origenY + 96, anchoLienzo, 2), UI.Acento * 0.7f);

            DibujarCatalogo(spriteBatch, fuente);
            DibujarGarage(spriteBatch, fuente);

            // ---- Pie: botón volver + mensaje ----
            int pieY = _origenY + 150 + 4 * (_altoTarjeta + SeparacionTarjetas) + 10;
            pieY = Math.Min(pieY, alto - 60);
            _rectVolver = new Rectangle(_origenX, pieY, 170, 44);
            UI.Boton(spriteBatch, _rectVolver, "< VOLVER", _rectVolver.Contains(_posicionMouse));
            UI.Texto(spriteBatch, UI.Fuente, "o presioná ESC", new Vector2(_rectVolver.Right + 14, pieY + 12), UI.TextoSecundario);

            if (_tiempoMensajeRestante > 0f)
            {
                DibujarMensaje(spriteBatch, ancho, pieY);
            }
        }

        private void DibujarFondoShowroom(SpriteBatch spriteBatch, int ancho, int alto)
        {
            UI.Rect(spriteBatch, new Rectangle(0, 0, ancho, alto), UI.FondoOscuro);
            UI.Degrade(spriteBatch, new Rectangle(0, 0, ancho, alto), new Color(30, 36, 52));

            // Tres "reflectores" de techo
            for (int i = 0; i < 3; i++)
            {
                float x = ancho * (0.2f + 0.3f * i);
                UI.Resplandor(spriteBatch, new Vector2(x, -40), ancho * 0.22f, alto * 0.55f, Color.White * 0.05f);
            }

            // Piso con líneas, como un salón de exposición
            int piso = (int)(alto * 0.78f);
            UI.Degrade(spriteBatch, new Rectangle(0, piso, ancho, alto - piso), Color.Black * 0.5f, true);
            for (int i = 0; i < 6; i++)
            {
                int y = piso + i * i * 6;
                UI.Rect(spriteBatch, new Rectangle(0, y, ancho, 1), Color.White * (0.03f + i * 0.005f));
            }
        }

        private void DibujarCatalogo(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            int x = _origenX;
            int y = _origenY + 112;

            List<Auto> catalogo = _concesionario.ObtenerCatalogo();

            UI.Texto(spriteBatch, UI.Negrita, "EN VENTA", new Vector2(x, y), UI.Acento);
            UI.Texto(spriteBatch, UI.Fuente, $"{catalogo.Count} disponibles", new Vector2(x + 120, y + 3), UI.TextoSecundario);

            // Flechas de scroll al costado del título
            _rectFlechaArriba = new Rectangle(x + _anchoColumna - 92, y - 4, 42, 32);
            _rectFlechaAbajo = new Rectangle(x + _anchoColumna - 44, y - 4, 42, 32);

            y += 38;

            if (catalogo.Count == 0)
            {
                Rectangle vacio = new Rectangle(x, y, _anchoColumna, _altoTarjeta);
                UI.Panel(spriteBatch, vacio, 0.7f);
                UI.TextoCentrado(spriteBatch, UI.Negrita, "¡Ya compraste todos los autos!", new Vector2(vacio.Center.X, vacio.Center.Y - 12), UI.TextoClaro);
                UI.TextoCentrado(spriteBatch, UI.Fuente, "Volvé cuando lleguen modelos nuevos.", new Vector2(vacio.Center.X, vacio.Center.Y + 16), UI.TextoSecundario);
                return;
            }

            int ultimoIndice = Math.Min(catalogo.Count, _scrollCatalogo + FilasVisibles);

            for (int i = _scrollCatalogo; i < ultimoIndice; i++)
            {
                Auto auto = catalogo[i];
                Rectangle fila = new Rectangle(x, y, _anchoColumna, _altoTarjeta);
                bool puedeComprar = _jugador.Dinero >= auto.Precio;

                DibujarFilaAuto(spriteBatch, fuente, fila, auto);

                Rectangle botonComprar = new Rectangle(fila.Right - 156, fila.Center.Y - 21, 140, 42);
                DibujarBotonColor(spriteBatch, botonComprar, "COMPRAR", puedeComprar, new Color(28, 120, 70));

                if (puedeComprar)
                {
                    _botonesComprar.Add((botonComprar, i));
                }
                else
                {
                    decimal falta = auto.Precio - _jugador.Dinero;
                    UI.TextoCentrado(spriteBatch, UI.Fuente, "Te faltan " + UI.Dinero(falta),
                        new Vector2(botonComprar.Center.X, botonComprar.Bottom + 14), UI.Rojo, 0.8f, false);
                }

                y += fila.Height + SeparacionTarjetas;
            }

            bool hayMasArriba = _scrollCatalogo > 0;
            bool hayMasAbajo = _scrollCatalogo + FilasVisibles < catalogo.Count;

            UI.Boton(spriteBatch, _rectFlechaArriba, "^", _rectFlechaArriba.Contains(_posicionMouse), hayMasArriba);
            UI.Boton(spriteBatch, _rectFlechaAbajo, "v", _rectFlechaAbajo.Contains(_posicionMouse), hayMasAbajo);
        }

        private void DibujarGarage(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            int x = _origenX + _anchoColumna + 40;
            int y = _origenY + 112;

            IReadOnlyList<Auto> autos = _jugador.AutosComprados;

            UI.Texto(spriteBatch, UI.Negrita, "TU GARAGE", new Vector2(x, y), UI.Acento);
            UI.Texto(spriteBatch, UI.Fuente, $"{autos.Count} {(autos.Count == 1 ? "auto" : "autos")}", new Vector2(x + 140, y + 3), UI.TextoSecundario);
            y += 38;

            if (autos.Count == 0)
            {
                Rectangle vacio = new Rectangle(x, y, _anchoColumna, _altoTarjeta);
                UI.Panel(spriteBatch, vacio, 0.7f);
                UI.TextoCentrado(spriteBatch, UI.Negrita, "Todavía no compraste ningún auto.", new Vector2(vacio.Center.X, vacio.Center.Y - 12), UI.TextoClaro);
                UI.TextoCentrado(spriteBatch, UI.Fuente, "Elegí uno de la izquierda para empezar a correr.", new Vector2(vacio.Center.X, vacio.Center.Y + 16), UI.TextoSecundario);
                return;
            }

            foreach (Auto auto in autos)
            {
                Rectangle fila = new Rectangle(x, y, _anchoColumna, _altoTarjeta);
                bool esElEquipado = _jugador.AutoActual == auto;

                DibujarFilaAuto(spriteBatch, fuente, fila, auto);

                if (esElEquipado)
                {
                    UI.Borde(spriteBatch, fila, 2, UI.Acento);
                }

                Rectangle botonEquipar = new Rectangle(fila.Right - 156, fila.Y + 12, 140, 38);
                Rectangle botonVender = new Rectangle(fila.Right - 156, fila.Bottom - 46, 140, 36);

                if (esElEquipado)
                {
                    UI.Rect(spriteBatch, botonEquipar, UI.Acento * 0.15f);
                    UI.Borde(spriteBatch, botonEquipar, 1, UI.Acento);
                    UI.TextoCentrado(spriteBatch, UI.Negrita, "EN USO", new Vector2(botonEquipar.Center.X, botonEquipar.Center.Y + 1), UI.Acento);
                }
                else
                {
                    DibujarBotonColor(spriteBatch, botonEquipar, "EQUIPAR", true, new Color(36, 92, 160));
                    _botonesEquipar.Add((botonEquipar, auto));
                }

                DibujarBotonColor(spriteBatch, botonVender, "VENDER", true, new Color(140, 40, 48));
                _botonesVender.Add((botonVender, auto));

                y += fila.Height + SeparacionTarjetas;

                // Si no entran más tarjetas en pantalla, dejamos de dibujar.
                if (y + _altoTarjeta > spriteBatch.GraphicsDevice.Viewport.Height - 70)
                    break;
            }
        }

        /// <summary>
        /// Tarjeta de un auto: foto (con sus llantas puestas), nombre, precio y barras
        /// de estadísticas. ANTES: rectángulo gris con la imagen estirada a 150x100.
        /// </summary>
        private void DibujarFilaAuto(SpriteBatch spriteBatch, SpriteFont fuente, Rectangle fila, Auto auto)
        {
            bool mouseEncima = fila.Contains(_posicionMouse);
            UI.Panel(spriteBatch, fila, mouseEncima ? 1f : 0.9f);
            if (mouseEncima)
            {
                UI.Rect(spriteBatch, fila, Color.White * 0.03f);
            }

            // "Vidriera" donde se apoya el auto
            int anchoFoto = (int)MathHelper.Clamp(fila.Height * 1.9f, 190f, 260f);
            Rectangle foto = new Rectangle(fila.X + 10, fila.Y + 10, anchoFoto, fila.Height - 20);
            UI.Rect(spriteBatch, foto, new Color(12, 14, 19));
            UI.Resplandor(spriteBatch, new Vector2(foto.Center.X, foto.Y), foto.Width * 0.6f, foto.Height * 0.9f, Color.White * 0.08f);
            UI.Degrade(spriteBatch, new Rectangle(foto.X, foto.Bottom - 22, foto.Width, 22), Color.White * 0.05f, true);
            UI.Borde(spriteBatch, foto, 1, Color.Black * 0.6f);

            if (auto.TexturaAuto != null)
            {
                Rectangle zonaAuto = new Rectangle(foto.X + 14, foto.Y + 10, foto.Width - 28, foto.Height - 22);
                auto.DibujarEnArea(spriteBatch, zonaAuto, Color.White);
            }

            // Datos
            int textoX = foto.Right + 18;
            UI.Texto(spriteBatch, UI.Negrita, auto.Modelo, new Vector2(textoX, fila.Y + 12), UI.TextoClaro);

            bool alcanza = _jugador.Dinero >= auto.Precio || _jugador.AutosComprados.Contains(auto);
            UI.Texto(spriteBatch, UI.Negrita, UI.Dinero(auto.Precio), new Vector2(textoX, fila.Y + 40), alcanza ? UI.Acento : UI.Rojo);

            int anchoBarra = Math.Max(60, fila.Right - 170 - (textoX + 105));
            int barraY = fila.Y + 74;
            if (fila.Height >= 120)
            {
                DibujarEstadistica(spriteBatch, "VEL. MÁX.", (auto.VelocidadMaximaBase - 6f) / 3f, textoX, barraY, anchoBarra, UI.Azul);
                DibujarEstadistica(spriteBatch, "ACELERAC.", (auto.AceleracionBase - 0.10f) / 0.10f, textoX, barraY + 24, anchoBarra, UI.Verde);
            }
            else
            {
                DibujarEstadistica(spriteBatch, "VEL. MÁX.", (auto.VelocidadMaximaBase - 6f) / 3f, textoX, barraY, anchoBarra, UI.Azul);
            }
        }

        private void DibujarEstadistica(SpriteBatch spriteBatch, string nombre, float valor, int x, int y, int anchoBarra, Color color)
        {
            UI.Texto(spriteBatch, UI.Fuente, nombre, new Vector2(x, y - 3), UI.TextoSecundario, 0.85f, false);
            UI.Barra(spriteBatch, new Rectangle(x + 100, y, anchoBarra, 10), valor, color);
        }

        private void DibujarBotonColor(SpriteBatch spriteBatch, Rectangle rect, string texto, bool habilitado, Color color)
        {
            UI.Boton(spriteBatch, rect, texto, habilitado && rect.Contains(_posicionMouse), habilitado, color);
        }

        private void DibujarMensaje(SpriteBatch spriteBatch, int anchoPantalla, int y)
        {
            float opacidad = MathHelper.Clamp(_tiempoMensajeRestante / 0.4f, 0f, 1f);
            Color color = _mensajeEsError ? UI.Rojo : UI.Verde;

            Vector2 medida = UI.Negrita.MeasureString(_mensajeFeedback);
            Rectangle caja = new Rectangle((int)(anchoPantalla / 2f - medida.X / 2f - 24), y, (int)medida.X + 48, 44);

            UI.Rect(spriteBatch, caja, Color.Black * 0.8f * opacidad);
            UI.Rect(spriteBatch, new Rectangle(caja.X, caja.Y, 4, caja.Height), color * opacidad);
            UI.Borde(spriteBatch, caja, 1, color * 0.6f * opacidad);
            UI.TextoCentrado(spriteBatch, UI.Negrita, _mensajeFeedback, new Vector2(caja.Center.X, caja.Center.Y + 1), UI.TextoClaro * opacidad);
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
