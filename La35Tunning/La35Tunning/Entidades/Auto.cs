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
        public const float DistanciaMeta = 1600f;
        private const float PosicionLargada = 100f;

        public bool Descalificado { get; private set; } = false;
        public bool LlegoAMeta { get; private set; } = false;

        public float ProgresoCarrera
        {
            get
            {
                float avance = (_posicion.X - PosicionLargada) / DistanciaMeta;
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

        public void Descalificar()
        {
            Descalificado = true;
        }

        public void ReiniciarParaCarrera()
        {
            _posicion.X = PosicionLargada;
            _velocidadActual = 0f;
            Descalificado = false;
            LlegoAMeta = false;
        }

        public bool ActualizarEnCarrera(GameTime gameTime, bool semaforoEnVerde)
        {
            var estadoTeclado = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            bool intentaAcelerar = estadoTeclado.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W);

            if (!semaforoEnVerde)
            {
                return intentaAcelerar;
            }

            if (Descalificado || LlegoAMeta)
            {
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