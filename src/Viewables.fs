module Xelmish.Viewables

open Model

type Viewable = { kind: ViewableType; rect: Rectangle; children: Viewable list }
and ViewableType =
    | Colour of colour: Colour
    | Image of texture: string * colour: Colour
    | Text of font: string * text: string * colour: Colour
    | Clickable of onClick: (unit -> unit)

type Style = {
    font: string
    foreColour: Colour
    backColour: Colour
    borderWidth: int
}

let text (text: string) =
    fun x y style drawState ->
        let spriteFont = drawState.fonts.[style.font]
        let textSize = spriteFont.MeasureString (text)
        { 
            kind = Text (style.font, text, style.foreColour)
            rect = rect x y (int textSize.X) (int textSize.Y)
            children = []
        }

let button onClick (text: string) =
    fun x y style drawState ->
        let spriteFont = drawState.fonts.[style.font]
        let textSize = spriteFont.MeasureString (text)
        let bs = style.borderWidth
        let innerText = { 
            kind = Text (style.font, text, style.foreColour)
            rect = rect (x + bs) (y + bs) (int textSize.X) (int textSize.Y)
            children = []
        }
        { 
            kind = Clickable onClick
            rect = rect x y (int textSize.X + bs*2) (int textSize.Y + bs*2)
            children = [
                { 
                    kind = Colour style.backColour
                    rect = rect x y (int textSize.X + bs*2) (int textSize.Y + bs*2)
                    children = [ innerText ] 
                }
            ]
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