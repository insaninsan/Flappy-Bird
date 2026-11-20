namespace FlappyBird;

/// <summary>
/// Oyunun çizildiği yüzey. DoubleBuffered sayesinde her karede
/// yeniden çizimde titreme olmaz. Odak alabilir, böylece klavye olayları buraya gelir.
/// </summary>
public class GameCanvas : Panel
{
    public GameCanvas()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        base.OnMouseDown(e);
    }
}
