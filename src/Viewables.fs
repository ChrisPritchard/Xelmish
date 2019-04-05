module Xelmish.Viewables

open Model

type Viewable = { kind: ViewableType; rect: Rectangle; children: Viewable list }
and ViewableType =
    | Colour of colour: Colour
    | Image of texture: string * colour: Colour
    | Text of font: string * text: string * colour: Colour
    | Clickable of onClick: (unit -> unit)

let bs = 2 // possibly have a mutable style map?
    
let button onClick (text: string) =
    fun x y font fc bc drawState ->
        let spriteFont = drawState.fonts.[font]
        let textSize = spriteFont.MeasureString (text)
        let innerText = { 
            kind = Text (font, text, fc)
            rect = rect (x + bs) (y + bs) (int textSize.X) (int textSize.Y)
            children = []
        }
        { 
            kind = Colour bc
            rect = rect x y (int textSize.X + bs*2) (int textSize.Y + bs*2)
            children = [ innerText ] 
        }

let renderView spriteBatch gameTime gameState view =
    
    let rec processItems sx sy sf items =
        let mutable x, y, font = sx, sy, sf
        for item in items do
            ()

    processItems 0 0 None view
        
    // for each item, derive new position
    // position can be overriden by explicit x, y options

    // [x y w h] becomes set x x; set y y etc. then contents gets rendered at that position. x y then gets appended by parent
    // set x; set y; render; add w to x if row else add h to y if column