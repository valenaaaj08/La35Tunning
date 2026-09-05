using System;
using La35Tunning.Entidades;
using La35Tunning.Escenas;
using La35Tunning.Factories;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



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
        private PantallaConfiguracion _pantallaConfiguracion;
        private Sistemas.Concesionario _concesionarioSistema;
        private PantallaConcesionario _pantallaConcesionario;

        private SpriteFont _fuente;

        private PantallaCarrera _pantallaCarrera;
        private Sistemas.Camera2D _camara;

        private Texture2D _texturaPixel;
        private Song _musicaMenu;
        private string _mensajeError = "";
        private float _tiempoMensajeError = 0f;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            // Obtener la resolución actual del monitor
            int anchoMonitor = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int altoMonitor = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.PreferredBackBufferWidth = anchoMonitor;
            _graphics.PreferredBackBufferHeight = altoMonitor;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        // Se ejecuta UNA sola vez al arrancar, antes de cargar cualquier imagen/sonido.
        // Acá va solo lógica y datos que no dependan de Content (texturas, fuentes, etc).
        protected override void Initialize()
        {
            _jugador = new Jugador("Valentin", 7000000m);
            base.Initialize();
        }

        // Se ejecuta UNA sola vez, justo después de Initialize().
        // Acá SÍ está garantizado que la GraphicsDevice (grafica) está lista, por eso todo lo
        // que use Content.Load<>() (texturas, fuentes, sonidos) va en este método.
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _graphics.HardwareModeSwitch = false;
            _graphics.ApplyChanges();

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
                _menuPrincipal = new MenuPrincipal(Content, GraphicsDevice);
                _musicaMenu = Content.Load<Song>("Sonidos/Fiat 600 - Tussiwarriors");
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.7f;
                MediaPlayer.Play(_musicaMenu);
                
                // El jugador comienza sin auto - debe comprar uno en el concesionario
                _pantallaTaller = new PantallaTaller(Content, _jugador);
                _pantallaConfiguracion = new PantallaConfiguracion(GraphicsDevice);

                Texture2D texturaLlantaDefault = Content.Load<Texture2D>("llantaDefault");
                _concesionarioSistema = new Sistemas.Concesionario(Content, texturaLlantaDefault);
                _pantallaConcesionario = new PantallaConcesionario(_concesionarioSistema, _jugador, GraphicsDevice);

                Auto autoRival = AutoFactory.CrearGol(Content, texturaLlantaDefault);
                _texturaPixel = new Texture2D(GraphicsDevice, 1, 1);
                _texturaPixel.SetData(new[] { Color.White });

                _pantallaCarrera = new PantallaCarrera(null, autoRival, new Sistemas.Semaforo(
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
            // Actualizar tiempo del mensaje de error
            if (_tiempoMensajeError > 0f)
                _tiempoMensajeError -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (_estadoActual)
            {
                case EstadoJuego.MenuPrincipal:
                    if (_menuPrincipal != null)
                    {
                        _menuPrincipal.Update(gameTime);
                        if (_menuPrincipal.SiguienteEstado.HasValue)
                        {
                            CambiarEstado(_menuPrincipal.SiguienteEstado.Value);
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
                        CambiarEstado(EstadoJuego.MenuPrincipal);
                    }
                    break;

                case EstadoJuego.Concesionario:
                    _pantallaConcesionario?.Update(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        CambiarEstado(EstadoJuego.MenuPrincipal);
                    }
                    break;

                case EstadoJuego.Carrera:
                    _pantallaCarrera?.Update(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                        CambiarEstado(EstadoJuego.MenuPrincipal);
                    break;

                case EstadoJuego.Configuracion:
                    _pantallaConfiguracion?.Update(gameTime);
                    if (_pantallaConfiguracion != null && _pantallaConfiguracion.DebeVolver)
                        CambiarEstado(EstadoJuego.MenuPrincipal);
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
                        
                        // Mostrar mensaje de error temporal si existe
                        if (_tiempoMensajeError > 0f)
                        {
                            _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, 100), Color.Black * 0.7f);
                            _spriteBatch.DrawString(_fuente, _mensajeError, new Vector2(50, 20), Color.Red);
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
                        _pantallaConcesionario?.Draw(_spriteBatch, _fuente, GraphicsDevice);
                        break;

                    case EstadoJuego.Carrera:
                        if (_pantallaCarrera != null && _camara != null && _jugador.AutoActual != null)
                        {
                            _camara.Update(_jugador.AutoActual.Posicion);

                            _spriteBatch.Begin(transformMatrix: _camara.Transform);
                            _pantallaCarrera.Draw(_spriteBatch);
                            _spriteBatch.End();

                            _spriteBatch.Begin();
                            _pantallaCarrera.DibujarHud(_spriteBatch, _fuente);
                        }
                        else if (_pantallaCarrera != null && _jugador.AutoActual == null)
                        {
                            _spriteBatch.Begin();
                            // Mostrar mensaje de error si se intenta correr sin auto
                            string mensajeError = "¡No se puede correr sin auto, wachin!";
                            Vector2 tamañoTexto = _fuente.MeasureString(mensajeError);
                            int ancho = GraphicsDevice.Viewport.Width;
                            int alto = GraphicsDevice.Viewport.Height;
                            Vector2 posicion = new Vector2(
                                (ancho - tamañoTexto.X) / 2,
                                (alto - tamañoTexto.Y) / 2);
                            
                            // Fondo semi-transparente
                            _texturaPixel.SetData(new[] { Color.Black });
                            _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, ancho, alto), Color.Black * 0.5f);
                            
                            // Texto de error en rojo
                            _spriteBatch.DrawString(_fuente, mensajeError, posicion, Color.Red);
                            _spriteBatch.DrawString(_fuente, "Presiona ESC para volver", 
                                new Vector2((ancho - _fuente.MeasureString("Presiona ESC para volver").X) / 2, posicion.Y + 80), 
                                Color.White);
                        }
                        break;

                    case EstadoJuego.Configuracion:
                        _spriteBatch.Begin();
                        _pantallaConfiguracion?.Draw(_spriteBatch, _fuente, GraphicsDevice);
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

        private void CambiarEstado(EstadoJuego nuevoEstado)
        {
            // Validar que el jugador tenga un auto antes de entrar a carrera
            if (nuevoEstado == EstadoJuego.Carrera)
            {
                if (_jugador.AutoActual == null)
                {
                    // No cambiar estado, el jugador debe comprar un auto primero
                    _mensajeError = "¡No se puede correr sin auto, wachin!";
                    _tiempoMensajeError = 3f;
                    System.Diagnostics.Debug.WriteLine("Necesitas comprar un auto primero.");
                    return;
                }
                if (_pantallaCarrera != null)
                {
                    _pantallaCarrera.CambiarAutoJugador(_jugador.AutoActual);
                }
            }

            if (nuevoEstado == EstadoJuego.Configuracion)
            {
                _pantallaConfiguracion?.Reiniciar();
            }

            bool estadoActualTieneMusica = _estadoActual == EstadoJuego.MenuPrincipal || _estadoActual == EstadoJuego.Configuracion;
            bool nuevoEstadoTieneMusica = nuevoEstado == EstadoJuego.MenuPrincipal || nuevoEstado == EstadoJuego.Configuracion;

            if (estadoActualTieneMusica && !nuevoEstadoTieneMusica)
            {
                MediaPlayer.Stop();
            }
            else if (!estadoActualTieneMusica && nuevoEstadoTieneMusica && _musicaMenu != null)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(_musicaMenu);
            }

            if (nuevoEstado == EstadoJuego.Concesionario)
            {
                _pantallaConcesionario?.Reiniciar();
            }

            _estadoActual = nuevoEstado;
            
            // Reiniciar pantalla de configuración si volvemos a ella
            if (nuevoEstado == EstadoJuego.Configuracion && _pantallaConfiguracion != null)
            {
                _pantallaConfiguracion.Reiniciar();
            }
        }
    }
}