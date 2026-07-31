using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using La35Tunning.Modelos;

namespace La35Tunning.Entidades
{
    public class Auto : Entidad
    {
        private string _modelo;
        private float _velocidadMaximaBase;
        private float _aceleracionBase;
        private float _velocidadActual = 0f;
        private int _precio;

        // Texturas del auto para diferentes pantallas
        private Texture2D _texturaAuto;      // Perfil (Carreras / Concesionario)
        private Texture2D _texturaTaller;    // Frente con capot abierto (Taller)
        private Texture2D _llantaDelantera;
        private Texture2D _llantaTrasera;
        public float AnguloLlanta { get; set; } = 0f;

        // Slots específicos para cada tipo de componente (nacen con stock por defecto)
        private Motor _motorActual;
        private Turbo _turboActual;
        private Transmision _transmisionActual;
        private Intercooler _intercoolerActual;
        private Neumatico _neumaticoActual;

        public int Precio { get { return _precio; } }
        public string Modelo { get { return _modelo; } }
        public Texture2D TexturaAuto { get { return _texturaAuto; } }
        public Texture2D TexturaTaller { get { return _texturaTaller; } }

        public Motor MotorActual { get { return _motorActual; } }
        public Turbo TurboActual { get { return _turboActual; } }
        public Transmision TransmisionActual { get { return _transmisionActual; } }
        public Intercooler IntercoolerActual { get { return _intercoolerActual; } }
        public Neumatico NeumaticoActual { get { return _neumaticoActual; } }

        // --- Datos específicos del modo carrera (picada de 400m) ---

        // 1600 unidades de mundo ≈ representan los "400 metros" de la
        // propuesta. No usamos metros reales porque no importa la escala
        // exacta: lo que importa es que sea la misma distancia para los
        // dos autos y que se sienta bien jugable en pantalla.
        public const float DistanciaMeta = 1600f;

        // Dónde arranca el auto en la pista (coordenada X de largada).
        private const float PosicionLargada = 100f;

        public bool Descalificado { get; private set; } = false;
        public bool LlegoAMeta { get; private set; } = false;

        // Progreso de 0.0 (arrancando) a 1.0 (llegó a la meta). Útil para
        // dibujar una barra de progreso o saber cuándo terminó la carrera.
        public float ProgresoCarrera
        {
            get
            {
                float avance = (_posicion.X - PosicionLargada) / DistanciaMeta;
                // Clamp = "recortar" el valor para que no se pase de [0, 1].
                // Es el equivalente a Math.min(1, Math.max(0, avance)) en Java.
                return MathHelper.Clamp(avance, 0f, 1f);
            }
        }

        public Auto(string modelo, float velocidadBase, float aceleracionBase, int precio, Texture2D textura, Texture2D texturaTaller = null)
        {
            _modelo = modelo;
            _velocidadMaximaBase = velocidadBase;
            _aceleracionBase = aceleracionBase;
            _texturaAuto = textura;
            _texturaTaller = texturaTaller;
            _precio = precio;
            _posicion = new Vector2(100, 200);

            // Inicialización de componentes de fábrica (Stock con multiplicador 1.0 y costo 0)
            _motorActual = new Motor("Motor de Fábrica", 1.0f, 0);
            _turboActual = new Turbo("Sin Turbo (Stock)", 1.0f, 0);
            _transmisionActual = new Transmision("Transmisión de Fábrica", 1.0f, 0);
            _intercoolerActual = new Intercooler("Sin Intercooler", 1.0f, 0);
            _neumaticoActual = new Neumatico("Neumáticos de Fábrica", 1.0f, 0);
        }

        public void InstalarLlantas(Texture2D llantaDelantera, Texture2D llantaTrasera)
        {
            _llantaDelantera = llantaDelantera;
            _llantaTrasera = llantaTrasera;
        }

        public List<Componente> ObtenerComponentesInstalados()
        {
            var lista = new List<Componente>();
            if (_motorActual != null) lista.Add(_motorActual);
            if (_turboActual != null) lista.Add(_turboActual);
            if (_transmisionActual != null) lista.Add(_transmisionActual);
            if (_intercoolerActual != null) lista.Add(_intercoolerActual);
            if (_neumaticoActual != null) lista.Add(_neumaticoActual);
            return lista;
        }

        public float MultiplicadorMotorTotal
        {
            get
            {
                float acumulador = 1.0f;
                foreach (var pieza in ObtenerComponentesInstalados())
                {
                    acumulador *= pieza.MultiplicadorRendimiento;
                }
                return acumulador;
            }
        }

        public void InstalarPieza(Componente nuevaPieza)
        {
            if (nuevaPieza is Motor motor)
            {
                _motorActual = motor;
            }
            else if (nuevaPieza is Turbo turbo)
            {
                _turboActual = turbo;
            }
            else if (nuevaPieza is Transmision transmision)
            {
                _transmisionActual = transmision;
            }
            else if (nuevaPieza is Intercooler intercooler)
            {
                _intercoolerActual = intercooler;
            }
            else if (nuevaPieza is Neumatico neumatico)
            {
                _neumaticoActual = neumatico;
            }
        }

        public void MostrarFichaTecnica()
        {
            System.Console.WriteLine($"--- Ficha Técnica: {_modelo} ---");
            foreach (var pieza in ObtenerComponentesInstalados())
            {
                System.Console.WriteLine($"- {pieza.Nombre} (Mod: {pieza.MultiplicadorRendimiento})");
            }
        }

        public int CalcularValorTotal()
        {
            int valorTotal = Precio;
            foreach (var componente in ObtenerComponentesInstalados())
            {
                valorTotal += componente.Costo;
            }
            return valorTotal;
        }

        // Mueve el auto una distancia fija sobre el eje X y detecta si
        // llegó a la meta. A diferencia de ActualizarEnCarrera, este método
        // NO lee el teclado: sirve para autos que no controla el jugador
        // local (el rival "maniquí" de prueba ahora, y más adelante la
        // IA Fantasma o el auto del rival sincronizado por red).
        public void AvanzarDistancia(float distancia)
        {
            if (Descalificado || LlegoAMeta)
                return;

            _posicion.X += distancia;

            if (_posicion.X >= PosicionLargada + DistanciaMeta)
            {
                LlegoAMeta = true;
            }
        }

        // Se llama desde PantallaCarrera cuando el semáforo detecta que
        // este auto salió antes de tiempo.
        public void Descalificar()
        {
            Descalificado = true;
        }

        // Prepara el auto para el arranque de una nueva carrera: lo manda
        // a la línea de largada y limpia el estado de la carrera anterior.
        public void ReiniciarParaCarrera()
        {
            _posicion.X = PosicionLargada;
            _velocidadActual = 0f;
            Descalificado = false;
            LlegoAMeta = false;
        }

        // Esta es la actualización que se usa DURANTE una carrera (la llama
        // PantallaCarrera, no Game1). Es distinta del Update() de más abajo
        // porque acá el auto no se mueve libremente: depende del semáforo.
        //
        // semaforoEnVerde: si todavía es false y el jugador aprieta W,
        // es salida anticipada. PantallaCarrera decide qué pasa con el
        // semáforo; acá el auto solo reporta si "quiso" acelerar.
        public bool ActualizarEnCarrera(GameTime gameTime, bool semaforoEnVerde)
        {
            var estadoTeclado = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            bool intentaAcelerar = estadoTeclado.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W);

            if (!semaforoEnVerde)
            {
                // Todavía no largó la carrera: el auto no se mueve, pero
                // devolvemos "true" si el jugador quiso salir antes de
                // tiempo, para que PantallaCarrera lo descalifique.
                return intentaAcelerar;
            }

            if (Descalificado || LlegoAMeta)
            {
                // Ya terminó su participación en esta carrera (perdió por
                // salida en falso o ya cruzó la meta): no reacciona más.
                return false;
            }

            float aceleracionReal = _aceleracionBase * MultiplicadorMotorTotal;
            float velocidadMaximaReal = _velocidadMaximaBase * MultiplicadorMotorTotal;

            if (intentaAcelerar)
            {
                _velocidadActual += aceleracionReal;
                if (_velocidadActual > velocidadMaximaReal)
                {
                    _velocidadActual = velocidadMaximaReal;
                }
            }
            else
            {
                // En carrera no permitimos frenar con S (sería raro en una
                // picada), simplemente pierde un poco de velocidad si
                // soltás el acelerador, como fricción/aire.
                _velocidadActual -= 0.05f;
                if (_velocidadActual < 0f) _velocidadActual = 0f;
            }

            _posicion.X += _velocidadActual;

            if (_velocidadActual != 0)
            {
                AnguloLlanta += _velocidadActual * (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }

            if (_posicion.X >= PosicionLargada + DistanciaMeta)
            {
                LlegoAMeta = true;
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            var estadoTeclado = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            float aceleracionReal = _aceleracionBase * MultiplicadorMotorTotal;
            float velocidadMaximaReal = _velocidadMaximaBase * MultiplicadorMotorTotal;

            if (estadoTeclado.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                _velocidadActual += aceleracionReal;
                if (_velocidadActual > velocidadMaximaReal)
                {
                    _velocidadActual = velocidadMaximaReal;
                }
            }
            else if (estadoTeclado.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                _velocidadActual -= 0.2f;
                if (_velocidadActual < 0f) _velocidadActual = 0f;
            }
            else
            {
                _velocidadActual -= 0.05f;
                if (_velocidadActual < 0f) _velocidadActual = 0f;
            }

            _posicion.X += _velocidadActual;

            if (_velocidadActual != 0)
            {
                AnguloLlanta += _velocidadActual * (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texturaAuto, _posicion, Color.White);

            if (_llantaDelantera != null && _llantaTrasera != null)
            {
                Vector2 posicionRuedaDelantera = _posicion + new Vector2(360, 730);
                Vector2 posicionRuedaTrasera = _posicion + new Vector2(1330, 730);

                float escalaLlanta = 1f;

                Vector2 origenDelantera = new Vector2(_llantaDelantera.Width / 2f, _llantaDelantera.Height / 2f);
                Vector2 origenTrasera = new Vector2(_llantaTrasera.Width / 2f, _llantaTrasera.Height / 2f);

                spriteBatch.Draw(_llantaDelantera, posicionRuedaDelantera, null, Color.White, AnguloLlanta, origenDelantera, escalaLlanta, SpriteEffects.None, 0f);
                spriteBatch.Draw(_llantaTrasera, posicionRuedaTrasera, null, Color.White, AnguloLlanta, origenTrasera, escalaLlanta, SpriteEffects.None, 0f);
            }
        }
    }
}