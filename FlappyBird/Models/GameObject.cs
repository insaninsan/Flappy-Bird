namespace FlappyBird.Models;

/// <summary>
/// Ekranda konumu olan, her karede güncellenen ve kendini çizebilen
/// tüm oyun nesnelerinin ortak taban sınıfı.
/// </summary>
public abstract class GameObject
{
    protected GameObject(float x, float y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float X { get; protected set; }
    public float Y { get; protected set; }
    public int Width { get; }
    public int Height { get; }

    /// <summary>Nesnenin ekranda kapladığı dikdörtgen.</summary>
    public RectangleF Bounds => new(X, Y, Width, Height);

    /// <summary>Her oyun karesinde (Timer tick) bir kez çağrılır.</summary>
    public abstract void Update();

    /// <summary>Nesneyi verilen Graphics yüzeyine çizer.</summary>
    public abstract void Draw(Graphics g);
}
