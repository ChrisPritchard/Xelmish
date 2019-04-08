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

    let text = text "connection"
    let button s event (width, height) (x, y) = [
        colour Colours.blue (width, height) (x, y)
        text 16. Colours.white (-0.5, -0.5) s (x + width/2, y+height/2)
        clickable event (width, height) (x, y)
    ]

    [
        text 30. Colours.black (0., 0.) (sprintf "Counter value: %i" model.Count) (100, 60)
        layout [ Even; Even; Even; Even ] [ Even; Even; ] [
            listCell 0 0 (button "- counter" (fun () -> dispatch Decrement))
            listCell 0 1 (button "+ counter" (fun () -> dispatch Increment))
            textCell 1 0 (text 20. Colours.black (0., 0.) (sprintf "Step size: %i" model.StepSize))
            listCell 2 0 (button "- step size" (fun () -> dispatch (SetStepSize (model.StepSize - 1))))
            listCell 2 1 (button "+ step size" (fun () -> dispatch (SetStepSize (model.StepSize + 1))))
            listCell 3 0 (button "reset" (fun () -> dispatch Reset))
        ] (220, 100) (100, 100)
    ]

[<EntryPoint>]
let main _ =
    let config = {
        resolution = Windowed (420, 300)
        clearColour = Some Colours.white
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
    }

    Program.mkSimple init update view
    |> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0