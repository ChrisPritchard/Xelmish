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
    [
        Window (100, 100, 600, 400, [
            Text (sprintf "Counter value: %i" model.Count)
            Row [
                Button ("- counter", fun () -> dispatch Decrement)
                Button ("+ counter", fun () -> dispatch Increment)
            ]
            Text (sprintf "Step size: %i" model.StepSize)
            Row [
                Button ("- step size", fun () -> dispatch (SetStepSize (model.StepSize - 1)))
                Button ("+ step size", fun () -> dispatch (SetStepSize (model.StepSize + 1)))
            ]
            Button ("reset", fun () -> dispatch Reset)
        ])
    ]

[<EntryPoint>]
let main _ =
    let config = {
        resolution = Windowed (800, 600)
        clearColour = Some (rgb 255uy 0uy 0uy)
        mouseVisible = true
    }

    Program.mkSimple init update view
    |> Xelmish.Program.runGameLoop config
    0