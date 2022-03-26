using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public struct GameState
    {
        public Body LeftPaddle { get; internal set; }
        public Body RightPaddle { get; internal set; }
        public List<Ball> Balls { get; internal set; }
        public List<Player> Players { get; internal set; }
        public int Round { get; internal set; }
        public GamePhase Phase { get; internal set; }
        public GamePhase PrevPhase { get; internal set; }
    }
    /// <summary>
    /// Used to pass a fragment of game state to client.
    /// </summary>
    public struct GameStateFragment
    {

        public Body LeftPaddle;
        public float LeftPaddlePosition;
        public float LeftPaddleSpeed;
        public float? LeftPaddleLength;

        public Body RightPaddle;
        public float RightPaddlePosition;
        public float RightPaddleSpeed;
        public float? RightPaddleLength;

        public List<Ball> Balls;
        public List<Point> BallPositions;
        public List<Vector> BallSpeeds;

        public List<Player> Players;
        public List<Point> PlayerPositions;
        public List<Vector> PlayerSpeeds;

        public int? Round;
        
        public GamePhase? NewPhase;
        public GamePhase? OldPhase;

        public int? MinTopTenScore;
    }

}

