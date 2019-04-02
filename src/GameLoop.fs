 module Xelmish.XnaCore

 open Elmish
 open Microsoft.Xna.Framework
 open Microsoft.Xna.Framework.Graphics
 
 type GameLoop<'arg, 'model, 'msg, 'view>(program: Program<'arg, 'model, 'msg, 'view>) as this = 
        inherit Game()

        let graphics = new GraphicsDeviceManager(this)
        let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

        let mutable view = Unchecked.defaultof<'view>

        override __.LoadContent() = 
            spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        override __.Update(gameTime) =
            ()

        override __.Draw(gameTime) =
            this.GraphicsDevice.Clear Color.CornflowerBlue