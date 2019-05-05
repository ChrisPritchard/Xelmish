module Xelmish.Helpers

open System

/// function used to display a primitive FPS counter in the console out
let printFps fps =
    let left, top, fore, back, shown = 
        Console.CursorLeft, Console.CursorTop,
        Console.ForegroundColor, Console.BackgroundColor,
        Console.CursorVisible

    Console.CursorLeft <- 0
    Console.CursorTop <- 0
    Console.ForegroundColor <- ConsoleColor.Black
    Console.BackgroundColor <- ConsoleColor.White
    Console.CursorVisible <- false

    printf " FPS: %d " fps

    Console.CursorLeft <- left
    Console.CursorTop <- top
    Console.ForegroundColor <- fore
    Console.BackgroundColor <- back

    if (left, top) <> (0, 0) then 
        Console.CursorVisible <- shown