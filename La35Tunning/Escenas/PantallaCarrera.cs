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
        private readonly Modelos.Jugador _jugador;
        private const decimal PremioPorGanar = 50000;

        private readonly float _carrilJugadorY;
        private readonly float _carrilRivalY;

        private readonly Vector2 _posicionSemaforo = new Vector2(700, 20);

        public EstadoCarrera Estado { get; private set; }

        private float _cronometro;
        private float _tiempoFinalJugador;
        private float _tiempoFinalRival;

        private const float VelocidadRivalProvisoria = 6.5f;

        private KeyboardState _tecladoAnterior;

        public PantallaCarrera(Auto autoJugador, Auto autoRival, Semaforo semaforo, float carrilJugadorY, float carrilRivalY, Texture2D texturaPixel, Modelos.Jugador jugador)
        {
            _autoJugador = autoJugador;
            _autoRival = autoRival;
            _semaforo = semaforo;
            _carrilJugadorY = carrilJugadorY;
            _carrilRivalY = carrilRivalY;
            _texturaPixel = texturaPixel;
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
                Estado = EstadoCarrera.Corriendo;
                _cronometro = 0f;
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
            spriteBatch.DrawString(fuente, _semaforo.TextoActual(), new Vector2(650, 20), Color.White);
            spriteBatch.Draw(_semaforo.TexturaActual(), new Rectangle(700, 70, 100, 180), Color.White);
        }
    }
}
