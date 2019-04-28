/// Constants and loaded configuration (e.g. the spritemap) are specified in here
module Config

open Xelmish.Model

let resWidth = 800
let resHeight = 600

let padding = 30

let invaderSpacing = 12
let invadersPerRow = 11
let invaderRows = 5
let invaderShuffleAmount = 20
let invaderShuffleIncrease = 25L

let projectileHeight = 10
let projectileSpeed = 6

let spritemap = 
    System.IO.File.ReadAllLines "./spritemap.txt"
    |> Array.map (fun line -> 
        let sa = line.Split ([|'\t';','|], System.StringSplitOptions.RemoveEmptyEntries)
        sa.[0], (int sa.[1], int sa.[2], int sa.[3], int sa.[4]))
    |> Map.ofArray

let sizeMulti = 2

type InvaderKind = {
    width: int
    height: int
    offset: int
    colour: Colour
    animations: (int * int * int * int) []
}

let largeSize = 
    let width, height = 
        let w, h, _, _ = spritemap.["invader-large-0"]
        w * sizeMulti, h * sizeMulti
    {   width = width
        height = height
        offset = 0
        colour = Colour.OrangeRed
        animations = [|
                spritemap.["invader-large-0"]
                spritemap.["invader-large-1"]
            |] }

let mediumSize = 
    let width, height = 
        let w, h, _, _ = spritemap.["invader-medium-0"]
        w * sizeMulti, h * sizeMulti
    {   width = width
        height = height
        offset = (largeSize.width - width) / 2
        colour = Colour.Gold
        animations = [|
                spritemap.["invader-medium-0"]
                spritemap.["invader-medium-1"]
            |] }

let smallSize = 
    let width, height = 
        let w, h, _, _ = spritemap.["invader-small-0"]
        w * sizeMulti, h * sizeMulti
    {   width = width
        height = height
        offset = (largeSize.width - width) / 2
        colour = Colour.Cyan
        animations = [|
                spritemap.["invader-small-0"]
                spritemap.["invader-small-1"]
            |] }

let animationSpeed = 100L

let playerWidth, playerHeight = 
    let (w, h, _, _) = spritemap.["player"]
    w * sizeMulti, h * sizeMulti

let playerY = resHeight - (playerHeight + padding)
let playerSpeed = 5

let explosionWidth, explosionHeight = 
    let (w, h, _, _) = spritemap.["invader-death"]
    w * sizeMulti, h * sizeMulti
let explosionDuration = 1