using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Sistemas;
using La35Tunning.Entidades;

namespace La35Tunning

{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Camera2D _camara;
        private SpriteBatch _spriteBatch;
        private Texture2D _texturaGol;
        private Texture2D _texturaUno;
        private Texture2D _texturaClio;
        private Texture2D _texturaCorsa;

        private Tienda _concesionario;
        private Jugador _jugadorPrincipal;
        private Auto _autoGol;
        private Auto _autoUno;
        private Auto _autoClio;
        private Auto _autoCorsa;
        private Texture2D _texturaLlantaDefault;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here



            // Configuración para pantalla completa
            _graphics.IsFullScreen = true;
            _camara = new Camera2D(GraphicsDevice);

            //aca puse un ajuste automatico a la resolucion del monitor
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _graphics.ApplyChanges();



            base.Initialize();

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 1. Cargamos las texturas exactamente con las mayúsculas como están en tu carpeta Content
            _texturaUno = Content.Load<Texture2D>("Uno");
            _texturaGol = Content.Load<Texture2D>("Gol");
            _texturaClio = Content.Load<Texture2D>("Clio");
            _texturaCorsa = Content.Load<Texture2D>("Corsa");
            _texturaLlantaDefault = Content.Load<Texture2D>("LlantaDefault");

            // 2. Creamos los autos con sus datos base
            _autoGol = new Auto("Volkswagen Gol G3", 8f, 0.15f, 4500000, _texturaGol);
            _autoUno = new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, _texturaUno);
            _autoClio = new Auto("Renault Clio", 8.5f, 0.16f, 5200000, _texturaClio);
            _autoCorsa = new Auto("Chevrolet Corsa", 8f, 0.15f, 4200000, _texturaCorsa);

            // 3. Les instalamos la llanta por defecto a todos
            _autoGol.InstalarLlantas(_texturaLlantaDefault, _texturaLlantaDefault);
            _autoUno.InstalarLlantas(_texturaLlantaDefault, _texturaLlantaDefault);
            _autoClio.InstalarLlantas(_texturaLlantaDefault, _texturaLlantaDefault);
            _autoCorsa.InstalarLlantas(_texturaLlantaDefault, _texturaLlantaDefault);

            // 4. Inicializamos la tienda y cargamos los vehículos
            _concesionario = new Tienda();
            _concesionario.AgregarAuto(_autoGol);
            _concesionario.AgregarAuto(_autoUno);
            _concesionario.AgregarAuto(_autoClio);
            _concesionario.AgregarAuto(_autoCorsa);

            _jugadorPrincipal = new Jugador("Valentin", 5000000);

            // Compra automática inicial para probar rápido
            _concesionario.VenderAuto(_jugadorPrincipal, 0);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var teclado = Keyboard.GetState();

            if (_jugadorPrincipal.AutoActual == null)
            {
                if (teclado.IsKeyDown(Keys.D1))
                {
                    _concesionario.VenderAuto(_jugadorPrincipal, 0);
                }
                else if (teclado.IsKeyDown(Keys.D2))
                {
                    _concesionario.VenderAuto(_jugadorPrincipal, 1);
                }
            }

            // Si ya tiene auto, actualizamos su física y movemos la cámara una sola vez
            if (_jugadorPrincipal.AutoActual != null)
            {
                _jugadorPrincipal.AutoActual.Update(gameTime);
                _camara.Update(_jugadorPrincipal.AutoActual.Posicion);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camara.Transform);
            

            
            
            if (_jugadorPrincipal.AutoActual != null)
            {
                _jugadorPrincipal.AutoActual.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
