[<RequireQualifiedAccess>]
module Xelmish.Program

let runGameLoop program =
    use loop = new GameLoop()
    loop.Run ()