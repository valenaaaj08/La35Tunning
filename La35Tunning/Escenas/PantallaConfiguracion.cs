using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using La35Tunning.Sistemas;

namespace La35Tunning.Escenas
{
    public class PantallaConfiguracion
    {
        private readonly Texture2D _texturaPixel;
        private readonly GraphicsDevice _graphicsDevice;
        private MouseState _mouseAnterior;
        private KeyboardState _tecladoAnterior;
        private bool _entradaInicializada;
        private int _volumen = 0;

        // ---- NUEVO ----
        // Todas las posiciones están pensadas sobre un "lienzo" de 900x500 que después
        // se centra en la pantalla con ObtenerDesplazamiento(). Así el mismo rectángulo
        // sirve para DIBUJAR el botón y para saber si le hicieron CLIC.
        private const int AnchoLienzo = 900;
        private const int AltoLienzo = 500;

        private static readonly Rectangle RectPanel = new Rectangle(150, 20, 600, 460);
        private static readonly Rectangle RectBajarVolumen = new Rectangle(200, 215, 70, 50);
        private static readonly Rectangle RectSubirVolumen = new Rectangle(630, 215, 70, 50);
        private static readonly Rectangle RectBarraVolumen = new Rectangle(290, 232, 320, 16);
        private static readonly Rectangle RectZonaClicBarra = new Rectangle(290, 220, 320, 40);
        private static readonly Rectangle RectVolver = new Rectangle(300, 380, 300, 56);

        private Point _mouseEnLienzo;
        private float _tiempoDesdeEntrada;

        public bool DebeVolver { get; private set; }

        public PantallaConfiguracion(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _texturaPixel = new Texture2D(graphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });
            SoundEffect.MasterVolume = 0f;
            MediaPlayer.Volume = 0f;
        }

        public void Reiniciar()
        {
            DebeVolver = false;
            _entradaInicializada = false;
            _tiempoDesdeEntrada = 0f;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();
            KeyboardState tecladoActual = Keyboard.GetState();
            _tiempoDesdeEntrada += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 desplazamiento = ObtenerDesplazamiento();
            _mouseEnLienzo = mouseActual.Position - new Point((int)desplazamiento.X, (int)desplazamiento.Y);

            if (!_entradaInicializada)
            {
                _mouseAnterior = mouseActual;
                _tecladoAnterior = tecladoActual;
                _entradaInicializada = true;
                return;
            }

            bool hizoClic = mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released;

            if (hizoClic)
            {
                Point posicion = _mouseEnLienzo;
                if (RectBajarVolumen.Contains(posicion)) CambiarVolumen(-10);
                if (RectSubirVolumen.Contains(posicion)) CambiarVolumen(10);
                if (RectVolver.Contains(posicion)) DebeVolver = true;
            }

            // NUEVO: se puede hacer clic (o arrastrar) directamente sobre la barra de volumen.
            if (mouseActual.LeftButton == ButtonState.Pressed && RectZonaClicBarra.Contains(_mouseEnLienzo))
            {
                float proporcion = (_mouseEnLienzo.X - RectBarraVolumen.X) / (float)RectBarraVolumen.Width;
                int nuevoVolumen = (int)Math.Round(MathHelper.Clamp(proporcion, 0f, 1f) * 10f) * 10;
                CambiarVolumen(nuevoVolumen - _volumen);
            }

            if (tecladoActual.IsKeyDown(Keys.Left) && _tecladoAnterior.IsKeyUp(Keys.Left)) CambiarVolumen(-10);
            if (tecladoActual.IsKeyDown(Keys.Right) && _tecladoAnterior.IsKeyUp(Keys.Right)) CambiarVolumen(10);
            if (tecladoActual.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape)) DebeVolver = true;

            _mouseAnterior = mouseActual;
            _tecladoAnterior = tecladoActual;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            int ancho = graphicsDevice.Viewport.Width;
            int alto = graphicsDevice.Viewport.Height;

            // Oscurecemos lo que haya detrás (Game1 dibuja el menú abajo): queda como ventana emergente.
            float aparicion = MathHelper.Clamp(_tiempoDesdeEntrada / 0.2f, 0f, 1f);
            UI.Rect(spriteBatch, new Rectangle(0, 0, ancho, alto), Color.Black * (0.7f * aparicion));

            Rectangle panel = Mover(RectPanel);
            UI.Panel(spriteBatch, panel, 0.97f, UI.Acento);

            UI.TextoCentrado(spriteBatch, UI.Titulo, "CONFIGURACIÓN", Mover(new Vector2(450, 78)), UI.Acento);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "Ajustá el sonido del juego", Mover(new Vector2(450, 122)), UI.TextoSecundario);

            UI.Rect(spriteBatch, Mover(new Rectangle(200, 150, 500, 1)), UI.BordePanel);

            UI.Texto(spriteBatch, UI.Negrita, "VOLUMEN", Mover(new Vector2(200, 172)), UI.TextoClaro);
            UI.TextoDerecha(spriteBatch, UI.Negrita, $"{_volumen}%", Mover(new Vector2(700, 172)), _volumen == 0 ? UI.Rojo : UI.Acento);

            UI.Boton(spriteBatch, Mover(RectBajarVolumen), "<", RectBajarVolumen.Contains(_mouseEnLienzo));
            UI.Boton(spriteBatch, Mover(RectSubirVolumen), ">", RectSubirVolumen.Contains(_mouseEnLienzo));

            // Barra de volumen con "perilla"
            Rectangle barra = Mover(RectBarraVolumen);
            UI.Barra(spriteBatch, barra, _volumen / 100f, UI.Acento);
            for (int i = 1; i < 10; i++)
            {
                int x = barra.X + barra.Width * i / 10;
                UI.Rect(spriteBatch, new Rectangle(x, barra.Bottom + 4, 1, 5), UI.TextoSecundario * 0.5f);
            }
            Vector2 perilla = new Vector2(barra.X + barra.Width * (_volumen / 100f), barra.Center.Y);
            UI.CirculoLleno(spriteBatch, perilla + new Vector2(0, 2), 13, Color.Black * 0.5f);
            UI.CirculoLleno(spriteBatch, perilla, 12, Color.White);
            UI.CirculoLleno(spriteBatch, perilla, 6, UI.Acento);

            string estado = _volumen == 0 ? "Sonido silenciado" : "Música del menú activada";
            UI.TextoCentrado(spriteBatch, UI.Fuente, estado, Mover(new Vector2(450, 290)), _volumen == 0 ? UI.Rojo * 0.9f : UI.Verde);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "Flechas IZQUIERDA / DERECHA para ajustar", Mover(new Vector2(450, 322)), UI.TextoSecundario);

            UI.Boton(spriteBatch, Mover(RectVolver), "VOLVER", RectVolver.Contains(_mouseEnLienzo), true, new Color(52, 58, 76));
            UI.TextoCentrado(spriteBatch, UI.Fuente, "o presioná ESC", Mover(new Vector2(450, 455)), UI.TextoSecundario * 0.8f);
        }

        private void CambiarVolumen(int cantidad)
        {
            _volumen = MathHelper.Clamp(_volumen + cantidad, 0, 100);
            float volumenNormalizado = _volumen / 100f;
            SoundEffect.MasterVolume = volumenNormalizado;
            MediaPlayer.Volume = volumenNormalizado;
        }

        private void DibujarBoton(SpriteBatch spriteBatch, Rectangle rectangulo, string texto, SpriteFont fuente)
        {
            spriteBatch.Draw(_texturaPixel, rectangulo, Color.DarkSlateGray);
            Vector2 medidaTexto = fuente.MeasureString(texto);
            Vector2 posicionTexto = new Vector2(
                rectangulo.Center.X - medidaTexto.X / 2,
                rectangulo.Center.Y - medidaTexto.Y / 2);
            spriteBatch.DrawString(fuente, texto, posicionTexto, Color.Cyan);
        }

        /// <summary>
        /// ANTES: devolvía Vector2.Zero (todo quedaba pegado arriba a la izquierda).
        /// AHORA: devuelve cuánto hay que correr el lienzo de 900x500 para que quede
        /// en el centro de la pantalla.
        /// </summary>
        private Vector2 ObtenerDesplazamiento()
        {
            int ancho = _graphicsDevice.Viewport.Width;
            int alto = _graphicsDevice.Viewport.Height;
            return new Vector2(
                Math.Max(0, (ancho - AnchoLienzo) / 2),
                Math.Max(0, (alto - AltoLienzo) / 2));
        }

        private Rectangle Mover(Rectangle rectangulo)
        {
            Vector2 desplazamiento = ObtenerDesplazamiento();
            // ANTES solo se movía en X (faltaba sumar el desplazamiento en Y).
            return new Rectangle(rectangulo.X + (int)desplazamiento.X, rectangulo.Y + (int)desplazamiento.Y, rectangulo.Width, rectangulo.Height);
        }

        private Vector2 Mover(Vector2 posicion)
        {
            return posicion + ObtenerDesplazamiento();
        }
    }
}
