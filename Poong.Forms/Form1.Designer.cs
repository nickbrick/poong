
namespace Poong.Forms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.GamePanel = new System.Windows.Forms.Panel();
            this.Ball = new System.Windows.Forms.PictureBox();
            this.RightPaddle = new System.Windows.Forms.PictureBox();
            this.LeftPaddle = new System.Windows.Forms.PictureBox();
            this.phaseLabel = new System.Windows.Forms.Label();
            this.TweenTimer = new System.Windows.Forms.Timer(this.components);
            this.leftPlayersLabel = new System.Windows.Forms.Label();
            this.rightPlayersLabel = new System.Windows.Forms.Label();
            this.messagesLabel = new System.Windows.Forms.Label();
            this.GamePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ball)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightPaddle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftPaddle)).BeginInit();
            this.SuspendLayout();
            // 
            // GamePanel
            // 
            this.GamePanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.GamePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GamePanel.Controls.Add(this.Ball);
            this.GamePanel.Controls.Add(this.RightPaddle);
            this.GamePanel.Controls.Add(this.LeftPaddle);
            this.GamePanel.Location = new System.Drawing.Point(299, 142);
            this.GamePanel.Margin = new System.Windows.Forms.Padding(0);
            this.GamePanel.MaximumSize = new System.Drawing.Size(7000, 5000);
            this.GamePanel.Name = "GamePanel";
            this.GamePanel.Size = new System.Drawing.Size(700, 500);
            this.GamePanel.TabIndex = 0;
            this.GamePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GamePanel_Paint);
            this.GamePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GamePanel_MouseMove);
            // 
            // Ball
            // 
            this.Ball.BackColor = System.Drawing.SystemColors.HighlightText;
            this.Ball.Location = new System.Drawing.Point(240, 210);
            this.Ball.Name = "Ball";
            this.Ball.Size = new System.Drawing.Size(20, 20);
            this.Ball.TabIndex = 2;
            this.Ball.TabStop = false;
            // 
            // RightPaddle
            // 
            this.RightPaddle.BackColor = System.Drawing.SystemColors.HighlightText;
            this.RightPaddle.Location = new System.Drawing.Point(480, 216);
            this.RightPaddle.Name = "RightPaddle";
            this.RightPaddle.Size = new System.Drawing.Size(20, 80);
            this.RightPaddle.TabIndex = 1;
            this.RightPaddle.TabStop = false;
            // 
            // LeftPaddle
            // 
            this.LeftPaddle.BackColor = System.Drawing.SystemColors.HighlightText;
            this.LeftPaddle.Location = new System.Drawing.Point(0, 216);
            this.LeftPaddle.Name = "LeftPaddle";
            this.LeftPaddle.Size = new System.Drawing.Size(20, 80);
            this.LeftPaddle.TabIndex = 0;
            this.LeftPaddle.TabStop = false;
            // 
            // phaseLabel
            // 
            this.phaseLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.phaseLabel.Font = new System.Drawing.Font("OCR A Extended", 28.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.phaseLabel.ForeColor = System.Drawing.Color.Snow;
            this.phaseLabel.Location = new System.Drawing.Point(251, 0);
            this.phaseLabel.MinimumSize = new System.Drawing.Size(200, 0);
            this.phaseLabel.Name = "phaseLabel";
            this.phaseLabel.Size = new System.Drawing.Size(800, 67);
            this.phaseLabel.TabIndex = 1;
            this.phaseLabel.Text = "phase";
            this.phaseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // leftPlayersLabel
            // 
            this.leftPlayersLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPlayersLabel.Font = new System.Drawing.Font("OCR A Extended", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.leftPlayersLabel.ForeColor = System.Drawing.Color.Snow;
            this.leftPlayersLabel.Location = new System.Drawing.Point(0, 0);
            this.leftPlayersLabel.MinimumSize = new System.Drawing.Size(200, 0);
            this.leftPlayersLabel.Name = "leftPlayersLabel";
            this.leftPlayersLabel.Size = new System.Drawing.Size(294, 794);
            this.leftPlayersLabel.TabIndex = 2;
            this.leftPlayersLabel.Text = "left players";
            // 
            // rightPlayersLabel
            // 
            this.rightPlayersLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightPlayersLabel.Font = new System.Drawing.Font("OCR A Extended", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rightPlayersLabel.ForeColor = System.Drawing.Color.Snow;
            this.rightPlayersLabel.Location = new System.Drawing.Point(1005, 0);
            this.rightPlayersLabel.MinimumSize = new System.Drawing.Size(200, 0);
            this.rightPlayersLabel.Name = "rightPlayersLabel";
            this.rightPlayersLabel.Size = new System.Drawing.Size(294, 794);
            this.rightPlayersLabel.TabIndex = 3;
            this.rightPlayersLabel.Text = "right players";
            this.rightPlayersLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // messagesLabel
            // 
            this.messagesLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.messagesLabel.Font = new System.Drawing.Font("OCR A Extended", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.messagesLabel.ForeColor = System.Drawing.Color.Snow;
            this.messagesLabel.Location = new System.Drawing.Point(251, 67);
            this.messagesLabel.MinimumSize = new System.Drawing.Size(200, 0);
            this.messagesLabel.Name = "messagesLabel";
            this.messagesLabel.Size = new System.Drawing.Size(800, 67);
            this.messagesLabel.TabIndex = 4;
            this.messagesLabel.Text = "message";
            this.messagesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlText;
            this.ClientSize = new System.Drawing.Size(1299, 794);
            this.Controls.Add(this.messagesLabel);
            this.Controls.Add(this.rightPlayersLabel);
            this.Controls.Add(this.leftPlayersLabel);
            this.Controls.Add(this.phaseLabel);
            this.Controls.Add(this.GamePanel);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.GamePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Ball)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightPaddle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftPaddle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel GamePanel;
        private System.Windows.Forms.PictureBox Ball;
        private System.Windows.Forms.PictureBox RightPaddle;
        private System.Windows.Forms.PictureBox LeftPaddle;
        private System.Windows.Forms.Label phaseLabel;
        private System.Windows.Forms.Timer TweenTimer;
        private System.Windows.Forms.Label leftPlayersLabel;
        private System.Windows.Forms.Label rightPlayersLabel;
        private System.Windows.Forms.Label messagesLabel;
    }
}

