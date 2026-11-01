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
        private readonly Auto _autoRival;
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
        private const float EscalaAguja = 0.70f;
        private const float PosicionPivotePantallaX = 246f;
        private const float PosicionPivotePantallaY = 260f;
        private const float PosicionOrigenAgujaX = 130f;
        private const float PosicionOrigenAgujaY = 130f;
        private const float AnguloAgujaEnVelocidadCero = -45f;
        private const float AnguloAgujaEnVelocidadMaxima = 250f;

        public EstadoCarrera Estado { get; private set; }

        private float _cronometro;
        private float _tiempoFinalJugador;
        private float _tiempoFinalRival;

        private const float VelocidadRivalProvisoria = 6.5f;

        private KeyboardState _tecladoAnterior;

        public PantallaCarrera(Auto autoJugador, Auto autoRival, Semaforo semaforo, float carrilJugadorY, float carrilRivalY,
            Texture2D texturaPixel, Texture2D texturaVelocimetro, Texture2D texturaAgujaVelocimetro, Modelos.Jugador jugador)
        {
            _autoJugador = autoJugador;
            _autoRival = autoRival;
            _semaforo = semaforo;
            _carrilJugadorY = carrilJugadorY;
            _carrilRivalY = carrilRivalY;
            _texturaPixel = texturaPixel;
            _texturaVelocimetro = texturaVelocimetro;
            _texturaAgujaVelocimetro = texturaAgujaVelocimetro;
            _jugador = jugador;

            if (_autoJugador != null)
            {
                IniciarNuevaCarrera();
            }
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

            Estado = EstadoCarrera.Largada;
        }

        public void CambiarAutoJugador(Auto autoJugador)
        {
            if (autoJugador == null || ReferenceEquals(_autoJugador, autoJugador))
                return;

            _autoJugador = autoJugador;
            IniciarNuevaCarrera();
        }

        public void Update(GameTime gameTime)
        {
            if (_autoJugador == null || _autoRival == null)
                return;

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
            }

            if (_semaforo.HuboSalidaAnticipada)
            {
                _autoJugador.Descalificar();
                Estado = EstadoCarrera.Terminada;
                return;
            }

            if (_semaforo.EstaEnVerde)
            {
                _tiempoMostrarVerde += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_tiempoMostrarVerde >= DuracionVerde)
                {
                    Estado = EstadoCarrera.Corriendo;
                    _cronometro = 0f;
                }
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
            _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;

            _autoJugador.ActualizarEnCarrera(gameTime, semaforoEnVerde: true);
            _autoRival.AvanzarDistancia(VelocidadRivalProvisoria);

            if (_autoJugador.LlegoAMeta && _tiempoFinalJugador == 0f)
                _tiempoFinalJugador = _cronometro;

            if (_autoRival.LlegoAMeta && _tiempoFinalRival == 0f)
                _tiempoFinalRival = _cronometro;

            if (TerminoLaCarrera())
            {
                Estado = EstadoCarrera.Terminada;

                if (GanoElJugador())
                {
                    _jugador.SumarDinero(PremioPorGanar);
                }
            }
        }

        private void ActualizarPantallaDeResultado()
        {
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_autoJugador == null || _autoRival == null)
                return;

            for (float x = 100f; x <= 100f + Auto.DistanciaMeta; x += 200f)
            {
                spriteBatch.Draw(_texturaPixel, new Rectangle((int)x, 150, 4, 350), Color.Gray);
            }

            _autoJugador.Draw(spriteBatch);
            _autoRival.Draw(spriteBatch);
        }

        public void DibujarHud(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            if (_autoJugador == null)
                return;

            spriteBatch.DrawString(fuente, "CARRERA", new Vector2(50, 20), Color.Gold);
            spriteBatch.DrawString(fuente, $"Dinero: ${_jugador.Dinero}", new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(fuente, $"Meta: {_autoJugador.ProgresoCarrera * 100:0}%", new Vector2(50, 80), Color.White);
            spriteBatch.DrawString(fuente, "W = ACELERAR   S = FRENAR   ESC = MENU", new Vector2(50, 110), Color.LightGray);

            if (_texturaVelocimetro != null && _texturaAgujaVelocimetro != null)
            {
                int anchoPantalla = spriteBatch.GraphicsDevice.Viewport.Width;
                int altoPantalla = spriteBatch.GraphicsDevice.Viewport.Height;

                float escalaTablero = EscalaTablero;
                int anchoTablero = (int)(_texturaVelocimetro.Width * escalaTablero);
                int altoTablero = (int)(_texturaVelocimetro.Height * escalaTablero);
                int tableroX = anchoPantalla - anchoTablero - 30;
                int tableroY = altoPantalla - altoTablero - 30;

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

                float velocidadMaxima = 12f;
                float velocidadPorcentaje = _autoJugador.VelocidadActual / velocidadMaxima;
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

                float escalaAguja = EscalaAguja;
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
            }

            if (Estado == EstadoCarrera.Largada)
            {
                int anchoPantalla = spriteBatch.GraphicsDevice.Viewport.Width;
                int altoPantalla = spriteBatch.GraphicsDevice.Viewport.Height;

                if (_tiempoOscurecimiento > 0f)
                {
                    float progreso = _tiempoOscurecimiento / DuracionOscurecimiento;
                    int transparencia = (int)(180f * progreso);
                    Rectangle pantallaCompleta = new Rectangle(0, 0, anchoPantalla, altoPantalla);
                    spriteBatch.Draw(_texturaPixel, pantallaCompleta, Color.Black * (transparencia / 255f));
                }

                int anchoSemaforo = 220;
                int altoSemaforo = 360;
                Rectangle rectanguloSemaforo = new Rectangle(
                    (anchoPantalla / 2) - (anchoSemaforo / 2),
                    (altoPantalla / 2) - (altoSemaforo / 2),
                    anchoSemaforo,
                    altoSemaforo);

                spriteBatch.Draw(_semaforo.TexturaActual(), rectanguloSemaforo, Color.White);

                string contador = _semaforo.TextoActual();
                if (contador != "")
                {
                    Vector2 tamañoTexto = fuente.MeasureString(contador);
                    float escala = contador.Length <= 1 ? 4f : 1.8f;
                    Vector2 posicionTexto = new Vector2(
                        (anchoPantalla / 2f) - ((tamañoTexto.X * escala) / 2f),
                        (altoPantalla / 2f) - ((tamañoTexto.Y * escala) / 2f));

                    Color colorTexto = _semaforo.HuboSalidaAnticipada ? Color.Red : Color.White;
                    spriteBatch.DrawString(fuente, contador, posicionTexto, colorTexto, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
                }
            }

            if (Estado == EstadoCarrera.Terminada)
            {
                int anchoPantalla = spriteBatch.GraphicsDevice.Viewport.Width;
                int altoPantalla = spriteBatch.GraphicsDevice.Viewport.Height;

                string resultado = "GANASTE";
                if (_autoJugador.Descalificado)
                    resultado = "SALIDA ANTICIPADA";
                else if (!GanoElJugador())
                    resultado = "PERDISTE";

                Vector2 posicionResultado = new Vector2(
                    (anchoPantalla / 2f) - (fuente.MeasureString(resultado).X / 2f),
                    (altoPantalla / 2f) - 150f);

                spriteBatch.DrawString(fuente, resultado, posicionResultado, Color.Yellow);
                spriteBatch.DrawString(
                    fuente,
                    "ENTER = REPETIR CARRERA",
                    new Vector2((anchoPantalla / 2f) - 150f, altoPantalla / 2f),
                    Color.White);
                spriteBatch.DrawString(
                    fuente,
                    "ESC = VOLVER AL MENU",
                    new Vector2((anchoPantalla / 2f) - 130f, (altoPantalla / 2f) + 40f),
                    Color.LightGray);
            }
        }
    }
}
