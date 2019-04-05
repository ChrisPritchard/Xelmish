module Xelmish.Viewables

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type Renderable =
| Text of x:int * y:int * font:string * fontSize:float * content:string
| Texture of x:int * y:int * w:int * h:int * Texture2D
| Colour of x:int * y:int * w:int * h:int * Colour

let button attr contents =
    Colour (rgb 200uy 200uy 200uy)

let renderView spriteBatch gameTime gameState view =
    // for each item, derive new position
    // position can be overriden by explicit x, y options

    // [x y w h] becomes set x x; set y y etc. then contents gets rendered at that position. x y then gets appended by parent
    // set x; set y; render; add w to x if row else add h to y if column