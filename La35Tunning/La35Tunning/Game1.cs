using System;
using La35Tunning.Entidades;
using La35Tunning.Escenas;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;



namespace La35Tunning
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private EstadoJuego _estadoActual = EstadoJuego.MenuPrincipal;

        private Jugador _jugador;
        private MenuPrincipal _menuPrincipal;
        private PantallaTaller _pantallaTaller;

        private SpriteFont _fuente;

        private PantallaCarrera _pantallaCarrera;
        private Sistemas.Camera2D _camara;

        private Texture2D _texturaPixel;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        // Se ejecuta UNA sola vez al arrancar, antes de cargar cualquier imagen/sonido.
        // Acá va solo lógica y datos que no dependan de Content (texturas, fuentes, etc).
        protected override void Initialize()
        {
            _jugador = new Jugador("Valentin", 5000000m);
            base.Initialize();
        }

        // Se ejecuta UNA sola vez, justo después de Initialize().
        // Acá SÍ está garantizado que la GraphicsDevice (grafica) está lista, por eso todo lo
        // que use Content.Load<>() (texturas, fuentes, sonidos) va en este método.
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            try
            {
                // Intentamos cargar la fuente principal
                _fuente = Content.Load<SpriteFont>("FuentePrincipal");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando fuente: " + ex.Message);
            }

            try
            {
                // Inicializamos el menú y la pantalla del taller
                _menuPrincipal = new MenuPrincipal(Content);
                Texture2D texturaUnoTemp = Content.Load<Texture2D>("Uno");
                _jugador.AsignarAuto(new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, texturaUnoTemp));
                _pantallaTaller = new PantallaTaller(Content, _jugador);

                Texture2D texturaRival = Content.Load<Texture2D>("gol");
                Auto autoRival = new Auto("Volkswagen Gol G3", 8f, 0.15f, 4500000, texturaRival);
                _texturaPixel = new Texture2D(GraphicsDevice, 1, 1);
                _texturaPixel.SetData(new[] { Color.White });

                _pantallaCarrera = new PantallaCarrera(_jugador.AutoActual, autoRival, new Sistemas.Semaforo(
                    Content.Load<Texture2D>("semaforo1"), Content.Load<Texture2D>("semaforo2"),
                    Content.Load<Texture2D>("semaforo3"), Content.Load<Texture2D>("semaforo4"),
                    Content.Load<Texture2D>("semaforo5"), Content.Load<Texture2D>("semaforoFallida")),
                    200f, 400f, _texturaPixel, _jugador);

                _camara = new Sistemas.Camera2D(GraphicsDevice);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error inicializando pantallas: " + ex.Message);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            switch (_estadoActual)
            {
                case EstadoJuego.MenuPrincipal:
                    if (_menuPrincipal != null)
                    {
                        _menuPrincipal.Update(gameTime);
                        if (_menuPrincipal.SiguienteEstado.HasValue)
                        {
                            _estadoActual = _menuPrincipal.SiguienteEstado.Value;
                        }
                    }
                    break;

                case EstadoJuego.Taller:
                    try
                    {
                        if (_pantallaTaller != null)
                        {
                            _pantallaTaller.Update(gameTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error en Update de Taller: " + ex.Message);
                    }

                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        _estadoActual = EstadoJuego.MenuPrincipal;
                    }
                    break;

                case EstadoJuego.Concesionario:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        _estadoActual = EstadoJuego.MenuPrincipal;
                    }
                    break;

                case EstadoJuego.Carrera:
                    _pantallaCarrera?.Update(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                        _estadoActual = EstadoJuego.MenuPrincipal;
                    break;

            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

           

            if (_fuente != null)
            {
                switch (_estadoActual)
                {
                    case EstadoJuego.MenuPrincipal:
                        _spriteBatch.Begin();
                        if (_menuPrincipal != null)
                        {
                            _menuPrincipal.Draw(_spriteBatch, _fuente, GraphicsDevice);
                        }
                        break;

                    case EstadoJuego.Taller:
                        _spriteBatch.Begin();
                        if (_pantallaTaller != null)
                        {
                            try
                            {
                                _pantallaTaller.Draw(_spriteBatch, _fuente);
                            }
                            catch (Exception ex)
                            {
                                _spriteBatch.DrawString(_fuente, "Error en Pantalla Taller: " + ex.Message, new Vector2(50, 50), Color.Red);
                            }
                        }
                        else
                        {
                            _spriteBatch.DrawString(_fuente, "Pantalla Taller no inicializada", new Vector2(200, 200), Color.Yellow);
                        }
                        break;

                    case EstadoJuego.Concesionario:
                        _spriteBatch.Begin();
                        _spriteBatch.DrawString(_fuente, "Pantalla Concesionario (En desarrollo)", new Vector2(200, 200), Color.White);
                        _spriteBatch.DrawString(_fuente, "Presiona [ ESC ] para volver al menu", new Vector2(200, 250), Color.Gray);
                        break;

                    case EstadoJuego.Carrera:
                        if (_pantallaCarrera != null && _camara != null)
                        {
                            _camara.Update(_jugador.AutoActual.Posicion);

                            _spriteBatch.Begin(transformMatrix: _camara.Transform);
                            _pantallaCarrera.Draw(_spriteBatch);
                            _spriteBatch.End();

                            _spriteBatch.Begin();
                            _pantallaCarrera.DibujarHud(_spriteBatch, _fuente);
                        }
                        break;
                }
            }
            else
            {
                GraphicsDevice.Clear(Color.DarkRed);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}