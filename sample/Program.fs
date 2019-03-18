open System
open Elmish
open System.Threading

type Model = {
    count: int
}

type Message = 
    | Increment
    | Reset

let update message model = 
    match message with
    | Increment -> { model with count = model.count + 1 }, Cmd.none
    | Reset -> { model with count = 0 }, Cmd.none

let init () =
    { count = 0 }, Cmd.none

let mutable timerInstance = Unchecked.defaultof<Timer>

let timer _ =
    Cmd.ofSub (fun dispatch -> 
        timerInstance <- 
            let callback _ = dispatch Increment
            new Timer(TimerCallback(callback), null, 0, 500))

let view model dispatch = printfn "%A" model

[<EntryPoint>]
let main _ =
    
    Program.mkProgram init update view 
    |> Program.withSubscription timer
    |> Program.run

    0
