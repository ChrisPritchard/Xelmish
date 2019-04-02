[<RequireQualifiedAccess>]
module Xelmish.Program

open XnaCore

let runGameLoop program =
    use loop = new GameLoop (program)
    loop.Run ()