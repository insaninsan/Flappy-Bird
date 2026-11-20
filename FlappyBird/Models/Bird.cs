using System.Drawing.Drawing2D;

namespace FlappyBird.Models;

/// <summary>
/// Oyuncunun kontrol ettiği kuş. Yer çekimi ile aşağı düşer,
/// Flap() çağrıldığında yukarı doğru hız kazanır.
/// </summary>
public class Bird : GameObject
{
    private const float Gravity = 0.5f;       // Her karede hıza eklenen değer
    private const float FlapStrength = -8f;   // Zıplayınca verilen hız (eksi = yukarı)
    private const float MaxFallSpeed = 10f;   // Düşüş hızının üst sınırı

    private readonly float startY;

    public Bird(float x, float y) : base(x, y, 34, 26)
    {
        startY = y;
    }

    /// <summary>Dikey hız. Pozitif değer aşağı, negatif değer yukarı hareket demektir.</summary>
    public float Velocity { get; private set; }

    /// <summary>
    /// Çarpışma kontrolünde kullanılan kutu. Çizimden biraz küçüktür,
    /// böylece kuşun köşesi borunun köşesine değince oyun haksız yere bitmez.
    /// </summary>
    public RectangleF HitBox => RectangleF.Inflate(Bounds, -3, -3);

    public void Flap() => Velocity = FlapStrength;

    public override void Update()
    {
        Velocity = Math.Min(Velocity + Gravity, MaxFallSpeed);
        Y += Velocity;

        // Ekranın üstünden dışarı çıkmasın
        if (Y < 0)
        {
            Y = 0;
            Velocity = 0;
        }
    }

    public void Reset()
    {
        Y = startY;
        Velocity = 0;
    }

    public override void Draw(Graphics g)
    {
        GraphicsState saved = g.Save();

        // Kuşu merkezine taşıyıp hızına göre eğiyoruz: yükselirken burnu yukarı, düşerken aşağı.
        float angle = Math.Clamp(Velocity * 4, -25f, 60f);
        g.TranslateTransform(X + Width / 2f, Y + Height / 2f);
        g.RotateTransform(angle);

        var body = new RectangleF(-Width / 2f, -Height / 2f, Width, Height);
        g.FillEllipse(Brushes.Gold, body);
        g.DrawEllipse(Pens.Black, body);

        // Kanat
        g.FillEllipse(Brushes.Orange, -Width / 2f + 4, -2, 14, 9);

        // Göz
        g.FillEllipse(Brushes.White, 5, -Height / 2f + 4, 10, 10);
        g.FillEllipse(Brushes.Black, 10, -Height / 2f + 7, 4, 4);

        // Gaga
        PointF[] beak =
        {
            new(Width / 2f - 3, -2),
            new(Width / 2f + 8, 2),
            new(Width / 2f - 3, 6),
        };
        g.FillPolygon(Brushes.OrangeRed, beak);

        g.Restore(saved);
    }
}
