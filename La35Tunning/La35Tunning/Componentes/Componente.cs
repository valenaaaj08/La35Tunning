namespace La35Tunning.Modelos
{
    public class Componente
    {
        public string Nombre { get; set; }
        public int Costo { get; set; }
        public float MultiplicadorRendimiento { get; set; }

        public Componente(string nombre, int costo, float multiplicadorRendimiento)
        {
            Nombre = nombre;
            Costo = costo;
            MultiplicadorRendimiento = multiplicadorRendimiento;
        }
    }
}