using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FlappyBird.Models;

namespace FlappyBird;

/// <summary>
/// Oyunun çizildiği yüzey. Her InvalidateVisual() çağrısında Render() ile
/// arka plan, borular, zemin, kuş ve durum yazıları yeniden çizilir.
/// </summary>
public class GameCanvas : Control
{
    private static readonly IBrush SkyBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.FromRgb(110, 197, 233), 0),
            new GradientStop(Color.FromRgb(190, 235, 250), 1),
        },
    };

    private static readonly IBrush GroundBrush = new ImmutableSolidColorBrush(Color.FromRgb(222, 184, 135));
    private static readonly IBrush GrassBrush = new ImmutableSolidColorBrush(Color.FromRgb(110, 190, 70));
    private static readonly IBrush OverlayBrush = new ImmutableSolidColorBrush(Color.FromArgb(150, 0, 0, 0));
    private static readonly IPen GroundLinePen = new Pen(Brushes.DarkGreen, 1);

    private static readonly Typeface BoldFont = new(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
    private static readonly Typeface NormalFont = new(FontFamily.Default);

    public GameCanvas()
    {
        ClipToBounds = true;   // Ekran dışına taşan borular pencerenin başka yerine çizilmesin
        Focusable = true;
    }

    /// <summary>Çizilecek oyun. Pencere tarafından atanır.</summary>
    public GameManager? Game { get; set; }

    public override void Render(DrawingContext context)
    {
        if (Game is null)
            return;

        DrawBackground(context);
        foreach (PipePair pipe in Game.Pipes)
            pipe.Draw(context);
        DrawGround(context);
        Game.Bird.Draw(context);

        switch (Game.State)
        {
            case GameState.Ready:
                DrawCenteredText(context, "Hazır!", 44, BoldFont, 170);
                DrawCenteredText(context, "SPACE / ↑ / tıkla: zıpla", 16, NormalFont, 240);
                break;

            case GameState.Playing:
                DrawCenteredText(context, Game.Score.ToString(), 44, BoldFont, 30);
                break;

            case GameState.Paused:
                DrawCenteredText(context, Game.Score.ToString(), 44, BoldFont, 30);
                context.FillRectangle(OverlayBrush, new Rect(40, 200, Bounds.Width - 80, 150));
                DrawCenteredText(context, "Duraklatıldı", 40, BoldFont, 220);
                DrawCenteredText(context, "Devam etmek için P / Esc", 16, NormalFont, 295);
                break;

            case GameState.GameOver:
                DrawGameOver(context);
                break;
        }
    }

    private void DrawBackground(DrawingContext context)
    {
        context.FillRectangle(SkyBrush, new Rect(0, 0, Bounds.Width, Game!.PlayAreaHeight));

        // Birkaç sabit bulut
        DrawCloud(context, 60, 90);
        DrawCloud(context, 300, 160);
        DrawCloud(context, 180, 300);
    }

    private static void DrawCloud(DrawingContext context, double x, double y)
    {
        context.DrawEllipse(Brushes.White, null, new Rect(x, y, 60, 30));
        context.DrawEllipse(Brushes.White, null, new Rect(x + 25, y - 15, 50, 40));
        context.DrawEllipse(Brushes.White, null, new Rect(x + 55, y, 50, 30));
    }

    private void DrawGround(DrawingContext context)
    {
        double top = Game!.PlayAreaHeight;
        context.FillRectangle(GroundBrush, new Rect(0, top, Bounds.Width, GameManager.GroundHeight));
        context.FillRectangle(GrassBrush, new Rect(0, top, Bounds.Width, 14));
        context.DrawLine(GroundLinePen, new Point(0, top), new Point(Bounds.Width, top));
    }

    private void DrawGameOver(DrawingContext context)
    {
        context.FillRectangle(OverlayBrush, new Rect(40, 150, Bounds.Width - 80, 260));

        DrawCenteredText(context, "GAME OVER", 44, BoldFont, 170);
        DrawCenteredText(context, $"Skor: {Game!.Score}", 22, BoldFont, 245);

        string best = Game.IsNewHighScore ? "Yeni rekor!" : $"En yüksek: {Game.HighScore}";
        DrawCenteredText(context, best, 22, BoldFont, 285);

        DrawCenteredText(context, "Enter / R: yeniden başla", 16, NormalFont, 350);
    }

    /// <summary>Metni yatayda ortalayıp gölgeli olarak çizer.</summary>
    private void DrawCenteredText(DrawingContext context, string text, double size, Typeface font, double y)
    {
        FormattedText shadow = CreateText(text, size, font, Brushes.Black);
        FormattedText main = CreateText(text, size, font, Brushes.White);

        double x = (Bounds.Width - main.Width) / 2;
        context.DrawText(shadow, new Point(x + 2, y + 2));
        context.DrawText(main, new Point(x, y));
    }

    private static FormattedText CreateText(string text, double size, Typeface font, IBrush brush) =>
        new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, brush);
}
