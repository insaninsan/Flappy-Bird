namespace FlappyBird.Models;

/// <summary>
/// En yüksek skoru bir metin dosyasında saklar.
/// Dosya hataları uygulamayı kapatmaz; hata mesajı LastError'da tutulur.
/// </summary>
public class HighScoreStore
{
    private readonly string filePath;

    public HighScoreStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YMH219-FlappyBird",
            "highscore.txt"))
    {
    }

    public HighScoreStore(string filePath)
    {
        this.filePath = filePath;
    }

    /// <summary>Son okuma/yazma işleminde oluşan hata; hata yoksa null.</summary>
    public string? LastError { get; private set; }

    public int Load()
    {
        LastError = null;
        try
        {
            if (!File.Exists(filePath))
                return 0;

            string text = File.ReadAllText(filePath).Trim();
            return int.TryParse(text, out int score) && score >= 0 ? score : 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LastError = ex.Message;
            return 0;
        }
    }

    public bool Save(int score)
    {
        LastError = null;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, score.ToString());
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LastError = ex.Message;
            return false;
        }
    }
}
