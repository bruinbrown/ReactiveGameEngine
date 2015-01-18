namespace CardboardTaskForceReactive

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open ReactiveGameEngine.Reactive
open CardboardTaskForceReactive.CameraModule
open CardboardTaskForceReactive.Model
open CardboardTaskForceReactive.UIService

type InteractionType =
    | Tap of Vector2
    | Swipe of Vector2

type AspectRatio =
    | Widescreen
    | Retro

type UserInteractionComponent (game, interaction_mapper:InteractionType -> unit) =
    inherit GameComponent (game)

    do TouchPanel.EnabledGestures <- GestureType.Tap ||| GestureType.HorizontalDrag ||| GestureType.VerticalDrag

    override __.Update(gameTime) =

        let gestures = [while TouchPanel.IsGestureAvailable do yield TouchPanel.ReadGesture ()]

        let mapped = gestures
                     |> List.map (fun t -> match t.GestureType with
                                           | GestureType.Tap -> Tap(t.Position)
                                           | GestureType.HorizontalDrag -> Swipe(t.Delta)
                                           | GestureType.VerticalDrag -> Swipe(t.Delta)
                                           | _ -> failwith "Invalid extra gesture type")
        

        mapped
        |> List.iter (fun t -> interaction_mapper t)

        base.Update(gameTime)
    
type ReactiveGameEngineGame (aspectRatio) as this =
    inherit Game ()

    let x_res, y_res = match aspectRatio with
                       | Widescreen -> (1920, 1080)
                       | Retro -> (1280, 960)

    let scale_factor_x = (float32 x_res) / (float32 this.Window.ClientBounds.Width)
    let scale_factor_y = (float32 y_res) / (float32 this.Window.ClientBounds.Height)

    let scale_factor = Vector2(scale_factor_x, scale_factor_y)

    let graphics = new GraphicsDeviceManager(this)

    let mutable spriteBatch = None

    override this.LoadContent () =
        do spriteBatch <- Some(new SpriteBatch(this.GraphicsDevice))
        spawn_camera (Vector(0,0)) (x_res, y_res)
        //spawn_ui_service
        //spawn_map
        //spawn_dying
        //new UserInteractionComponent(this, fun _ -> ())
        base.LoadContent ()

    override this.Update gameTime =
        base.Update gameTime

    override this.Draw gameTime =
        this.GraphicsDevice.Clear Color.CornflowerBlue
        let entities = ReactorHost.Instance.Entities
        spriteBatch
        |> Option.iter(fun t -> t.Begin (SpriteSortMode.BackToFront, BlendState.AlphaBlend)
                                entities |> List.iter (fun s -> s.Render(gameTime,t))
                                t.End ())
        base.Draw gameTime

module Startup =
    [<EntryPoint>]
    let main _ = 
        use game = new ReactiveGameEngineGame(Retro)
        game.Run ()
        0 // return an integer exit code
