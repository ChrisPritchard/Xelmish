module internal Xelmish.XnaCore

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Model
open Viewables

type GameLoop (config: GameConfig) as this = 
    inherit Game ()

    let graphics = new GraphicsDeviceManager (this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable view: (Viewable list) option = None

    let clearColor = Option.map xnaColor config.clearColour
    let defaultBounds = 0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight
    
    let mutable gameState = {
        keyboardState = Unchecked.defaultof<KeyboardState>
        mouseState = Unchecked.defaultof<MouseState>
        lastMouseState = Unchecked.defaultof<MouseState>
        textures = Map.empty<string, Texture2D>
        whiteTexture = Unchecked.defaultof<Texture2D>
        fonts = Map.empty<string, SpriteFont>
    }

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
        spriteBatch <- new SpriteBatch(graphics.GraphicsDevice)
        gameState <- { gameState with whiteTexture = new Texture2D(this.GraphicsDevice, 1, 1) }
        gameState.whiteTexture.SetData<Color> [|Color.White|]

        let (textures, fonts) =
            ((Map.empty, Map.empty), config.assetsToLoad)
            ||> List.fold (fun (textures, fonts) ->
                function
                | Texture (key, path) -> 
                    use stream = File.OpenRead path
                    let texture = Texture2D.FromStream (this.GraphicsDevice, stream)
                    Map.add key texture textures, fonts
                | Font (key, path) -> 
                    let font = this.Content.Load<SpriteFont> path
                    textures, Map.add key font fonts)
        gameState <- { gameState with textures = textures; fonts = fonts }

    override __.Update _ =
        gameState <- 
            { gameState with 
                keyboardState = Keyboard.GetState ()
                lastMouseState = gameState.mouseState
                mouseState = Mouse.GetState () }

    override __.Draw gameTime =
        Option.iter this.GraphicsDevice.Clear clearColor
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)

        match view with
        | None -> __.Exit ()
        | Some v -> List.iter (renderViewable spriteBatch gameTime gameState defaultBounds) v

        spriteBatch.End ()