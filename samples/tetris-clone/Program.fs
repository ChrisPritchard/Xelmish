
open System
open Xelmish.Model

type Model =
| StartPage
| Playing of PlayingState
| GameOver of score:int
| Quit
and PlayingState = {
    grid: Colour[,]
    block: (int * int) list * Colour
    score: int
}

type Message = 
| Tick
| Drop
| Left
| Right
| Rotate
| RemoveLine

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
