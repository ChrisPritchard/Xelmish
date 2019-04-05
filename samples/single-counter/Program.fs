open Elmish
open Xelmish.Model
open Xelmish.Viewables

type Model =
  { Count: int
    StepSize: int }

let init () =
  { Count = 0
    StepSize = 1 }

type Msg =
  | Increment
  | Decrement
  | SetStepSize of int
  | Reset

let update msg m =
  match msg with
  | Increment -> { m with Count = m.Count + m.StepSize }
  | Decrement -> { m with Count = m.Count - m.StepSize }
  | SetStepSize x -> { m with StepSize = x }
  | Reset -> init ()

//let view model dispatch =
//    [
//        Position (100, 50, 400, 50, 
//            Text ("connection", sprintf "Counter value: %i" model.Count))

//        Window (100, 100, 600, 200, [
//            Row [
//                Button ("connection", "- counter", fun () -> dispatch Decrement)
//                Button ("connection", "+ counter", fun () -> dispatch Increment)
//            ]
//            Text ("connection", sprintf "Step size: %i" model.StepSize)
//            Row [
//                Button ("connection", "- step size", fun () -> dispatch (SetStepSize (model.StepSize - 1)))
//                Button ("connection", "+ step size", fun () -> dispatch (SetStepSize (model.StepSize + 1)))
//            ]
//            Button ("connection", "reset", fun () -> dispatch Reset)
//        ])
//    ]

let view model dispatch =
    [
        text [ font "connection"; x 100; y 50; width 400; height 50 ] [ rawText (sprintf "Counter value: %i" model.Count)) ]
            
        column [ font "connection"; x 100; y 100; width 600; height 200 ] [
            row [] [ 
                button [ click (fun () -> dispatch Decrement) ] [ rawText "- counter" ]
                button [ click (fun () -> dispatch Increment) ] [ rawText "+ counter" ]
            ]
            text [] [ rawText (sprintf "Step size: %i" model.StepSize) ]
            row [] [ 
                button [ click (fun () -> dispatch (SetStepSize (model.StepSize - 1))) ] [ rawText "- step size" ]
                button [ click (fun () -> dispatch (SetStepSize (model.StepSize + 1))) ] [ rawText "+ step size" ]
            ]
            button [ click (fun () -> dispatch Reset) ] [ rawText "reset" ]
        ]
    ]

[<EntryPoint>]
let main _ =
    let config = {
        resolution = Windowed (800, 600)
        clearColour = Some (rgb 255uy 0uy 0uy)
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
    }

    Program.mkSimple init update view
    |> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0