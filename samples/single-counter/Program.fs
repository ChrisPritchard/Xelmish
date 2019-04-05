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

let view model dispatch =
    let textStyle = { font = "connection"; colour = rgb 255uy 255uy 255uy }
    let buttonStyle = { font = "connection"; colour = rgb 0uy 0uy 0uy; backColour = rgb 200uy 200uy 200uy }
    [
        Position (100, 50, 400, 50, 
            Text (textStyle, sprintf "Counter value: %i" model.Count))

        Window (100, 100, 600, 200, [
            Row [
                Button (buttonStyle, "- counter", fun () -> dispatch Decrement)
                Button (buttonStyle, "+ counter", fun () -> dispatch Increment)
            ]
            Text (textStyle, sprintf "Step size: %i" model.StepSize)
            Row [
                Button (buttonStyle, "- step size", fun () -> dispatch (SetStepSize (model.StepSize - 1)))
                Button (buttonStyle, "+ step size", fun () -> dispatch (SetStepSize (model.StepSize + 1)))
            ]
            Button (buttonStyle, "reset", fun () -> dispatch Reset)
        ])
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