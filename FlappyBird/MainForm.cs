using System.Drawing.Drawing2D;
using FlappyBird.Models;

namespace FlappyBird;

/// <summary>
/// Ana pencere: arayüz kontrollerini oluşturur, Timer ile oyun döngüsünü çalıştırır,
/// klavye olaylarını GameManager'a iletir ve oyunu çizer.
/// </summary>
public class MainForm : Form
{
    private const int GameWidth = 480;
    private const int GameHeight = 640;

    private readonly GameManager game;
    private readonly System.Windows.Forms.Timer gameTimer;

    private readonly GameCanvas canvas;
    private readonly Button btnNewGame;
    private readonly Label lblScore;
    private readonly Label lblHighScore;
    private readonly Label lblLevel;
    private readonly ToolStripStatusLabel lblStatus;

    private readonly Font bigFont = new("Segoe UI", 36, FontStyle.Bold);
    private readonly Font mediumFont = new("Segoe UI", 16, FontStyle.Bold);
    private readonly Font smallFont = new("Segoe UI", 11, FontStyle.Regular);
    private readonly Font labelFont = new("Segoe UI", 10, FontStyle.Bold);
    private readonly Brush groundBrush = new SolidBrush(Color.FromArgb(222, 184, 135));
    private readonly Brush grassBrush = new SolidBrush(Color.FromArgb(110, 190, 70));
    private readonly Brush overlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));

    // Tuş basılı tutulduğunda KeyDown sürekli tekrar eder; kuşun her basışta yalnızca
    // bir kez zıplaması için tuşun bırakıldığını KeyUp ile takip ediyoruz.
    private bool flapKeyHeld;

    public MainForm()
    {
        Text = "YMH219 - Flappy Bird";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;  // Tuşlar hangi kontrol odakta olursa olsun önce formun KeyDown'ına gelsin

        game = new GameManager(GameWidth, GameHeight, new HighScoreStore());

        // --- Üst bilgi çubuğu ---
        btnNewGame = new Button
        {
            Text = "Yeni Oyun",
            AutoSize = true,
            TabStop = false,
            Margin = new Padding(8, 8, 16, 8),
        };
        btnNewGame.Click += BtnNewGame_Click;

        lblScore = CreateInfoLabel();
        lblHighScore = CreateInfoLabel();
        lblLevel = CreateInfoLabel();

        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            WrapContents = false,
            BackColor = Color.FromArgb(45, 45, 48),
        };
        topPanel.Controls.AddRange(new Control[] { btnNewGame, lblScore, lblHighScore, lblLevel });

        // --- Alt durum çubuğu ---
        lblStatus = new ToolStripStatusLabel();
        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(lblStatus);

        // --- Oyun alanı ---
        canvas = new GameCanvas { Dock = DockStyle.Fill, BackColor = Color.SkyBlue };
        canvas.Paint += Canvas_Paint;
        canvas.MouseDown += Canvas_MouseDown;
        canvas.PreviewKeyDown += Canvas_PreviewKeyDown;

        // Dock sırası: önce Fill eklenir, sonra Top/Bottom; böylece canvas kalan alanı doldurur.
        Controls.Add(canvas);
        Controls.Add(topPanel);
        Controls.Add(statusStrip);
        ClientSize = new Size(GameWidth, GameHeight + topPanel.Height + statusStrip.Height);

        // --- Oyun döngüsü (yaklaşık 50 kare/saniye) ---
        gameTimer = new System.Windows.Forms.Timer { Interval = 20 };
        gameTimer.Tick += GameTimer_Tick;

        // --- Olaylar ---
        game.ScoreChanged += Game_ScoreChanged;
        game.StateChanged += Game_StateChanged;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        Shown += MainForm_Shown;

        UpdateInfo();
    }

    private Label CreateInfoLabel() => new()
    {
        AutoSize = true,
        ForeColor = Color.White,
        Font = labelFont,
        Margin = new Padding(0, 14, 16, 0),
    };

    // ===================== Olay işleyicileri =====================

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        canvas.Focus();
        gameTimer.Start();

        if (game.HighScoreError is not null)
            ShowHighScoreError();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        game.Update();
        canvas.Invalidate();  // Paint olayını tetikler
    }

    private void BtnNewGame_Click(object? sender, EventArgs e) => StartNewGame();

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            game.Flap();
    }

    /// <summary>
    /// Yön tuşları normalde kontroller arasında odak değiştirmek için kullanılır ve
    /// KeyDown'a hiç ulaşmaz. Burada onları "giriş tuşu" olarak işaretliyoruz.
    /// </summary>
    private void Canvas_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode is Keys.Up or Keys.Down)
            e.IsInputKey = true;
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Space:
            case Keys.Up:
            case Keys.W:
                if (!flapKeyHeld)
                {
                    flapKeyHeld = true;
                    game.Flap();
                }
                e.SuppressKeyPress = true;
                break;

            case Keys.Enter:
            case Keys.R:
                if (game.State == GameState.GameOver)
                    StartNewGame();
                e.SuppressKeyPress = true;
                break;
        }
    }

    private void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Up or Keys.W)
            flapKeyHeld = false;
    }

    private void Game_ScoreChanged(object? sender, EventArgs e) => UpdateInfo();

    private void Game_StateChanged(object? sender, EventArgs e)
    {
        if (game.State == GameState.GameOver)
        {
            gameTimer.Stop();  // Oyun durur
            if (game.HighScoreError is not null)
                ShowHighScoreError();
        }

        UpdateInfo();
        canvas.Invalidate();
    }

    // ===================== Yardımcı metotlar =====================

    private void StartNewGame()
    {
        game.NewGame();
        flapKeyHeld = false;
        gameTimer.Start();
        canvas.Focus();  // Butona tıklandıktan sonra tuşlar tekrar oyuna gitsin
        canvas.Invalidate();
    }

    private void UpdateInfo()
    {
        lblScore.Text = $"Skor: {game.Score}";
        lblHighScore.Text = $"En Yüksek: {game.HighScore}";
        lblLevel.Text = $"Seviye: {game.Level}";

        lblStatus.Text = game.State switch
        {
            GameState.Ready => "Hazır — başlamak için SPACE / ↑ tuşuna bas veya tıkla",
            GameState.Playing => $"Oynanıyor — hız: {game.Speed:0.0}",
            GameState.GameOver => "Oyun bitti — yeniden başlamak için Enter / R veya Yeni Oyun",
            _ => string.Empty,
        };
    }

    private void ShowHighScoreError()
    {
        MessageBox.Show(
            "En yüksek skor dosyasına erişilemedi:\n" + game.HighScoreError,
            "Uyarı",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    // ===================== Çizim =====================

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        DrawBackground(g);
        foreach (PipePair pipe in game.Pipes)
            pipe.Draw(g);
        DrawGround(g);
        game.Bird.Draw(g);

        switch (game.State)
        {
            case GameState.Ready:
                DrawCenteredText(g, "Hazır!", bigFont, 170);
                DrawCenteredText(g, "SPACE / ↑ / tıkla: zıpla", smallFont, 240);
                break;

            case GameState.Playing:
                DrawCenteredText(g, game.Score.ToString(), bigFont, 30);
                break;

            case GameState.GameOver:
                DrawGameOver(g);
                break;
        }
    }

    private void DrawBackground(Graphics g)
    {
        var sky = new Rectangle(0, 0, GameWidth, game.PlayAreaHeight);
        using var skyBrush = new LinearGradientBrush(
            sky, Color.FromArgb(110, 197, 233), Color.FromArgb(190, 235, 250), LinearGradientMode.Vertical);
        g.FillRectangle(skyBrush, sky);

        // Birkaç sabit bulut
        DrawCloud(g, 60, 90);
        DrawCloud(g, 300, 160);
        DrawCloud(g, 180, 300);
    }

    private static void DrawCloud(Graphics g, int x, int y)
    {
        g.FillEllipse(Brushes.White, x, y, 60, 30);
        g.FillEllipse(Brushes.White, x + 25, y - 15, 50, 40);
        g.FillEllipse(Brushes.White, x + 55, y, 50, 30);
    }

    private void DrawGround(Graphics g)
    {
        int top = game.PlayAreaHeight;
        g.FillRectangle(groundBrush, 0, top, GameWidth, GameManager.GroundHeight);
        g.FillRectangle(grassBrush, 0, top, GameWidth, 14);
        g.DrawLine(Pens.DarkGreen, 0, top, GameWidth, top);
    }

    private void DrawGameOver(Graphics g)
    {
        g.FillRectangle(overlayBrush, 40, 150, GameWidth - 80, 260);

        DrawCenteredText(g, "GAME OVER", bigFont, 170);
        DrawCenteredText(g, $"Skor: {game.Score}", mediumFont, 245);

        string best = game.IsNewHighScore ? "Yeni rekor!" : $"En yüksek: {game.HighScore}";
        DrawCenteredText(g, best, mediumFont, 285);

        DrawCenteredText(g, "Enter / R: yeniden başla", smallFont, 350);
    }

    /// <summary>Metni yatayda ortalayıp gölgeli olarak çizer.</summary>
    private static void DrawCenteredText(Graphics g, string text, Font font, int y)
    {
        using var format = new StringFormat { Alignment = StringAlignment.Center };
        var area = new RectangleF(0, y, GameWidth, font.Height + 10);
        var shadow = area with { X = area.X + 2, Y = area.Y + 2 };

        g.DrawString(text, font, Brushes.Black, shadow, format);
        g.DrawString(text, font, Brushes.White, area, format);
    }

    // ===================== Kaynak yönetimi =====================

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            gameTimer.Dispose();
            bigFont.Dispose();
            mediumFont.Dispose();
            smallFont.Dispose();
            labelFont.Dispose();
            groundBrush.Dispose();
            grassBrush.Dispose();
            overlayBrush.Dispose();
        }
        base.Dispose(disposing);
    }
}
