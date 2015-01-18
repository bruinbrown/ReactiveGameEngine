namespace CardboardTaskForceReactive

    module private MapParser =
        open CardboardTaskForceReactive.Model
        open System.IO
        open System.Text.RegularExpressions

        type internal Metadata =
            { Name : string
              MapVersion : int
              Description : string
              Size : Vector<int> }

        type internal MapBuilding =
            { BuildingType : int
              TeamCapture : int * int }

        type internal MapEntity =
            { EntityType : int
              EntityHealth : int
              EntityTeam : int }

        type internal MapSquare =
            { SquareType : int
              Building : MapBuilding option
              Entity : MapEntity option }

        type internal MapCamera =
            { Position : int * int }

        type internal MapDefinition =
            { Metadata : Metadata
              Squares : Map<int * int, MapSquare> }

        exception private InvalidMapVersion of string

        let internal parser_v1 serialized_map =
            
            Unchecked.defaultof<MapDefinition>

        let internal parse_map  (lines: string) =
            let string_reader = new System.IO.StringReader(lines)
            let version = string_reader.ReadLine() |> int
            let data = string_reader.ReadToEnd()
            match version with
            | 1 -> parser_v1 data
            | _ -> let error_message = sprintf "Unexpected map version. Parser v%i is not supported" version
                   raise <| InvalidMapVersion(error_message)

        let internal file_based_map_parser = File.ReadAllText >> parse_map

    module MapModule =
        open Microsoft.Xna.Framework.Graphics
        open CardboardTaskForceReactive.Model.Vector
        open EntityModule
        open BuildingModule
        open MapParser
        open ReactiveGameEngine.Reactive
        open CardboardTaskForceReactive.Model

        type Building =
            { BuildingTexture : Texture2D }

        type Square =
            { Unit : string option
              Building : string option
              SquareTexture : Texture2D }
            member this.BuildingState
                with get () = this.Building
                              |> Option.map (fun t -> ((!!t).Head.State :?> BuildingState))
            member this.UnitState
                with get () = this.Unit
                              |> Option.map (fun t -> ((!!t).Head.State :?> EntityState))

        type MapState =
            { Map : Map<Vector<int>, Square>
              CurrentTeam : Team
              LocalTeam : Team }

        let private spawn_unit content_provider state unit_type position =
            ()

        let private convert_map_definition map =
            Unchecked.defaultof<MapState>

        let private flip_turn = function | Team1 -> Team2 | Team2 -> Team1

        let private build_unit content_provider (state, context) vec typ =
            match state.Map.[vec].Unit, state.Map.[vec].Building with
            | None, Some(building) -> let address = System.Guid.NewGuid().ToString()
                                      spawn_entity content_provider address state.CurrentTeam typ vec
                                      let square = { state.Map.[vec] with Unit = Some(address) }
                                      { state with Map = state.Map |> Map.remove vec |> Map.add vec square }
            | _ -> state

        let private trim_dead_units (state, context) =
            let map = state.Map |> Map.map (fun k v -> match v.UnitState with
                                                       | Some x -> if x.Health < 0 then { v with Unit = None }
                                                                       else v
                                                       | None -> v)
            { state with Map = map }

        let private end_turn (state, context) =
            { state with CurrentTeam = flip_turn state.CurrentTeam }

        let private move_unit (state, context) vector target =
            match state.Map.[vector].Unit, state.Map.[target].Unit with
            | Some(unit'), None -> let sq_curr = { state.Map.[vector] with Unit = None }
                                   let sq_next = { state.Map.[target] with Unit = Some(unit') }
                                   let map = state.Map |> Map.remove vector |> Map.remove target |> Map.add vector sq_curr |> Map.add target sq_next
                                   unit' <! MoveTo(target)
                                   { state with Map = map }
            | _ -> state

        let private message_handler content_provider (state, context) message =
            match message with
            | TrimDeadUnits -> trim_dead_units (state, context)
            | MoveUnit (vec, target) -> move_unit (state, context) vec target
            | BuildUnit (vec, typ) -> build_unit content_provider (state, context) vec typ
            | EndTurn -> end_turn (state, context)

        let load_map content_provider map_file =
            let map = file_based_map_parser map_file
                      |> convert_map_definition
            reactor {
                path "map"
                defaultState map
                messageHandler (message_handler content_provider)
            } |> spawn

        let start_new_game_from_map content_provider map_file starting_team local_team =
            ()