using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Modelos;
using La35Tunning.Escenas;
using System;

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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _jugador = new Jugador("Valentin", 5000000m);
            base.Initialize();
        }

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
                _pantallaTaller = new PantallaTaller(Content, _jugador);
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
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            if (_fuente != null)
            {
                switch (_estadoActual)
                {
                    case EstadoJuego.MenuPrincipal:
                        if (_menuPrincipal != null)
                        {
                            _menuPrincipal.Draw(_spriteBatch, _fuente, GraphicsDevice);
                        }
                        break;

                    case EstadoJuego.Taller:
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
                        _spriteBatch.DrawString(_fuente, "Pantalla Concesionario (En desarrollo)", new Vector2(200, 200), Color.White);
                        _spriteBatch.DrawString(_fuente, "Presiona [ ESC ] para volver al menu", new Vector2(200, 250), Color.Gray);
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