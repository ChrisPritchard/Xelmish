// this component represents the player

module Player

open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

type Model = {
    x: int
    laser: (int * int) option
    state: PlayerState
}
and PlayerState = Alive | Dying of int

let init () = {
        x = resWidth / 2 - (playerWidth / 2)
        laser = None
        state = Alive
    }

type Message = 
    | Move of dir: int
    | Shoot
    | MoveLaser

let leftClamp, rightClamp = padding, resWidth - padding - playerWidth

let update message model =
    match message with
    | Move dir ->
        let projected = model.x + dir * playerSpeed
        let newPos = min rightClamp (max leftClamp projected)
        { model with x = newPos }
    | Shoot -> 
        match model.laser with
        | Some _ -> model
        | None ->
            let pos = model.x + playerWidth / 2, resHeight - (playerHeight + padding) - projectileHeight - 1
            { model with laser = Some pos }
    | MoveLaser ->
        match model.laser with
        | None -> model
        | Some (x, y) ->
            let (nx, ny) = x, y - playerProjectileSpeed
            if ny < 0 then { model with laser = None }
            else { model with laser = Some (nx, ny) }

let view model dispatch =
    match model.state with
    | Alive ->
        [
            yield sprite spritemap.["player"] (playerWidth, playerHeight) (model.x, playerY) Colour.White

            match model.laser with
            | Some (x, y) -> 
                yield colour Colour.White (projectileWidth, projectileHeight) (x, y)
            | _ -> 
                if model.state = Alive then yield onkeydown Keys.Space (fun () -> dispatch Shoot)

            if model.state = Alive then

                yield onupdate (fun _ -> if model.laser <> None then dispatch MoveLaser)

                yield whilekeydown Keys.Left (fun () -> dispatch (Move -1))
                yield whilekeydown Keys.A (fun () -> dispatch (Move -1))
                yield whilekeydown Keys.Right (fun () -> dispatch (Move 1))
                yield whilekeydown Keys.D (fun () -> dispatch (Move 1))
        ]
    | Dying _ ->
        [
            match model.laser with
            | Some (x, y) -> 
                yield colour Colour.White (projectileWidth, projectileHeight) (x, y)
            | _ -> ()

            yield sprite spritemap.["player-death"] (playerWidth, playerHeight) (model.x, playerY) Colour.White
        ]