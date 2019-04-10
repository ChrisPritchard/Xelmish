
open System
open Xelmish.Model

type Model = {
    state: State
} and State =
    | StartPage
    | Playing of Playing.Model
    | GameOver of score:int
    | Quit

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
