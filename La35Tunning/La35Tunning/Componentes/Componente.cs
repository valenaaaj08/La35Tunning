namespace La35Tunning.Modelos
{
    public class Componente
    {
        private string _nombre;
        private float _multiplicadorRendimiento;
        private int _costo;

        public string Nombre { get { return _nombre; } }
        public float MultiplicadorRendimiento { get { return _multiplicadorRendimiento; } }
        public int Costo { get { return _costo; } }

        public Componente(string nombre, float multiplicadorRendimiento, int costo)
        {
            _nombre = nombre;
            _multiplicadorRendimiento = multiplicadorRendimiento;
            _costo = costo;
        }
    }
}