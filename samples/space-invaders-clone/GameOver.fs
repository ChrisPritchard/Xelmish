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
    wasVictory: bool
}

let init wasVictory score =
    let highScore = 
        if File.Exists highScoreFile 
        then Some (int (File.ReadAllText highScoreFile))
        else None
    let newHighScore = match highScore with Some i when i >= score -> false | _ -> true
    if newHighScore then
        File.WriteAllText (highScoreFile, score.ToString())
    { highScore = highScore; score = score; newHighScore = newHighScore; wasVictory = wasVictory }

type Message = 
    | StartGame

let view model dispatch = 
    let centredText colour = text "PressStart2P" 24. colour (-0.5, 0.)
    let textMid = resWidth / 2
    [
        if not model.wasVictory then
            yield centredText Colour.Red "GAME  OVER!" (textMid, 90)
            yield centredText Colour.OrangeRed "YOU  FAILED  TO  FEND  OFF  THE  INVADERS  :(" (textMid, 130)
        else
            yield centredText (rgba 0uy 255uy 0uy 255uy) "VICTORY!" (textMid, 90)
            yield centredText Colour.OrangeRed "YOU  DEFEATED  THE  INVASION!" (textMid, 130)

        yield centredText Colour.White "SCORE" (textMid, 200)
        yield centredText Colour.White (sprintf "%04i" model.score) (textMid, 240)

        if not model.newHighScore then
            match model.highScore with
            | Some score -> 
                yield centredText Colour.Cyan "HIGH  SCORE" (textMid, 280)
                yield centredText Colour.Cyan (sprintf "%04i" score) (textMid, 320)
            | _ -> ()
        else
            yield centredText Colour.Cyan "NEW  HIGH  SCORE!" (textMid, 280)

        yield centredText Colour.OrangeRed "(P)LAY  AGAIN" (textMid, 450)
        yield centredText Colour.OrangeRed "(Q)UIT" (textMid, 480)

        yield onkeydown Keys.P (fun () -> dispatch StartGame)
        yield onkeydown Keys.Q exit
        yield onkeydown Keys.Escape exit
    ]