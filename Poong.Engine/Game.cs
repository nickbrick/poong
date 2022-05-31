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

        public static Configuration Config { get; private set; }

        private Flock leftFlock;
        private Flock rightFlock;
        private readonly Timer clock;
        private GamePhase phase = GamePhase.PreGame;
        private GamePhase prevPhase = GamePhase.PreGame;
        private Queue<GamePhase> nextPhases = new Queue<GamePhase>(new[] { GamePhase.Ready, GamePhase.Playing });
        private readonly Paddle leftPaddle;
        private readonly Paddle rightPaddle;
        private readonly List<Ball> balls;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Client> clients =
                     new System.Collections.Concurrent.ConcurrentDictionary<Guid, Client>();
        private List<KeyValuePair<Guid, Client>> InactiveClients => clients.Where(kv =>
                         kv.Value.Player.Side != Side.None
                         && (kv.Value.Player.RoundStartPosition - kv.Value.Player.Position).Magnitude <= Single.Epsilon).ToList();
        private List<Player> AllPlayers { get; set; }
        private List<Player> LeftPlayers => AllPlayers.Where(player => player.Side == Side.Left).ToList();
        private List<Player> RightPlayers => AllPlayers.Where(player => player.Side == Side.Right).ToList();
        private List<Player> AlivePlayers => AllPlayers.Where(player => player.Side != Side.None).ToList();
        private List<Player> DyingPlayers => AllPlayers.Where(player => player.Side == Side.None && player.RoundDied == round).ToList();
        private List<Player> DeadPlayers => AllPlayers.Where(player => player.Side == Side.None && player.RoundDied != round).ToList();
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
        private int phaseTime = 0;
        private Side lastGoalSide = Side.None;
        private Player lastWinner;
        private int ongoingBroadcasts = 0;

        public Game() : this(new Configuration())
        {
        }
        public Game(Configuration config)
        {
            Config = config;
            leftPaddle = new Paddle(Side.Left, Config.PaddleInitialLength);
            rightPaddle = new Paddle(Side.Right, Config.PaddleInitialLength);
            balls = new List<Ball> { new Ball() };
            balls.ForEach(b => b.BoundaryTouching += Ball_BoundaryTouching);

            AllPlayers = new List<Player>(2 * Game.Config.MaxPlayersPerPaddle);

            clock = new Timer(Config.TickMilliseconds);
            phaseTime = Transformation.ToTicks(Cooldowns.Pregame);
            clock.Elapsed += Tick;
            BallCollided += Game_BallCollided;

            if (Config.UseBoids)
            {
                leftFlock = new Flock();
                rightFlock = new Flock();
                for (int i = 0; i < Config.BoidsPerPaddle; i++)
                {
                    leftFlock.Boids.Add(new Boid(AddNewPlayerToGame(null, Side.Left)));
                    rightFlock.Boids.Add(new Boid(AddNewPlayerToGame(null, Side.Right)));
                }
            }

            Start();
        }
        private void Start()
        {
            clock.Enabled = true;
        }
        private void Pause()
        {
            clock.Enabled = false;
        }

        private void BroadcastState()
        {
            if (ongoingBroadcasts > 0)
            {
                Debug.WriteLine("Skipping update");
                return;
            }
            ongoingBroadcasts += 1;
            foreach (var client in clients.Values)
            {
                client.State = NextFragment;
            }
            NextFragment = new GameStateFragment();
            ongoingBroadcasts -= 1;

        }
        private void Tick(object sender, ElapsedEventArgs e)
        {
            leftPaddle.Update(gameTime);
            rightPaddle.Update(gameTime);
            balls.ForEach(b => b.Update(gameTime));

            if (Config.UseBoids)
            {
                leftFlock.X = leftPaddle.Center.X;
                leftFlock.Y = (balls.First().Center.Y * 3 + leftPaddle.Center.Y) / 4;
                leftFlock.Update();

                rightFlock.X = rightPaddle.Center.X;
                rightFlock.Y = (balls.First().Center.Y * 3 + rightPaddle.Center.Y) / 4;
                rightFlock.Update();
            }
            if (phaseTime > 0)
            {
                phaseTime -= 1;
                if (phase == GamePhase.PreGame && AllPlayers.Count < 2) phaseTime += 1;
                if (phaseTime == 0)
                {
                    if (nextPhases.TryDequeue(out var nextPhase))
                        ChangePhase(nextPhase);
                }
            }

            if (gameTime % Config.TicksPerUpdate == 0)
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
        }

        private void ChangePhase(GamePhase phase, GamePhase[] nextPhases = null)
        {
            prevPhase = this.phase;
            Game_GamePhaseEnded(this, prevPhase);
            switch (phase)
            {
                case GamePhase.PreGame:
                    phaseTime = Transformation.ToTicks(Cooldowns.Pregame);
                    break;
                case GamePhase.Ready:
                    phaseTime = Transformation.ToTicks(Cooldowns.Goal);
                    break;
                case GamePhase.Playing:
                    break;
                case GamePhase.Endgame:
                    phaseTime = Transformation.ToTicks(Cooldowns.Endgame);
                    break;
            }
            this.phase = phase;
            Game_GamePhaseStarting(this, phase);
            if (nextPhases != null)
                foreach (GamePhase nextPhase in nextPhases)
                    this.nextPhases.Enqueue(nextPhase);

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
                leftPaddle.Length -= Config.PaddleDecay;
                if (leftPaddle.Length <= Config.PowerPaddleLengthThreshold)
                    ball.Speed.Y = MathF.CopySign(Config.PowerPaddleVerticalDeflectSpeed, ball.Speed.Y);
                NextFragment.LeftPaddleLength = leftPaddle.Length;
            }
            if (e.Collisions.HasAnyFlag(Collisions.RightPaddleFace))
            {
                DeflectBallOffPaddleFace(ball, rightPaddle);
                rightPaddle.Length -= Config.PaddleDecay;
                if (rightPaddle.Length <= Config.PowerPaddleLengthThreshold)
                    ball.Speed.Y = MathF.CopySign(Config.PowerPaddleVerticalDeflectSpeed, ball.Speed.Y);
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
            //System.Diagnostics.Debug.WriteLine($"T {gameTime} {e.Collisions} {(balls.First().Speed.Y > 0f ? "🔽" : "🔼")}");
            NextFragment.BallPositions = balls.Select(ball => ball.Corner).ToList();
            NextFragment.BallSpeeds = balls.Select(ball => ball.Speed).ToList();
        }
        private void Game_GoalScored(Side goalSide)
        {
            lastGoalSide = goalSide;
            balls.ForEach(ball => ball.Reset());
            leftPaddle.Length = Config.PaddleInitialLength;
            rightPaddle.Length = Config.PaddleInitialLength;

            KillTeamAndRedestribute(goalSide);

            var points = 1 << round - 1;
            DyingPlayers.ForEach(player =>
            {
                player.Score += points;
                player.Client?.Notify($"You died! For making it to round {round}, you earn {points} point{(points > 1 ? "s" : "")}.");
            });

            if (AlivePlayerCount >= 2)
            {
                AlivePlayers.ForEach(player => player.Client?.Notify($"Your side scored! Get ready for the next round." + (Config.ShowPlayerSide ? $" You are {player.Side}." : "")));
                DeadPlayers.ForEach(player => player.Client?.Notify($"{goalSide} side died! Survivors split up and move on to the next round."));
                round += 1;
                ChangePhase(GamePhase.Ready, new[] { GamePhase.Playing });
            }
            else if (AlivePlayerCount == 1)
            {
                points = 1 << round;
                var winner = AlivePlayers.Single();
                winner.Score += points;
                winner.Client?.Notify($"You won! For making it to round {round}, you earn {points} points.");
                lastWinner = winner;

                DeadPlayers.ForEach(player => player.Client?.Notify($"{winner.Name} wins the game!"));

                ChangePhase(GamePhase.Endgame, new[] { GamePhase.PreGame, GamePhase.Ready, GamePhase.Playing });
                round = 1;
            }
            else
            {
                ChangePhase(GamePhase.Endgame, new[] { GamePhase.PreGame, GamePhase.Ready, GamePhase.Playing });
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
                    if (Config.UseBoids)
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
                    if (Config.KickAfk)
                    {
                        inactiveClients.ForEach(async kv => await KickInactiveClient(kv.Value));
                    }
                    break;
            }
        }
        private void Game_GamePhaseStarting(object sender, GamePhase e)
        {
            switch (e)
            {
                case GamePhase.PreGame:
                    AllPlayers.ForEach(player => player.Client?.Notify($"New game starting soon."));
                    UpdateRanking();
                    NextFragment.Players = AlivePlayers;
                    break;
                case GamePhase.Ready:
                    if (round == 1)
                        AlivePlayers.ForEach(player => player.Client?.Notify($"{player.Name}: Get ready for the first round."+(Config.ShowPlayerSide?$" You are {player.Side}.":"")));
                    break;
                case GamePhase.Playing:
                    balls.ForEach(ball => ball.Launch(lastGoalSide));
                    NextFragment.BallSpeeds = balls.Select(ball => ball.Speed).ToList();
                    AlivePlayers.ForEach(player => player.Client?.Notify($"Round {round}: {AlivePlayerCount} players remain."));
                    DeadPlayers.ForEach(player => player.Client?.Notify($"Round {round}: {AlivePlayerCount} players remain. You are dead! Please wait to join the next game."));
                    break;
                case GamePhase.Endgame:
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
        public Client Connect(float originX, float originY, float scaleX, float scaleY)
        {
            if (clients.Count == 0)
                Start();

            var client = new Client
            {
                Transformation = new Transformation(originX, originY, scaleX, scaleY),
            };

            return client;
        }
        public void Join(Client client, string name = null)
        {
            if (name == null) name = AnimalNames[new Random().Next(AnimalNames.Length)];
            var newPlayer = new Player(name, client);
            client.Player = newPlayer;

            AddPlayerToGame(newPlayer);

            client.State = GetFullStateFragment();

            PlayerJoined?.Invoke(this, new PlayerEventArgs(client.Player));
            Debug.WriteLine($"players: {LeftPlayerCount} left, {RightPlayerCount} right, {AllPlayers.Count(player => player.Side == Side.None)} dead");
            clients.GetOrAdd(client.Player.Id, client); // Need to send full state to client before adding the client to game
        }
        public void Disconnect(Client client) // TODO look into player scramble bug on disconnect
        {
            Debug.WriteLine("Disconnecting client");
            ClientDisconnected?.Invoke(this, new PlayerEventArgs(client.Player));
            AllPlayers.Remove(client.Player);
            clients.Remove(client.Player.Id, out _);
            if (clients.Count == 0)
                Pause();
        }
        public async System.Threading.Tasks.Task KickInactiveClient(Client client)
        {
            client.Notify("You have been kicked for inactivity.");
            await System.Threading.Tasks.Task.Delay(100);
            Disconnect(client);
        }
        // TODO these are a fucking mess sort them out
        private Player AddNewPlayerToGame(string name, Side side = Side.None)
        {
            if (name == null) name = AnimalNames[new Random().Next(AnimalNames.Length)];
            var newPlayer = new Player(name);
            AddPlayerToGame(newPlayer, side);
            return newPlayer;
        }
        private Player AddPlayerToGame(Player player, Side side = Side.None)
        {
            AllPlayers.Add(player);
            if (phase == GamePhase.PreGame)
            {
                player.Client?.Notify($"Welcome, {player.Name}. The game will start soon.");
                return AddPlayerToTeam(player, side);
            }
            player.Client?.Notify($"Welcome, {player.Name}. You will join as soon as this game ends.");
            return player;
        }
        private Player AddPlayerToTeam(Player player, Side side = Side.None)
        {
            if ((LeftPlayerCount < RightPlayerCount && side == Side.None) || side == Side.Left)
            {
                player.Side = Side.Left;
                player.Paddle = leftPaddle;
                leftPaddle.Players.Add(player);
            }
            else if (side != Side.Left)
            {
                player.Side = Side.Right;
                player.Paddle = rightPaddle;
                rightPaddle.Players.Add(player);
            }
            return player;
        }
        private void KillTeamAndRedestribute(Side side)
        {
            AllPlayers.ForEach(player => { if (player.Side == side) { player.RoundDied = round; player.Side = Side.None; } });
            AllPlayers = AllPlayers.OrderByDescending(player => player.Side).ToList(); // living players first
            AllPlayers.ForEach(player => { if (AllPlayers.IndexOf(player) < AlivePlayerCount / 2) player.Side = side; }); // half of the living players go the team that lost
            NextFragment.MinTopTenScore = MinTopTenScore;

            if (Config.UseBoids)
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
        private void UpdateRanking()
        {
            AllPlayers.ForEach(player => player.Rank = AllPlayers.Count(player_ => player_.Score > player.Score) + 1);
        }
        private void Ball_BoundaryTouching(object sender, Ball.BoundaryTouchingEventArgs e)
        {
            Ball ball = (Ball)sender;

            if (e.Boundaries.HasFlag(Boundaries.TopBoundary))
                BallCollided(ball, new BallEventArgs(Collisions.TopBoundary));
            if (e.Boundaries.HasFlag(Boundaries.BottomBoundary))
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