open Elmish
open Xelmish.Model 
open Xelmish.Viewables 

type Model = 
    { 
        T: int 
    } 

let init () = 
    { 
        T = 0
    }, Cmd.none

type Message =
    | E

let update message model =
    match message with
    | E -> model, Cmd.none

let view model dispatch =
    [
        onkeydown Keys.Escape exit
    ]

Program.mkProgram init update view
|> Xelmish.Program.runSimpleGameLoop [] (600, 600) Colour.Black