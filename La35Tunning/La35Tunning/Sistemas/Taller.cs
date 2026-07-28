using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using La35Tunning.Entidades;
using La35Tunning.Modelos;

namespace La35Tunning.Sistemas
{
    public class Taller
    {
        private List<Componente> _catalogoPiezas = new List<Componente>();
        private List<Texture2D> _catalogoLlantas = new List<Texture2D>();

        public Taller(ContentManager content)
        {
            // Cargamos las 6 llantas (Default + 5 opcionales por número)
            _catalogoLlantas.Add(content.Load<Texture2D>("LlantaDefault"));
            _catalogoLlantas.Add(content.Load<Texture2D>("Llanta1"));
            _catalogoLlantas.Add(content.Load<Texture2D>("Llanta2"));
            _catalogoLlantas.Add(content.Load<Texture2D>("Llanta3"));
            _catalogoLlantas.Add(content.Load<Texture2D>("Llanta4"));
            _catalogoLlantas.Add(content.Load<Texture2D>("Llanta5"));

            // Motores (Stage 1, 2 y 3)
            _catalogoPiezas.Add(new Motor("Motor Stage 1", 1.10f, 40000));
            _catalogoPiezas.Add(new Motor("Motor Stage 2", 1.20f, 90000));
            _catalogoPiezas.Add(new Motor("Motor Stage 3", 1.35f, 160000));

            // Turbos (Stage 1, 2 y 3)
            _catalogoPiezas.Add(new Turbo("Turbo Stage 1", 1.15f, 50000));
            _catalogoPiezas.Add(new Turbo("Turbo Stage 2", 1.25f, 110000));
            _catalogoPiezas.Add(new Turbo("Turbo Stage 3", 1.45f, 200000));

            // Transmisiones (Stage 1, 2 y 3)
            _catalogoPiezas.Add(new Transmision("Transmisión Stage 1", 1.08f, 30000));
            _catalogoPiezas.Add(new Transmision("Transmisión Stage 2", 1.16f, 75000));
            _catalogoPiezas.Add(new Transmision("Transmisión Stage 3", 1.25f, 130000));

            // Intercooler de competición
            _catalogoPiezas.Add(new Intercooler("Intercooler de Competición", 1.18f, 65000));

            // Neumáticos especiales
            _catalogoPiezas.Add(new Neumatico("Neumáticos Semislicks", 1.12f, 45000));
            _catalogoPiezas.Add(new Neumatico("Neumáticos de Drag", 1.22f, 95000));
        }

        public List<Componente> CatalogoPiezas { get { return _catalogoPiezas; } }
        public List<Texture2D> CatalogoLlantas { get { return _catalogoLlantas; } }

        public bool InstalarPieza(Auto auto, int indicePieza)
        {
            if (auto == null)
                return false;

            if (indicePieza < 0 || indicePieza >= _catalogoPiezas.Count)
                return false;

            Componente piezaSeleccionada = _catalogoPiezas[indicePieza];

            // Instala la pieza reemplazando la anterior de su mismo tipo en el auto
            auto.InstalarPieza(piezaSeleccionada);
            return true;
        }

        public bool CambiarLlantas(Auto auto, int indiceLlantaDelantera, int indiceLlantaTrasera)
        {
            if (auto == null)
                return false;

            if (indiceLlantaDelantera < 0 || indiceLlantaDelantera >= _catalogoLlantas.Count ||
                indiceLlantaTrasera < 0 || indiceLlantaTrasera >= _catalogoLlantas.Count)
                return false;

            Texture2D llantaDelantera = _catalogoLlantas[indiceLlantaDelantera];
            Texture2D llantaTrasera = _catalogoLlantas[indiceLlantaTrasera];
            
            auto.InstalarLlantas(llantaDelantera, llantaTrasera);
            return true;
        }
    }
}