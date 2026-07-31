using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Sistemas
{
    // Todos los "momentos" posibles en los que puede estar el semáforo.
    // Esto es lo que en Java harías con un enum también (la sintaxis es
    // casi idéntica). Lo usamos para saber qué imagen dibujar y para que
    // PantallaCarrera sepa si ya se puede acelerar o no.
    public enum EstadoSemaforo
    {
        Luz1,
        Luz2,
        Luz3,
        Luz4,
        Verde,    // ya se puede acelerar: la carrera arrancó
        Fallida   // alguien apretó W antes de tiempo: descalificado
    }

    // Semaforo = un "árbol de largada" como los de drag racing real.
    // Prende 4 luces ámbar en secuencia y, después de una espera QUE VARÍA
    // cada carrera, prende la luz verde. Esa variación aleatoria es a
    // propósito: si el tiempo fuera siempre el mismo, el jugador podría
    // memorizarlo y "adivinar" la salida en vez de reaccionar de verdad,
    // que es justamente la mecánica de habilidad que pide la propuesta.
    public class Semaforo
    {
        // Una textura por cada estado visual posible.
        private Texture2D _texturaLuz1;
        private Texture2D _texturaLuz2;
        private Texture2D _texturaLuz3;
        private Texture2D _texturaLuz4;
        private Texture2D _texturaVerde;
        private Texture2D _texturaFallida;

        // Cuánto dura cada luz ámbar antes de pasar a la siguiente.
        private const float DuracionLuz = 0.6f;

        // El tramo final (última luz ámbar -> verde) es aleatorio entre
        // estos dos valores, en segundos.
        private const float EsperaVerdeMinima = 0.8f;
        private const float EsperaVerdeMaxima = 2.2f;

        private float _tiempoRestanteEnEstado;
        private readonly Random _random = new Random();

        // { get; private set; } = se puede LEER desde afuera de la clase
        // (Game1, PantallaCarrera, etc.) pero solo se puede MODIFICAR desde
        // adentro de Semaforo. Es el equivalente a tener un getter público
        // y el campo privado en Java, pero en una sola línea.
        public EstadoSemaforo Estado { get; private set; }

        // Propiedades "de conveniencia" para no repetir comparaciones
        // por todos lados. "=>" acá es una expression-bodied property:
        // es lo mismo que escribir "get { return ...; }" pero más corto.
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

        // Vuelve a poner el semáforo en la primera luz. Se llama cada vez
        // que arranca una carrera nueva.
        public void Reiniciar()
        {
            Estado = EstadoSemaforo.Luz1;
            _tiempoRestanteEnEstado = DuracionLuz;
        }

        // PantallaCarrera nos avisa acá si algún jugador apretó W.
        // Si el semáforo todavía no está en verde, es salida anticipada.
        public void NotificarIntentoDeAcelerar()
        {
            if (Estado != EstadoSemaforo.Verde && Estado != EstadoSemaforo.Fallida)
            {
                Estado = EstadoSemaforo.Fallida;
            }
        }

        public void Update(GameTime gameTime)
        {
            // Verde y Fallida son estados finales: una vez ahí, el
            // semáforo ya no tiene que seguir contando tiempo.
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
            // switch sobre un enum: igual que en Java. La diferencia es
            // que acá no hace falta "break" en C# 8+ con esta sintaxis
            // clásica igual lo dejamos por claridad.
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
                    // Acá está la espera aleatoria antes del verde.
                    // _random.NextDouble() da un número entre 0.0 y 1.0,
                    // lo estiramos al rango [EsperaVerdeMinima, EsperaVerdeMaxima].
                    _tiempoRestanteEnEstado = EsperaVerdeMinima +
                        (float)(_random.NextDouble() * (EsperaVerdeMaxima - EsperaVerdeMinima));
                    break;

                case EstadoSemaforo.Luz4:
                    Estado = EstadoSemaforo.Verde;
                    break;
            }
        }

        // Devuelve la textura que corresponde dibujar según el estado actual.
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
    }
}
