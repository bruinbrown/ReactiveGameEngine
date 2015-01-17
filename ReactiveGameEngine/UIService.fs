namespace CardboardTaskForceReactive

module UI =
    open Microsoft.Xna.Framework
    open CardboardTaskForceReactive.Model
    open Microsoft.Xna.Framework.Graphics
    open ReactiveGameEngine.Reactive

    type IUIElement =
        abstract member ShouldCloseOnOutsideTap : bool with get
        abstract member Bounds : Rectangle<int> with get
        abstract member Name : string with get
        abstract member HandleTap : Vector<int> -> unit
        abstract member Render : (string -> Texture2D) -> SpriteBatch -> unit
        abstract member Children : IUIElement list with get
        abstract member ZIndex : float32 with get

    let pause_menu =
        { new IUIElement with
              member x.ZIndex: float32 = 
                  0.0f
              
              member x.Children: IUIElement list = 
                  []
              
              member x.Bounds: Rectangle<int> = 
                  failwith "Not implemented yet"
              
              member x.HandleTap(arg1: Vector<int>): unit = 
                  failwith "Not implemented yet"
              
              member x.Name: string = 
                  "Pause menu"
              
              member x.Render(arg1: string -> Texture2D) (arg2: SpriteBatch): unit = 
                  failwith "Not implemented yet"
              
              member x.ShouldCloseOnOutsideTap: bool = 
                  true
              }

    let pause_button =
        { new IUIElement with
              member x.ZIndex: float32 = 
                  1.0f
              
              member x.Children: IUIElement list = 
                  []
              
              member x.Name: string = 
                  "Pause"
              
              member x.Bounds: Rectangle<int> = 
                  Rectangle(Vector(25,25), Vector(125, 125))
              
              member x.HandleTap(arg1: Vector<int>): unit = 
                  "ui" <! pause_menu
              
              member x.Render(arg1: string -> Texture2D) (arg2: SpriteBatch): unit = 
                  failwith "Not implemented yet"
              
              member x.ShouldCloseOnOutsideTap: bool = 
                  false
              }

module UIService =
    open ReactiveGameEngine.Reactive
    open Microsoft.Xna.Framework
    open UI

    type UIMessage<'a, 'b> =
        | AddNewUIItem of IUIElement
        | ProcessScreenTap of Vector2

    type UIState =
        { UIElements : string list }

    let default_state () =
        { UIElements = [] }

    let message_handler (message, context) state =

        state

    let spawn_ui_service () =
        reactor {
            path "ui"
            defaultState (default_state ())
        } |> ignore