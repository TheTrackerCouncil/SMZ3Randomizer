# SMZ3 Cas' Multiplayer

This document will cover all of the different parts of the code involved with multiplayer, giving a basic description of each of the classes.

## Important Classes

- Randomizer.Multiplayer.Server - Project for the SignalR server
    - MultiplayerHub - SignalR Hub that receives messages from players
    - MultiplayerGame - Class which is insantiated for each multiplayer game
    - MultiplayerPlayer - Represents a single player and their connection to the server
    - MultiplayerDbService - Singleton class which is used to save/load game data in the database
    - GameManager - Class that removes expired games from memory/the database
- Randomizer.Multiplayer.Client - Project for client services to both connect to the server and manage games
    - MultiplayerClientService - Service for connecting to the SignalR server
    - MultiplayerGameService - Generic service for creating games and acting as an inbetween for game messages between Tracker, MultiplayerClientService, and the specific service for the game type
        - GameServices - Folder for all of the specific game types
            - MultiplayerGameTypeService - Abstract class for the game types with most of the functionality for creaitng seeds, building messages to send to the server, and determining what actions tracker should take
            - MultiworldGameService - The service specific to the multiworld game type.
        - EventHandlers - EventHandlers for tracker to subscribe to with details on what happened
- Randomizer.Shared
    - Multiplayer - Request/Response messages to go between the client and server
- Randomizer.SMZ3.Tracking
    - VoiceCommands
        - MultiplayerModule - Tracker module which will listen to tracker events and call out to the MultiplayerGameService and vice versa for when the local player or other players track locations.
- Randomizer.App
    - Windows
        - MultiplayerConnectWindow - Window for creating and joining games
        - MultiplayerStatusWindow - Window for viewing the list of players, submitting configs, and starting/forfeiting games

## Example Flows

### Create Game
![CreateGame drawio](https://user-images.githubusercontent.com/63823784/210160787-06cae011-c309-4405-8882-f4cf10957734.png)

### Start Game
![StartGame drawio](https://user-images.githubusercontent.com/63823784/210160790-27a295be-d350-4d5e-9bd0-325a7e925644.png)

### Track Location
![TrackLocation drawio](https://user-images.githubusercontent.com/63823784/210160791-76af5ba7-5a7a-44f5-a884-209ebb1ee7bc.png)
