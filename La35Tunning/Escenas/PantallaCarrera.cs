using System;
using System.Collections.Generic;
using La35Tunning.Entidades;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace La35Tunning.Escenas
{
    public enum EstadoCarrera
    {
        Largada,
        Corriendo,
        Terminada
    }

    public class PantallaCarrera : IPantallas
    {
        private Auto _autoJugador;
        private Auto _autoRival;                 // CAMBIO: ya no es readonly, en red se usa el auto del otro jugador
        private readonly Auto _autoRivalLocal;   // NUEVO: el rival de siempre, para volver a él al salir de la red

        // ---- NUEVO: modo en red ----
        private ClienteRed _red;                 // null = carrera normal contra la máquina
        private string _nombreRival = "RIVAL";
        private string _resultadoRed;            // "GANASTE", "PERDISTE", "ERROR" o null si todavía no llegó
        private string _motivoRed = "";
        private bool _metaEnviada;

        // ---- NUEVO: cartel que aparece al pasar un cambio ----
        private string _textoCambio = "";
        private float _tiempoTextoCambio;

        public bool EnRed { get { return _red != null; } }
        private readonly Semaforo _semaforo;
        private readonly Texture2D _texturaPixel;
        private readonly Texture2D _texturaVelocimetro;
        private readonly Texture2D _texturaAgujaVelocimetro;
        private readonly Modelos.Jugador _jugador;
        private const decimal PremioPorGanar = 50000;

        private readonly float _carrilJugadorY;
        private readonly float _carrilRivalY;

        private readonly Vector2 _posicionSemaforo = new Vector2(700, 20);
        private EstadoSemaforo _estadoSemaforoAnterior;
        private float _tiempoOscurecimiento;
        private const float DuracionOscurecimiento = 0.75f;
        private float _tiempoMostrarVerde;
        private const float DuracionVerde = 0.8f;

        private const float EscalaTablero = 0.55f;
        // CAMBIO: valores medidos sobre las imágenes (antes estaban a ojo y la aguja se pasaba del 9).
        private const float EscalaAguja = 0.64f;                // así la aguja llega justo hasta las rayitas
        private const float PosicionPivotePantallaX = 261f;     // centro del arco del reloj en Velocimetro.png
        private const float PosicionPivotePantallaY = 236f;
        private const float PosicionOrigenAgujaX = 143.5f;      // centro del botón azul de Aguja_Velocimetro.png
        private const float PosicionOrigenAgujaY = 124.5f;
        private const float AnguloAgujaEnVelocidadCero = -46.6f;     // apunta al 0
        private const float AnguloAgujaEnVelocidadMaxima = 181.4f;   // apunta al 9 (antes 250: se pasaba ~70 grados)

        public EstadoCarrera Estado { get; private set; }

        private float _cronometro;
        private float _tiempoFinalJugador;
        private float _tiempoFinalRival;

        private const float VelocidadRivalProvisoria = 7.0f;   // CAMBIO: antes 6.5 desde el primer cuadro
        private const float AceleracionRival = 0.025f;         // NUEVO: ahora el rival acelera de a poco
        private float _velocidadRival;

        private KeyboardState _tecladoAnterior;

        // ==================================================================
        // NUEVO: escenario (todas las medidas en "unidades de mundo")
        // ==================================================================
        // Cuánto mundo se ve de alto en pantalla. La cámara calcula el zoom con esto,
        // así la pista ocupa siempre lo mismo en cualquier resolución.
        public const float AltoVisibleMundo = 760f;
        private const float CentroCamaraY = 382f;

        private const float YVeredaLejana = 292f;     // arriba: vereda de enfrente
        private const float YCordonLejano = 326f;
        private const float YAsfaltoArriba = 334f;
        private const float YAsfaltoAbajo = 624f;
        private const float YVeredaCercana = 632f;
        private const float YGuardarrail = 690f;

        private const float InicioMundoX = -1600f;
        private const float FinMundoX = Auto.FrenteEnMeta + 2600f;
        private const float UnidadesPorMetro = Auto.DistanciaMeta / 400f;   // la picada es de 400 metros

        private readonly Random _azar = new Random();
        private readonly List<Particula> _humo = new List<Particula>();
        private float _tiempoReaccion = -1f;      // segundos entre el verde y el primer W
        private Camera2D _camara;                 // se guarda en DibujarFondo para ubicar cosas del mundo en el HUD

        // El "árbol" de luces está en la vereda de enfrente, justo pasando la línea de largada.
        private const float AltoSemaforoMundo = 240f;
        private static readonly Vector2 BaseSemaforoMundo = new Vector2(Auto.FrenteEnLargada + 70f, YVeredaLejana + 8f);
        private bool _premioCobrado;

        private struct Particula
        {
            public Vector2 Posicion;
            public Vector2 Velocidad;
            public float Vida;
            public float VidaMaxima;
            public float Tamano;
        }

        public PantallaCarrera(Auto autoJugador, Auto autoRival, Semaforo semaforo, float carrilJugadorY, float carrilRivalY,
            Texture2D texturaPixel, Texture2D texturaVelocimetro, Texture2D texturaAgujaVelocimetro, Modelos.Jugador jugador)
        {
            _autoJugador = autoJugador;
            _autoRival = autoRival;
            _autoRivalLocal = autoRival;
            _semaforo = semaforo;
            _carrilJugadorY = carrilJugadorY;
            _carrilRivalY = carrilRivalY;
            _texturaPixel = texturaPixel;
            _texturaVelocimetro = texturaVelocimetro;
            _texturaAgujaVelocimetro = texturaAgujaVelocimetro;
            _jugador = jugador;

            LimpiarFondoGrisDelTablero(_texturaVelocimetro);

            if (_autoJugador != null)
            {
                IniciarNuevaCarrera();
            }
        }

        /// <summary>
        /// Velocimetro.png trae un rectángulo gris semitransparente alrededor del reloj.
        /// Esos píxeles tienen alfa 200 (los del reloj tienen 255), así que los
        /// volvemos transparentes una sola vez al cargar.
        /// </summary>
        private static void LimpiarFondoGrisDelTablero(Texture2D textura)
        {
            if (textura == null)
                return;

            Color[] datos = new Color[textura.Width * textura.Height];
            textura.GetData(datos);
            for (int i = 0; i < datos.Length; i++)
            {
                if (datos[i].A < 250)
                    datos[i] = Color.Transparent;
            }
            textura.SetData(datos);
        }

        public void IniciarNuevaCarrera()
        {
            if (_autoJugador == null || _autoRival == null)
                return;

            _semaforo.Reiniciar();

            _autoJugador.ReiniciarParaCarrera();
            _autoJugador.Posicion = new Vector2(_autoJugador.Posicion.X, _carrilJugadorY);

            _autoRival.ReiniciarParaCarrera();
            _autoRival.Posicion = new Vector2(_autoRival.Posicion.X, _carrilRivalY);

            _cronometro = 0f;
            _tiempoFinalJugador = 0f;
            _tiempoFinalRival = 0f;
            _estadoSemaforoAnterior = _semaforo.Estado;
            _tiempoOscurecimiento = DuracionOscurecimiento;
            _tiempoMostrarVerde = 0f;
            _tiempoReaccion = -1f;
            _premioCobrado = false;
            _resultadoRed = null;
            _motivoRed = "";
            _metaEnviada = false;
            _tiempoTextoCambio = 0f;
            _velocidadRival = 0f;
            _humo.Clear();

            Estado = EstadoCarrera.Largada;
        }

        public void CambiarAutoJugador(Auto autoJugador)
        {
            if (autoJugador == null || ReferenceEquals(_autoJugador, autoJugador))
                return;

            _autoJugador = autoJugador;
            IniciarNuevaCarrera();
        }

        // ==================================================================
        // NUEVO: CARRERA EN RED
        // ==================================================================

        /// <summary>Arranca una carrera contra otro jugador. La llama Game1 cuando el servidor manda INICIO.</summary>
        public void IniciarCarreraEnRed(ClienteRed red, Auto autoRival, string nombreRival, float esperaVerde)
        {
            _red = red;
            _autoRival = autoRival;
            _nombreRival = nombreRival;
            IniciarNuevaCarrera();
            _semaforo.ReiniciarConEspera(esperaVerde);   // mismo semáforo que el otro jugador
            _estadoSemaforoAnterior = _semaforo.Estado;
        }

        /// <summary>Corta la conexión y vuelve al rival de siempre. Game1 la llama al salir de la carrera.</summary>
        public void SalirDeRed()
        {
            _red?.Cerrar();
            _red = null;
            _autoRival = _autoRivalLocal;
            _nombreRival = "RIVAL";
        }

        /// <summary>Lee todo lo que mandó el servidor desde el cuadro anterior.</summary>
        private void ProcesarMensajesDeRed()
        {
            while (_red.TryRecibir(out string mensaje))
            {
                string[] partes = mensaje.Split('|');

                if (partes[0] == "RIVAL")
                {
                    _autoRival.MoverA(ClienteRed.LeerNumero(partes[1]));
                }
                else if (partes[0] == "RESULTADO")
                {
                    _resultadoRed = partes[1];
                    _motivoRed = partes.Length > 2 ? partes[2] : "";
                    Estado = EstadoCarrera.Terminada;

                    if (_resultadoRed == "GANASTE" && !_premioCobrado)
                    {
                        _jugador.SumarDinero(PremioPorGanar);
                        _premioCobrado = true;
                    }
                }
                else if (partes[0] == "DESCONECTADO" && _resultadoRed == null)
                {
                    _resultadoRed = "ERROR";
                    _motivoRed = "Se perdió la conexión con el servidor.";
                    Estado = EstadoCarrera.Terminada;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_autoJugador == null || _autoRival == null)
                return;

            if (EnRed)
                ProcesarMensajesDeRed();

            switch (Estado)
            {
                case EstadoCarrera.Largada:
                    ActualizarLargada(gameTime);
                    break;

                case EstadoCarrera.Corriendo:
                    ActualizarCarrera(gameTime);
                    break;

                case EstadoCarrera.Terminada:
                    ActualizarPantallaDeResultado();
                    break;
            }

            ActualizarHumo((float)gameTime.ElapsedGameTime.TotalSeconds);
            _tecladoAnterior = Keyboard.GetState();
        }

        private void ActualizarLargada(GameTime gameTime)
        {
            _semaforo.Update(gameTime);

            if (_estadoSemaforoAnterior != _semaforo.Estado)
            {
                _estadoSemaforoAnterior = _semaforo.Estado;
                _tiempoOscurecimiento = DuracionOscurecimiento;
            }

            if (_tiempoOscurecimiento > 0f)
            {
                _tiempoOscurecimiento -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_tiempoOscurecimiento < 0f)
                    _tiempoOscurecimiento = 0f;
            }

            bool salioAntes = _autoJugador.ActualizarEnCarrera(gameTime, semaforoEnVerde: false);
            if (salioAntes)
            {
                _semaforo.NotificarIntentoDeAcelerar();
                _red?.Enviar("SALIDA_ANTICIPADA");   // NUEVO: en red, el servidor decide que perdiste
            }

            if (_semaforo.HuboSalidaAnticipada)
            {
                _autoJugador.Descalificar();
                Estado = EstadoCarrera.Terminada;
                return;
            }

            if (_semaforo.EstaEnVerde)
            {
                // CAMBIO: antes la carrera arrancaba 0.8 s DESPUÉS del verde y durante ese
                // tiempo apretar W no hacía nada (se perdía el tiempo de reacción, que es
                // lo más importante de una picada). Ahora arranca en el mismo instante del
                // verde; el semáforo en verde se sigue mostrando DuracionVerde segundos.
                Estado = EstadoCarrera.Corriendo;
                _cronometro = 0f;
                _tiempoMostrarVerde = 0f;
            }
        }

        private bool GanoElJugador()
        {
            bool ganaPorMeta = _autoJugador.LlegoAMeta && (!_autoRival.LlegoAMeta || _tiempoFinalJugador <= _tiempoFinalRival);
            return ganaPorMeta;
        }

        private bool TerminoLaCarrera()
        {
            return _autoJugador.LlegoAMeta || _autoRival.LlegoAMeta;
        }

        private void ActualizarCarrera(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _cronometro += dt;
            _tiempoMostrarVerde += dt;

            // NUEVO: tiempo de reacción = cuánto tardó en apretar W desde que se puso verde.
            if (_tiempoReaccion < 0f && Keyboard.GetState().IsKeyDown(Keys.W))
            {
                _tiempoReaccion = _cronometro;
            }

            _autoJugador.ActualizarEnCarrera(gameTime, semaforoEnVerde: true);

            // NUEVO: ESPACIO pasa un cambio (solo cuenta el momento de apretar, no mantenerla)
            bool apretoEspacio = Keyboard.GetState().IsKeyDown(Keys.Space) && _tecladoAnterior.IsKeyUp(Keys.Space);
            if (apretoEspacio && !_autoJugador.LlegoAMeta)
            {
                string resultado = _autoJugador.PasarCambio();
                if (resultado != "")
                {
                    _textoCambio = resultado;
                    _tiempoTextoCambio = 1f;
                }
            }
            if (_tiempoTextoCambio > 0f)
                _tiempoTextoCambio -= dt;
            if (!EnRed)   // en red, al rival lo mueve el servidor
            {
                _velocidadRival = Math.Min(_velocidadRival + AceleracionRival, VelocidadRivalProvisoria);
                _autoRival.AvanzarDistancia(_velocidadRival);
            }

            // NUEVO: humo de gomas al salir (jugador mientras acelera fuerte, rival el primer segundo).
            if (Keyboard.GetState().IsKeyDown(Keys.W) && _autoJugador.VelocidadActual < 5f)
                EmitirHumo(_autoJugador);
            if (_cronometro < 1.1f)
                EmitirHumo(_autoRival);

            if (_autoJugador.LlegoAMeta && _tiempoFinalJugador == 0f)
                _tiempoFinalJugador = _cronometro;

            if (_autoRival.LlegoAMeta && _tiempoFinalRival == 0f)
                _tiempoFinalRival = _cronometro;

            if (EnRed)
            {
                // NUEVO: le contamos al servidor dónde estamos y, al llegar, nuestro tiempo.
                // Quién gana NO lo decidimos acá: lo decide el servidor y llega en "RESULTADO".
                _red.Enviar("POS|" + ClienteRed.Numero(_autoJugador.Posicion.X));
                if (_autoJugador.LlegoAMeta && !_metaEnviada)
                {
                    _red.Enviar("META|" + ClienteRed.Numero(_tiempoFinalJugador));
                    _metaEnviada = true;
                }
                return;
            }

            if (TerminoLaCarrera())
            {
                Estado = EstadoCarrera.Terminada;

                if (GanoElJugador())
                {
                    _jugador.SumarDinero(PremioPorGanar);
                    _premioCobrado = true;
                }
            }
        }

        private void ActualizarPantallaDeResultado()
        {
            if (EnRed)
                return;   // en red no hay revancha con ENTER: hay que volver a conectarse

            var tecladoActual = Keyboard.GetState();
            bool sePresionoAhora = tecladoActual.IsKeyDown(Keys.Enter) && !_tecladoAnterior.IsKeyDown(Keys.Enter);

            if (sePresionoAhora)
            {
                IniciarNuevaCarrera();
            }
        }

        private void MostrarResultado()
        {
            if (_autoJugador.Descalificado)
            {
                System.Diagnostics.Debug.WriteLine("Salida anticipada: ganó el rival.");
                return;
            }

            bool ganoJugador = _autoJugador.LlegoAMeta &&
                (!_autoRival.LlegoAMeta || _tiempoFinalJugador <= _tiempoFinalRival);

            if (ganoJugador)
                System.Diagnostics.Debug.WriteLine($"¡Ganó el jugador! Tiempo: {_tiempoFinalJugador:0.000}s");
            else
                System.Diagnostics.Debug.WriteLine($"Ganó el rival. Tiempo: {_tiempoFinalRival:0.000}s");
        }

        // ==================================================================
        // HUMO (sistema de partículas muy simple)
        // ==================================================================

        private void EmitirHumo(Auto auto)
        {
            // Sale de la rueda de atrás (la izquierda, porque los autos miran a la derecha).
            Vector2 origen = new Vector2(auto.Posicion.X + Auto.AnchoEnCarrera * 0.16f, auto.Posicion.Y - 6f);

            Particula particula = new Particula
            {
                Posicion = origen + new Vector2((float)_azar.NextDouble() * 10f, (float)_azar.NextDouble() * -6f),
                Velocidad = new Vector2(-40f - (float)_azar.NextDouble() * 70f, -10f - (float)_azar.NextDouble() * 35f),
                VidaMaxima = 0.7f + (float)_azar.NextDouble() * 0.5f,
                Tamano = 14f + (float)_azar.NextDouble() * 10f
            };
            particula.Vida = particula.VidaMaxima;
            _humo.Add(particula);
        }

        private void ActualizarHumo(float dt)
        {
            for (int i = _humo.Count - 1; i >= 0; i--)
            {
                Particula p = _humo[i];   // es un struct: hay que copiarlo, cambiarlo y volver a guardarlo
                p.Vida -= dt;
                if (p.Vida <= 0f)
                {
                    _humo.RemoveAt(i);
                    continue;
                }

                p.Posicion += p.Velocidad * dt;
                p.Velocidad *= 0.97f;
                p.Tamano += 60f * dt;
                _humo[i] = p;
            }
        }

        private void DibujarHumo(SpriteBatch spriteBatch)
        {
            foreach (Particula p in _humo)
            {
                float vida = p.Vida / p.VidaMaxima;   // 1 = recién nacida, 0 = por desaparecer
                UI.Resplandor(spriteBatch, p.Posicion, p.Tamano, p.Tamano * 0.8f, new Color(215, 215, 222) * (0.55f * vida));
            }
        }

        // ==================================================================
        // CÁMARA
        // ==================================================================

        /// <summary>
        /// NUEVO: dónde tiene que mirar la cámara. Antes se centraba en la esquina del auto
        /// del jugador (quedaba corrido y no se veía el camino de adelante). Ahora el auto
        /// queda a un tercio de la pantalla, mirando hacia donde va, y la pista centrada.
        /// </summary>
        public Vector2 ObtenerCentroCamara(float anchoVisibleMundo)
        {
            if (_autoJugador == null)
                return new Vector2(0, CentroCamaraY);

            float centroAuto = _autoJugador.Posicion.X + Auto.AnchoEnCarrera / 2f;
            return new Vector2(centroAuto + anchoVisibleMundo * 0.17f, CentroCamaraY);
        }

        // ==================================================================
        // FONDO CON PARALLAX (se dibuja SIN la cámara, en coordenadas de pantalla)
        // ==================================================================

        /// <summary>
        /// Cielo de atardecer, sol y dos filas de edificios que se mueven más lento
        /// que la pista (efecto "parallax"): da sensación de profundidad y velocidad.
        /// </summary>
        public void DibujarFondo(SpriteBatch spriteBatch, Camera2D camara)
        {
            _camara = camara;
            int ancho = spriteBatch.GraphicsDevice.Viewport.Width;
            int alto = spriteBatch.GraphicsDevice.Viewport.Height;
            float zoom = camara.Zoom;
            float camaraX = camara.Centro.X;
            float horizonte = camara.MundoAPantalla(new Vector2(0, YVeredaLejana)).Y;

            // Cielo: azul oscuro arriba -> violeta -> naranja en el horizonte
            UI.Rect(spriteBatch, new Rectangle(0, 0, ancho, (int)horizonte + 2), new Color(22, 28, 62));
            UI.Degrade(spriteBatch, new Rectangle(0, 0, ancho, (int)horizonte + 2), new Color(118, 64, 120), true);
            UI.Degrade(spriteBatch, new Rectangle(0, (int)(horizonte * 0.45f), ancho, (int)(horizonte * 0.55f) + 2), new Color(246, 140, 74), true);

            // Algunas estrellas
            for (int i = 0; i < 40; i++)
            {
                float x = UI.AzarFijo(i, 7) * ancho;
                float y = UI.AzarFijo(i, 11) * horizonte * 0.45f;
                float brillo = 0.25f + 0.5f * UI.AzarFijo(i, 13);
                UI.Rect(spriteBatch, new Rectangle((int)x, (int)y, 2, 2), Color.White * brillo);
            }

            // Sol (se mueve apenas: está "muy lejos")
            Vector2 sol = new Vector2(ancho * 0.7f - camaraX * zoom * 0.015f, horizonte - alto * 0.09f);
            UI.Resplandor(spriteBatch, sol, alto * 0.42f, alto * 0.3f, new Color(255, 140, 70) * 0.45f);
            UI.CirculoLleno(spriteBatch, sol, alto * 0.065f, new Color(255, 214, 150));

            // Edificios lejanos (silueta) y cercanos (con color y ventanas)
            DibujarCapaEdificios(spriteBatch, camaraX, zoom, ancho, horizonte, 0.12f, 150f, 0.16f, 0.36f, 3, false);
            UI.Degrade(spriteBatch, new Rectangle(0, (int)(horizonte - alto * 0.12f), ancho, (int)(alto * 0.12f)), new Color(246, 140, 74) * 0.25f, true);
            DibujarCapaEdificios(spriteBatch, camaraX, zoom, ancho, horizonte, 0.45f, 270f, 0.12f, 0.27f, 5, true);
        }

        private void DibujarCapaEdificios(SpriteBatch spriteBatch, float camaraX, float zoom, int anchoPantalla, float baseY,
            float parallax, float anchoLote, float altoMinimo, float altoMaximo, int semilla, bool cercanos)
        {
            int altoPantalla = spriteBatch.GraphicsDevice.Viewport.Height;
            float desplazamiento = camaraX * parallax;
            float medioAnchoVisible = anchoPantalla / 2f / zoom;

            int primero = (int)Math.Floor((desplazamiento - medioAnchoVisible) / anchoLote) - 1;
            int ultimo = (int)Math.Ceiling((desplazamiento + medioAnchoVisible) / anchoLote) + 1;

            Color[] paleta =
            {
                new Color(112, 156, 186),   // celeste (como la escuela del menú)
                new Color(200, 178, 140),   // crema
                new Color(170, 98, 82),     // ladrillo
                new Color(132, 150, 120),   // verde gris
                new Color(176, 140, 160),   // rosado viejo
            };

            for (int i = primero; i <= ultimo; i++)
            {
                float a1 = UI.AzarFijo(i, semilla);
                float a2 = UI.AzarFijo(i, semilla + 1);
                float a3 = UI.AzarFijo(i, semilla + 2);

                float anchoEdificio = anchoLote * (cercanos ? 0.86f + a1 * 0.14f : 0.55f + a1 * 0.45f);
                float altoEdificio = altoPantalla * (altoMinimo + a2 * (altoMaximo - altoMinimo));

                float x = anchoPantalla / 2f + (i * anchoLote - desplazamiento) * zoom;
                Rectangle edificio = new Rectangle((int)x, (int)(baseY - altoEdificio), (int)(anchoEdificio * zoom), (int)altoEdificio + 1);

                if (!cercanos)
                {
                    UI.Rect(spriteBatch, edificio, new Color(56, 42, 82));
                    // Ventanitas encendidas sueltas
                    for (int v = 0; v < 6; v++)
                    {
                        if (UI.AzarFijo(i * 10 + v, semilla + 5) > 0.55f)
                        {
                            int vx = edificio.X + (int)(UI.AzarFijo(i * 10 + v, semilla + 6) * (edificio.Width - 6));
                            int vy = edificio.Y + (int)(UI.AzarFijo(i * 10 + v, semilla + 7) * (edificio.Height - 6));
                            UI.Rect(spriteBatch, new Rectangle(vx, vy, 3, 4), new Color(255, 200, 120) * 0.7f);
                        }
                    }
                    continue;
                }

                // Edificio cercano: frente de color oscurecido por el atardecer
                Color color = Color.Lerp(paleta[(int)(a3 * paleta.Length) % paleta.Length], new Color(40, 30, 60), 0.45f);
                UI.Rect(spriteBatch, edificio, color);
                UI.Rect(spriteBatch, new Rectangle(edificio.X, edificio.Y, edificio.Width, (int)(6 * zoom)), Color.Black * 0.25f);   // cornisa
                UI.Rect(spriteBatch, new Rectangle(edificio.Right - (int)(4 * zoom), edificio.Y, (int)(4 * zoom), edificio.Height), Color.Black * 0.2f);

                // Ventanas en grilla
                int columnas = 2 + (int)(a1 * 3);
                int pisos = Math.Max(1, (int)(altoEdificio / (54 * zoom)) - 1);
                float anchoVentana = edificio.Width / (columnas * 1.8f + 0.8f);
                float altoVentana = 26 * zoom;
                for (int c = 0; c < columnas; c++)
                {
                    for (int p = 0; p < pisos; p++)
                    {
                        int vx = (int)(edificio.X + anchoVentana * 0.8f + c * anchoVentana * 1.8f);
                        int vy = (int)(edificio.Y + 18 * zoom + p * 54 * zoom);
                        bool encendida = UI.AzarFijo(i * 100 + c * 10 + p, semilla + 3) > 0.6f;
                        Color colorVentana = encendida ? new Color(255, 204, 128) : new Color(30, 34, 52);
                        UI.Rect(spriteBatch, new Rectangle(vx, vy, (int)anchoVentana, (int)altoVentana), colorVentana);
                        if (encendida)
                            UI.Resplandor(spriteBatch, new Vector2(vx + anchoVentana / 2f, vy + altoVentana / 2f), anchoVentana, altoVentana, new Color(255, 180, 90) * 0.18f);
                    }
                }

                // Toldo a rayas en planta baja (algunos edificios)
                if (a2 > 0.45f)
                {
                    int toldoY = (int)(baseY - 46 * zoom);
                    int franjas = 8;
                    float anchoFranja = edificio.Width * 0.7f / franjas;
                    for (int f = 0; f < franjas; f++)
                    {
                        Color colorFranja = f % 2 == 0 ? new Color(196, 60, 60) : new Color(230, 220, 210);
                        UI.Rect(spriteBatch, new Rectangle((int)(edificio.X + edificio.Width * 0.15f + f * anchoFranja), toldoY, (int)anchoFranja + 1, (int)(14 * zoom)), colorFranja * 0.8f);
                    }
                }
            }
        }

        // ==================================================================
        // MUNDO (se dibuja CON la cámara: Game1 usa transformMatrix)
        // ==================================================================

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_autoJugador == null || _autoRival == null)
                return;

            float largo = FinMundoX - InicioMundoX;
            int x0 = (int)InicioMundoX;

            // Vereda de enfrente
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YVeredaLejana, (int)largo, (int)(YCordonLejano - YVeredaLejana)), new Color(98, 92, 108));
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YVeredaLejana, (int)largo, 3), Color.Black * 0.35f);
            for (float x = InicioMundoX; x < FinMundoX; x += 80f)
            {
                UI.Rect(spriteBatch, new Rectangle((int)x, (int)YVeredaLejana, 2, (int)(YCordonLejano - YVeredaLejana)), new Color(78, 72, 88));
            }

            DibujarFaroles(spriteBatch);
            DibujarCartelesDeDistancia(spriteBatch);
            DibujarPorticoDeMeta(spriteBatch);
            DibujarSemaforoEnLaPista(spriteBatch);

            // Cordón + asfalto
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YCordonLejano, (int)largo, (int)(YAsfaltoArriba - YCordonLejano)), new Color(176, 174, 184));
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YAsfaltoArriba, (int)largo, (int)(YAsfaltoAbajo - YAsfaltoArriba)), new Color(46, 48, 58));
            UI.Degrade(spriteBatch, new Rectangle(x0, (int)YAsfaltoArriba, (int)largo, 40), Color.Black * 0.35f);

            // Marcas de goma de los carriles (más oscuras cerca de la largada, como en una pista real)
            DibujarMarcasDeGoma(spriteBatch, _carrilRivalY);
            DibujarMarcasDeGoma(spriteBatch, _carrilJugadorY);

            // Líneas blancas de los bordes y línea divisoria punteada
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YAsfaltoArriba + 10, (int)largo, 4), Color.White * 0.75f);
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YAsfaltoAbajo - 14, (int)largo, 4), Color.White * 0.75f);
            float yDivisoria = (_carrilRivalY + _carrilJugadorY) / 2f + 6f;
            for (float x = InicioMundoX; x < FinMundoX; x += 140f)
            {
                UI.Rect(spriteBatch, new Rectangle((int)x, (int)yDivisoria, 70, 4), new Color(250, 214, 80) * 0.85f);
            }

            // Línea de largada y de meta
            UI.Rect(spriteBatch, new Rectangle((int)Auto.FrenteEnLargada, (int)YAsfaltoArriba + 10, 8, (int)(YAsfaltoAbajo - YAsfaltoArriba) - 20), Color.White * 0.9f);
            DibujarCuadriculado(spriteBatch, (int)Auto.FrenteEnMeta, (int)YAsfaltoArriba + 10, 3, (int)((YAsfaltoAbajo - YAsfaltoArriba - 20) / 16f), 16);

            // Vereda de este lado
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YAsfaltoAbajo, (int)largo, (int)(YVeredaCercana - YAsfaltoAbajo)), new Color(176, 174, 184));
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YVeredaCercana, (int)largo, (int)(YGuardarrail - YVeredaCercana)), new Color(108, 102, 118));
            for (float x = InicioMundoX; x < FinMundoX; x += 90f)
            {
                UI.Rect(spriteBatch, new Rectangle((int)x, (int)YVeredaCercana, 2, (int)(YGuardarrail - YVeredaCercana)), new Color(86, 80, 96));
            }

            // Autos (primero el de atrás) con el humo debajo
            DibujarHumo(spriteBatch);
            _autoRival.Draw(spriteBatch);
            _autoJugador.Draw(spriteBatch);

            // Guardarraíl en primer plano
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YGuardarrail + 22, (int)largo, 400), new Color(20, 22, 28));
            for (float x = InicioMundoX; x < FinMundoX; x += 160f)
            {
                UI.Rect(spriteBatch, new Rectangle((int)x, (int)YGuardarrail, 12, 46), new Color(54, 56, 66));
            }
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YGuardarrail + 4, (int)largo, 16), new Color(150, 156, 168));
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YGuardarrail + 4, (int)largo, 3), new Color(210, 214, 222));
            UI.Rect(spriteBatch, new Rectangle(x0, (int)YGuardarrail + 17, (int)largo, 3), Color.Black * 0.3f);
        }

        private void DibujarMarcasDeGoma(SpriteBatch spriteBatch, float yCarril)
        {
            UI.Rect(spriteBatch, new Rectangle((int)InicioMundoX, (int)yCarril - 10, (int)(FinMundoX - InicioMundoX), 14), Color.Black * 0.12f);
            UI.Rect(spriteBatch, new Rectangle((int)Auto.FrenteEnLargada - 260, (int)yCarril - 10, 900, 14), Color.Black * 0.25f);
        }

        private void DibujarCuadriculado(SpriteBatch spriteBatch, int x, int y, int columnas, int filas, int lado)
        {
            for (int c = 0; c < columnas; c++)
            {
                for (int f = 0; f < filas; f++)
                {
                    Color color = (c + f) % 2 == 0 ? Color.White : new Color(20, 20, 24);
                    UI.Rect(spriteBatch, new Rectangle(x + c * lado, y + f * lado, lado, lado), color);
                }
            }
        }

        private void DibujarFaroles(SpriteBatch spriteBatch)
        {
            for (float x = InicioMundoX + 60f; x < FinMundoX; x += 420f)
            {
                int poste = (int)x;
                UI.Rect(spriteBatch, new Rectangle(poste, 118, 6, (int)YVeredaLejana - 112), new Color(44, 46, 56));
                UI.Rect(spriteBatch, new Rectangle(poste, 118, 48, 5), new Color(44, 46, 56));
                UI.Rect(spriteBatch, new Rectangle(poste + 34, 116, 24, 9), new Color(246, 236, 200));
                UI.Resplandor(spriteBatch, new Vector2(poste + 46, 130), 80f, 56f, new Color(255, 206, 130) * 0.35f);
                UI.Resplandor(spriteBatch, new Vector2(poste + 46, YCordonLejano - 6), 110f, 16f, new Color(255, 206, 130) * 0.22f);
            }
        }

        private void DibujarCartelesDeDistancia(SpriteBatch spriteBatch)
        {
            for (int metros = 100; metros < 400; metros += 100)
            {
                float x = Auto.FrenteEnLargada + metros * UnidadesPorMetro;
                DibujarCartel(spriteBatch, x, $"{metros} m", Color.White, new Color(30, 30, 36));
            }
            DibujarCartel(spriteBatch, Auto.FrenteEnLargada - 120f, "LARGADA", UI.Acento, new Color(30, 24, 6));
        }

        private void DibujarCartel(SpriteBatch spriteBatch, float x, string texto, Color fondo, Color colorTexto)
        {
            Vector2 medida = UI.Negrita.MeasureString(texto);
            int ancho = (int)medida.X + 24;
            UI.Rect(spriteBatch, new Rectangle((int)x - 2, 236, 5, (int)YVeredaLejana - 232), new Color(60, 62, 70));
            Rectangle cartel = new Rectangle((int)x - ancho / 2, 200, ancho, 38);
            UI.Rect(spriteBatch, new Rectangle(cartel.X + 3, cartel.Y + 4, cartel.Width, cartel.Height), Color.Black * 0.4f);
            UI.Rect(spriteBatch, cartel, fondo);
            UI.Borde(spriteBatch, cartel, 2, Color.Black * 0.5f);
            UI.TextoCentrado(spriteBatch, UI.Negrita, texto, new Vector2(cartel.Center.X, cartel.Center.Y + 1), colorTexto, 1f, false);
        }

        private void DibujarPorticoDeMeta(SpriteBatch spriteBatch)
        {
            int x = (int)Auto.FrenteEnMeta;
            UI.Rect(spriteBatch, new Rectangle(x - 30, 120, 8, (int)YVeredaLejana - 116), new Color(60, 62, 70));
            UI.Rect(spriteBatch, new Rectangle(x + 200, 120, 8, (int)YVeredaLejana - 116), new Color(60, 62, 70));

            Rectangle cartel = new Rectangle(x - 40, 110, 258, 64);
            UI.Rect(spriteBatch, new Rectangle(cartel.X + 4, cartel.Y + 5, cartel.Width, cartel.Height), Color.Black * 0.4f);
            UI.Rect(spriteBatch, cartel, new Color(22, 22, 28));
            DibujarCuadriculado(spriteBatch, cartel.X, cartel.Y, cartel.Width / 12, 1, 12);
            DibujarCuadriculado(spriteBatch, cartel.X + 12, cartel.Bottom - 12, cartel.Width / 12 - 1, 1, 12);
            UI.TextoCentrado(spriteBatch, UI.Titulo, "META", new Vector2(cartel.Center.X, cartel.Center.Y + 2), UI.Acento, 0.7f);
        }

        /// <summary>El "árbol" de luces también está en la pista, al lado de la largada.</summary>
        private void DibujarSemaforoEnLaPista(SpriteBatch spriteBatch)
        {
            Texture2D textura = _semaforo.TexturaActual();
            if (textura == null)
                return;

            Rectangle limites = UI.LimitesOpacos(textura);
            float alto = AltoSemaforoMundo;
            float escala = alto / limites.Height;
            Vector2 base_ = BaseSemaforoMundo;

            Color brillo = ColorDelSemaforo();
            if (brillo != Color.Transparent)
            {
                UI.Resplandor(spriteBatch, new Vector2(base_.X, base_.Y - alto * 0.62f), 90f, 110f, brillo * 0.45f);
            }

            spriteBatch.Draw(textura, base_, limites, Color.White, 0f,
                new Vector2(limites.Width / 2f, limites.Height), escala, SpriteEffects.None, 0f);
        }

        /// <summary>Color del brillo según qué luces están prendidas en cada sprite.</summary>
        private Color ColorDelSemaforo()
        {
            switch (_semaforo.Estado)
            {
                case EstadoSemaforo.Luz1: return Color.Transparent;            // todo apagado
                case EstadoSemaforo.Luz2: return new Color(255, 190, 90);      // pre-stage
                case EstadoSemaforo.Luz3: return new Color(255, 170, 60);      // ámbar (mitad)
                case EstadoSemaforo.Luz4: return new Color(255, 200, 90);      // ámbar (todas)
                case EstadoSemaforo.Verde: return new Color(80, 255, 120);
                case EstadoSemaforo.Fallida: return new Color(255, 60, 60);
                default: return Color.Transparent;
            }
        }

        // ==================================================================
        // HUD (sin cámara, coordenadas de pantalla)
        // ==================================================================

        public void DibujarHud(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            if (_autoJugador == null)
                return;

            int anchoPantalla = spriteBatch.GraphicsDevice.Viewport.Width;
            int altoPantalla = spriteBatch.GraphicsDevice.Viewport.Height;

            DibujarPanelTiempo(spriteBatch);
            DibujarBarraProgreso(spriteBatch, anchoPantalla);
            DibujarVelocimetro(spriteBatch, anchoPantalla, altoPantalla);

            // Controles abajo a la izquierda
            float x = 24;
            float y = altoPantalla - 52;
            x += UI.Tecla(spriteBatch, new Vector2(x, y), "W", "Acelerar (soltar = frenar)");
            x += UI.Tecla(spriteBatch, new Vector2(x, y), "ESPACIO", "Pasar cambio");
            UI.Tecla(spriteBatch, new Vector2(x, y), "ESC", "Menú");

            bool mostrarSemaforo = Estado == EstadoCarrera.Largada
                || (Estado == EstadoCarrera.Corriendo && _tiempoMostrarVerde < DuracionVerde + 0.5f);

            if (mostrarSemaforo)
            {
                float opacidad = 1f;
                if (Estado == EstadoCarrera.Corriendo)
                    opacidad = 1f - MathHelper.Clamp((_tiempoMostrarVerde - DuracionVerde) / 0.5f, 0f, 1f);

                DibujarSemaforoHud(spriteBatch, anchoPantalla, altoPantalla, opacidad);
            }

            if (Estado == EstadoCarrera.Terminada)
            {
                DibujarResultado(spriteBatch, anchoPantalla, altoPantalla);
            }
        }

        private void DibujarPanelTiempo(SpriteBatch spriteBatch)
        {
            Rectangle panel = new Rectangle(20, 20, 330, 132);
            UI.Panel(spriteBatch, panel, 0.82f, UI.Acento);

            UI.Texto(spriteBatch, UI.Fuente, "AV. LOPE DE VEGA  ·  400 m", new Vector2(panel.X + 18, panel.Y + 14), UI.TextoSecundario);

            float tiempo = Estado == EstadoCarrera.Terminada && _tiempoFinalJugador > 0f ? _tiempoFinalJugador : _cronometro;
            UI.Texto(spriteBatch, UI.Titulo, $"{tiempo:0.000}", new Vector2(panel.X + 16, panel.Y + 38), UI.TextoClaro);
            UI.Texto(spriteBatch, UI.Fuente, "seg", new Vector2(panel.X + 20 + UI.Titulo.MeasureString($"{tiempo:0.000}").X, panel.Y + 64), UI.TextoSecundario);

            int metrosRestantes = (int)Math.Ceiling(400f * (1f - _autoJugador.ProgresoCarrera));
            UI.Texto(spriteBatch, UI.Fuente, "FALTAN", new Vector2(panel.X + 18, panel.Y + 100), UI.TextoSecundario);
            UI.Texto(spriteBatch, UI.Negrita, $"{metrosRestantes} m", new Vector2(panel.X + 92, panel.Y + 97), UI.Acento);

            UI.Chip(spriteBatch, new Vector2(panel.X, panel.Bottom + 10), "DINERO", UI.Dinero(_jugador.Dinero), UI.Verde);
        }

        /// <summary>Mini-mapa arriba al centro: dónde está cada auto entre la largada y la meta.</summary>
        private void DibujarBarraProgreso(SpriteBatch spriteBatch, int anchoPantalla)
        {
            int ancho = (int)Math.Min(720, anchoPantalla * 0.4f);
            Rectangle barra = new Rectangle(anchoPantalla / 2 - ancho / 2, 34, ancho, 8);

            UI.Rect(spriteBatch, new Rectangle(barra.X - 70, barra.Y - 22, barra.Width + 140, 52), Color.Black * 0.45f);
            UI.Texto(spriteBatch, UI.Fuente, "SALIDA", new Vector2(barra.X - 62, barra.Y - 8), UI.TextoSecundario, 0.75f, false);
            UI.TextoDerecha(spriteBatch, UI.Fuente, "META", new Vector2(barra.Right + 62, barra.Y - 8), UI.Acento, 0.75f, false);

            UI.Rect(spriteBatch, barra, Color.White * 0.15f);
            DibujarCuadriculado(spriteBatch, barra.Right - 8, barra.Y - 4, 2, 4, 4);

            Vector2 rival = new Vector2(barra.X + barra.Width * _autoRival.ProgresoCarrera, barra.Center.Y);
            Vector2 jugador = new Vector2(barra.X + barra.Width * _autoJugador.ProgresoCarrera, barra.Center.Y);

            UI.CirculoLleno(spriteBatch, rival, 8, UI.Rojo);
            UI.TextoCentrado(spriteBatch, UI.Fuente, _nombreRival.ToUpper(), rival + new Vector2(0, -18), UI.Rojo, 0.7f);
            UI.CirculoLleno(spriteBatch, jugador, 9, UI.Acento);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "VOS", jugador + new Vector2(0, 19), UI.Acento, 0.7f);
        }

        private void DibujarVelocimetro(SpriteBatch spriteBatch, int anchoPantalla, int altoPantalla)
        {
            if (_texturaVelocimetro == null || _texturaAgujaVelocimetro == null)
                return;

            float escalaTablero = EscalaTablero * MathHelper.Clamp(altoPantalla / 1080f, 0.7f, 1.2f);
            int anchoTablero = (int)(_texturaVelocimetro.Width * escalaTablero);
            int altoTablero = (int)(_texturaVelocimetro.Height * escalaTablero);
            int tableroX = anchoPantalla - anchoTablero - 30;
            int tableroY = altoPantalla - altoTablero - 30;

            UI.Resplandor(spriteBatch, new Vector2(tableroX + anchoTablero / 2f, tableroY + altoTablero / 2f),
                anchoTablero * 0.75f, altoTablero * 0.75f, Color.Black * 0.7f);

            Vector2 posicionTablero = new Vector2(tableroX, tableroY);
            spriteBatch.Draw(
                _texturaVelocimetro,
                posicionTablero,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                escalaTablero,
                SpriteEffects.None,
                0f);

            // CAMBIO: la aguja ahora marca las RPM de la marcha actual (antes: velocidad / 12).
            float velocidadPorcentaje = _autoJugador.Rpm;
            if (velocidadPorcentaje < 0f) velocidadPorcentaje = 0f;
            if (velocidadPorcentaje > 1f) velocidadPorcentaje = 1f;

            float rotacionAguja = MathHelper.Lerp(
                MathHelper.ToRadians(AnguloAgujaEnVelocidadCero),
                MathHelper.ToRadians(AnguloAgujaEnVelocidadMaxima),
                velocidadPorcentaje);

            Vector2 origenAguja = new Vector2(
                PosicionOrigenAgujaX,
                PosicionOrigenAgujaY);

            Vector2 puntoPivotePantalla = posicionTablero + new Vector2(
                PosicionPivotePantallaX * escalaTablero,
                PosicionPivotePantallaY * escalaTablero);

            // La aguja se escala igual que el tablero (antes quedaba fija si cambiaba el tablero).
            float escalaAguja = EscalaAguja * (escalaTablero / EscalaTablero);
            spriteBatch.Draw(
                _texturaAgujaVelocimetro,
                puntoPivotePantalla,
                null,
                Color.White,
                rotacionAguja,
                origenAguja,
                escalaAguja,
                SpriteEffects.None,
                0f);

            // Velocidad en números (aproximada para mostrar)
            // NUEVO: marcha actual al lado del tablero. Se pone roja y titila cuando conviene cambiar.
            bool convieneCambiar = _autoJugador.Rpm >= 0.9f && _autoJugador.Marcha < Auto.CantidadMarchas && Estado == EstadoCarrera.Corriendo;
            bool titilar = convieneCambiar && (int)(_cronometro * 8) % 2 == 0;
            Vector2 posicionMarcha = new Vector2(tableroX - 70, tableroY + altoTablero / 2f);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "MARCHA", posicionMarcha + new Vector2(0, -48), UI.TextoSecundario);
            UI.TextoCentrado(spriteBatch, UI.Titulo, _autoJugador.Marcha.ToString(), posicionMarcha, titilar ? UI.Rojo : UI.TextoClaro, 1.6f);
            if (convieneCambiar)
                UI.TextoCentrado(spriteBatch, UI.Negrita, "¡CAMBIÁ!", posicionMarcha + new Vector2(0, 48), UI.Rojo);

            // NUEVO: cartel de "CAMBIO PERFECTO / BUEN CAMBIO / CAMBIO TEMPRANO"
            if (_tiempoTextoCambio > 0f)
            {
                Color colorCambio = _textoCambio.StartsWith("¡") ? UI.Verde : (_textoCambio.StartsWith("BUEN") ? UI.Acento : UI.Rojo);
                UI.TextoCentrado(spriteBatch, UI.Titulo, _textoCambio, new Vector2(anchoPantalla / 2f, altoPantalla * 0.82f),
                    colorCambio * MathHelper.Clamp(_tiempoTextoCambio * 2f, 0f, 1f), 0.8f);
            }

            int kmh = (int)(_autoJugador.VelocidadActual * 18f);
            UI.TextoCentrado(spriteBatch, UI.Titulo, kmh.ToString(), new Vector2(tableroX + anchoTablero / 2f, tableroY - 30), UI.TextoClaro, 0.9f);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "km/h", new Vector2(tableroX + anchoTablero / 2f, tableroY - 2), UI.TextoSecundario);
        }

        /// <summary>
        /// ANTES: el semáforo se estiraba a 220x360 fijo en el medio de la pantalla. Como
        /// cada sprite tiene distinto tamaño (99x298, 85x295, 89x300...) "saltaba" y se
        /// deformaba en cada cambio de luz, y el número de la cuenta regresiva tapaba las luces.
        /// AHORA: hay un solo semáforo, el de la pista (DibujarSemaforoEnLaPista), recortado
        /// a su parte visible y escalado sin deformar. Acá en el HUD solo:
        ///   1) oscurecemos la pantalla un instante en cada cambio de luz (efecto original),
        ///   2) volvemos a dibujar el semáforo ENCIMA del oscurecido para que resalte,
        ///   3) ponemos el texto AL COSTADO de las luces y la ayuda abajo.
        /// </summary>
        private void DibujarSemaforoHud(SpriteBatch spriteBatch, int anchoPantalla, int altoPantalla, float opacidad)
        {
            if (_camara == null)
                return;

            Texture2D textura = _semaforo.TexturaActual();
            if (textura == null)
                return;

            Rectangle limites = UI.LimitesOpacos(textura);
            float escala = AltoSemaforoMundo / limites.Height * _camara.Zoom;
            float alto = limites.Height * escala;
            float ancho = limites.Width * escala;
            Vector2 basePie = _camara.MundoAPantalla(BaseSemaforoMundo);   // dónde está el poste en la pantalla

            if (Estado == EstadoCarrera.Largada)
            {
                if (_tiempoOscurecimiento > 0f)
                {
                    float progreso = _tiempoOscurecimiento / DuracionOscurecimiento;
                    UI.Rect(spriteBatch, new Rectangle(0, 0, anchoPantalla, altoPantalla), Color.Black * (0.45f * progreso));
                }

                Color brillo = ColorDelSemaforo();
                if (brillo != Color.Transparent)
                {
                    UI.Resplandor(spriteBatch, new Vector2(basePie.X, basePie.Y - alto * 0.62f), ancho * 2.4f, alto * 0.5f, brillo * 0.3f);
                }

                spriteBatch.Draw(textura, basePie, limites, Color.White, 0f,
                    new Vector2(limites.Width / 2f, limites.Height), escala, SpriteEffects.None, 0f);
            }

            // Texto al costado de las luces (no encima)
            string texto = _semaforo.TextoActual();
            if (texto != "")
            {
                Color colorTexto = UI.TextoClaro;
                if (_semaforo.EstaEnVerde) colorTexto = UI.Verde;
                if (_semaforo.HuboSalidaAnticipada) colorTexto = UI.Rojo;

                float escalaTexto = texto.Length <= 1 ? 2.4f : 1f;
                Vector2 medida = UI.Titulo.MeasureString(texto) * escalaTexto;
                Vector2 posicion = new Vector2(basePie.X + ancho / 2f + 40f, basePie.Y - alto * 0.6f - medida.Y / 2f);
                UI.Texto(spriteBatch, UI.Titulo, texto, posicion, colorTexto * opacidad, escalaTexto);
            }

            // Ayuda abajo al centro, sobre la vereda (no tapa los autos)
            if (Estado == EstadoCarrera.Largada && !_semaforo.EstaEnVerde)
            {
                string ayuda = "Esperá el VERDE y apretá  W";
                string aviso = "Si acelerás antes, quedás descalificado";
                Vector2 medida = UI.Negrita.MeasureString(ayuda);
                Rectangle caja = new Rectangle((int)(anchoPantalla / 2f - medida.X / 2f - 30), altoPantalla - 150, (int)medida.X + 60, 70);
                UI.Rect(spriteBatch, caja, Color.Black * 0.7f);
                UI.Rect(spriteBatch, new Rectangle(caja.X, caja.Y, caja.Width, 3), UI.Acento);
                UI.TextoCentrado(spriteBatch, UI.Negrita, ayuda, new Vector2(caja.Center.X, caja.Y + 22), UI.TextoClaro);
                UI.TextoCentrado(spriteBatch, UI.Fuente, aviso, new Vector2(caja.Center.X, caja.Y + 50), UI.TextoSecundario, 0.9f);
            }
        }

        private void DibujarResultado(SpriteBatch spriteBatch, int anchoPantalla, int altoPantalla)
        {
            UI.Rect(spriteBatch, new Rectangle(0, 0, anchoPantalla, altoPantalla), Color.Black * 0.55f);

            bool descalificado = _autoJugador.Descalificado;
            bool gano = !descalificado && GanoElJugador();

            string titulo = "GANASTE";
            Color colorTitulo = UI.Verde;
            string subtitulo = "¡Le sacaste la ficha al rival!";
            if (descalificado)
            {
                titulo = "SALIDA ANTICIPADA";
                colorTitulo = UI.Rojo;
                subtitulo = "Aceleraste antes del verde: quedás descalificado.";
            }
            else if (!gano)
            {
                titulo = "PERDISTE";
                colorTitulo = UI.Rojo;
                subtitulo = "El rival cruzó primero. ¡Mejorá el auto en el taller!";
            }

            // NUEVO: en red el resultado lo manda el servidor.
            if (EnRed && _resultadoRed != null)
            {
                gano = _resultadoRed == "GANASTE";
                titulo = gano ? "GANASTE" : (_resultadoRed == "ERROR" ? "SIN CONEXIÓN" : "PERDISTE");
                colorTitulo = gano ? UI.Verde : UI.Rojo;
                subtitulo = _motivoRed;
            }
            else if (EnRed && !descalificado)
            {
                titulo = "ESPERANDO...";
                colorTitulo = UI.Acento;
                subtitulo = "El servidor está decidiendo quién ganó.";
            }

            Rectangle panel = new Rectangle(anchoPantalla / 2 - 300, altoPantalla / 2 - 210, 600, 420);
            UI.Panel(spriteBatch, panel, 0.97f, colorTitulo);

            // Semáforo chiquito en rojo si fue salida anticipada (usa el sprite semaforoFallida)
            if (descalificado)
            {
                Texture2D fallida = _semaforo.TexturaActual();
                Rectangle limites = UI.LimitesOpacos(fallida);
                float escala = 150f / limites.Height;
                spriteBatch.Draw(fallida, new Vector2(panel.X + 50, panel.Y + 30), limites, Color.White, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
            }

            UI.TextoCentrado(spriteBatch, UI.Titulo, titulo, new Vector2(panel.Center.X, panel.Y + 62), colorTitulo, descalificado ? 0.85f : 1.2f);
            UI.TextoCentrado(spriteBatch, UI.Fuente, subtitulo, new Vector2(panel.Center.X, panel.Y + 116), UI.TextoSecundario);

            int y = panel.Y + 160;
            DibujarFilaResultado(spriteBatch, panel, ref y, "Tu tiempo", _tiempoFinalJugador > 0f ? $"{_tiempoFinalJugador:0.000} s" : "--", UI.TextoClaro);
            DibujarFilaResultado(spriteBatch, panel, ref y, "Tiempo del rival", _tiempoFinalRival > 0f ? $"{_tiempoFinalRival:0.000} s" : "--", UI.TextoClaro);
            DibujarFilaResultado(spriteBatch, panel, ref y, "Tiempo de reacción", _tiempoReaccion >= 0f ? $"{_tiempoReaccion:0.000} s" : "--", ColorReaccion());
            DibujarFilaResultado(spriteBatch, panel, ref y, "Premio", _premioCobrado ? "+ " + UI.Dinero(PremioPorGanar) : UI.Dinero(0), _premioCobrado ? UI.Verde : UI.TextoSecundario);

            float x = panel.X + 110;
            float yTeclas = panel.Bottom - 58;
            if (!EnRed)
                x += UI.Tecla(spriteBatch, new Vector2(x, yTeclas), "ENTER", "Revancha");
            UI.Tecla(spriteBatch, new Vector2(x, yTeclas), "ESC", "Volver al menú");
        }

        private void DibujarFilaResultado(SpriteBatch spriteBatch, Rectangle panel, ref int y, string etiqueta, string valor, Color colorValor)
        {
            UI.Rect(spriteBatch, new Rectangle(panel.X + 40, y + 34, panel.Width - 80, 1), UI.BordePanel * 0.6f);
            UI.Texto(spriteBatch, UI.Fuente, etiqueta, new Vector2(panel.X + 44, y + 6), UI.TextoSecundario);
            UI.TextoDerecha(spriteBatch, UI.Negrita, valor, new Vector2(panel.Right - 44, y + 3), colorValor);
            y += 44;
        }

        /// <summary>Verde si la reacción fue muy buena, amarillo si normal, rojo si lenta.</summary>
        private Color ColorReaccion()
        {
            if (_tiempoReaccion < 0f) return UI.TextoSecundario;
            if (_tiempoReaccion < 0.3f) return UI.Verde;
            if (_tiempoReaccion < 0.6f) return UI.Acento;
            return UI.Rojo;
        }
    }
}
