module Xelmish.Viewables

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type Viewable = 
    | Column of Attribute list * Viewable list
    | Row of Attribute list * Viewable list
    | Text of Attribute list * string
    | Clickable of (unit -> unit) * Viewable

let button attr contents =
    Colour (rgb 200uy 200uy 200uy)

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