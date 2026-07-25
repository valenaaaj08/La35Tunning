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



        // Textura del auto y de las llantas por separado
        private Texture2D _texturaAuto;
        private Texture2D _llantaDelantera;
        private Texture2D _llantaTrasera;
        public float AnguloLlanta { get; set; } = 0f;
        private List<Componente> _componentesInstalados = new List<Componente>();

        public int Precio { get { return _precio; } }
        public string Modelo { get { return _modelo; } }
        public Texture2D TexturaAuto { get { return _texturaAuto; } }

        public Auto(string modelo, float velocidadBase, float aceleracionBase, int precio, Texture2D textura)
        {
            _modelo = modelo;
            _velocidadMaximaBase = velocidadBase;
            _aceleracionBase = aceleracionBase;
            _texturaAuto = textura;
            _precio = precio;
            _posicion = new Vector2(100, 200);
        }

        // Método para equipar o cambiar las llantas
        public void InstalarLlantas(Texture2D llantaDelantera, Texture2D llantaTrasera)
        {
            _llantaDelantera = llantaDelantera;
            _llantaTrasera = llantaTrasera;
        }


        public float MultiplicadorMotorTotal
        {
            get
            {
                float acumulador = 1.0f;
                foreach (var pieza in _componentesInstalados)
                {
                    acumulador *= pieza.MultiplicadorRendimiento;
                }
                return acumulador;
            }
        }

        public void InstalarPieza(Componente nuevaPieza)
        {
            _componentesInstalados.Add(nuevaPieza);
        }

        public void MostrarFichaTecnica()
        {
            System.Console.WriteLine($"--- Ficha Técnica: {_modelo} ---");
            foreach (var pieza in _componentesInstalados)
            {
                System.Console.WriteLine($"- {pieza.Nombre} (Mod: {pieza.MultiplicadorRendimiento})");
            }
        }

        public int CalcularValorTotal()
        {
            int valorTotal = Precio;
            foreach (var componente in _componentesInstalados)
            {
                valorTotal += componente.Costo;
            }
            return valorTotal;
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
                AnguloLlanta += _velocidadActual * (float)gameTime.ElapsedGameTime.TotalSeconds * 2f; // El '2f' ajusta qué tan rápido giran visualmente
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 1. Dibujamos primero el chasis del auto
            spriteBatch.Draw(_texturaAuto, _posicion, Color.White);

            // 2. Si tiene llantas asignadas, las dibujamos arriba calculando su posición
            if (_llantaDelantera != null && _llantaTrasera != null)
            {
                Vector2 posicionRuedaDelantera = _posicion + new Vector2(360, 730);
                Vector2 posicionRuedaTrasera = _posicion + new Vector2(1330, 730);

                float escalaLlanta = 1f;

                // Creamos los orígenes correctos para cada una (o uno compartido si miden igual)
                Vector2 origenDelantera = new Vector2(_llantaDelantera.Width / 2f, _llantaDelantera.Height / 2f);
                Vector2 origenTrasera = new Vector2(_llantaTrasera.Width / 2f, _llantaTrasera.Height / 2f);

                // Usamos las variables correctas en el Draw
                spriteBatch.Draw(_llantaDelantera, posicionRuedaDelantera, null, Color.White, AnguloLlanta, origenDelantera, escalaLlanta, SpriteEffects.None, 0f);
                spriteBatch.Draw(_llantaTrasera, posicionRuedaTrasera, null, Color.White, AnguloLlanta, origenTrasera, escalaLlanta, SpriteEffects.None, 0f);
            }
        }
    }
}