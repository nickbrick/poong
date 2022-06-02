using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Poong.Engine;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Poong.Forms
{
    public partial class Form1 : Form
    {
        private const int pixelsPerGameUnit = 200;
        private Configuration poongConfig;
        private Game game;
        private Point lastMousePosition = new Point(0, 0);
        private List<Player> players;
        private List<Point> playerPositions;
        private PointF ballPositionF;
        private int round;
        private GamePhase phase;
        private Client client;
        private GameStateFragment initParameters;
        public Form1(Game game = null)
        {
            InitializeComponent();
            string jsonConfig = File.ReadAllText("poongConfig.json");
            poongConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Poong.Engine.Configuration>(jsonConfig);
            this.game = game == null ? new Poong.Engine.Game(poongConfig) : game;
            GamePanel.Width = (int)(poongConfig.HorizontalHalfSize * 2 * pixelsPerGameUnit);
            GamePanel.Height = (int)(poongConfig.VerticalHalfSize * 2 * pixelsPerGameUnit);
            client = this.game.Connect(
                poongConfig.HorizontalHalfSize * pixelsPerGameUnit,
                poongConfig.VerticalHalfSize * pixelsPerGameUnit,
                pixelsPerGameUnit,
                pixelsPerGameUnit);
            this.game.Join(client, "Human");
            initParameters = client.State;

            this.game.ClockTicked += Game_ClockTicked;
            client.StateChanged += Client_StateChanged;
            client.NotificationReceived += Client_NotificationReceived;
        }



        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Client_StateChanged(null, new GameStateEventArgs(initParameters));
        }
        private void Game_ClockTicked(object sender, ClockEventArgs e)
        {
            DoTweens();
        }

        private void DoTweens()
        {
            Invoke((MethodInvoker)delegate
            {
                if (phase == GamePhase.Playing)
                {
                    ballPositionF.X += (float)((Vector)(Ball.Tag)).X;
                    ballPositionF.Y += (float)((Vector)(Ball.Tag)).Y;
                    UpdateBallLocation();
                }
                LeftPaddle.Location = new Point(LeftPaddle.Location.X, LeftPaddle.Location.Y + (int)(float)LeftPaddle.Tag);
                RightPaddle.Location = new Point(RightPaddle.Location.X, RightPaddle.Location.Y + (int)(float)RightPaddle.Tag);
            });
        }

        private void Client_StateChanged(object sender, GameStateEventArgs e)
        {
            try
            {
                var state = e.State;
                Invoke((MethodInvoker)delegate
                {
                    if (state.Balls?.Count > 0)
                    {
                        ballPositionF = new PointF(state.Balls.First().Center.X, state.Balls.First().Center.Y);
                        UpdateBallLocation();
                        Ball.Size = new Size((int)initParameters.Balls.First().Size.Width, (int)initParameters.Balls.First().Size.Height);
                        Ball.Tag = state.Balls.First().Speed;
                    }
                    if (state.BallPositions?.Count > 0)
                    {
                        ballPositionF = new PointF(state.BallPositions.First().X, state.BallPositions.First().Y);
                        UpdateBallLocation();
                    }
                    if (state.BallSpeeds?.Count > 0)
                    {
                        Ball.Tag = state.BallSpeeds.First();
                    }
                    if (state.LeftPaddle != null)
                    {
                        LeftPaddle.Location = new Point((int)state.LeftPaddle.Corner.X, (int)state.LeftPaddle.Corner.Y);
                        LeftPaddle.Size = new Size((int)initParameters.LeftPaddle.Size.Width, (int)initParameters.LeftPaddle.Size.Height);
                        LeftPaddle.Tag = state.LeftPaddle.Speed;
                        LeftPaddle.Height = (int)state.LeftPaddle.Size.Height;
                    }
                    if (state.RightPaddle != null)
                    {
                        RightPaddle.Location = new Point((int)state.RightPaddle.Corner.X, (int)state.RightPaddle.Corner.Y);
                        RightPaddle.Size = new Size((int)initParameters.RightPaddle.Size.Width, (int)initParameters.RightPaddle.Size.Height);
                        RightPaddle.Tag = state.RightPaddle.Speed;
                        RightPaddle.Height = (int)state.RightPaddle.Size.Height;
                    }
                    LeftPaddle.Location = new Point(LeftPaddle.Location.X, (int)state.LeftPaddlePosition);
                    RightPaddle.Location = new Point(RightPaddle.Location.X, (int)state.RightPaddlePosition);

                    LeftPaddle.Tag = state.LeftPaddleSpeed;
                    RightPaddle.Tag = state.RightPaddleSpeed;

                    if (state.LeftPaddleLength != null)
                        LeftPaddle.Height = (int)state.LeftPaddleLength;
                    if (state.RightPaddleLength != null)
                        RightPaddle.Height = (int)state.RightPaddleLength;

                    if (state.Round != null)
                    {
                        round = state.Round.Value;
                    }

                    if (state.NewPhase != null)
                    {
                        if (state.NewPhase != GamePhase.Endgame)
                            phaseLabel.Text = $"Round {round}: {state.NewPhase.ToString()}";
                        else
                            phaseLabel.Text = $"Winner: {state.Players.Single(player => player.Side != Side.None).Name}";
                        phase = state.NewPhase.Value;
                    }
                    if (state.Players != null)
                    {
                        players = state.Players;//.Select(player => new Point((int)player.Position.X, (int)player.Position.Y)).ToList();
                        leftPlayersLabel.Text = String.Join("\n", players.Where(player => player.Side == Side.Left).Select(player => $"{player.Name}({player.Score})"));
                        rightPlayersLabel.Text = String.Join("\n", players.Where(player => player.Side == Side.Right).Select(player => $"{player.Name}({player.Score})"));
                    }
                    GamePanel.Invalidate();
                });
                client.TryGiveInput(lastMousePosition.X, lastMousePosition.Y);
                playerPositions = state.PlayerPositions.Select(position => new Point((int)position.X, (int)position.Y)).ToList();


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void Client_NotificationReceived(object sender, string e)
        {
            Invoke((MethodInvoker)delegate
            {
                messagesLabel.Text = e;
            });
        }
        private void GamePanel_MouseMove(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.White, 1))
                e.Graphics.DrawRectangles(pen, playerPositions.Select(position => new RectangleF(position.X, position.Y, 2, 2)).ToArray());
        }

        private void UpdateBallLocation()
        {
            Ball.Location = new Point((int)Math.Round(ballPositionF.X), (int)ballPositionF.Y);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Debug.WriteLine(e.KeyData.ToString());
            if (e.KeyData == Keys.Z)
            {
                new Form1(game).Show();
            }
        }
    }
}
