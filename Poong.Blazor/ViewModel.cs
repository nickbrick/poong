using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Poong.Engine;
using System.Drawing;
using Point = Poong.Engine.Point;
using Size = Poong.Engine.Size;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Poong.Blazor
{
    public class ViewModel
    {
        public Game Game;
        public List<Player> Players = new List<Player>();
        public Client Client;
        public PointF LastMousePosition = new PointF(0, 0);
        public Body LeftPaddle = new Body();
        public Body RightPaddle = new Body();
        public List<Body> Balls = new List<Body>();
        public List<Vector> BallSpeeds = new List<Vector>();
        public List<Point> PlayerPositions = new List<Point>();
        public float PlayerTweenTime => Game.Config.TickMilliseconds * Game.Config.TicksPerUpdate;
        public int Round;
        public GamePhase Phase;

        public Queue<string> Log = new Queue<string>(10);

        internal void LogMessage(string message)
        {
            Log.Enqueue(message);
            if (Log.Count > 10) Log.Dequeue();
        }

        public void Update(GameStateFragment gameState)
        {
            if (gameState.Balls?.Count > 0)
            {
                Balls = gameState.Balls.Select(ball => new Body { Left = ball.Left, Top = ball.Top, Width = ball.Size.Width, Height = ball.Size.Height, Speed = ball.Speed }).ToList();
            }
            if (gameState.BallPositions?.Count > 0)
            {
                for (int i = 0; i < gameState.BallPositions.Count; i++)
                {
                    var position = gameState.BallPositions[i];
                    Balls[i].Left = position.X;
                    Balls[i].Top = position.Y;
                    Balls[i].CalculateTarget(5000f);
                }
            }
            if (gameState.BallSpeeds?.Count > 0)
            {
                for (int i = 0; i < gameState.BallSpeeds.Count; i++)
                {
                    Balls[i].Speed = gameState.BallSpeeds[i];
                    Balls[i].CalculateTarget(5000f);
                }
            }
            if (gameState.LeftPaddle != null)
            {
                LeftPaddle.Left = gameState.LeftPaddle.Corner.X;
                LeftPaddle.Top = gameState.LeftPaddle.Corner.Y;
                LeftPaddle.Width = gameState.LeftPaddle.Size.Width;
                LeftPaddle.Height = gameState.LeftPaddle.Size.Height;
                LeftPaddle.Speed = gameState.LeftPaddle.Speed;
            }
            if (gameState.RightPaddle != null)
            {
                RightPaddle.Left = gameState.RightPaddle.Corner.X;
                RightPaddle.Top = gameState.RightPaddle.Corner.Y;
                RightPaddle.Width = gameState.RightPaddle.Size.Width;
                RightPaddle.Height = gameState.RightPaddle.Size.Height;
                RightPaddle.Speed = gameState.RightPaddle.Speed;
            }
            LeftPaddle.Top = gameState.LeftPaddlePosition;
            RightPaddle.Top = gameState.RightPaddlePosition;
            LeftPaddle.Speed.Y = gameState.LeftPaddleSpeed;
            LeftPaddle.CalculateTarget(2000f);
            RightPaddle.Speed.Y = gameState.RightPaddleSpeed;
            RightPaddle.CalculateTarget(2000f);

            if (gameState.LeftPaddleLength != null)
                LeftPaddle.Height = (int)gameState.LeftPaddleLength;
            if (gameState.RightPaddleLength != null)
                RightPaddle.Height = (int)gameState.RightPaddleLength;

            if (gameState.Round != null)
            {
                Round = gameState.Round.Value;
            }

            if (gameState.NewPhase != null)
            {
                if (gameState.NewPhase == GamePhase.Endgame)
                    LogMessage($"Winner: {gameState.Players.Single(player => player.Side != Side.None).Name}");
                Phase = gameState.NewPhase.Value;
                LogMessage($"Round {Round}: {Phase}");
            }
            if (gameState.Players != null)
            {
                Players = gameState.Players.Select(player => new Player(player) { IsTopTen = player.Score >= Game.MinTopTenScore }).ToList();
                LogMessage($"Players: {Players.Count(player => player.Side == Side.Left)} left, {Players.Count(player => player.Side == Side.Right)} right. You are {Client.Player.Name} and your side is {Client.Player.Side}.");
            }
            PlayerPositions = gameState.PlayerPositions;
            if (gameState.PlayerPositions != null)
            {
                for (int i = 0; i < PlayerPositions.Count; i++)
                {
                    Players[i].LastPosition = Players[i].Position;
                    Players[i].Position = PlayerPositions[i];
                }
            }
            Client.TryGiveInput(LastMousePosition.X, LastMousePosition.Y);
        }
        public class Player
        {
            public string Id;
            public string Name;
            public string DisplayName => IsTopTen || Game.Config.RenderAllNames ? $"{Name} ({Score})" : "";
            public Point Position;
            public Point LastPosition { get; set; }
            public Vector Speed => (Position - LastPosition) / Game.Config.TicksPerUpdate;
            public Side Side;
            public int Score { get; }
            public bool IsTopTen = false;
            public Player(Poong.Engine.Player player)
            {
                Id = player.Id.ToString();
                Name = player.Name;
                Position = player.Position;
                LastPosition = player.LastPosition;
                Side = player.Side;
                Score = player.Score;
            }
        }
        public class Body
        {
            public float Left { get; set; }
            public float Top { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }
            public Vector Speed { get; set; }
            public float? TargetLeft { get; set; }
            public float? TargetTop { get; set; }
            public float? TargetTime { get; set; }
            public void CalculateTarget(float projectionTime)
            {
                TargetTime = projectionTime;
                TargetLeft = Left + TargetTime / Game.Config.TickMilliseconds * Speed.Magnitude * MathF.Cos(Speed.Angle);
                TargetTop = Top + TargetTime / Game.Config.TickMilliseconds * Speed.Magnitude * MathF.Sin(Speed.Angle);
            }
        }
        public struct Dimensions
        {
            public float ClientWidth { get; set; }
            public float ClientHeight { get; set; }
            public float BoardWidth { get; set; }
            public float BoardHeight { get; set; }
        }
    }
}
