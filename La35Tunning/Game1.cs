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
        private PantallaRed _pantallaRed;               // NUEVO
        private Texture2D _texturaLlantaDefault;        // NUEVO: para armar el auto del rival en red

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
            int anchoMonitor = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int altoMonitor = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.PreferredBackBufferWidth = anchoMonitor;
            _graphics.PreferredBackBufferHeight = altoMonitor;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // CAMBIO: antes siempre "Valentin". Ahora un nombre provisorio (ej: "Piloto47")
            // que el jugador puede cambiar en la pantalla JUGAR EN RED.
            _jugador = new Jugador("Piloto" + new Random().Next(10, 100), 7000000m);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _graphics.HardwareModeSwitch = false;
            _graphics.ApplyChanges();

            try
            {
                _fuente = Content.Load<SpriteFont>("FuentePrincipal");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando fuente: " + ex.Message);
            }

            // NUEVO: fuentes en negrita para títulos y botones. Si alguna no carga,
            // UI.Inicializar usa la principal en su lugar y el juego sigue andando.
            SpriteFont fuenteNegrita = CargarFuenteOpcional("FuenteNegrita");
            SpriteFont fuenteTitulo = CargarFuenteOpcional("FuenteTitulo");
            UI.Inicializar(GraphicsDevice, _fuente, fuenteNegrita, fuenteTitulo);

            try
            {
                CrearPantallaMenuYMusica();
                CrearPantallasDelJuego();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error inicializando pantallas: " + ex.Message);
            }
        }

        private SpriteFont CargarFuenteOpcional(string nombre)
        {
            try
            {
                return Content.Load<SpriteFont>(nombre);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo cargar {nombre}: {ex.Message}");
                return null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            ActualizarTiempoDeErrores(gameTime);
            ActualizarEstadoActual(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (_fuente == null)
            {
                GraphicsDevice.Clear(Color.DarkRed);
                base.Draw(gameTime);
                return;
            }

            switch (_estadoActual)
            {
                case EstadoJuego.MenuPrincipal:
                    _spriteBatch.Begin();
                    DibujarMenu();
                    _spriteBatch.End();
                    break;

                case EstadoJuego.Taller:
                    _spriteBatch.Begin();
                    DibujarTaller();
                    _spriteBatch.End();
                    break;

                case EstadoJuego.Concesionario:
                    _spriteBatch.Begin();
                    DibujarConcesionario();
                    _spriteBatch.End();
                    break;

                case EstadoJuego.Carrera:
                    DibujarCarrera();
                    break;

                case EstadoJuego.Red:
                    _spriteBatch.Begin();
                    _pantallaRed.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case EstadoJuego.Configuracion:
                    _spriteBatch.Begin();
                    _menuPrincipal?.Draw(_spriteBatch, _fuente, GraphicsDevice);   // NUEVO: el menú queda de fondo
                    DibujarConfiguracion();
                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        private void CrearPantallaMenuYMusica()
        {
            _menuPrincipal = new MenuPrincipal(Content, GraphicsDevice);
            _menuPrincipal.JugadorActual = _jugador;
            _musicaMenu = Content.Load<Song>("Sonidos/Fiat 600 - Tussiwarriors");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0f;
            MediaPlayer.Play(_musicaMenu);
        }

        private void CrearPantallasDelJuego()
        {
            _pantallaTaller = new PantallaTaller(Content, _jugador);
            _pantallaConfiguracion = new PantallaConfiguracion(GraphicsDevice);

            Texture2D texturaLlantaDefault = Content.Load<Texture2D>("llantaDefault");
            _texturaLlantaDefault = texturaLlantaDefault;
            _pantallaRed = new PantallaRed(_jugador);
            _concesionarioSistema = new Sistemas.Concesionario(Content, texturaLlantaDefault);
            _pantallaConcesionario = new PantallaConcesionario(_concesionarioSistema, _jugador, GraphicsDevice);

            Auto autoRival = AutoFactory.CrearGol(Content, texturaLlantaDefault);
            _texturaPixel = new Texture2D(GraphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });

            _pantallaCarrera = new PantallaCarrera(
                null,
                autoRival,
                new Sistemas.Semaforo(
                    Content.Load<Texture2D>("semaforo1"),
                    Content.Load<Texture2D>("semaforo2"),
                    Content.Load<Texture2D>("semaforo3"),
                    Content.Load<Texture2D>("semaforo4"),
                    Content.Load<Texture2D>("semaforo5"),
                    Content.Load<Texture2D>("semaforoFallida")),
                590f,   // carril del jugador (línea del piso, adelante)
                440f,   // carril del rival (línea del piso, atrás)
                _texturaPixel,
                Content.Load<Texture2D>("Velocimetro"),
                Content.Load<Texture2D>("Aguja_Velocimetro"),
                _jugador);

            _camara = new Sistemas.Camera2D(GraphicsDevice);
        }

        private void ActualizarTiempoDeErrores(GameTime gameTime)
        {
            if (_tiempoMensajeError > 0f)
            {
                _tiempoMensajeError -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private void ActualizarEstadoActual(GameTime gameTime)
        {
            switch (_estadoActual)
            {
                case EstadoJuego.MenuPrincipal:
                    ActualizarMenuPrincipal(gameTime);
                    break;

                case EstadoJuego.Taller:
                    ActualizarTaller(gameTime);
                    break;

                case EstadoJuego.Concesionario:
                    ActualizarConcesionario(gameTime);
                    break;

                case EstadoJuego.Carrera:
                    ActualizarCarrera(gameTime);
                    break;

                case EstadoJuego.Configuracion:
                    ActualizarConfiguracion(gameTime);
                    break;

                case EstadoJuego.Red:
                    ActualizarRed(gameTime);
                    break;
            }
        }

        private void ActualizarMenuPrincipal(GameTime gameTime)
        {
            if (_menuPrincipal == null)
                return;

            _menuPrincipal.Update(gameTime);

            if (_menuPrincipal.QuiereSalir)
            {
                Exit();
                return;
            }

            if (_menuPrincipal.SiguienteEstado.HasValue)
            {
                CambiarEstado(_menuPrincipal.SiguienteEstado.Value);
            }
        }

        private void ActualizarTaller(GameTime gameTime)
        {
            try
            {
                _pantallaTaller?.Update(gameTime);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en Update de Taller: " + ex.Message);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || (_pantallaTaller != null && _pantallaTaller.DebeVolver))
            {
                CambiarEstado(EstadoJuego.MenuPrincipal);
            }
        }

        private void ActualizarConcesionario(GameTime gameTime)
        {
            _pantallaConcesionario?.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || (_pantallaConcesionario != null && _pantallaConcesionario.DebeVolver))
            {
                CambiarEstado(EstadoJuego.MenuPrincipal);
            }
        }

        private void ActualizarCarrera(GameTime gameTime)
        {
            _pantallaCarrera?.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                CambiarEstado(EstadoJuego.MenuPrincipal);
            }
        }

        /// <summary>NUEVO: pantalla de conexión. Cuando el servidor dice INICIO, arranca la carrera en red.</summary>
        private void ActualizarRed(GameTime gameTime)
        {
            _pantallaRed.Update(gameTime);

            if (_pantallaRed.DebeVolver)
            {
                CambiarEstado(EstadoJuego.MenuPrincipal);
                return;
            }

            if (_pantallaRed.CarreraLista)
            {
                Auto autoRival = AutoFactory.CrearAuto(_pantallaRed.ModeloRival, Content, _texturaLlantaDefault);

                _pantallaCarrera.CambiarAutoJugador(_jugador.AutoActual);
                _pantallaCarrera.IniciarCarreraEnRed(_pantallaRed.TomarCliente(), autoRival,
                    _pantallaRed.NombreRival, _pantallaRed.EsperaVerde);

                // No usamos CambiarEstado(Carrera) porque reiniciaría la carrera como si fuera contra la máquina.
                _estadoActual = EstadoJuego.Carrera;
            }
        }

        private void ActualizarConfiguracion(GameTime gameTime)
        {
            _pantallaConfiguracion?.Update(gameTime);

            if (_pantallaConfiguracion != null && _pantallaConfiguracion.DebeVolver)
            {
                CambiarEstado(EstadoJuego.MenuPrincipal);
            }
        }

        private void DibujarMenu()
        {
            if (_menuPrincipal != null)
            {
                _menuPrincipal.Draw(_spriteBatch, _fuente, GraphicsDevice);
            }

            if (_tiempoMensajeError > 0f)
            {
                // NUEVO: cartel centrado con estilo (antes: franja negra con texto rojo arriba a la izquierda).
                float opacidad = MathHelper.Clamp(_tiempoMensajeError / 0.4f, 0f, 1f);
                Vector2 medida = UI.Negrita.MeasureString(_mensajeError);
                int ancho = (int)medida.X + 60;
                Rectangle caja = new Rectangle(GraphicsDevice.Viewport.Width / 2 - ancho / 2, 110, ancho, 56);
                UI.Rect(_spriteBatch, caja, Color.Black * 0.85f * opacidad);
                UI.Rect(_spriteBatch, new Rectangle(caja.X, caja.Y, 5, caja.Height), UI.Rojo * opacidad);
                UI.Borde(_spriteBatch, caja, 1, UI.Rojo * 0.7f * opacidad);
                UI.TextoCentrado(_spriteBatch, UI.Negrita, _mensajeError, new Vector2(caja.Center.X, caja.Center.Y + 1), UI.TextoClaro * opacidad);
            }
        }

        private void DibujarTaller()
        {
            if (_pantallaTaller == null)
            {
                _spriteBatch.DrawString(_fuente, "Pantalla Taller no inicializada", new Vector2(200, 200), Color.Yellow);
                return;
            }

            try
            {
                _pantallaTaller.Draw(_spriteBatch, _fuente);
            }
            catch (Exception ex)
            {
                _spriteBatch.DrawString(_fuente, "Error en Pantalla Taller: " + ex.Message, new Vector2(50, 50), Color.Red);
            }
        }

        private void DibujarConcesionario()
        {
            _pantallaConcesionario?.Draw(_spriteBatch, _fuente, GraphicsDevice);
        }

        private void DibujarCarrera()
        {
            if (_pantallaCarrera == null)
                return;

            if (_camara != null && _jugador.AutoActual != null)
            {
                // NUEVO: el zoom depende del alto de la pantalla, así la pista se ve igual
                // en cualquier monitor (antes era 0.5 fijo y los autos quedaban chiquitos).
                _camara.Zoom = GraphicsDevice.Viewport.Height / PantallaCarrera.AltoVisibleMundo;
                float anchoVisibleMundo = GraphicsDevice.Viewport.Width / _camara.Zoom;
                _camara.Update(_pantallaCarrera.ObtenerCentroCamara(anchoVisibleMundo));

                // 1) Fondo (cielo y edificios con parallax) en coordenadas de pantalla
                _spriteBatch.Begin();
                _pantallaCarrera.DibujarFondo(_spriteBatch, _camara);
                _spriteBatch.End();

                // 2) Pista y autos con la cámara
                _spriteBatch.Begin(transformMatrix: _camara.Transform);
                _pantallaCarrera.Draw(_spriteBatch);
                _spriteBatch.End();

                _spriteBatch.Begin();
                _pantallaCarrera.DibujarHud(_spriteBatch, _fuente);
                _spriteBatch.End();
                return;
            }

            string mensajeError = "¡No se puede correr sin auto, wachin!";
            Vector2 tamanoTexto = _fuente.MeasureString(mensajeError);
            int ancho = GraphicsDevice.Viewport.Width;
            int alto = GraphicsDevice.Viewport.Height;
            Vector2 posicion = new Vector2((ancho - tamanoTexto.X) / 2, (alto - tamanoTexto.Y) / 2);

            // ANTES: _texturaPixel.SetData(new[] { Color.Black }); -> pintaba de negro la textura
            // que también usan otras pantallas. No hace falta: Color.Black * 0.5f ya la tiñe.
            _spriteBatch.Begin();
            _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, ancho, alto), Color.Black * 0.5f);
            _spriteBatch.DrawString(_fuente, mensajeError, posicion, Color.Red);
            _spriteBatch.DrawString(_fuente, "Presiona ESC para volver",
                new Vector2((ancho - _fuente.MeasureString("Presiona ESC para volver").X) / 2, posicion.Y + 80),
                Color.White);
            _spriteBatch.End();
        }

        private void DibujarConfiguracion()
        {
            _pantallaConfiguracion?.Draw(_spriteBatch, _fuente, GraphicsDevice);
        }

        private void MostrarErrorPantalla(string mensaje)
        {
            _mensajeError = mensaje;
            _tiempoMensajeError = 3f;
            System.Diagnostics.Debug.WriteLine(mensaje);
        }

        private void CambiarEstado(EstadoJuego nuevoEstado)
        {
            // NUEVO: al salir de una carrera en red se corta la conexión.
            if (_estadoActual == EstadoJuego.Carrera && nuevoEstado != EstadoJuego.Carrera)
            {
                _pantallaCarrera?.SalirDeRed();
            }

            if (nuevoEstado == EstadoJuego.Red)
            {
                if (_jugador.AutoActual == null)
                {
                    MostrarErrorPantalla("Para jugar en red primero equipá un auto.");
                    return;
                }
                _pantallaRed.Reiniciar();
            }

            if (nuevoEstado == EstadoJuego.Carrera)
            {
                if (_jugador.AutoActual == null)
                {
                    MostrarErrorPantalla("¡No se puede correr sin auto, wachin!");
                    return;
                }

                if (_pantallaCarrera != null)
                {
                    _pantallaCarrera.CambiarAutoJugador(_jugador.AutoActual);
                    _pantallaCarrera.IniciarNuevaCarrera();   // NUEVO: siempre arrancar con el semáforo desde cero
                }
            }

            // NUEVO: al entrar a una pantalla se "olvida" el clic que nos trajo hasta acá.
            if (nuevoEstado == EstadoJuego.MenuPrincipal)
            {
                _menuPrincipal?.Reiniciar();
            }

            if (nuevoEstado == EstadoJuego.Taller)
            {
                _pantallaTaller?.Reiniciar();
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

            if (nuevoEstado == EstadoJuego.Configuracion && _pantallaConfiguracion != null)
            {
                _pantallaConfiguracion.Reiniciar();
            }
        }

        private void ReiniciarPantallaSiHaceFalta()
        {
            if (_pantallaConfiguracion != null && _estadoActual == EstadoJuego.Configuracion)
            {
                _pantallaConfiguracion.Reiniciar();
            }
        }
    }
}