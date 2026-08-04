using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace La35Tunning.Escenas
{
    public class MenuPrincipal
    {
        private MouseState _mouseAnterior;
        private Texture2D _fondoMenu;
        
        // Texturas para los botones
        private Texture2D _texturaTaller;
        private Texture2D _texturaConcesionario;
        private Texture2D _texturaCorrer;

        // Rectángulos de posición y tamaño en pantalla para cada botón
        private Rectangle _rectBotonTaller = new Rectangle(280, 200, 240, 60);
        private Rectangle _rectBotonConcesionario = new Rectangle(280, 280, 240, 60);
        private Rectangle _rectBotonCorrer = new Rectangle(280, 360, 240, 60);

        public EstadoJuego? SiguienteEstado { get; private set; }

        public MenuPrincipal(ContentManager content)
        {
            // El menú se encarga de cargar sus propias imágenes
            _fondoMenu = content.Load<Texture2D>("FondoMenu");
            _texturaTaller = content.Load<Texture2D>("Taller");
            _texturaConcesionario = content.Load<Texture2D>("Concesionario");
            _texturaCorrer = content.Load<Texture2D>("Correr");
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();
            bool hizoClic = (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released);

            SiguienteEstado = null;

            if (hizoClic)
            {
                Point posicionMouse = mouseActual.Position;

                if (_rectBotonTaller.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Taller;
                }
                else if (_rectBotonConcesionario.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Concesionario;
                }
                // Si agregas lógica para Correr, lo manejás acá
            }

            _mouseAnterior = mouseActual;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            // 1. Dibujar fondo estirado a toda la pantalla
            if (_fondoMenu != null)
            {
                spriteBatch.Draw(_fondoMenu, new Rectangle(0, 0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
            }

            // 2. Dibujar las imágenes de los botones
            if (_texturaTaller != null)
                spriteBatch.Draw(_texturaTaller, _rectBotonTaller, Color.White);

            if (_texturaConcesionario != null)
                spriteBatch.Draw(_texturaConcesionario, _rectBotonConcesionario, Color.White);

            if (_texturaCorrer != null)
                spriteBatch.Draw(_texturaCorrer, _rectBotonCorrer, Color.White);
        }
    }
}