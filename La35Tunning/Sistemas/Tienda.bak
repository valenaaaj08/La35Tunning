using System.Collections.Generic;
using La35Tunning.Entidades;

namespace La35Tunning.Sistemas
{
    public class Tienda
    {
        // Lista interna para guardar los autos que están a la venta
        private List<Auto> _autosEnVenta = new List<Auto>();

        public Tienda()
        {
            _autosEnVenta = new List<Auto>();
        }

        // Método para agregar autos al catálogo
        public void AgregarAuto(Auto nuevoAuto)
        {
            _autosEnVenta.Add(nuevoAuto);
        }

        // Método para que el jugador compre un auto de la tienda
        public bool VenderAuto(Jugador comprador, int indiceAuto)
        {
            if (indiceAuto >= 0 && indiceAuto < _autosEnVenta.Count)
            {
                Auto autoElegido = _autosEnVenta[indiceAuto];

                if (comprador.Dinero >= autoElegido.Precio)
                {
                    comprador.Dinero -= autoElegido.Precio;
                    comprador.AutoActual = autoElegido;
                    
                    // Lo sacamos de la tienda porque ya fue comprado
                    _autosEnVenta.Remove(autoElegido);

                    System.Console.WriteLine($"¡Felicitaciones! Te compraste el {autoElegido.Modelo}");
                    return true;
                }
                else
                {
                    System.Console.WriteLine("¡No te alcanza la plata para este fierro!");
                    return false;
                }
            }

            System.Console.WriteLine("Opción inválida.");
            return false;
        }

        // Método para que la tienda le compre el auto usado al jugador (a mitad de precio, incluyendo piezas)
        public void ComprarAutoDeJugador(Jugador vendedor)
        {
            if (vendedor.AutoActual != null)
            {
                // 1. Calculamos el valor total del auto (precio base + todas sus modificaciones)
                int valorTotalAuto = vendedor.AutoActual.CalcularValorTotal();

                // 2. La tienda paga la mitad (50%)
                int precioDeRecompra = valorTotalAuto / 2;

                // 3. Le sumamos la plata al jugador
                vendedor.Dinero += precioDeRecompra;
                
                System.Console.WriteLine($"¡Vendiste tu auto por ${precioDeRecompra} (50% del valor total con modificaciones)!");
                
                // Opcional: el auto vuelve a quedar disponible en la tienda
                _autosEnVenta.Add(vendedor.AutoActual);

                // El jugador se queda sin auto actual
                vendedor.AutoActual = null;
            }
            else
            {
                System.Console.WriteLine("No tenés ningún auto para vender.");
            }
        }
    }
}
