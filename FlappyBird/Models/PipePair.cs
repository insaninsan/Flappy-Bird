using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace FlappyBird.Models;

/// <summary>
/// Aralarında geçiş boşluğu olan üst ve alt boru çifti.
/// Sağdan sola hareket eder; ekrandan çıkınca yeniden konumlandırılır.
/// </summary>
public class PipePair : GameObject
{
    public const int PipeWidth = 64;
    private const int CapHeight = 20;
    private const int CapOverhang = 4;

    private static readonly IBrush PipeBrush = new ImmutableSolidColorBrush(Color.FromRgb(83, 170, 60));
    private static readonly IBrush CapBrush = new ImmutableSolidColorBrush(Color.FromRgb(60, 140, 45));
    private static readonly IPen OutlinePen = new Pen(new ImmutableSolidColorBrush(Color.FromRgb(40, 90, 30)), 2);

    private readonly int areaHeight;

    /// <param name="x">Borunun sol kenarı.</param>
    /// <param name="gapY">Boşluğun üst kenarının Y değeri.</param>
    /// <param name="gapSize">Boşluğun yüksekliği.</param>
    /// <param name="areaHeight">Oyun alanının (zemin hariç) yüksekliği.</param>
    public PipePair(double x, int gapY, int gapSize, int areaHeight)
        : base(x, 0, PipeWidth, areaHeight)
    {
        GapY = gapY;
        GapSize = gapSize;
        this.areaHeight = areaHeight;
    }

    public int GapY { get; private set; }
    public int GapSize { get; }

    /// <summary>Her karede sola kayma miktarı (piksel).</summary>
    public double Speed { get; set; }

    /// <summary>Kuş bu borunun yanından geçti mi? (Aynı boru için iki kez puan verilmesin.)</summary>
    public bool Passed { get; set; }

    public bool IsOffScreen => X + Width < 0;

    public Rect TopPipe => new(X, 0, Width, GapY);
    public Rect BottomPipe => new(X, GapY + GapSize, Width, areaHeight - GapY - GapSize);

    public override void Update() => X -= Speed;

    /// <summary>Boruyu yeni bir konuma ve yeni bir boşluk yüksekliğine taşır.</summary>
    public void Reposition(double x, int gapY)
    {
        X = x;
        GapY = gapY;
        Passed = false;
    }

    public bool CollidesWith(Rect box) =>
        box.Intersects(TopPipe) || box.Intersects(BottomPipe);

    public override void Draw(DrawingContext context)
    {
        DrawPipe(context, TopPipe, capAtBottom: true);
        DrawPipe(context, BottomPipe, capAtBottom: false);
    }

    private static void DrawPipe(DrawingContext context, Rect r, bool capAtBottom)
    {
        if (r.Height <= 0)
            return;

        context.DrawRectangle(PipeBrush, OutlinePen, r);

        // Borunun ağız kısmı (biraz daha geniş kapak)
        double capY = capAtBottom ? r.Bottom - CapHeight : r.Y;
        var cap = new Rect(r.X - CapOverhang, capY, r.Width + 2 * CapOverhang, CapHeight);
        context.DrawRectangle(CapBrush, OutlinePen, cap);
    }
}
