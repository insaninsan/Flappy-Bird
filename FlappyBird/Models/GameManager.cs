namespace FlappyBird.Models;

public enum GameState
{
    Ready,     // Oyun başlamayı bekliyor
    Playing,   // Oyun devam ediyor
    GameOver,  // Kuş çarptı, oyun durdu
}

/// <summary>
/// Oyunun kurallarını ve durumunu yönetir: kuş, borular, skor, hız ve seviye.
/// Arayüzden bağımsızdır; form yalnızca Update() çağırır ve olayları dinler.
/// </summary>
public class GameManager
{
    public const int GroundHeight = 70;
    public const int PointsPerLevel = 5;  // Her 5 puanda bir seviye atlanır ve hız artar

    private const int PipeCount = 3;
    private const int PipeSpacing = 230;  // İki boru arasındaki yatay mesafe
    private const int GapSize = 160;
    private const int MinGapMargin = 60;  // Boşluğun ekran kenarına en fazla ne kadar yaklaşabileceği
    private const float StartSpeed = 3.5f;
    private const float SpeedIncrease = 0.5f;
    private const float MaxSpeed = 9f;

    private readonly Random random = new();
    private readonly List<PipePair> pipes = new();
    private readonly HighScoreStore highScoreStore;

    public GameManager(int width, int height, HighScoreStore highScoreStore)
    {
        Width = width;
        Height = height;
        this.highScoreStore = highScoreStore;

        Bird = new Bird(width / 4f, PlayAreaHeight / 2f);
        for (int i = 0; i < PipeCount; i++)
            pipes.Add(new PipePair(0, 0, GapSize, PlayAreaHeight));

        HighScore = highScoreStore.Load();
        NewGame();
    }

    /// <summary>Skor, seviye veya hız değiştiğinde tetiklenir.</summary>
    public event EventHandler? ScoreChanged;

    /// <summary>Oyun durumu (Ready / Playing / GameOver) değiştiğinde tetiklenir.</summary>
    public event EventHandler? StateChanged;

    public int Width { get; }
    public int Height { get; }
    public int PlayAreaHeight => Height - GroundHeight;

    public Bird Bird { get; }
    public IReadOnlyList<PipePair> Pipes => pipes;

    public GameState State { get; private set; }
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public float Speed { get; private set; }
    public int Level => Score / PointsPerLevel + 1;
    public bool IsNewHighScore { get; private set; }

    /// <summary>En yüksek skor dosyası okunamadı/yazılamadıysa hata mesajı.</summary>
    public string? HighScoreError => highScoreStore.LastError;

    public void NewGame()
    {
        Bird.Reset();
        Score = 0;
        Speed = StartSpeed;
        IsNewHighScore = false;

        // Borular ekranın sağından başlayarak eşit aralıklarla dizilir
        for (int i = 0; i < pipes.Count; i++)
            pipes[i].Reposition(Width + 100 + i * PipeSpacing, RandomGapY());

        ChangeState(GameState.Ready);
        ScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Oyuncu zıplama tuşuna bastığında çağrılır. Hazır durumdaysa oyunu da başlatır.</summary>
    public void Flap()
    {
        if (State == GameState.Ready)
            ChangeState(GameState.Playing);

        if (State == GameState.Playing)
            Bird.Flap();
    }

    /// <summary>Oyun döngüsünün bir adımı. Form'daki Timer her tick'te çağırır.</summary>
    public void Update()
    {
        if (State != GameState.Playing)
            return;

        Bird.Update();

        foreach (PipePair pipe in pipes)
        {
            pipe.Speed = Speed;
            pipe.Update();

            // Kuş borunun sağ kenarını geçtiyse puan
            if (!pipe.Passed && pipe.X + pipe.Width < Bird.X)
            {
                pipe.Passed = true;
                AddPoint();
            }

            // Ekrandan çıkan boru en sağdaki borunun arkasına taşınır
            if (pipe.IsOffScreen)
                pipe.Reposition(pipes.Max(p => p.X) + PipeSpacing, RandomGapY());
        }

        if (CheckCollision())
            EndGame();
    }

    private bool CheckCollision()
    {
        RectangleF hitBox = Bird.HitBox;

        if (hitBox.Bottom >= PlayAreaHeight)
            return true;  // Zemine çarptı

        return pipes.Any(p => p.CollidesWith(hitBox));
    }

    private void AddPoint()
    {
        Score++;

        if (Score % PointsPerLevel == 0)
            Speed = Math.Min(Speed + SpeedIncrease, MaxSpeed);

        ScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EndGame()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            IsNewHighScore = true;
            highScoreStore.Save(HighScore);
            ScoreChanged?.Invoke(this, EventArgs.Empty);
        }

        ChangeState(GameState.GameOver);
    }

    private int RandomGapY() =>
        random.Next(MinGapMargin, PlayAreaHeight - GapSize - MinGapMargin + 1);

    private void ChangeState(GameState newState)
    {
        State = newState;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
