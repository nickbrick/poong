using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Poong.Engine
{
    public partial class Game
    {
        private readonly string[] AnimalNames = Properties.Resources.AnimalNames.Split(';');
        public event EventHandler<ClockEventArgs> ClockTicked;
        public event EventHandler<BallEventArgs> BallCollided;
        public event EventHandler<PlayerEventArgs> PlayerJoined;
        public event EventHandler<PlayerEventArgs> ClientDisconnected;

        public const float TickMilliseconds = 20.0f;
        public const int TicksPerUpdate = 5;

        internal const float pixelSize = 0.04f;
        internal const float verticalHalfSize = 1.0f;
        internal const float paddleFaceDistance = 0.89f;
        internal const float horizontalHalfSize = 1.3f;

        internal const float paddleInitialLength = Game.pixelSize * 10.0f;
        internal const float paddleDecay = pixelSize;
        internal const float powerPaddleLengthThrehsold = 0;
        internal const float powerPaddleVerticalDeflectSpeed = 0.2f;

        internal const int maxPlayersPerPaddle = 512;

        private bool kickAfk = false;
        private bool useBoids = true;
        private int boidsPerPaddle = 4;
        private Flock leftFlock;
        private Flock rightFlock;
        private readonly Timer clock;
        private GamePhase phase = GamePhase.PreGame;
        private GamePhase prevPhase = GamePhase.PreGame;
        private Queue<GamePhase> nextPhase = new Queue<GamePhase>(new[] { GamePhase.Ready, GamePhase.Playing });
        private readonly Paddle leftPaddle;
        private readonly Paddle rightPaddle;
        private readonly List<Ball> balls;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Client> clients = new System.Collections.Concurrent.ConcurrentDictionary<Guid, Client>();
        private List<Player> AllPlayers { get; set; }
        private List<Player> LeftPlayers => AllPlayers.Where(player => player.Side == Side.Left).ToList();
        private List<Player> RightPlayers => AllPlayers.Where(player => player.Side == Side.Right).ToList();
        private List<Player> AlivePlayers => LeftPlayers.Union(RightPlayers).OrderBy(player => player.Id).ToList();
        private int PlayerCount => AllPlayers.Count;
        private int AlivePlayerCount => AllPlayers.Count(player => player.Side != Side.None);
        private int LeftPlayerCount => AllPlayers.Count(player => player.Side == Side.Left);
        private int RightPlayerCount => AllPlayers.Count(player => player.Side == Side.Right);
        public int MinTopTenScore => AlivePlayers.OrderByDescending(player => player.Score)
                                                 .TakeWhile(player => player.Score > 0 && AlivePlayers.Count(player_ => player_.Score >= player.Score) <= Math.Min(10, AlivePlayerCount))
                                                 .LastOrDefault()?.Score ?? Int32.MaxValue;
        private int round = 1;
        private GameStateFragment NextFragment;
        private long gameTime = 0;
        private int phaseTime = (int)Cooldowns.Pregame;
        private Side lastGoalSide = Side.None;
        private int ongoingBroadcasts = 0;


        public Game()
        {
            leftPaddle = new Paddle(Side.Left, paddleInitialLength);
            rightPaddle = new Paddle(Side.Right, paddleInitialLength);
            balls = new List<Ball> { new Ball() };
            balls.ForEach(b => b.BoundaryTouching += Ball_BoundaryTouching);

            //clients = new  List<Client>();
            AllPlayers = new List<Player>(2 * Game.maxPlayersPerPaddle);

            clock = new Timer(TickMilliseconds);
            clock.Elapsed += Tick;
            BallCollided += Game_BallCollided;

            Start();
        }



        private void Start()
        {
            clock.Enabled = true;
            if (useBoids)
            {
                leftFlock = new Flock();
                rightFlock = new Flock();
                for (int i = 0; i < boidsPerPaddle; i++)
                {
                    leftFlock.Boids.Add(new Boid(JoinTeam(Side.Left)));
                    rightFlock.Boids.Add(new Boid(JoinTeam(Side.Right)));
                }
            }
        }
        private void BroadcastState()
        {
            if (ongoingBroadcasts > 0)
            {
                Debug.WriteLine("skipping update");
                return;
            }
            ongoingBroadcasts += 1;
            foreach (var client in clients.Values)
            {
                client.State = NextFragment;
            }
            //clients. ForEach(client => { client.State = NextFragment; });
            NextFragment = new GameStateFragment();
            ongoingBroadcasts -= 1;

        }
        private void Tick(object sender, ElapsedEventArgs e)
        {
            leftPaddle.Update(gameTime);
            rightPaddle.Update(gameTime);
            balls.ForEach(b => b.Update(gameTime));

            if (useBoids)
            {
                leftFlock.X = leftPaddle.Center.X - (pixelSize * 6);
                leftFlock.Y = (balls.First().Center.Y * 3 + leftPaddle.Center.Y) / 4;
                leftFlock.Update();

                rightFlock.X = rightPaddle.Center.X;// + (pixelSize * 2);
                rightFlock.Y = (balls.First().Center.Y * 3 + rightPaddle.Center.Y) / 4;
                rightFlock.Update();
            }
            if (phaseTime > 0)
            {
                phaseTime -= 1;
                if (phase == GamePhase.PreGame && AllPlayers.Count < 2) phaseTime += 1;
                if (phaseTime == 0)
                {
                    //Game_GamePhaseEnded(this, this.phase);
                    if (nextPhase.TryDequeue(out var phase))
                        ChangePhase(phase);
                }
            }

            if (gameTime % TicksPerUpdate == 0)
            {
                LoadNextFragmentEssentials();
                BroadcastState();
            }

            ClockTicked?.Invoke(this, new ClockEventArgs(gameTime));
            gameTime += 1;
        }

        private void LoadNextFragmentEssentials()
        {
            NextFragment.LeftPaddlePosition = leftPaddle.Corner.Y;
            NextFragment.LeftPaddleSpeed = leftPaddle.Speed.Y;
            NextFragment.RightPaddlePosition = rightPaddle.Corner.Y;
            NextFragment.RightPaddleSpeed = rightPaddle.Speed.Y;
            NextFragment.PlayerPositions = AlivePlayers.Select(player => player.Position).ToList();
            //NextFragment.BallPositions = balls.Select(ball => ball.Corner).ToList();
        }

        private void ChangePhase(GamePhase phase, GamePhase[] nextPhases = null)
        {
            prevPhase = this.phase;
            Game_GamePhaseEnded(this, prevPhase);
            switch (phase)
            {
                case GamePhase.PreGame:
                    phaseTime = (int)Cooldowns.Pregame;
                    NextFragment.Players = AlivePlayers;
                    break;
                case GamePhase.Ready:
                    phaseTime = (int)Cooldowns.Goal;
                    break;
                case GamePhase.Playing:
                    balls.ForEach(ball => ball.Launch(lastGoalSide));
                    NextFragment.BallSpeeds = balls.Select(ball => ball.Speed).ToList();
                    break;
                case GamePhase.Endgame:
                    phaseTime = (int)Cooldowns.Endgame;
                    break;
            }
            this.phase = phase;
            if (nextPhases != null)
                foreach (GamePhase nextPhase in nextPhases)
                    this.nextPhase.Enqueue(nextPhase);

            NextFragment.NewPhase = phase;
            NextFragment.OldPhase = prevPhase;
        }
        private GameStateFragment GetFullStateFragment()
        {
            return new GameStateFragment
            {
                LeftPaddle = leftPaddle,
                LeftPaddlePosition = leftPaddle.Top,
                LeftPaddleSpeed = leftPaddle.Speed.Y,
                LeftPaddleLength = leftPaddle.Length,
                RightPaddle = rightPaddle,
                RightPaddlePosition = rightPaddle.Top,
                RightPaddleSpeed = rightPaddle.Speed.Y,
                RightPaddleLength = rightPaddle.Length,
                Balls = balls,
                BallPositions = balls.Select(ball => ball.Corner).ToList(),
                BallSpeeds = balls.Select(ball => ball.Speed).ToList(),
                Players = AlivePlayers,
                PlayerPositions = AlivePlayers.Select(player => player.Position).ToList(),
                Round = round,
                NewPhase = phase,
                OldPhase = phase
            };
        }

        private void Game_BallCollided(object sender, BallEventArgs e)
        {
            Ball ball = (Ball)sender;
            if (e.Collisions.HasAnyFlag(Collisions.TopBoundary) && ball.Speed.Y < 0)
                ball.Speed.FlipY();
            if (e.Collisions.HasAnyFlag(Collisions.BottomBoundary) && ball.Speed.Y > 0)
                ball.Speed.FlipY();
            if (e.Collisions.HasAnyFlag(Collisions.LeftPaddleFace))
            {
                DeflectBallOffPaddleFace(ball, leftPaddle);
                leftPaddle.Length -= paddleDecay;
                if (leftPaddle.Length <= powerPaddleLengthThrehsold)
                    ball.Speed.Y = MathF.CopySign(powerPaddleVerticalDeflectSpeed, ball.Speed.Y);
                NextFragment.LeftPaddleLength = leftPaddle.Length;
            }
            if (e.Collisions.HasAnyFlag(Collisions.RightPaddleFace))
            {
                DeflectBallOffPaddleFace(ball, rightPaddle);
                rightPaddle.Length -= paddleDecay;
                if (rightPaddle.Length <= powerPaddleLengthThrehsold)
                    ball.Speed.Y = MathF.CopySign(powerPaddleVerticalDeflectSpeed, ball.Speed.Y);
                NextFragment.RightPaddleLength = rightPaddle.Length;
            }

            if (e.Collisions.HasAnyFlag(Collisions.LeftPaddleSide))
                DeflectBallOffPaddleSide(ball, leftPaddle);
            if (e.Collisions.HasAnyFlag(Collisions.RightPaddleSide))
                DeflectBallOffPaddleSide(ball, rightPaddle);


            if (e.Collisions.HasAnyFlag(Collisions.LeftGoal | Collisions.RightGoal))
            {
                ball.Speed.FlipX();
                if (e.Collisions.HasFlag(Collisions.LeftGoal)) Game_GoalScored(Side.Left);
                if (e.Collisions.HasFlag(Collisions.RightGoal)) Game_GoalScored(Side.Right);

            }
            System.Diagnostics.Debug.WriteLine($"T {gameTime} {e.Collisions.ToString()} {(balls.First().Speed.Y > 0f ? "🔽":"🔼")}");
            NextFragment.BallPositions = balls.Select(ball => ball.Corner).ToList();
            NextFragment.BallSpeeds = balls.Select(ball => ball.Speed).ToList();
        }
        private void Game_GoalScored(Side goalSide)
        {
            lastGoalSide = goalSide;
            balls.ForEach(ball => ball.Reset());
            leftPaddle.Length = paddleInitialLength;
            rightPaddle.Length = paddleInitialLength;

            if (goalSide == Side.Left)
            {
                KillTeamAndRedestribute(Side.Left);
            }
            if (goalSide == Side.Right)
            {
                KillTeamAndRedestribute(Side.Right);
            }
            if (AlivePlayerCount >= 2)
            {
                ChangePhase(GamePhase.Ready, new[] { GamePhase.Playing });
                round += 1;
            }
            else if (AlivePlayerCount == 2)
            {
                round += 1;
                ChangePhase(GamePhase.Endgame, new[] { GamePhase.PreGame, GamePhase.Playing });
                AllPlayers.Single(player => player.Side != Side.None).Score += 1 << round - 1;
                round = 1;
            }
            else
            {
                ChangePhase(GamePhase.Endgame, new[] { GamePhase.PreGame, GamePhase.Playing });
                round = 1;
            }

            NextFragment.Round = round;
            NextFragment.Players = AlivePlayers.ToList();
            NextFragment.LeftPaddleLength = leftPaddle.Length;
            NextFragment.RightPaddleLength = rightPaddle.Length;
        }
        private void Game_GamePhaseEnded(object sender, GamePhase e)
        {
            switch (e)
            {
                case GamePhase.Endgame:
                    AllPlayers.ForEach(player =>
                    {
                        if (AllPlayers.IndexOf(player) % 2 == 0)
                        {
                            player.Side = Side.Left;
                            player.Paddle = leftPaddle;
                        }
                        else
                        {
                            player.Side = Side.Right;
                            player.Paddle = rightPaddle;
                        }
                    });
                    leftPaddle.Players = LeftPlayers;
                    rightPaddle.Players = RightPlayers;
                    if (useBoids)
                    {
                        leftFlock.Boids = LeftPlayers.Select(player => new Boid(player)).ToList();
                        rightFlock.Boids = RightPlayers.Select(player => new Boid(player)).ToList();
                    }
                    break;
                case GamePhase.Ready:
                    clients.Select(client => client.Value.Player).ToList().ForEach(player => player.RoundStartPosition = player.Position);
                    break;
                case GamePhase.Playing:
                    var inactiveClients = clients.Where(kv => 
                        kv.Value.Player.Side != Side.None
                        && (kv.Value.Player.RoundStartPosition - kv.Value.Player.Position).Magnitude <= Single.Epsilon)
                    .ToList();
                    if (kickAfk)
                        inactiveClients.ForEach(kv => Disconnect(kv.Value));
                    break;
            }
        }

        /// <summary>
        /// Join game with client transformation parameters.
        /// </summary>
        /// <param name="originX">X pixel coordinate of the center of the playing field.</param>
        /// <param name="originY">Y pixel coordinate of the center of the playing field.</param>
        /// <param name="scaleX">Number of pixels in one engine unit in the X direction.</param>
        /// <param name="scaleY">Number of pixels in one engine unit in the Y direction.</param>
        /// <returns>Generated client instance used to receive game state and pass input.</returns>
        public Client Join(float originX, float originY, float scaleX, float scaleY, string name = null)
        {
            var client = new Client
            {
                Transformation = new Transformation(originX, originY, scaleX, scaleY),
                Player = JoinTeam(Side.None, name)
            };
            if (phase != GamePhase.PreGame)
            {
                client.Player.Side = Side.None;
            }
            client.State = GetFullStateFragment();
            PlayerJoined?.Invoke(this, new PlayerEventArgs(client.Player));
            Debug.WriteLine($"players: {LeftPlayerCount} left, {RightPlayerCount} right, {AllPlayers.Count(player => player.Side == Side.None)} dead");
            clients.GetOrAdd(client.Player.Id, client); // Need to send full state to client before adding the client to game
            return client;
        }
        public void Disconnect(Client client)
        {
            Debug.WriteLine("Disconnecting client");
            ClientDisconnected?.Invoke(this, new PlayerEventArgs(client.Player));
            AllPlayers.Remove(client.Player);
            clients.Remove(client.Player.Id, out _);
        }
        private Player JoinTeam(Side side = Side.None, string name = null)
        {
            if (name == null) name = AnimalNames[new Random().Next(AnimalNames.Length)];
            var newPlayer = new Player(name);
            AllPlayers.Add(newPlayer);
            if ((LeftPlayerCount < RightPlayerCount && side == Side.None) || side == Side.Left)
            {
                newPlayer.Side = Side.Left;
                newPlayer.Paddle = leftPaddle;
                leftPaddle.Players.Add(newPlayer);
            }
            else if (side != Side.Left)
            {
                newPlayer.Side = Side.Right;
                newPlayer.Paddle = rightPaddle;
                rightPaddle.Players.Add(newPlayer);
            }
            return newPlayer;
        }
        private void KillTeamAndRedestribute(Side side)
        {
            //AllPlayers.RemoveAll(player => player.Side == side);
            AllPlayers.ForEach(player => { if (player.Side == side) { player.Side = Side.None; player.Score += 1 << round - 1; } });
            AllPlayers = AllPlayers.OrderByDescending(player => player.Side).ToList(); // living players first
            AllPlayers.ForEach(player => { if (AllPlayers.IndexOf(player) < AlivePlayerCount / 2) player.Side = side; }); // half of the living players go the team that lost
            NextFragment.MinTopTenScore = MinTopTenScore;
            if (AlivePlayerCount == 1) AlivePlayers.Single().Score += 1 << round;
            if (useBoids)
            {
                if (side == Side.Left)
                {
                    leftFlock.Boids.Clear();
                    leftFlock.Boids = rightFlock.Boids.Where(boid => boid.Player.Side == Side.Left).ToList();
                    leftFlock.Boids.ForEach(boid => boid.Center.X *= -1);
                    rightFlock.Boids = rightFlock.Boids.Where(boid => boid.Player.Side == Side.Right).ToList();
                }
                if (side == Side.Right)
                {
                    rightFlock.Boids.Clear();
                    rightFlock.Boids = leftFlock.Boids.Where(boid => boid.Player.Side == Side.Right).ToList();
                    rightFlock.Boids.ForEach(boid => boid.Center.X *= -1);
                    leftFlock.Boids = leftFlock.Boids.Where(boid => boid.Player.Side == Side.Left).ToList();
                }
            }
            leftPaddle.Players = LeftPlayers;
            rightPaddle.Players = RightPlayers;
        }

        private void Ball_BoundaryTouching(object sender, Ball.BoundaryTouchingEventArgs e)
        {
            Ball ball = (Ball)sender;

            if (e.FirstCrossedBoundary(Boundaries.TopBoundary))
                BallCollided(ball, new BallEventArgs(Collisions.TopBoundary));
            if (e.FirstCrossedBoundary(Boundaries.BottomBoundary))
                BallCollided(ball, new BallEventArgs(Collisions.BottomBoundary));
            if (e.FirstCrossedBoundary(Boundaries.LeftGoal))
                BallCollided(ball, new BallEventArgs(Collisions.LeftGoal));
            if (e.FirstCrossedBoundary(Boundaries.RightGoal))
                BallCollided(ball, new BallEventArgs(Collisions.RightGoal));
            if (e.Boundaries.HasFlag(Boundaries.LeftPaddle))
                if (leftPaddle.IsBallTouching(ball))
                    if (e.FirstCrossedBoundary(Boundaries.LeftPaddle))
                        BallCollided(ball, new BallEventArgs(Collisions.LeftPaddleFace));
                    else
                        BallCollided(ball, new BallEventArgs(Collisions.LeftPaddleSide));
            if (e.Boundaries.HasFlag(Boundaries.RightPaddle))
                if (rightPaddle.IsBallTouching(ball))
                    if (e.FirstCrossedBoundary(Boundaries.RightPaddle))
                        BallCollided(ball, new BallEventArgs(Collisions.RightPaddleFace));
                    else
                        BallCollided(ball, new BallEventArgs(Collisions.RightPaddleSide));
        }
        private void DeflectBallOffPaddleFace(Ball ball, Paddle paddle)
        {
            var distance = paddle.GetNormalizedDistanceFromCenter(ball);
            if (-1.0f < distance && distance < 1.0f)
            {
                ball.Speed.FlipX();
                ball.Speed.Y = distance * Math.Abs(ball.Speed.X);
            }
        }
        private void DeflectBallOffPaddleSide(Ball ball, Paddle paddle)
        {
            var distance = paddle.GetNormalizedDistanceFromCenter(ball);
            ball.Speed.FlipX();
            var newMagnitude = MathF.MaxMagnitude(ball.Speed.X, 2.0f * ball.Speed.Y);
            ball.Speed.Y = MathF.CopySign(newMagnitude, distance);
        }

    }
}
