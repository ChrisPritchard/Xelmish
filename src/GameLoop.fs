 module Xelmish.XnaCore

 open Microsoft.Xna.Framework
 open Microsoft.Xna.Framework.Graphics
 
 type GameLoop<'view>() as this = 
        inherit Game()

        let graphics = new GraphicsDeviceManager(this)
        let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
        let mutable view: 'view option = None

        member __.View
            with set value = 
                view <- value

        override __.LoadContent() = 
            spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        override __.Update(gameTime) =
            ()

        override __.Draw(gameTime) =
            this.GraphicsDevice.Clear Color.CornflowerBlue