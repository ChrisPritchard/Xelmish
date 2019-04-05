module Xelmish.Viewables

open Model

type Viewable = 
    | Colour of colour: Colour * rect: Rectangle * children: Viewable
    | Image of texture: string * colour: Colour * rect: Rectangle * children: Viewable
    | Text of font: string * text: string * colour: Colour * rect: Rectangle
    | Clickable of onClick: (unit -> unit) * rect: Rectangle * children: Viewable
    
let backgroundColour = rgb 150uy 150uy 150uy

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