[<RequireQualifiedAccess>]
module Xelmish.Program

open Elmish
open GameLoop
open Model

let runGameLoop config (program: Program<_, _, _, Viewable list>) =
    use loop = new GameLoop (config)
    let setState model dispatch =
        loop.View <- program.view model dispatch
    Program.run { program with setState = setState }
    loop.Run ()