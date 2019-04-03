module internal Xelmish.XnaCore

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Model
open Viewables
open System.IO

type GameLoop (config: GameConfig) as this = 
    inherit Game ()

    let graphics = new GraphicsDeviceManager (this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable view: (Viewable list) option = None

    let mutable keyboardState = Unchecked.defaultof<KeyboardState>
    let mutable mouseState = Unchecked.defaultof<MouseState>

    let clearColor = Option.map xnaColor config.clearColour
    let defaultBounds = 0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight
    
    let mutable fonts = Map.empty<string, SpriteFont>
    let mutable textures = Map.empty<string, Texture2D>
    //let mutable whiteTexture: Texture2D = null

    do 
        match config.resolution with
        | Windowed (w,h) -> 
            graphics.PreferredBackBufferWidth <- w
            graphics.PreferredBackBufferHeight <- h

        this.IsMouseVisible <- config.mouseVisible
        graphics.SynchronizeWithVerticalRetrace <- true
        this.IsFixedTimeStep <- false 
        
    member __.View
        with set value = 
            view <- value

    override __.LoadContent () = 
        spriteBatch <- new SpriteBatch (this.GraphicsDevice)
        config.assetsToLoad
        |> List.iter (
            function
            | Texture (key, path) -> 
                use stream = File.OpenRead path
                let texture = Texture2D.FromStream (this.GraphicsDevice, stream)
                textures <- Map.add key texture textures
            | Font (key, path) -> 
                let font = this.Content.Load<SpriteFont> path
                fonts <- Map.add key font fonts)

    override __.Update _ =
        keyboardState <- Keyboard.GetState ()
        mouseState <- Mouse.GetState ()

    override __.Draw gameTime =
        Option.iter this.GraphicsDevice.Clear clearColor
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)

        match view with
        | None -> __.Exit ()
        | Some v -> 
            let drawState = { 
                gameTime = gameTime
                keyboardState = keyboardState
                mouseState = mouseState 
                spriteBatch = spriteBatch
                textures = textures
                fonts = fonts }
            List.iter (renderViewable drawState defaultBounds) v

        spriteBatch.End ()