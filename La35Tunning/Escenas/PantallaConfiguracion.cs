using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace La35Tunning.Escenas
{
    public class PantallaConfiguracion
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly Texture2D _texturaPixel;
        private MouseState _mouseAnterior;
        private KeyboardState _tecladoAnterior;
        private bool _entradaInicializada;
        private readonly Point[] _resoluciones;
        private int _resolucionSeleccionada;
        private int _volumen = 70;

        public bool DebeVolver { get; private set; }

        public PantallaConfiguracion(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
            _texturaPixel = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });
            _graphics.IsFullScreen = true;
            Point resolucionActual = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            _resoluciones = new[]
            {
                resolucionActual,
                new Point(800, 600),
                new Point(1280, 720),
                new Point(1920, 1080)
            };
        }


        // Se llama cada vez que se entra a esta pantalla, para dejarla
        // lista de nuevo (el mismo patrón que Semaforo.Reiniciar()).
        public void Reiniciar()
        {
            DebeVolver = false;
            _entradaInicializada = false;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();
            KeyboardState tecladoActual = Keyboard.GetState();

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
                Point posicion = mouseActual.Position;
                if (new Rectangle(250, 170, 80, 50).Contains(posicion)) CambiarResolucion(-1);
                if (new Rectangle(550, 170, 80, 50).Contains(posicion)) CambiarResolucion(1);
                if (new Rectangle(250, 300, 80, 50).Contains(posicion)) CambiarVolumen(-10);
                if (new Rectangle(550, 300, 80, 50).Contains(posicion)) CambiarVolumen(10);
                if (new Rectangle(300, 430, 300, 55).Contains(posicion)) DebeVolver = true;
            }

            if (tecladoActual.IsKeyDown(Keys.Left) && _tecladoAnterior.IsKeyUp(Keys.Left)) CambiarVolumen(-10);
            if (tecladoActual.IsKeyDown(Keys.Right) && _tecladoAnterior.IsKeyUp(Keys.Right)) CambiarVolumen(10);
            if (tecladoActual.IsKeyDown(Keys.Up) && _tecladoAnterior.IsKeyUp(Keys.Up)) CambiarResolucion(1);
            if (tecladoActual.IsKeyDown(Keys.Down) && _tecladoAnterior.IsKeyUp(Keys.Down)) CambiarResolucion(-1);
            if (tecladoActual.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape)) DebeVolver = true;

            _mouseAnterior = mouseActual;
            _tecladoAnterior = tecladoActual;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            spriteBatch.DrawString(fuente, "CONFIGURACION", new Vector2(300, 70), Color.Gold);
            spriteBatch.DrawString(fuente, "RESOLUCION", new Vector2(300, 135), Color.White);
            DibujarBoton(spriteBatch, new Rectangle(250, 170, 80, 50), "<", fuente);
            DibujarBoton(spriteBatch, new Rectangle(550, 170, 80, 50), ">", fuente);
            spriteBatch.DrawString(fuente, $"{_resoluciones[_resolucionSeleccionada].X} x {_resoluciones[_resolucionSeleccionada].Y}", new Vector2(350, 180), Color.White);
            spriteBatch.DrawString(fuente, "VOLUMEN", new Vector2(300, 265), Color.White);
            DibujarBoton(spriteBatch, new Rectangle(250, 300, 80, 50), "<", fuente);
            DibujarBoton(spriteBatch, new Rectangle(550, 300, 80, 50), ">", fuente);
            spriteBatch.DrawString(fuente, $"{_volumen}%", new Vector2(390, 315), Color.White);
            spriteBatch.DrawString(fuente, "PANTALLA COMPLETA", new Vector2(300, 365), Color.White);
            spriteBatch.DrawString(fuente, "ACTIVADA", new Vector2(390, 395), Color.Cyan);
            spriteBatch.DrawString(fuente, "VOLVER", new Vector2(390, 445), Color.Gold);
        }

        private void CambiarResolucion(int direccion)
        {
            _resolucionSeleccionada = MathHelper.Clamp(_resolucionSeleccionada + direccion, 0, _resoluciones.Length - 1);
            Point resolucion = _resoluciones[_resolucionSeleccionada];
            _graphics.PreferredBackBufferWidth = resolucion.X;
            _graphics.PreferredBackBufferHeight = resolucion.Y;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = true;
            _graphics.ApplyChanges();
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

    }
}