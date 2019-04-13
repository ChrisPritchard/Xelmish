
type Model = {
    playerX: int
    invaders: (int * int) list
    projectiles: (int * int * int) list
}

let init () = {
    playerX = 0
    invaders = []
    projectiles = []
}

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0
