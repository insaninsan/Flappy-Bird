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

    private static readonly Brush PipeBrush = new SolidBrush(Color.FromArgb(83, 170, 60));
    private static readonly Brush CapBrush = new SolidBrush(Color.FromArgb(60, 140, 45));
    private static readonly Pen OutlinePen = new(Color.FromArgb(40, 90, 30), 2);

    private readonly int areaHeight;

    /// <param name="x">Borunun sol kenarı.</param>
    /// <param name="gapY">Boşluğun üst kenarının Y değeri.</param>
    /// <param name="gapSize">Boşluğun yüksekliği.</param>
    /// <param name="areaHeight">Oyun alanının (zemin hariç) yüksekliği.</param>
    public PipePair(float x, int gapY, int gapSize, int areaHeight)
        : base(x, 0, PipeWidth, areaHeight)
    {
        GapY = gapY;
        GapSize = gapSize;
        this.areaHeight = areaHeight;
    }

    public int GapY { get; private set; }
    public int GapSize { get; }

    /// <summary>Her karede sola kayma miktarı (piksel).</summary>
    public float Speed { get; set; }

    /// <summary>Kuş bu borunun yanından geçti mi? (Aynı boru için iki kez puan verilmesin.)</summary>
    public bool Passed { get; set; }

    public bool IsOffScreen => X + Width < 0;

    public RectangleF TopPipe => new(X, 0, Width, GapY);
    public RectangleF BottomPipe => new(X, GapY + GapSize, Width, areaHeight - GapY - GapSize);

    public override void Update() => X -= Speed;

    /// <summary>Boruyu yeni bir konuma ve yeni bir boşluk yüksekliğine taşır.</summary>
    public void Reposition(float x, int gapY)
    {
        X = x;
        GapY = gapY;
        Passed = false;
    }

    public bool CollidesWith(RectangleF box) =>
        box.IntersectsWith(TopPipe) || box.IntersectsWith(BottomPipe);

    public override void Draw(Graphics g)
    {
        DrawPipe(g, TopPipe, capAtBottom: true);
        DrawPipe(g, BottomPipe, capAtBottom: false);
    }

    private static void DrawPipe(Graphics g, RectangleF r, bool capAtBottom)
    {
        if (r.Height <= 0)
            return;

        g.FillRectangle(PipeBrush, r);
        g.DrawRectangle(OutlinePen, r.X, r.Y, r.Width, r.Height);

        // Borunun ağız kısmı (biraz daha geniş kapak)
        float capY = capAtBottom ? r.Bottom - CapHeight : r.Y;
        var cap = new RectangleF(r.X - CapOverhang, capY, r.Width + 2 * CapOverhang, CapHeight);
        g.FillRectangle(CapBrush, cap);
        g.DrawRectangle(OutlinePen, cap.X, cap.Y, cap.Width, cap.Height);
    }
}
