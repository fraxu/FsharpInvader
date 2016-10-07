#I "packages/MonoGame.Framework.DesktopGL/lib/net40"
#r "OpenTK.dll"
#r "NVorbis.dll"
#r "MonoGame.Framework.dll"

#load "GameState.fs"
#load "GameEngine.fs"
#load "FsharpInvader.fs"

open System
open FsharpInvader
open GameState
open GameEngine
open Microsoft.Xna.Framework.Input
open System.Threading

let RunWindow (state) =
    let (game:Window ref) = ref Unchecked.defaultof<Window>

    let start() = 
        game := new Window(state)
        let g = !game
        g.Run()

    let thread = Thread start
    thread.IsBackground <- true
    thread.SetApartmentState ApartmentState.STA
    thread.Start()
    Thread.Sleep(1000)

    !game

let emptyState = 
    { sprites = [] 
      eventHandler = (fun event state -> state) }

let game = RunWindow(emptyState)

fsi.AddPrintTransformer(fun (state : GameState) -> 
    game.SetState(state)
    state |> box)

// Select all lines above and run those in F# inrteractive by hitting Alt + Enter
// You should get empty game window


// Some helper functions
let addSpriteToState sprite state =
    { state with sprites = sprite :: state.sprites}

let handleEvents f sprite events state =
    let rec handle f sprite events state =
        match events with
        | [] -> (sprite, state)
        | (head :: tail) ->
            let newSprite, newState = f head sprite state
            handle f newSprite tail newState
    
    let newSprite, newState = handle f sprite events state

    match newSprite with
    | Some s ->
        addSpriteToState s newState
    | None ->
        newState

let createSprite spriteType (x, y) texture eventHandler =
    let w, h = getImageWidthAndHeight texture

    { spriteType = spriteType
      x = x
      y = y
      texture = texture
      width = w
      height = h
      eventHandler = eventHandler }

// DIY add shooting(remember to delete offscreen bullets), collision...
let createPlayer () =
    let playerEventHandler (sprite : Sprite) events state =
        let handleEvent event sprite state =
            match sprite, event with
            | Some sprite, KeyboardInput (_, current) ->
                    if current.IsKeyDown(Keys.Left) then
                        Some { sprite with x = sprite.x }, state
                    else 
                        Some sprite, state
            | _ ->
                sprite, state

        handleEvents handleEvent (Some sprite) events
    
    createSprite
        (Player 100)
        (300, 500)
        "graphics/playerShip1_blue.png"
        playerEventHandler

// DIY add enemies...

// Just send GameState to interactive to update game window 
{ sprites = [ createPlayer() ] 
  eventHandler = (fun event state -> state) }


