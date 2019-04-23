module internal Xelmish.GameLoop

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Model
open Helpers

type GameLoop (config: GameConfig) as this = 
    inherit Game ()

    let graphics = new GraphicsDeviceManager (this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable assets = Unchecked.defaultof<LoadedAssets>
    let mutable inputs = {
        keyboardState = Unchecked.defaultof<KeyboardState>
        lastKeyboardState = Unchecked.defaultof<KeyboardState>
        mouseState = Unchecked.defaultof<MouseState>
        lastMouseState = Unchecked.defaultof<MouseState>
        gameTime = Unchecked.defaultof<GameTime>
    }
    
    let mutable fps = 0
    let mutable lastFpsUpdate = 0.
    let fpsUpdateInterval = 200.

    let mutable view: Viewable list = []

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
        let whiteTexture = new Texture2D(this.GraphicsDevice, 1, 1)
        whiteTexture.SetData<Color> [|Color.White|]

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
        assets <- { textures = textures; fonts = fonts; whiteTexture = whiteTexture }

    override __.Update gameTime =
        inputs <- 
            {   lastKeyboardState = inputs.keyboardState
                keyboardState = Keyboard.GetState ()
                lastMouseState = inputs.mouseState
                mouseState = Mouse.GetState ()
                gameTime = gameTime }

    override __.Draw gameTime =
        Option.iter this.GraphicsDevice.Clear config.clearColour
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)

        try
            for viewable in view do
                viewable assets inputs spriteBatch
        with
            | :? QuitGame -> __.Exit()

        spriteBatch.End ()
        
        if config.showFpsInConsole then 
            if gameTime.TotalGameTime.TotalMilliseconds - lastFpsUpdate > fpsUpdateInterval then
                fps <- int (1. / gameTime.ElapsedGameTime.TotalSeconds)
                lastFpsUpdate <- gameTime.TotalGameTime.TotalMilliseconds
                printFps fps