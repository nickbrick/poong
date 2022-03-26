using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class GameStateEventArgs : EventArgs
    {
        public GameStateFragment State { get; set; }
        public GameStateEventArgs(GameStateFragment state)
        {
            State = state;
        }
    }
    public class BallEventArgs : EventArgs
    {
        public Collisions Collisions { get; set; }
        public BallEventArgs(Collisions collisions)
        {
            Collisions = collisions;
        }
    }
    public class ClockEventArgs : EventArgs
    {
        public long Time { get; set; }
        public ClockEventArgs (long time)
        {
            Time = time;
        }
    }
    public class PlayerEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public PlayerEventArgs(Player player)
        {
            Player = player;
        }
    }
}
