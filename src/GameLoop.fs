module Xelmish.XnaCore

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Xelmish.Model

type GameLoop<'view> (config: GameLoopConfig) as this = 
    inherit Game ()

    let graphics = new GraphicsDeviceManager (this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable view: 'view option = None
    let mutable keyboardState = Unchecked.defaultof<KeyboardState>
    let mutable mouseState = Unchecked.defaultof<MouseState>

    let clearColor = Option.map xnaColor config.clearColour

    do 
        match config.resolution with
        | Windowed (w,h) -> 
            graphics.PreferredBackBufferWidth <- w
            graphics.PreferredBackBufferHeight <- h

        this.IsMouseVisible <- config.mouseVisible
        graphics.SynchronizeWithVerticalRetrace <- true
        this.IsFixedTimeStep <- false 

    let renderView gameTime viewElements =
        ()

    member __.View
        with set value = 
            view <- value

    override __.LoadContent () = 
        spriteBatch <- new SpriteBatch (this.GraphicsDevice)

    override __.Update _ =
        keyboardState <- Keyboard.GetState ()
        mouseState <- Mouse.GetState ()

    override __.Draw gameTime =

        Option.iter this.GraphicsDevice.Clear clearColor

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)

        match view with
        | None -> __.Exit ()
        | Some v -> renderView gameTime v

        spriteBatch.End ()