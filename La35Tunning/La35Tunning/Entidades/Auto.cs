using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using La35Tunning.Modelos; // Asegúrate de tener este using si Componente está en otra carpeta

namespace La35Tunning.Entidades
{
    public class Auto : Entidad
    {
        private string _modelo;
        private float _velocidadMaximaBase;
        private float _aceleracionBase;
        private float _velocidadActual = 0f;

        private int _precio;

        public int Precio { get { return _precio; } }

        // Textura (la imagen del auto)
        private Texture2D _texturaAuto;

        // Lista interna de objetos Componente en lugar de solo strings
        private List<Componente> _componentesInstalados = new List<Componente>();

        public Auto(string modelo, float velocidadBase, float aceleracionBase, int precio, Texture2D textura)
        {
            _modelo = modelo;
            _velocidadMaximaBase = velocidadBase;
            _aceleracionBase = aceleracionBase;
            _texturaAuto = textura;
            _precio = precio;
            _posicion = new Vector2(100, 300);
        }

        // Getter que calcula acumulativamente el multiplicador total recorriendo las piezas instaladas
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

        // Método para instalar un componente completo
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
            int valorTotal = Precio; // Arranca con el precio base del auto

            // Sumamos el precio de cada componente o pieza que tenga instalada
            foreach (var componente in _componentesInstalados)
            {
                valorTotal += componente.Costo;
            }

            return valorTotal;
        }

        public string Modelo { get { return _modelo; } }

        public override void Update(GameTime gameTime)
        {
            var estadoTeclado = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            // Usamos el getter para calcular el rendimiento real en tiempo de ejecución
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
        }

        // AQUÍ DIBUJAMOS EL AUTO EN PANTALLA
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texturaAuto, _posicion, Color.White);
        }
    }
}