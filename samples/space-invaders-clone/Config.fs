/// Constants and loaded configuration (e.g. the spritemap) are specified in here
module Config

let resWidth = 800
let resHeight = 600

let padding = 30

let invaderSpacing = 12
let invadersPerRow = 11
let invaderRows = 5
let invaderShuffleAmount = 10
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

let smallSize = let (w, h, _, _) = spritemap.["invader-small-0"] in w * sizeMulti, h * sizeMulti
let mediumSize = let (w, h, _, _) = spritemap.["invader-medium-0"] in w * sizeMulti, h * sizeMulti
let largeSize = let (w, h, _, _) = spritemap.["invader-large-0"] in w * sizeMulti, h * sizeMulti

let animationSpeed = 100L

let playerWidth, playerHeight = let (w, h, _, _) = spritemap.["player"] in w * sizeMulti, h * sizeMulti
let playerY = resHeight - (playerHeight + padding)
let playerSpeed = 5