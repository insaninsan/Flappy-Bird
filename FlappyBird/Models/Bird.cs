using Avalonia;
using Avalonia.Media;

namespace FlappyBird.Models;

/// <summary>
/// Oyuncunun kontrol ettiği kuş. Yer çekimi ile aşağı düşer,
/// Flap() çağrıldığında yukarı doğru hız kazanır.
/// </summary>
public class Bird : GameObject
{
    private const double Gravity = 0.5;       // Her karede hıza eklenen değer
    private const double FlapStrength = -8;   // Zıplayınca verilen hız (eksi = yukarı)
    private const double MaxFallSpeed = 10;   // Düşüş hızının üst sınırı

    private static readonly IPen OutlinePen = new Pen(Brushes.Black, 1);

    private readonly double startY;
    private readonly Geometry beak;

    public Bird(double x, double y) : base(x, y, 34, 26)
    {
        startY = y;
        beak = CreateBeak();
    }

    /// <summary>Dikey hız. Pozitif değer aşağı, negatif değer yukarı hareket demektir.</summary>
    public double Velocity { get; private set; }

    /// <summary>
    /// Çarpışma kontrolünde kullanılan kutu. Çizimden biraz küçüktür,
    /// böylece kuşun köşesi borunun köşesine değince oyun haksız yere bitmez.
    /// </summary>
    public Rect HitBox => Bounds.Deflate(3);

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

    public override void Draw(DrawingContext context)
    {
        // Kuşu hızına göre eğiyoruz: yükselirken burnu yukarı, düşerken aşağı.
        double angle = Math.Clamp(Velocity * 4, -25, 60) * Math.PI / 180;

        // Önce kuşun merkezi etrafında döndür, sonra ekrandaki konumuna taşı.
        Matrix transform = Matrix.CreateRotation(angle)
                         * Matrix.CreateTranslation(X + Width / 2.0, Y + Height / 2.0);

        using (context.PushTransform(transform))
        {
            var body = new Rect(-Width / 2.0, -Height / 2.0, Width, Height);
            context.DrawEllipse(Brushes.Gold, OutlinePen, body);

            // Kanat
            context.DrawEllipse(Brushes.Orange, null, new Rect(-Width / 2.0 + 4, -2, 14, 9));

            // Göz
            context.DrawEllipse(Brushes.White, null, new Rect(5, -Height / 2.0 + 4, 10, 10));
            context.DrawEllipse(Brushes.Black, null, new Rect(10, -Height / 2.0 + 7, 4, 4));

            // Gaga
            context.DrawGeometry(Brushes.OrangeRed, null, beak);
        }
    }

    /// <summary>Gaga üçgenini bir kez oluşturur (kuşun merkezine göre koordinatlar).</summary>
    private Geometry CreateBeak()
    {
        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(Width / 2.0 - 3, -2), isFilled: true);
            ctx.LineTo(new Point(Width / 2.0 + 8, 2));
            ctx.LineTo(new Point(Width / 2.0 - 3, 6));
            ctx.EndFigure(isClosed: true);
        }
        return geometry;
    }
}
