using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;

namespace La35Tunning.Escenas
{
    public class MenuPrincipal
    {
        private MouseState _mouseAnterior;
        private Texture2D _fondoMenu;

        private Texture2D _texturaTaller;
        private Texture2D _texturaConcesionario;
        private Texture2D _texturaCorrer;
        private Texture2D _texturaPixel;
        private Texture2D _texturaEngranaje;
        private bool _mouseSobreConfiguracion;
        private readonly GraphicsDevice _graphicsDevice;

        private Rectangle _rectBotonCorrer = new Rectangle(292, 380, 225, 105);
        private Rectangle _rectBotonConcesionario = new Rectangle(292, 500, 225, 105);
        private Rectangle _rectBotonTaller = new Rectangle(292, 620, 225, 105);
        private Rectangle _rectBotonConfiguracion;

        // ---- NUEVO: animaciones y datos para el diseño nuevo ----
        private Rectangle _rectBotonSalir;
        private Rectangle _rectBotonRed;       // NUEVO: JUGAR EN RED
        private float _hoverRed;
        private Point _posicionMouse;
        private bool _entradaInicializada;
        private float _tiempo;                 // tiempo total en el menú (para el "latido" del botón CORRER)
        private float _tiempoDesdeEntrada;     // para el fundido de entrada
        private float _rotacionEngranaje;
        private float _hoverTaller, _hoverCorrer, _hoverConcesionario, _hoverSalir;   // 0 = normal, 1 = mouse encima

        public EstadoJuego? SiguienteEstado { get; private set; }

        /// <summary>NUEVO: Game1 lo revisa y, si es true, cierra el juego (antes no había forma de salir).</summary>
        public bool QuiereSalir { get; private set; }

        /// <summary>NUEVO: para mostrar nombre, auto y plata en la barra de abajo.</summary>
        public Jugador JugadorActual { get; set; }

        public MenuPrincipal(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _fondoMenu = content.Load<Texture2D>("FondoMenu");
            _texturaTaller = content.Load<Texture2D>("Taller");
            _texturaConcesionario = content.Load<Texture2D>("Concesionario");
            _texturaCorrer = content.Load<Texture2D>("Correr");
            _texturaPixel = new Texture2D(graphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });

            // Antes 64 px: ahora 128 para que se vea nítido al achicarlo.
            _texturaEngranaje = CrearTexturaEngranaje(graphicsDevice, 128);

            int anchoPantalla = graphicsDevice.PresentationParameters.BackBufferWidth;
            _rectBotonConfiguracion = new Rectangle(anchoPantalla - 74, 10, 64, 64);
        }

        /// <summary>
        /// NUEVO: se llama cada vez que volvemos al menú. Evita que el clic que hicimos
        /// en otra pantalla (por ejemplo en "VOLVER") se tome como clic en un botón del menú.
        /// </summary>
        public void Reiniciar()
        {
            _entradaInicializada = false;
            _tiempoDesdeEntrada = 0f;
            SiguienteEstado = null;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _tiempo += dt;
            _tiempoDesdeEntrada += dt;

            ActualizarRectangulos();
            MouseState mouseActual = Mouse.GetState();
            _posicionMouse = mouseActual.Position;

            SiguienteEstado = null;

            if (!_entradaInicializada)
            {
                _mouseAnterior = mouseActual;
                _entradaInicializada = true;
                return;
            }

            bool hizoClic = (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released);

            _mouseSobreConfiguracion = _rectBotonConfiguracion.Contains(mouseActual.Position);

            // Animación suave de "hover": cada valor va de 0 a 1 en ~0.12 s.
            _hoverTaller = UI.Acercar(_hoverTaller, _rectBotonTaller.Contains(_posicionMouse) ? 1f : 0f, 8f, dt);
            _hoverCorrer = UI.Acercar(_hoverCorrer, _rectBotonCorrer.Contains(_posicionMouse) ? 1f : 0f, 8f, dt);
            _hoverConcesionario = UI.Acercar(_hoverConcesionario, _rectBotonConcesionario.Contains(_posicionMouse) ? 1f : 0f, 8f, dt);
            _hoverRed = UI.Acercar(_hoverRed, _rectBotonRed.Contains(_posicionMouse) ? 1f : 0f, 8f, dt);
            _hoverSalir = UI.Acercar(_hoverSalir, _rectBotonSalir.Contains(_posicionMouse) ? 1f : 0f, 8f, dt);

            if (_mouseSobreConfiguracion)
                _rotacionEngranaje += dt * 2.5f;   // el engranaje gira mientras el mouse está encima

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

                else if (_rectBotonCorrer.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Carrera;
                }
                else if (_rectBotonConfiguracion.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Configuracion;
                }
                else if (_rectBotonRed.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Red;
                }
                else if (_rectBotonSalir.Contains(posicionMouse))
                {
                    QuiereSalir = true;
                }
            }

            _mouseAnterior = mouseActual;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            ActualizarRectangulos();
            int ancho = graphicsDevice.Viewport.Width;
            int alto = graphicsDevice.Viewport.Height;

            // 1) Fondo SIN deformar (antes se estiraba a la pantalla).
            if (_fondoMenu != null)
            {
                UI.FondoCubriendo(spriteBatch, _fondoMenu, ancho, alto, Color.White);
            }

            // 2) Degradé oscuro abajo para que los botones resalten sobre la calle.
            UI.Degrade(spriteBatch, new Rectangle(0, (int)(alto * 0.42f), ancho, (int)(alto * 0.58f) + 1), Color.Black * 0.78f, true);

            // 3) Los tres botones (CORRER en el medio y más grande).
            float latido = 0.5f + 0.5f * (float)Math.Sin(_tiempo * 3f);
            DibujarBotonImagen(spriteBatch, _texturaTaller, _rectBotonTaller, _hoverTaller, 0f);
            DibujarBotonImagen(spriteBatch, _texturaConcesionario, _rectBotonConcesionario, _hoverConcesionario, 0f);
            DibujarBotonImagen(spriteBatch, _texturaCorrer, _rectBotonCorrer, _hoverCorrer, 0.12f + 0.12f * latido);

            // NUEVO: botón para jugar contra otro jugador en la red local
            UI.Boton(spriteBatch, _rectBotonRed, "JUGAR EN RED (LAN)", _hoverRed > 0.5f, true, new Color(30, 90, 150));

            // 4) Botones de arriba a la derecha: SALIR y configuración.
            UI.Boton(spriteBatch, _rectBotonSalir, "SALIR", _hoverSalir > 0.5f, true, new Color(70, 30, 36));
            DibujarEngranaje(spriteBatch);

            // 5) Barra inferior con los datos del jugador.
            DibujarBarraJugador(spriteBatch, ancho, alto);

            // 6) Fundido desde negro al entrar al menú.
            float fundido = 1f - MathHelper.Clamp(_tiempoDesdeEntrada / 0.35f, 0f, 1f);
            if (fundido > 0f)
            {
                UI.Rect(spriteBatch, new Rectangle(0, 0, ancho, alto), Color.Black * fundido);
            }
        }

        /// <summary>
        /// Dibuja una de las imágenes-botón: recortada a su parte visible, un poquito
        /// más grande y brillante cuando el mouse está encima, con un resplandor detrás.
        /// </summary>
        private void DibujarBotonImagen(SpriteBatch spriteBatch, Texture2D textura, Rectangle rect, float hover, float brilloExtra)
        {
            if (textura == null)
                return;

            float escala = 1f + 0.06f * hover;
            int anchoEscalado = (int)(rect.Width * escala);
            int altoEscalado = (int)(rect.Height * escala);
            Rectangle destino = new Rectangle(
                rect.Center.X - anchoEscalado / 2,
                rect.Center.Y - altoEscalado / 2 - (int)(6 * hover),
                anchoEscalado,
                altoEscalado);

            float intensidadBrillo = 0.45f * hover + brilloExtra;
            if (intensidadBrillo > 0f)
            {
                UI.Resplandor(spriteBatch, new Vector2(destino.Center.X, destino.Center.Y),
                    destino.Width * 0.72f, destino.Height * 0.95f, UI.Acento * intensidadBrillo);
            }

            // Sombra en el piso
            UI.Resplandor(spriteBatch, new Vector2(rect.Center.X, rect.Bottom + 8),
                rect.Width * 0.5f, 16f, Color.Black * 0.7f);

            Color tinte = Color.Lerp(new Color(215, 215, 220), Color.White, hover);
            spriteBatch.Draw(textura, destino, UI.LimitesOpacos(textura), tinte);
        }

        private void DibujarEngranaje(SpriteBatch spriteBatch)
        {
            Vector2 centro = new Vector2(_rectBotonConfiguracion.Center.X, _rectBotonConfiguracion.Center.Y);
            float radio = _rectBotonConfiguracion.Width / 2f;

            UI.CirculoLleno(spriteBatch, centro + new Vector2(0, 3), radio + 2, Color.Black * 0.5f);
            UI.CirculoLleno(spriteBatch, centro, radio + 2, _mouseSobreConfiguracion ? UI.Acento : UI.BordePanel);
            UI.CirculoLleno(spriteBatch, centro, radio, UI.ColorPanel);

            Color colorEngranaje = _mouseSobreConfiguracion ? Color.Gold : Color.White;
            float escala = (radio * 1.45f) / _texturaEngranaje.Width;
            Vector2 origen = new Vector2(_texturaEngranaje.Width / 2f, _texturaEngranaje.Height / 2f);
            spriteBatch.Draw(_texturaEngranaje, centro, null, colorEngranaje, _rotacionEngranaje, origen, escala, SpriteEffects.None, 0f);
        }

        private void DibujarBarraJugador(SpriteBatch spriteBatch, int ancho, int alto)
        {
            Rectangle barra = new Rectangle(0, alto - 68, ancho, 68);
            UI.Rect(spriteBatch, barra, Color.Black * 0.6f);
            UI.Rect(spriteBatch, new Rectangle(0, barra.Y, ancho, 2), UI.Acento * 0.8f);

            if (JugadorActual == null)
                return;

            float y = barra.Y + 16;
            Rectangle chip = UI.Chip(spriteBatch, new Vector2(24, y), "PILOTO", JugadorActual.Nombre, UI.TextoClaro);

            string auto = JugadorActual.AutoActual != null ? JugadorActual.AutoActual.Modelo : "Sin auto";
            Color colorAuto = JugadorActual.AutoActual != null ? UI.Azul : UI.Rojo;
            chip = UI.Chip(spriteBatch, new Vector2(chip.Right + 16, y), "AUTO", auto, colorAuto);

            UI.Chip(spriteBatch, new Vector2(chip.Right + 16, y), "DINERO", UI.Dinero(JugadorActual.Dinero), UI.Verde);

            string ayuda = JugadorActual.AutoActual == null
                ? "Comprá y equipá un auto en el CONCESIONARIO para poder correr"
                : "Elegí una opción con el mouse";
            UI.TextoDerecha(spriteBatch, UI.Fuente, ayuda, new Vector2(ancho - 24, barra.Y + 22), UI.TextoSecundario);
        }

        /// <summary>
        /// ANTES: posiciones fijas pensadas para una ventana de 800 px de ancho.
        /// AHORA: todo se calcula según el tamaño real de la pantalla, así queda
        /// centrado en cualquier monitor (1366x768, 1920x1080, etc.).
        /// </summary>
        private void ActualizarRectangulos()
        {
            int ancho = _graphicsDevice.Viewport.Width;
            int alto = _graphicsDevice.Viewport.Height;

            int altoBoton = (int)MathHelper.Clamp(alto * 0.17f, 100f, 210f);
            int anchoBoton = (int)(altoBoton * 1.97f);           // proporción de las imágenes de los botones
            int altoCorrer = (int)(altoBoton * 1.15f);
            int anchoCorrer = (int)(anchoBoton * 1.15f);
            int separacion = (int)Math.Max(24, ancho * 0.025f);

            int anchoTotal = anchoBoton * 2 + anchoCorrer + separacion * 2;
            int x = (ancho - anchoTotal) / 2;
            int centroY = (int)(alto * 0.68f);

            _rectBotonTaller = new Rectangle(x, centroY - altoBoton / 2, anchoBoton, altoBoton);
            x += anchoBoton + separacion;
            _rectBotonCorrer = new Rectangle(x, centroY - altoCorrer / 2, anchoCorrer, altoCorrer);
            x += anchoCorrer + separacion;
            _rectBotonConcesionario = new Rectangle(x, centroY - altoBoton / 2, anchoBoton, altoBoton);

            _rectBotonRed = new Rectangle(ancho / 2 - 170, _rectBotonCorrer.Bottom + 22, 340, 50);

            _rectBotonConfiguracion = new Rectangle(ancho - 84, 20, 60, 60);
            _rectBotonSalir = new Rectangle(_rectBotonConfiguracion.X - 136, 28, 120, 44);
        }

        private Texture2D CrearTexturaEngranaje(GraphicsDevice graphicsDevice, int tamano)
        {
            Color[] datos = new Color[tamano * tamano];
            Vector2 centro = new Vector2(tamano / 2f, tamano / 2f);

            float radioExterior = tamano * 0.46f;
            float radioInterior = tamano * 0.32f;
            float radioAgujero = tamano * 0.14f;
            const int cantidadDientes = 8;

            for (int y = 0; y < tamano; y++)
            {
                for (int x = 0; x < tamano; x++)
                {
                    Vector2 punto = new Vector2(x, y) - centro;
                    float distancia = punto.Length();
                    float angulo = (float)System.Math.Atan2(punto.Y, punto.X);

                    bool estaEnUnDiente = System.Math.Cos(angulo * cantidadDientes) > 0.2f;
                    float radioLimite = estaEnUnDiente ? radioExterior : radioInterior;

                    bool esCuerpoDelEngranaje = distancia <= radioLimite;
                    bool esAgujeroCentral = distancia <= radioAgujero;

                    datos[y * tamano + x] = (esCuerpoDelEngranaje && !esAgujeroCentral)
                        ? Color.White
                        : Color.Transparent;
                }
            }

            Texture2D textura = new Texture2D(graphicsDevice, tamano, tamano);
            textura.SetData(datos);
            return textura;
        }
    }
}
