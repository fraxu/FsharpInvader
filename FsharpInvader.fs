module FsharpInvader 

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open GameEngine
open GameState
    
let createGameState () =
    { sprites = [] 
      eventHandler = (fun event state -> state) } 

[<EntryPoint>]
let main argv =

    let state = createGameState ()
    let w = new Window(state)
    w.Run()

    0
