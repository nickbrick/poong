using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poong.Engine
{
    public class Client
    {
        public event EventHandler<GameStateEventArgs> StateChanged;
        public event EventHandler<string> NotificationReceived;
        public Transformation Transformation { get; set; }
        public Player Player { get; set; }
        private GameStateFragment _state;
        public GameStateFragment State
        {
            get { return ToScreen(_state); }
            set
            {
                _state = value;
                StateChanged?.Invoke(this, new GameStateEventArgs(State));
            }
        }

        private GameStateFragment ToScreen(GameStateFragment state)
        {
            if (state.LeftPaddle != null)
            {
                state.LeftPaddle = new Body(
                    Transformation.ToScreen(state.LeftPaddle.Center),
                    Transformation.ToScreen(state.LeftPaddle.Size),
                    Transformation.ToScreen(state.LeftPaddle.Speed)
                );
            }
            state.LeftPaddlePosition = Transformation.ToScreen(state.LeftPaddlePosition, Transformation.Axis.Y, Transformation.Mode.Position);
            state.LeftPaddleSpeed = Transformation.ToScreen(state.LeftPaddleSpeed, Transformation.Axis.Y, Transformation.Mode.Quantity);
            if (state.LeftPaddleLength.HasValue)
                state.LeftPaddleLength = Transformation.ToScreen(state.LeftPaddleLength.Value, Transformation.Axis.Y, Transformation.Mode.Quantity);

            if (state.RightPaddle != null)
            {
                state.RightPaddle = new Body(
                    Transformation.ToScreen(state.RightPaddle.Center),
                    Transformation.ToScreen(state.RightPaddle.Size),
                    Transformation.ToScreen(state.RightPaddle.Speed)
                );
            }
            state.RightPaddlePosition = Transformation.ToScreen(state.RightPaddlePosition, Transformation.Axis.Y, Transformation.Mode.Position);
            state.RightPaddleSpeed = Transformation.ToScreen(state.RightPaddleSpeed, Transformation.Axis.Y, Transformation.Mode.Quantity);
            if (state.RightPaddleLength.HasValue)
                state.RightPaddleLength = Transformation.ToScreen(state.RightPaddleLength.Value, Transformation.Axis.Y, Transformation.Mode.Quantity);

            if (state.Balls != null)
            {
                state.Balls = state.Balls.Select(ball => new Ball(
                    Transformation.ToScreen(ball.Center),
                    Transformation.ToScreen(ball.Size),
                    Transformation.ToScreen(ball.Speed)
                    )).ToList();
            }
            if (state.BallPositions != null)
                state.BallPositions = state.BallPositions.Select(position => Transformation.ToScreen(position)).ToList();

            if (state.BallSpeeds != null)
                state.BallSpeeds = state.BallSpeeds.Select(speed => Transformation.ToScreen(speed)).ToList();


            if (state.Players != null)
            {
                state.Players = state.Players.Select(player => new Player(player)
                {
                    Position = Transformation.ToScreen(player.Position),
                    LastPosition = Transformation.ToScreen(player.LastPosition)
                }
                ).ToList();
            }
            if (state.PlayerPositions != null)
                state.PlayerPositions = state.PlayerPositions.Select(position => Transformation.ToScreen(position)).ToList();

            return state;
        }

        public void TryGiveInput(float x, float y)
        {
            if (Player == null) return;
            Player.Position = Transformation.ToEngine(new Point(x, y));
        }
        public void Notify(string message)
        {
            System.Diagnostics.Debug.WriteLine("SEnding");
            NotificationReceived?.Invoke(this, message);
            System.Diagnostics.Debug.WriteLine("sented");

        }
    }
}
