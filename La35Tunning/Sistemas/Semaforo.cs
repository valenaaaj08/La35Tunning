using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Sistemas
{
    public enum EstadoSemaforo
    {
        Luz1,
        Luz2,
        Luz3,
        Luz4,
        Verde,
        Fallida
    }

    public class Semaforo
    {
        private Texture2D _texturaLuz1;
        private Texture2D _texturaLuz2;
        private Texture2D _texturaLuz3;
        private Texture2D _texturaLuz4;
        private Texture2D _texturaVerde;
        private Texture2D _texturaFallida;

        private const float DuracionLuz = 1.0f;

        private const float EsperaVerdeMinima = 1.6f;
        private const float EsperaVerdeMaxima = 3.5f;

        private float _tiempoRestanteEnEstado;
        private float _esperaFijaVerde = -1f;   // NUEVO: en red la elige el servidor (-1 = al azar)
        private readonly Random _random = new Random();

        public EstadoSemaforo Estado { get; private set; }

        public bool EstaEnVerde => Estado == EstadoSemaforo.Verde;
        public bool HuboSalidaAnticipada => Estado == EstadoSemaforo.Fallida;

        public Semaforo(Texture2D luz1, Texture2D luz2, Texture2D luz3, Texture2D luz4, Texture2D verde, Texture2D fallida)
        {
            _texturaLuz1 = luz1;
            _texturaLuz2 = luz2;
            _texturaLuz3 = luz3;
            _texturaLuz4 = luz4;
            _texturaVerde = verde;
            _texturaFallida = fallida;

            Reiniciar();
        }

        public void Reiniciar()
        {
            Estado = EstadoSemaforo.Luz1;
            _tiempoRestanteEnEstado = DuracionLuz;
            _esperaFijaVerde = -1f;
        }

        /// <summary>NUEVO: en red los dos jugadores tienen que ver el verde al mismo tiempo,
        /// así que la espera no es al azar: la manda el servidor.</summary>
        public void ReiniciarConEspera(float segundos)
        {
            Reiniciar();
            _esperaFijaVerde = segundos;
        }

        public void NotificarIntentoDeAcelerar()
        {
            if (Estado != EstadoSemaforo.Verde && Estado != EstadoSemaforo.Fallida)
            {
                Estado = EstadoSemaforo.Fallida;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (Estado == EstadoSemaforo.Verde || Estado == EstadoSemaforo.Fallida)
                return;

            _tiempoRestanteEnEstado -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_tiempoRestanteEnEstado <= 0f)
            {
                AvanzarSiguienteLuz();
            }
        }

        private void AvanzarSiguienteLuz()
        {
            switch (Estado)
            {
                case EstadoSemaforo.Luz1:
                    Estado = EstadoSemaforo.Luz2;
                    _tiempoRestanteEnEstado = DuracionLuz;
                    break;

                case EstadoSemaforo.Luz2:
                    Estado = EstadoSemaforo.Luz3;
                    _tiempoRestanteEnEstado = DuracionLuz;
                    break;

                case EstadoSemaforo.Luz3:
                    Estado = EstadoSemaforo.Luz4;
                    _tiempoRestanteEnEstado = _esperaFijaVerde >= 0f
                        ? _esperaFijaVerde
                        : EsperaVerdeMinima + (float)(_random.NextDouble() * (EsperaVerdeMaxima - EsperaVerdeMinima));
                    break;

                case EstadoSemaforo.Luz4:
                    Estado = EstadoSemaforo.Verde;
                    break;
            }
        }

        public Texture2D TexturaActual()
        {
            switch (Estado)
            {
                case EstadoSemaforo.Luz1: return _texturaLuz1;
                case EstadoSemaforo.Luz2: return _texturaLuz2;
                case EstadoSemaforo.Luz3: return _texturaLuz3;
                case EstadoSemaforo.Luz4: return _texturaLuz4;
                case EstadoSemaforo.Verde: return _texturaVerde;
                case EstadoSemaforo.Fallida: return _texturaFallida;
                default: return _texturaLuz1;
            }
        }

        public string TextoActual()
        {
            switch (Estado)
            {
                case EstadoSemaforo.Luz1: return "3";
                case EstadoSemaforo.Luz2: return "2";
                case EstadoSemaforo.Luz3: return "1";
                case EstadoSemaforo.Luz4: return "";
                case EstadoSemaforo.Verde: return "¡VERDE - ARRANCAR!";
                case EstadoSemaforo.Fallida: return "SALIDA ANTICIPADA";
                default: return "";
            }
        }
    }
}
