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
        yield text 30. Colours.black (0., 0.) (sprintf "Counter value: %i" model.Count) (100, 60)

        yield! 
            stack 100 [
                row 220 [
                    button "- counter" (fun () -> dispatch Decrement) (100, 20)
                    button "+ counter" (fun () -> dispatch Increment) (100, 20)
                ]
                fun p -> [text 20. Colours.black (0., 0.) (sprintf "Step size: %i" model.StepSize) p]
                row 220 [
                    button "- step size" (fun () -> dispatch (SetStepSize (model.StepSize - 1))) (100, 20)
                    button "+ step size" (fun () -> dispatch (SetStepSize (model.StepSize + 1))) (100, 20)
                ]
                button "reset" (fun () -> dispatch Reset) (100, 20)
            ] (100, 100)
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