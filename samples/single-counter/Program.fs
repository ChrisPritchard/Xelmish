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
    let button s event (x, y) = 
        let width, height = 100, 30 
        [
            colour Colour.Blue (width, height) (x, y)
            text 20. Colour.White (-0.5, -0.5) s (x + width/2, y+height/2)
            onclick event (width, height) (x, y)
        ]

    [
        yield text 30. Colour.Black (0., 0.) (sprintf "Counter value: %i" model.Count) (100, 60)
        yield! button "- counter" (fun () -> dispatch Decrement) (100, 100)
        yield! button "+ counter" (fun () -> dispatch Increment) (220, 100)
        yield text 20. Colour.Black (0., 0.) (sprintf "Step size: %i" model.StepSize) (100, 140)
        yield! button "- step size" (fun () -> dispatch (SetStepSize (model.StepSize - 1))) (100, 180)
        yield! button "+ step size" (fun () -> dispatch (SetStepSize (model.StepSize + 1))) (220, 180)
        yield! button "reset" (fun () -> dispatch Reset) (100, 220)

        yield onkeydown Keys.Escape exit
    ]

[<EntryPoint>]
let main _ =
    let config = {
        resolution = Windowed (420, 300)
        clearColour = Some Colour.White
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
        showFpsInConsole = true
    }

    Program.mkSimple init update view
    |> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0