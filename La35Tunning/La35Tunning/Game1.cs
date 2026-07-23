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
        private Vector2 _posicionAuto;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _posicionAuto = new Vector2(200, 200);


            // Configuración para pantalla completa
            _graphics.IsFullScreen = true;

            //aca puse un ajuste automatico a la resolucion del monitor
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _graphics.ApplyChanges();



            base.Initialize();

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //aca les asigno una textura a cada variable, asi luego al crear el auto se la agrego
            _texturaUno = Content.Load<Texture2D>("Uno");
            _texturaGol = Content.Load<Texture2D>("Gol");
            _texturaClio = Content.Load<Texture2D>("Clio");
            _texturaCorsa = Content.Load<Texture2D>("Corsa");

            //aca creo los autos y les paso la textura correspondiente
            _autoGol = new Auto("Volkswagen Gol G3", 8f, 0.15f, 4500000, _texturaGol);
            _autoUno = new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, _texturaUno); // Un poquito más ágil de abajo
            _autoClio = new Auto("Renault Clio", 8.5f, 0.16f, 5200000, _texturaClio);
            _autoCorsa = new Auto("Chevrolet Corsa", 8f, 0.15f, 4200000, _texturaCorsa);

            // 2. Inicializamos la tienda y le cargamos los autos disponibles
            _concesionario = new Tienda();
            _concesionario.AgregarAuto(_autoGol);
            _concesionario.AgregarAuto(_autoUno);
            _concesionario.AgregarAuto(_autoClio);
            _concesionario.AgregarAuto(_autoCorsa);

            _jugadorPrincipal = new Jugador("Valentin", 5000000);

            _concesionario.VenderAuto(_jugadorPrincipal, 0);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
