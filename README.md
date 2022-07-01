# Pooooooooooooooooooooooooooong
## A Pong-like battle royale MMO made in .NET Core & Blazor.
Two paddles, each controlled by the average mouse position of multiple players. A ball bounces back and forth at a constant horizontal speed, but the paddles shrink to keep the pace moving. Each round, the players are halved until only one remains.
# Poong.Engine
Class library that implements the game engine.

Quick class overview:

`Game`: Create an instance of `Poong.Engine.Game` in your backend application. It configures itself by looking for a valid JSON configuration string: first in the `POONG_CONFIG` environment variable, then in `/poongConfig.json`, and defaults to the `Configuration` class defaults. The game engine ticks on a `System.Timers.Timer`. The engine supports ticking internally at a faster rate and updating the clients at a slower rate, but in practice this did not seem to be of much benefit.

`Client`: This class handles communication between the game and the user. It passes input to the game and the game state to the user. Physical quantities like positions, sizes, and speeds get transformed between the game engine and client window coordinate systems via the client's `Transformation` instance. The game uses `GameStateFragment`s to transmit the minimum required information to each client on each tick.

`Player`: Players influence their side's paddle's position, as long as they are not dead. Human players are managed by their `Client` instance. Bot players are managed by their `Boid`. Players get points added to their score depending on what round they die in.

`Flock/Boid`: A stripped-down implementation of the standard [Boids](https://en.wikipedia.org/wiki/Boids) model. Boids are used as bots, moving around based on the ball's position and their paddle's position. They act independently of each other, but can be extended to react to their peers.
# Poong.Forms
A basic single player desktop implementation in WinForms for testing the engine.
# Poong.Blazor
The actual Blazor Server web app implementation. The ball and paddles are implemented as Blazor components wrapped around HTML elements. Their positions are interpolated using CSS animations. Players are rendered on an HTML canvas overlay using JS interop.
# Try it
https://pooooooooooooooooooooooooooong.herokuapp.com/