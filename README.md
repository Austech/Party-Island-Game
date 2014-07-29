Party-Island-Game
=================
#What
Party Island is a PC party game where players play mini-games for points and move around a map.

##What this repository contains
This repository contains 3 main projects for the game:

* __PartyIsland/Server__ for maintaining and sending game state to the client, as well as taking and parsing player inputs
* __Party Island Unity__ for rendering game states and parsing inputs. (This is what the players will be using to play).
* __PartyIsland/Common__ for common code used in both Servers and Clients/Rendering. This is where the beefy logic of the gameplay is located

#Game Architecture
The code around the game consists of a few main parts:

* __Game Systems__ for decoupling main game features. For example, the Board state is considered it's own system. Character Selection? A system as well. Peripheral Input? You bet
* __Game Events__ for communication between game objects. For example, when a user presses the "W" key, the input system parses this and throws an INPUT_EVENT to any systems listening.
* __Game State Encoding__ for parsing and wrapping up a game system or state into a byte array. This is mainly used when sending states over a network to be parsed by clients, as well as game saving.

##Events
This game heavily uses event based events. Much like the description above, almost anything notable is an event, from player input to game specific changes. This allows for easy decoupling, and using the Observer Pattern, is easy to receive/filter the events you want.

If you're interested in the types of events, check out [The GameEvent.cs file](https://github.com/Austech/Party-Island-Game/blob/master/PartyIsland/Common/GameEvent.cs)

##Networking Design
Because of the nature of the game, it's important for network code to be as flexible as possible for mini-games. The network flow works similar to source or quake, but here's some specifics:

##State syncing
1. Game Server contains it's own state of the game, this is "The Real State", and must keep all clients updated
2. Every certain amount of ticks, the Game Server sends a BOARD_ENCODE event to all clients. This is more or less a game state update packet
3. When the client's event handler receives a BOARD_ENCODE packet, the client's board state can decode it and update itself accordingly
4. The client's renderer peeks at the Board Object and renders based on Board data
5. Because of event handlers, a renderer may display effects based on certain events occuring. For example, a PLAYER_ROLLED event may produce a "Roll dice sound" or dice explosion effects

##Input handling
1. When a client presses a key, an INPUT_EVENT is sent to the server, as well as it's own game
2. Because of the Common library, the client's game should produce similar results when parsing events, allowing for easy client prediction
3. Server receives INPUT_EVENT, and just like the client, produces a result on the server side
4. If the client produces a different result, the State Syncing will automatically handle any errors.