[<RequireQualifiedAccess>]
module Xelmish.Program

open Elmish
open XnaCore
open Xelmish.Viewables

let runGameLoop config (program: Program<_, _, _, Viewable list>) =
    use loop = new GameLoop (config)
    let setState model dispatch =
        loop.View <- Some (program.view model dispatch)
    Program.run { program with setState = setState }
    loop.Run ()