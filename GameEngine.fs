module GameEngine

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open GameState

let getImageWidthAndHeight fileName = 
    use file = 
        Path.Combine(__SOURCE_DIRECTORY__, fileName)
        |> System.Drawing.Image.FromFile
    
    file.Width, file.Height

let checkCollision (a : Sprite) (b : Sprite) =
     Rectangle(a.x, a.y, a.width, a.height)
        .Intersects(Rectangle(b.x, b.y, b.width, b.height))

type Window (gameState : GameState) as this =
    inherit Game()

    let mutable spriteBatch : SpriteBatch = null
    let mutable textures : Map<string, Texture2D> = Map.empty
    let mutable gameState = gameState
    let mutable oldKeyboardState = Keyboard.GetState()
    let mutable graphics = null

    do 
        graphics <- new GraphicsDeviceManager(this)
        graphics.PreferredBackBufferWidth <- 800
        graphics.PreferredBackBufferHeight <- 600


    let loadImage (device : GraphicsDevice) file =
        let path = Path.Combine(__SOURCE_DIRECTORY__, file)
        use stream = File.OpenRead(path)
        let texture = Texture2D.FromStream(device, stream)
        let textureData = Array.create<Color> (texture.Width * texture.Height) Color.Transparent
        texture.GetData(textureData)
        texture

    member this.SetState(state) =
        gameState <- state
        
    override this.Initialize() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)
        base.Initialize ()
        ()

    override this.LoadContent() =
        ()
 
    override this.Update (gameTime) =
        let currentKeyState = Keyboard.GetState()
        let input = KeyboardInput (oldKeyboardState, currentKeyState)

        let updatedGameState =
            gameState
            |> gameState.eventHandler (Tick gameTime.ElapsedGameTime.TotalMilliseconds)
            |> gameState.eventHandler input 

        let getCollisions index sprite sprites = 
            sprites
            |> List.mapi (fun i target -> 
                if (index <> i) && (checkCollision sprite target) then
                    Some (Collision target)
                else 
                    None)
            |> List.choose id

        let handleSpriteTickAndInput = 
            updatedGameState.sprites
            |> List.mapi(fun index x -> 
                let collissions = getCollisions index x updatedGameState.sprites
                x.eventHandler x ([Tick gameTime.ElapsedGameTime.TotalMilliseconds; input] @ collissions) updatedGameState)
            |> List.fold (>>) id

        oldKeyboardState <- currentKeyState

        gameState <- handleSpriteTickAndInput { updatedGameState with sprites = [] }
        ()
    
    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.Black

        let drawSprite (sprite : Sprite) =
            let texture = 
                match Map.tryFind sprite.texture textures with
                | Some texture ->
                    texture
                | None ->
                    let newTexture = loadImage this.GraphicsDevice sprite.texture
                    textures <- Map.add sprite.texture newTexture textures
                    newTexture

            let rectangle = 
                Rectangle(
                    sprite.x,
                    sprite.y,
                    texture.Width,
                    texture.Height)
            
            spriteBatch.Draw(texture, rectangle , Color.White)  
    
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied)

        gameState.sprites
        |> List.iter drawSprite

        spriteBatch.End()

        ()