using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FlappyBird.Models;

namespace FlappyBird;

/// <summary>
/// Ana pencere: zamanlayıcı ile oyun döngüsünü çalıştırır, klavye ve fare
/// olaylarını GameManager'a iletir, skor/durum bilgilerini günceller.
/// </summary>
public partial class MainWindow : Window
{
    private const int GameWidth = 480;
    private const int GameHeight = 640;

    private readonly GameManager game;
    private readonly DispatcherTimer gameTimer;

    // Tuş basılı tutulduğunda KeyDown sürekli tekrar eder; kuşun her basışta yalnızca
    // bir kez zıplaması için tuşun bırakıldığını KeyUp ile takip ediyoruz.
    private bool flapKeyHeld;

    public MainWindow()
    {
        InitializeComponent();

        game = new GameManager(GameWidth, GameHeight, new HighScoreStore());
        GameView.Game = game;

        // Oyun döngüsü: yaklaşık 50 kare/saniye
        gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        gameTimer.Tick += GameTimer_Tick;

        game.ScoreChanged += Game_ScoreChanged;
        game.StateChanged += Game_StateChanged;

        // Tunnel: tuş olayı, odaktaki kontrole ulaşmadan önce pencereye gelir.
        // Böylece Space/ok tuşları butonlara veya odak değiştirmeye gitmeden oyunda kullanılır.
        AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, Window_KeyUp, RoutingStrategies.Tunnel);

        GameView.PointerPressed += GameView_PointerPressed;
        Opened += MainWindow_Opened;
        Closed += MainWindow_Closed;
        Deactivated += MainWindow_Deactivated;

        UpdateInfo();
    }

    // ===================== Olay işleyicileri =====================

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        GameView.Focus();
        gameTimer.Start();
    }

    private void MainWindow_Closed(object? sender, EventArgs e) => gameTimer.Stop();

    /// <summary>Pencere arka plana geçerse (başka uygulamaya geçildi) oyun kendiliğinden duraklar.</summary>
    private void MainWindow_Deactivated(object? sender, EventArgs e)
    {
        if (game.State == GameState.Playing)
            game.TogglePause();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        game.Update();
        GameView.InvalidateVisual();  // Yeniden çizimi tetikler
    }

    private void NewGameButton_Click(object? sender, RoutedEventArgs e) => StartNewGame();

    private void GameView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(GameView).Properties.IsLeftButtonPressed)
            game.Flap();
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
            case Key.Up:
            case Key.W:
                if (!flapKeyHeld)
                {
                    flapKeyHeld = true;
                    game.Flap();
                }
                e.Handled = true;
                break;

            case Key.Enter:
            case Key.R:
                if (game.State == GameState.GameOver)
                    StartNewGame();
                e.Handled = true;
                break;

            case Key.P:
            case Key.Escape:
                game.TogglePause();
                e.Handled = true;
                break;
        }
    }

    private void Window_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Space or Key.Up or Key.W)
            flapKeyHeld = false;
    }

    private void Game_ScoreChanged(object? sender, EventArgs e) => UpdateInfo();

    private void Game_StateChanged(object? sender, EventArgs e)
    {
        if (game.State == GameState.GameOver)
            gameTimer.Stop();  // Oyun durur

        UpdateInfo();
        GameView.InvalidateVisual();
    }

    // ===================== Yardımcı metotlar =====================

    private void StartNewGame()
    {
        game.NewGame();
        flapKeyHeld = false;
        gameTimer.Start();
        GameView.Focus();
        GameView.InvalidateVisual();
    }

    private void UpdateInfo()
    {
        ScoreText.Text = $"Skor: {game.Score}";
        HighScoreText.Text = $"En Yüksek: {game.HighScore}";
        LevelText.Text = $"Seviye: {game.Level}";

        string status = game.State switch
        {
            GameState.Ready => "Hazır — başlamak için SPACE / ↑ tuşuna bas veya tıkla",
            GameState.Playing => $"Oynanıyor — hız: {game.Speed:0.0}   (P: duraklat)",
            GameState.Paused => "Duraklatıldı — devam etmek için P / Esc tuşuna bas",
            GameState.GameOver => "Oyun bitti — yeniden başlamak için Enter / R veya Yeni Oyun",
            _ => string.Empty,
        };

        // Skor dosyasına erişilemediyse kullanıcıya durum çubuğunda bildir
        if (game.HighScoreError is not null)
            status += $"   ⚠ En yüksek skor dosyasına erişilemedi: {game.HighScoreError}";

        StatusText.Text = status;
    }
}
