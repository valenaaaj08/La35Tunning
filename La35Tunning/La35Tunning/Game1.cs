using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Sistemas;
using La35Tunning.Entidades;
using La35Tunning.Pantallas;

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

        private Concesionario _concesionario;
        private Jugador _jugadorPrincipal;
        private Auto _autoGol;
        private Auto _autoUno;
        private Auto _autoClio;
        private Auto _autoCorsa;
        private Texture2D _texturaLlantaDefault;

        // --- Pantalla de carrera (prototipo local, Etapa 2) ---
        private Semaforo _semaforo;
        private PantallaCarrera _pantallaCarrera;


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

            // 1. Cargamos las texturas exactamente con el mismo nombre (y mayúsculas/minúsculas)
            // que tienen los archivos dentro de Content/, tal como están registrados en Content.mgcb
            _texturaUno = Content.Load<Texture2D>("Uno");
            _texturaGol = Content.Load<Texture2D>("gol");
            _texturaClio = Content.Load<Texture2D>("clio");
            _texturaCorsa = Content.Load<Texture2D>("corsa");
            _texturaLlantaDefault = Content.Load<Texture2D>("llantaDefault");

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
            _concesionario = new Concesionario();
            _concesionario.AgregarAuto(_autoGol);
            _concesionario.AgregarAuto(_autoUno);
            _concesionario.AgregarAuto(_autoClio);
            _concesionario.AgregarAuto(_autoCorsa);

            _jugadorPrincipal = new Jugador("Valentin", 5000000);

            // Compra automática inicial para probar rápido
            _concesionario.VenderAuto(_jugadorPrincipal, 0);

            // 5. Armamos el semáforo con sus 6 estados posibles.
            _semaforo = new Semaforo(
                Content.Load<Texture2D>("semaforo1"),
                Content.Load<Texture2D>("semaforo2"),
                Content.Load<Texture2D>("semaforo3"),
                Content.Load<Texture2D>("semaforo4"),
                Content.Load<Texture2D>("semaforo5"),
                Content.Load<Texture2D>("semaforoFallida"));

            // 6. Armamos la pantalla de carrera: el jugador contra _autoUno
            // como rival PROVISORIO (todavía no tenemos matchmaking en red
            // ni la IA Fantasma, así que usamos un auto fijo del catálogo
            // para poder probar la carrera ahora mismo).
            _pantallaCarrera = new PantallaCarrera(
                autoJugador: _jugadorPrincipal.AutoActual,
                autoRival: _autoUno,
                semaforo: _semaforo,
                carrilJugadorY: 150f,
                carrilRivalY: 450f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var teclado = Keyboard.GetState();

            // Antes de tener auto, dejamos las teclas 1/2 para comprar rápido
            // (esto es de prueba, en la Etapa 5 esto va a ser un menú real).
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

            // Con auto y pantalla de carrera armada, dejamos que sea
            // PantallaCarrera quien decida cómo se mueve el auto del
            // jugador (respeta el semáforo, detecta salida en falso, etc.)
            // en vez de moverlo libremente como antes.
            if (_pantallaCarrera != null)
            {
                _pantallaCarrera.Update(gameTime);
                _camara.Update(_jugadorPrincipal.AutoActual.Posicion);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // --- 1. Mundo del juego: se dibuja CON la transformación de
            // cámara, así que se mueve/hace zoom siguiendo al auto. ---
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camara.Transform);

            if (_pantallaCarrera != null)
            {
                _pantallaCarrera.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            // --- 2. HUD: se dibuja SIN transformación de cámara, así que
            // queda fijo en la pantalla (el semáforo no se tiene que mover
            // ni escalar cuando la cámara sigue al auto). ---
            if (_pantallaCarrera != null)
            {
                _spriteBatch.Begin();
                _pantallaCarrera.DibujarHud(_spriteBatch);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
