[<RequireQualifiedAccess>]
module Xelmish.Program

open Elmish
open GameLoop
open Model

let runGameLoop config (program: Program<_, _, _, (Viewable list) * GameMessage>) =
    use loop = new GameLoop (config)
    let setState model dispatch =
        let view, message = program.view model dispatch
        match message with
        | Exit -> loop.Exit ()
        | NoOp -> loop.View <- view
    Program.run { program with setState = setState }
    loop.Run ()