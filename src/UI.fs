/// This module contains a mini-framework for creating User Interface elements.
/// It is modelled to be similar to the Giraffe View Engine, from the F# Giraffe Framework (itself inspired by Suave)
module Xelmish.UI

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

/// Note: use the col/row/text/button/viewables functions rather than these types directly
type Element = {
    elementType: ElementType
    attributes: Attribute list
}
/// Note: use the col/row/text/button/viewables functions rather than these types directly
and ElementType =
    | Row of children: Element list
    | Column of children: Element list
    | Text of string
    | Button of text: string
    | Viewables of withInfo: (DrawInfo -> Viewable list)
and DrawInfo = { globalStyle: GlobalStyle; x: int; y: int; width: int; height: int }
/// Note: use the onclick/fontname/colour etc functions than these types directly
and Attribute = 
    | GlobalStyle of style: (GlobalStyle -> GlobalStyle)
    | LocalStyle of style: (LocalStyle -> LocalStyle)
    | OnClick of event: (unit -> unit)
/// Note: use the onclick/fontname/colour etc functions than these types directly
and GlobalStyle = {
    fontName: string
    fontSize: float
    alignment: float * float
    colour: Colour
    backgroundColour: Colour
    buttonColours: ButtonColours
    enabled: bool    
}
and ButtonColours = {
    text: Colour
    defaultBackground: Colour
    disabledBackground: Colour
    hoverBackground: Colour option
    pressedBackground: Colour option
}
/// Note: use the onclick/fontname/colour etc functions than these types directly
and LocalStyle = {
    margin: Size
    padding: Size
    borderSize: int
    borderColour: Colour
    width: Size option
    height: Size option
}
/// Note: use the px/pct functions than these types directly
and Size = Percent of float | Pixels of int

/// Specifies a column, with its children distributed vertically
let col attributes children = { elementType = Column children; attributes = attributes }
/// Specifies a row, with its children distributed horizontally
let row attributes children = { elementType = Row children; attributes = attributes }
/// Specifies some text to render
let text attributes s = { elementType = Text s; attributes = attributes } 
/// Specifies a button (background colour with text) to render
let button attributes s = { elementType = Button s; attributes = attributes }
/// Allows drawing something custom (e.g. a set of images or animations) using the derived characteristics
let viewables attributes impl = { elementType = Viewables impl; attributes = attributes }
/// Allows drawing something custom (e.g. an image or animation) using the derived characteristics
let viewable attributes impl = { elementType = Viewables (impl >> List.singleton); attributes = attributes }

/// A function to call when the containing element is clicked
let onclick f = OnClick f

/// A size in pixels
let px n = Pixels n
/// A size in percent (of its wrapping container)
let pct n = Percent n
    
let fontName s = GlobalStyle (fun style -> { style with fontName = s })
let fontSize s = GlobalStyle (fun style -> { style with fontSize = s })
let colour s = GlobalStyle (fun style -> { style with colour = s })
let backgroundColour s = GlobalStyle (fun style -> { style with backgroundColour = s })
let buttonTextColour s = GlobalStyle (fun style -> { style with buttonColours = { style.buttonColours with text = s } })
let buttonBackgroundColour s = GlobalStyle (fun style -> { style with buttonColours = { style.buttonColours with defaultBackground = s } })
let buttonDisabledColour s = GlobalStyle (fun style -> { style with buttonColours = { style.buttonColours with disabledBackground = s } })
let buttonHoverColour s = GlobalStyle (fun style -> { style with buttonColours = { style.buttonColours with hoverBackground = Some s } })
let buttonPressedColour s = GlobalStyle (fun style -> { style with buttonColours = { style.buttonColours with pressedBackground = Some s } })
let enabled s = GlobalStyle (fun style -> { style with enabled = s })
/// For printed text, what its alignment should be. From 0. to 1. top left to bottom right
let alignment x y = GlobalStyle (fun style -> 
    let x = if abs x > 1. then abs x % 1. else abs x
    let y = if abs y > 1. then abs y % 1. else abs y
    { style with alignment = x, y }) // alignment can only be from 0. to 1.

let margin s = LocalStyle (fun style -> { style with margin = s })
let padding s = LocalStyle (fun style -> { style with padding = s })
let borderSize s = LocalStyle (fun style -> { style with borderSize = s })
/// Border colour if the element's border size is greater than 0
let borderColour s = LocalStyle (fun style -> { style with borderColour = s })
let width i = LocalStyle (fun style -> { style with width = Some i })
let height i = LocalStyle (fun style -> { style with height = Some i })

let private defaultLocalStyle = {
    margin = px 0
    padding = px 0
    borderSize = 0
    borderColour = Colour.Transparent
    width = None
    height = None
}

let private styles globalStyle attributes =
    ((globalStyle, defaultLocalStyle), attributes) 
    ||> List.fold (fun (globalStyle, localStyle) -> 
        function 
        | GlobalStyle f -> f globalStyle, localStyle 
        | LocalStyle f -> globalStyle, f localStyle
        | _ -> globalStyle, localStyle)

let rec private renderRow globalStyle renderImpl left totalSpace spaceRemaining childrenRemaining =
    [
        match childrenRemaining with
        | [] -> ()
        | child::rest ->
            let width = 
                snd (styles globalStyle child.attributes)
                |> fun localStyle -> localStyle.width
                |> Option.bind (function 
                    | Pixels x -> Some x 
                    | Percent x -> Some (x * float totalSpace |> int)) 
                |> Option.defaultValue (spaceRemaining / childrenRemaining.Length)
            yield! renderImpl left width child
            yield! renderRow globalStyle renderImpl (left + width) totalSpace (spaceRemaining - width) rest
    ]
    
let rec private renderCol globalStyle renderImpl top totalSpace spaceRemaining childrenRemaining =
    [ 
        match childrenRemaining with
        | [] -> ()
        | child::rest ->
            let height = 
                snd (styles globalStyle child.attributes)
                |> fun localStyle -> localStyle.height
                |> Option.bind (function 
                    | Pixels x -> Some x 
                    | Percent x -> Some (x * float totalSpace |> int)) 
                |> Option.defaultValue (spaceRemaining / childrenRemaining.Length)
            yield! renderImpl top height child
            yield! renderCol globalStyle renderImpl (top + height) totalSpace (spaceRemaining - height) rest 
    ]

let private renderColour (x, y) (width, height) colour =
    OnDraw (fun loadedAssets _ spriteBatch -> 
        spriteBatch.Draw(loadedAssets.whiteTexture, rect x y width height, colour))

let private drawText loadedAssets (spriteBatch: SpriteBatch) fontName fontSize alignment colour (x, y) (width, height) (text: string) =
    let font = loadedAssets.fonts.[fontName]
    let measured = font.MeasureString (text)
    let scale = let v = float32 fontSize / measured.Y in Vector2(v, v)

    let ox, oy = alignment
    let relWidth, relHeight = float32 (float width * ox), float32 (float height * oy)
    let offWidth, offHeight = float32 ox * measured.X * scale.X, float32 oy * measured.Y * scale.Y

    let origin = Vector2 (relWidth - offWidth, relHeight - offHeight)
    let position = Vector2.Add (origin, Vector2(float32 x, float32 y))

    spriteBatch.DrawString (font, text, position, colour, 0.f, Vector2.Zero, scale, SpriteEffects.None, 0.f)

let private renderText globalStyle (x, y) (width, height) text = 
    OnDraw (fun loadedAssets _ spriteBatch -> 
        drawText 
            loadedAssets spriteBatch 
            globalStyle.fontName globalStyle.fontSize globalStyle.alignment globalStyle.colour 
            (x, y) (width, height) text)

let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

let private renderButton globalStyle (x, y) (width, height) text =
    OnDraw (fun loadedAssets inputs spriteBatch -> 
        
        let mouseOver = isInside x y width height inputs.mouseState.X inputs.mouseState.Y

        let backgroundColour = 
            if not globalStyle.enabled then globalStyle.buttonColours.disabledBackground
            elif not mouseOver then globalStyle.buttonColours.defaultBackground
            elif inputs.mouseState.LeftButton <> ButtonState.Pressed then 
                globalStyle.buttonColours.hoverBackground 
                |> Option.defaultValue globalStyle.buttonColours.defaultBackground
            else
                globalStyle.buttonColours.pressedBackground  
                |> Option.orElse globalStyle.buttonColours.hoverBackground  
                |> Option.defaultValue globalStyle.buttonColours.defaultBackground

        spriteBatch.Draw(loadedAssets.whiteTexture, rect x y width height, backgroundColour)

        drawText 
            loadedAssets spriteBatch 
            globalStyle.fontName globalStyle.fontSize (0.5, 0.5) globalStyle.buttonColours.text 
            (x, y) (width, height) text)

let private renderBorder (x, y) (width, height) borderWidth borderColour = 
    [
        renderColour (x, y) (width, borderWidth) borderColour
        renderColour (x, y + height - borderWidth) (width, borderWidth) borderColour
        renderColour (x, y) (borderWidth, height) Colour.Red
        renderColour (x + width - borderWidth, y) (borderWidth, height) borderColour
    ]

let rec private render debugOutlines globalStyle (x, y) (width, height) element = 
    [
        if debugOutlines then
            yield! renderBorder (x, y) (width, height) 1 Colour.Red

        let newGlobalStyle, localStyle = styles globalStyle element.attributes            

        // Apply margin
        let topMargin, leftMargin = 
            match localStyle.margin with 
            | Pixels n -> n, n 
            | Percent p -> int (p * float height), int (p * float width)
        let x, y = x + leftMargin, y + topMargin
        let width, height = width - (2 * leftMargin), height - (2 * topMargin)
        
        if debugOutlines then
            yield! renderBorder (x, y) (width, height) 1 Colour.Green
        
        // Test for onclick
        let onClick = List.tryPick (function OnClick e -> Some e | _ -> None) element.attributes
        match onClick with
        | Some e when newGlobalStyle.enabled ->
            yield OnUpdate (fun inputs -> 
                if (inputs.mouseState.X, inputs.mouseState.Y) ||> isInside x y width height then
                    if inputs.mouseState.LeftButton = ButtonState.Pressed 
                    && inputs.lastMouseState.LeftButton <> ButtonState.Pressed then
                        e ())
        | _ -> ()

        // background colour (if different than prior)
        if newGlobalStyle.backgroundColour <> globalStyle.backgroundColour then
            yield renderColour (x, y) (width, height) newGlobalStyle.backgroundColour

        // border
        if localStyle.borderSize > 0 then
            yield! renderBorder (x, y) (width, height) localStyle.borderSize localStyle.borderColour

        // Apply padding            
        let topPadding, leftPadding = 
            match localStyle.padding with 
            | Pixels n -> n, n 
            | Percent p -> int (p * float height), int (p * float width)
        let x, y = x + leftPadding, y + topPadding
        let width, height = width - (2 * leftPadding), height - (2 * topPadding)
                
        if debugOutlines then
            yield! renderBorder (x, y) (width, height) 1 Colour.Blue

        match element.elementType with
        | Row children -> 
            let renderImpl = fun left width child -> render debugOutlines newGlobalStyle (left, y) (width, height) child
            yield! renderRow newGlobalStyle renderImpl x width width children
        | Column children -> 
            let renderImpl = fun top height child -> render debugOutlines newGlobalStyle (x, top) (width, height) child
            yield! renderCol newGlobalStyle renderImpl y height height children
        | Text s ->
            yield renderText newGlobalStyle (x, y) (width, height) s
        | Button s -> 
            yield renderButton newGlobalStyle (x, y) (width, height) s
        | Viewables impl ->
            yield! impl { globalStyle = newGlobalStyle; x = x; y = y; width = width; height = height }
    ]

let renderUI showDebugOutlines defaultFont (x, y) (width, height) rootElement =
    let defaultGlobalStyle = {
        fontName = defaultFont
        fontSize = 16.
        alignment = 0., 0.
        colour = Colour.Black
        backgroundColour = Colour.Transparent
        buttonColours = {
            text = Colour.Black
            defaultBackground = Colour.Gray
            disabledBackground = Colour.LightGray
            hoverBackground = None
            pressedBackground = None
        }
        enabled = true
    }
    render showDebugOutlines defaultGlobalStyle (x, y) (width, height) rootElement