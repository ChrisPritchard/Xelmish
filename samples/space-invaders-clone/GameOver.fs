module GameOver

open System
open System.IO
open Common
open Xelmish.Viewables
open Xelmish.Model

type Model = {
    highScore: int option
    score: int
    newHighScore: bool
}

let init score =
    let highScore = 
        if File.Exists highScoreFile 
        then Some (int (File.ReadAllText highScoreFile))
        else None
    let newHighScore = match highScore with Some i when i >= score -> false | _ -> true
    if newHighScore then
        File.WriteAllText (highScoreFile, score.ToString())
    { highScore = highScore; score = score; newHighScore = newHighScore }

type Message = 
    | StartGame

let view model dispatch = 
    let centredText colour = text "PressStart2P" 24. colour (-0.5, 0.)
    let textMid = resWidth / 2
    [
        yield centredText Colour.OrangeRed "GAME  OVER!" (textMid, 90)

        yield centredText Colour.White "SCORE" (textMid, 180)
        yield centredText Colour.White (sprintf "%04i" model.score) (textMid, 210)

        if not model.newHighScore then
            match model.highScore with
            | Some score -> 
                yield centredText Colour.Cyan "HIGH  SCORE" (textMid, 260)
                yield centredText Colour.Cyan (sprintf "%04i" score) (textMid, 290)
            | _ -> ()
        else
            yield centredText Colour.Cyan "NEW  HIGH  SCORE!" (textMid, 260)

        yield centredText Colour.OrangeRed "(P)LAY  AGAIN" (textMid, 450)
        yield centredText Colour.OrangeRed "(Q)UIT" (textMid, 480)

        yield onkeydown Keys.P (fun () -> dispatch StartGame)
        yield onkeydown Keys.Q exit
        yield onkeydown Keys.Escape exit
    ]