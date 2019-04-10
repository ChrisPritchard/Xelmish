
open System
open Xelmish.Model

type Model =
| StartPage
| Playing of PlayingState
| GameOver of score:int
| Quit
and PlayingState = {
    grid: Colour[,]
    block: (int * int) * Shape * int
    score: int
}
and Shape = {
    positions: ((int * int) list) []
    colour: Colour
}

let shapes = [
    {
        positions = 
            [|
                [0,0; 1,0; 0,1; 1,1]
            |]
        colour = Colours.cyan
    }
    {
        positions = 
            [|
                [0,0; 1,0; 2,0; 3,0]
                [2,0; 2,1; 2,2; 2,3]
            |]
        colour = Colours.red
    }
    {
        positions =
            [|
                [0,0; 1,0; 1,1; 2,1]
                [2,0; 2,1; 1,1; 1,2]
            |] 
        colour = Colours.green
    }
    {
        positions =
            [|
                [1,0; 2,0; 1,1; 0,1]
                [1,0; 1,1; 2,1; 2,2]
            |] 
        colour = Colours.blue
    }
    {
        positions =
            [|
                [0,0; 1,0; 2,0; 0,1]
                [1,0; 1,1; 1,2; 2,2]
            |] 
        colour = Colours.yellow
    }
    {
        positions =
            [|
                [0,0; 1,0; 2,0; 2,1]
                [1,0; 1,1; 1,2; 2,0]
            |] 
        colour = Colours.magenta
    }
    {
        positions = 
            [|
                [0,0; 1,0; 2,0; 1,1]
                [1,0; 1,1; 1,2; 0,1]
                [0,1; 1,1; 2,1; 1,0]
                [1,0; 1,1; 1,2; 2,1]
            |]
        colour = Colours.magenta
    }
]

type Message = 
| Tick
| Drop
| Left
| Right
| Rotate

let tilesFor ((x, y), shape, rotationIndex) =
    shape.positions.[rotationIndex] 
    |> List.map (fun (dx, dy) -> x + dx, y + dy)

let moveShape (dx, dy) model =
    match model with
    | Playing state ->
        // change position
        // calculate tiles
        // if out of bounds, ignore
        // if overlap, add prior position to grid and spawn new
        // else set new position
        model
    | _ -> model

let rotateShape model =
    match model with
    | Playing state ->
        // change rotationIndex
        // calculate tiles
        // if out of bounds, ignore
        // if overlap, add prior position to grid and spawn new
        // else set new position
        model
    | _ -> model

let update message model =
    match message with
    | Tick | Drop -> moveShape (0, 1) model
    | Left -> moveShape (-1, 0) model
    | Right -> moveShape (1, 0) model
    | Rotate -> rotateShape model

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
