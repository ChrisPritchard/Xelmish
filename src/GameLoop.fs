 module Xelmish

 open Microsoft.Xna.Framework
 open Microsoft.Xna.Framework.Graphics
 
 type GameLoop() as this = 
        inherit Game()

        let graphics = new GraphicsDeviceManager(this)
        let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

        override __.LoadContent() = 
            spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        override __.Update(gameTime) =
            ()

        override __.Draw(gameTime) =
            this.GraphicsDevice.Clear Color.CornflowerBlue