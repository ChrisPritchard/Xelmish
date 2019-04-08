open System
open Elmish
open Xelmish.Model
open Xelmish.Viewables

let text = text "connection"
let button s event (x, y) = 
    let width, height = 100, 30 
    [
        colour Colours.blue (width, height) (x, y)
        text 20. Colours.white (-0.5, -0.5) s (x + width/2, y+height/2)
        clickable event (width, height) (x, y)
    ]

module Clock =

    type Model =
        {   Time: DateTimeOffset
            UseUtc: bool }

    let init () =
        {   Time = DateTimeOffset.Now
            UseUtc = false }

    type Msg =
        | Tick of DateTimeOffset
        | ToggleUtc

    let update msg m =
        match msg with
        | Tick t -> { m with Time = t }
        | ToggleUtc -> { m with UseUtc = not m.UseUtc }

    let view model dispatch (x, y) =
        let timeFormat = fun (date: DateTime) -> 
            System.String.Format("Today is {0:MMMM dd, yyyy}. The time is {0:HH:mm:ssK}. It is {0:dddd}.", date)
        let timeString = 
            if model.UseUtc then timeFormat model.Time.UtcDateTime 
            else timeFormat model.Time.LocalDateTime
        [
            yield text 20. Colours.white (0., 0.) timeString (x, y)
            yield! button "Toggle UTC" (fun () -> dispatch ToggleUtc) (x + 200, y)
        ]

module CounterWithClock =

    type Model =
        {   Count: int
            StepSize: int
            Clock: Clock.Model }

    let init () =
        {   Count = 0
            StepSize = 1
            Clock = Clock.init () }

    type Msg =
        | Increment
        | Decrement
        | SetStepSize of int
        | Reset
        | ClockMsg of Clock.Msg

    let update msg m =
        match msg with
        | Increment -> { m with Count = m.Count + m.StepSize }
        | Decrement -> { m with Count = m.Count - m.StepSize }
        | SetStepSize x -> { m with StepSize = x }
        | Reset -> { m with Count = 0; StepSize = 1 }
        | ClockMsg msg -> { m with Clock = Clock.update msg m.Clock }

    let view model dispatch (x, y) =
        [
            yield text 30. Colours.black (0., 0.) (sprintf "Counter value: %i" model.Count) (x, y)
            yield! button "- counter" (fun () -> dispatch Decrement) (x, y + 40)
            yield! button "+ counter" (fun () -> dispatch Increment) (x + 120, y + 40)
            yield text 20. Colours.black (0., 0.) (sprintf "Step size: %i" model.StepSize) (x, y + 80)
            yield! button "- step size" (fun () -> dispatch (SetStepSize (model.StepSize - 1))) (x, y + 120)
            yield! button "+ step size" (fun () -> dispatch (SetStepSize (model.StepSize + 1))) (x + 120, y + 120)
            yield! button "reset" (fun () -> dispatch Reset) (x, y + 140)
            yield! Clock.view model.Clock dispatch (x, y + 180)
        ]

module App =

    type Model =
        {   ClockCounter1: CounterWithClock.Model
            ClockCounter2: CounterWithClock.Model }

    let init () =
        {   ClockCounter1 = CounterWithClock.init ()
            ClockCounter2 = CounterWithClock.init () }

    type Msg =
        | ClockCounter1Msg of CounterWithClock.Msg
        | ClockCounter2Msg of CounterWithClock.Msg

    let update msg m =
        match msg with
        | ClockCounter1Msg msg ->
            { m with ClockCounter1 = CounterWithClock.update msg m.ClockCounter1 }
        | ClockCounter2Msg msg ->
            { m with ClockCounter2 = CounterWithClock.update msg m.ClockCounter2 }

    //let bindings model dispatch =
    //    [
    //        "ClockCounter1" |> Binding.subModel
    //        (fun m -> m.ClockCounter1) CounterWithClock.bindings ClockCounter1Msg
    //        "ClockCounter2" |> Binding.subModel
    //        (fun m -> m.ClockCounter2) CounterWithClock.bindings ClockCounter2Msg
    //    ]

let timerTick dispatch =
    let timer = new System.Timers.Timer(1000.)
    timer.Elapsed.Add (fun _ -> 
    let clockMsg =
        DateTimeOffset.Now
        |> Clock.Tick
        |> CounterWithClock.ClockMsg
    dispatch <| App.ClockCounter1Msg clockMsg
    dispatch <| App.ClockCounter2Msg clockMsg)
    timer.Start()
  
[<EntryPoint; STAThread>]
let main _ =
    let config = {
        resolution = Windowed (800, 600)
        clearColour = Some (rgb 200uy 200uy 200uy)
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
    }

    Program.mkSimple App.init App.update App.view
    |> Program.withSubscription (fun m -> Cmd.ofSub timerTick)
    |> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0