using Avalonia;
using Avalonia.Media;

namespace FlappyBird.Models;

/// <summary>
/// Ekranda konumu olan, her karede güncellenen ve kendini çizebilen
/// tüm oyun nesnelerinin ortak taban sınıfı.
/// </summary>
public abstract class GameObject
{
    protected GameObject(double x, double y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public double X { get; protected set; }
    public double Y { get; protected set; }
    public int Width { get; }
    public int Height { get; }

    /// <summary>Nesnenin ekranda kapladığı dikdörtgen.</summary>
    public Rect Bounds => new(X, Y, Width, Height);

    /// <summary>Her oyun karesinde (timer tick) bir kez çağrılır.</summary>
    public abstract void Update();

    /// <summary>Nesneyi verilen çizim yüzeyine çizer.</summary>
    public abstract void Draw(DrawingContext context);
}
