[<RequireQualifiedAccess>]
module Xelmish.Program

open Elmish
open XnaCore

let runGameLoop config (program: Program<_, _, _, 'view>) =
    use loop = new GameLoop<'view> (config)
    let setState model dispatch =
        loop.View <- Some (program.view model dispatch)
    Program.run { program with setState = setState }
    loop.Run ()