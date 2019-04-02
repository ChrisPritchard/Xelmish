 module Xelmish.XnaCore

 open Microsoft.Xna.Framework
 open Microsoft.Xna.Framework.Graphics
 open Microsoft.Xna.Framework.Input
 
 type GameLoop<'view>() as this = 
        inherit Game()

        let graphics = new GraphicsDeviceManager(this)
        let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

        let mutable view: 'view option = None
        let mutable keyboardState = Unchecked.defaultof<KeyboardState>
        let mutable mouseState = Unchecked.defaultof<MouseState>

        do 
            this.IsMouseVisible <- true
            graphics.SynchronizeWithVerticalRetrace <- true
            this.IsFixedTimeStep <- false 

        let renderView gameTime viewElements =
            ()

        member __.View
            with set value = 
                view <- value

        override __.LoadContent() = 
            spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        override __.Update _ =
            keyboardState <- Keyboard.GetState()
            mouseState <- Mouse.GetState()

        override __.Draw gameTime =

            this.GraphicsDevice.Clear Color.CornflowerBlue
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)

            match view with
            | None -> __.Exit ()
            | Some v -> renderView gameTime v

            spriteBatch.End()