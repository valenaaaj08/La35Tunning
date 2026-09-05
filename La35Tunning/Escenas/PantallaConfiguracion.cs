using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace La35Tunning.Escenas
{
    public class PantallaConfiguracion
    {
        private readonly Texture2D _texturaPixel;
        private readonly GraphicsDevice _graphicsDevice;
        private MouseState _mouseAnterior;
        private KeyboardState _tecladoAnterior;
        private bool _entradaInicializada;
        private int _volumen = 70;

        public bool DebeVolver { get; private set; }

        public PantallaConfiguracion(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _texturaPixel = new Texture2D(graphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });
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
                Vector2 desplazamiento = ObtenerDesplazamiento();
                Point posicion = mouseActual.Position - new Point((int)desplazamiento.X, (int)desplazamiento.Y);
                if (new Rectangle(250, 220, 80, 50).Contains(posicion)) CambiarVolumen(-10);
                if (new Rectangle(550, 220, 80, 50).Contains(posicion)) CambiarVolumen(10);
                if (new Rectangle(300, 350, 300, 55).Contains(posicion)) DebeVolver = true;
            }

            if (tecladoActual.IsKeyDown(Keys.Left) && _tecladoAnterior.IsKeyUp(Keys.Left)) CambiarVolumen(-10);
            if (tecladoActual.IsKeyDown(Keys.Right) && _tecladoAnterior.IsKeyUp(Keys.Right)) CambiarVolumen(10);
            if (tecladoActual.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape)) DebeVolver = true;

            _mouseAnterior = mouseActual;
            _tecladoAnterior = tecladoActual;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            spriteBatch.DrawString(fuente, "CONFIGURACION", Mover(new Vector2(300, 70)), Color.Gold);
            spriteBatch.DrawString(fuente, "VOLUMEN", Mover(new Vector2(300, 150)), Color.White);
            DibujarBoton(spriteBatch, Mover(new Rectangle(250, 220, 80, 50)), "<", fuente);
            DibujarBoton(spriteBatch, Mover(new Rectangle(550, 220, 80, 50)), ">", fuente);
            spriteBatch.DrawString(fuente, $"{_volumen}%", Mover(new Vector2(390, 235)), Color.White);
            DibujarBoton(spriteBatch, Mover(new Rectangle(300, 350, 300, 55)), "VOLVER", fuente);
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

        private Vector2 ObtenerDesplazamiento()
        {
            return Vector2.Zero; // Sin desplazamiento en fullscreen
        }

        private Rectangle Mover(Rectangle rectangulo)
        {
            Vector2 desplazamiento = ObtenerDesplazamiento();
            return new Rectangle(rectangulo.X + (int)desplazamiento.X, rectangulo.Y, rectangulo.Width, rectangulo.Height);
        }

        private Vector2 Mover(Vector2 posicion)
        {
            return posicion + ObtenerDesplazamiento();
        }

    }
}