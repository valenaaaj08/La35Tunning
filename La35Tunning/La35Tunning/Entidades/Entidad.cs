using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Entidades
{
    public abstract class Entidad
    {
        protected Vector2 _posicion;
        protected Texture2D _textura;

        public Vector2 Posicion
        {
            get => _posicion;
            set => _posicion = value;
        }

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_textura != null)
            {
                spriteBatch.Draw(_textura, _posicion, Color.White);
            }
        }
    }
}