open System
open Elmish
open Xelmish.Model
open Xelmish.Viewables

let text = text "connection"
let button s event (width, height) (x, y) = [
    colour Colours.blue (width, height) (x, y)
    text 16. Colours.white (-0.5, -0.5) s (x + width/2, y+height/2)
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

    let view model dispatch =
        let timeFormat = fun (date: DateTime) -> 
            System.String.Format("Today is {0:MMMM dd, yyyy}. The time is {0:HH:mm:ssK}. It is {0:dddd}.", date)
        let timeString = 
            if model.UseUtc then timeFormat model.Time.UtcDateTime 
            else timeFormat model.Time.LocalDateTime
        [
            yield text 20. Colours.white (0., 0.) timeString (0, 0)
            yield! button "Toggle UTC" (fun () -> dispatch ToggleUtc) (100, 20) (200, 0)
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

  //let bindings () =
  //  [
  //    "CounterValue" |> Binding.oneWay (fun m -> m.Count)
  //    "Increment" |> Binding.cmd (fun m -> Increment)
  //    "Decrement" |> Binding.cmd (fun m -> Decrement)
  //    "StepSize" |> Binding.twoWay 
  //      (fun m -> float m.StepSize)
  //      (fun v m -> int v |> SetStepSize)
  //    "Reset" |> Binding.cmdIf
  //      (fun m -> Reset)
  //      (fun m ->
  //        let i = init ()
  //        m.Count <> i.Count || m.StepSize <> i.StepSize
  //      )
  //    "Clock" |> Binding.subModel (fun m -> m.Clock) Clock.bindings ClockMsg
  //  ]

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