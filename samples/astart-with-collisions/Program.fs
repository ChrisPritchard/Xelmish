open System
open Elmish
open Xelmish.Model 
open Xelmish.Viewables 

type Model = 
    { 
        tileLayer: Tiles.tileLayer 
    } 

let init () = 
    let rndMap cols rows = 
        [|
            for y = 0 to rows - 1 do 
                for x = 0 to cols - 1 do 
                    yield Random.Shared.Next(0, 2) // [0, 2) or [0, 1]. 
        |]
    { 
        tileLayer = 
            Tiles.TileLayer.fromArea 100 100 (0, 0) (600, 600) (rndMap 100 100)
    }, Cmd.none

type Message =
    | E

let update message model =
    match message with
    | E -> model, Cmd.none

let view model dispatch =
    [
        Tiles.TileLayer.renderTileLayerColor (function | 0 -> Colour.Transparent | _ -> Colour.Aqua) model.tileLayer
        onkeydown Keys.Escape exit
    ]

Program.mkProgram init update view
|> Xelmish.Program.runSimpleGameLoop [] (600, 600) Colour.Black