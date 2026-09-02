using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Sistemas
{
    public class Camera2D
    {
        public Matrix Transform { get; private set; }
        public float Zoom { get; set; } = 0.5f; 
        private GraphicsDevice _graphicsDevice;

        public Camera2D(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Update(Vector2 targetPosition)
        {
            var viewport = _graphicsDevice.Viewport;

            // Combinamos la traslación para centrar el auto con la escala de zoom
            Transform = Matrix.CreateTranslation(new Vector3(-targetPosition.X, -targetPosition.Y, 0)) *
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(viewport.Width / 2f, viewport.Height / 2f, 0));
        }
    }
}