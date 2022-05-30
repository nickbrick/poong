using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class Player
    {
        public Client Client { get; }
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Point Position { get; internal set; }
        public Point LastPosition { get; internal set; }
        public Point RoundStartPosition { get; internal set; }
        public Vector Speed => new Vector(Position.X - LastPosition.X, Position.Y - LastPosition.Y);
        public Side Side { get; internal set; }
        public int Score { get; internal set; }
        public int RoundDied { get; internal set; } = 0;
        public int Rank { get; internal set; } = 0;
        internal Paddle Paddle { get; set; }
        internal Player()
        {
            Id = Guid.NewGuid();
            Name = Id.ToString();
            Position = new Point(0);
            LastPosition = new Point(0);
            RoundStartPosition = new Point(0);
        }
        internal Player(string name) : this()
        {
            if (name != null)
                Name = name;
        }
        internal Player(string name, Client client) : this()
        {
            if (name != null)
                Name = name;
            Client = client;
        }
        internal Player(Player player)
        {
            Client = player.Client;
            Id = player.Id;
            Name = player.Name;
            Position = player.Position;
            LastPosition = player.LastPosition;
            Side = player.Side;
            Paddle = player.Paddle;
            Score = player.Score;
        }
    }
}
