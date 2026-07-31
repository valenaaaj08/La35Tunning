using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Entidades;
using La35Tunning.Sistemas;

namespace La35Tunning.Pantallas
{
    // Las 3 etapas por las que pasa una carrera. Es lo mismo que un enum
    // en Java, y lo usamos para que Update() sepa qué lógica correr.
    public enum EstadoCarrera
    {
        Largada,    // el semáforo está haciendo la cuenta de luces
        Corriendo,  // ya se prendió el verde, los autos avanzan
        Terminada   // alguien ganó, alguien se descalificó, etc.
    }

    // Pantalla del MVP: semáforo + picada de 400m en vista lateral.
    //
    // IMPORTANTE (para que no te sorprenda al leerla): el "auto rival"
    // ahora mismo es un maniquí que avanza a velocidad fija, SOLO para
    // poder probar la carrera vos solo sin depender todavía del servidor
    // ni de la IA Fantasma (esas son las Etapas 3, 4 y 9 de la propuesta).
    // Cuando eso esté listo, en vez de moverlo con AvanzarDistancia() fijo,
    // se va a mover con la posición que mande el servidor o la IA.
    public class PantallaCarrera : IPantalla
    {
        private readonly Auto _autoJugador;
        private readonly Auto _autoRival;
        private readonly Semaforo _semaforo;

        // Carriles (posición Y fija) para separar visualmente los dos autos.
        private readonly float _carrilJugadorY;
        private readonly float _carrilRivalY;

        private readonly Vector2 _posicionSemaforo = new Vector2(700, 20);

        public EstadoCarrera Estado { get; private set; }

        private float _cronometro;          // segundos desde que se prendió el verde
        private float _tiempoFinalJugador;  // en qué segundo cruzó la meta el jugador (0 = todavía no)
        private float _tiempoFinalRival;

        // Velocidad fija del rival de prueba. Ver el comentario de la
        // clase: esto es TEMPORAL, solo para el prototipo local.
        private const float VelocidadRivalProvisoria = 6.5f;

        // KeyboardState del frame anterior, para detectar el instante
        // exacto en que se APRETÓ Enter (y no mientras se mantiene
        // apretado). Sin esto, "Terminada" reiniciaría la carrera muchas
        // veces en un solo segundo.
        private KeyboardState _tecladoAnterior;

        public PantallaCarrera(Auto autoJugador, Auto autoRival, Semaforo semaforo, float carrilJugadorY, float carrilRivalY)
        {
            _autoJugador = autoJugador;
            _autoRival = autoRival;
            _semaforo = semaforo;
            _carrilJugadorY = carrilJugadorY;
            _carrilRivalY = carrilRivalY;

            IniciarNuevaCarrera();
        }

        // Deja todo listo para largar: semáforo en la primera luz, autos
        // en la línea de largada, cronómetro en cero.
        public void IniciarNuevaCarrera()
        {
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

        public void Update(GameTime gameTime)
        {
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

            // semaforoEnVerde: false -> ActualizarEnCarrera no mueve el auto,
            // solo nos devuelve si el jugador intentó acelerar de prepo.
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

        private void ActualizarCarrera(GameTime gameTime)
        {
            _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;

            _autoJugador.ActualizarEnCarrera(gameTime, semaforoEnVerde: true);
            _autoRival.AvanzarDistancia(VelocidadRivalProvisoria);

            // Registramos el tiempo exacto de cada uno la primera vez que
            // cruzan la meta (por eso comparamos contra 0, "todavía no llegó").
            if (_autoJugador.LlegoAMeta && _tiempoFinalJugador == 0f)
                _tiempoFinalJugador = _cronometro;

            if (_autoRival.LlegoAMeta && _tiempoFinalRival == 0f)
                _tiempoFinalRival = _cronometro;

            if (_autoJugador.LlegoAMeta || _autoRival.LlegoAMeta)
            {
                Estado = EstadoCarrera.Terminada;
                MostrarResultado();
            }
        }

        private void ActualizarPantallaDeResultado()
        {
            // Con Enter se arranca otra carrera. Comparamos con el
            // teclado del frame anterior para que sea "apretar", no
            // "mantener apretado".
            var tecladoActual = Keyboard.GetState();
            bool sePresionoAhora = tecladoActual.IsKeyDown(Keys.Enter) && !_tecladoAnterior.IsKeyDown(Keys.Enter);

            if (sePresionoAhora)
            {
                IniciarNuevaCarrera();
            }
        }

        // TODO(próximo paso): esto hoy escribe el resultado en la consola
        // de depuración porque todavía no tenemos un SpriteFont cargado en
        // el proyecto para dibujar texto en pantalla. En cuanto agreguemos
        // uno, este resultado va a mostrarse como un cartel en el juego.
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
            // Esto dibuja los elementos que viven "en el mundo" del juego
            // (los autos), o sea que se mueven junto con la cámara.
            _autoJugador.Draw(spriteBatch);
            _autoRival.Draw(spriteBatch);
        }

        // El semáforo es HUD: tiene que quedarse fijo en la pantalla sin
        // importar hacia dónde se mueva la cámara siguiendo al auto. Por
        // eso es un método aparte: Game1 lo va a dibujar en un SpriteBatch
        // "sin cámara" (sin la Matrix de transformación), mientras que
        // Draw(spriteBatch) de arriba sí se dibuja "con cámara".
        public void DibujarHud(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_semaforo.TexturaActual(), _posicionSemaforo, Color.White);

            // TODO: acá va a ir el resto del HUD (cronómetro, distancia,
            // cartel de resultado) apenas tengamos un SpriteFont cargado.
        }
    }
}
