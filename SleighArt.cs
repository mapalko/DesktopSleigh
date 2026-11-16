using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace DesktopSleigh;

internal static class SleighArt
{
    public const int Width = 152;
    public const int Height = 32;

    private static readonly Color Outline = Color.FromArgb(65, 36, 35);
    private static readonly Color Red = Color.FromArgb(196, 38, 51);
    private static readonly Color Gold = Color.FromArgb(244, 191, 76);
    private static readonly Color Snow = Color.FromArgb(255, 249, 229);

    public static Bitmap CreateFrame(int step, bool waving)
    {
        var bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.None;

        using (var reins = new Pen(Gold))
        {
            g.DrawLines(reins, [new(29, 17), new(41, 20), new(54, 18), new(69, 20),
                new(82, 18), new(97, 20), new(110, 18), new(125, 20), new(138, 18)]);
        }

        DrawReindeer(g, 40, step, false);
        DrawReindeer(g, 68, 1 - step, false);
        DrawReindeer(g, 96, step, false);
        DrawReindeer(g, 124, 1 - step, true);
        DrawSleigh(g, waving);
        return bitmap;
    }

    private static void DrawReindeer(Graphics g, int x, int step, bool lead)
    {
        GraphicsState state = g.Save();
        g.TranslateTransform(x, 0);
        g.SetClip(new Rectangle(0, 0, 28, Height));
        Color outline = Color.FromArgb(72, 45, 37);
        Color coat = Color.FromArgb(170, 105, 54);
        Color shade = Color.FromArgb(116, 67, 43);
        Color cream = Color.FromArgb(255, 237, 197);
        Color red = Color.FromArgb(191, 43, 54);

        Stroke(g, outline, 1, new(19, 8), new(18, 5), new(18, 1));
        Stroke(g, outline, 1, new(18, 4), new(15, 3), new(14, 1));
        Stroke(g, outline, 1, new(18, 3), new(21, 1));
        Stroke(g, outline, 1, new(22, 8), new(24, 5), new(25, 1));
        Stroke(g, outline, 1, new(24, 4), new(27, 2), new(27, 0));
        Stroke(g, cream, 1, new(18, 2), new(18, 4));
        Stroke(g, outline, 2, new(7, 23), new(step == 0 ? 4 : 9, 27), new(step == 0 ? 3 : 8, 30));
        Stroke(g, outline, 2, new(16, 23), new(step == 0 ? 14 : 20, 27), new(step == 0 ? 13 : 21, 30));
        Shape(g, coat, outline, new(3, 19), new(5, 17), new(11, 17), new(15, 18), new(16, 15),
            new(18, 13), new(18, 9), new(20, 8), new(23, 8), new(25, 10), new(24, 12), new(27, 13),
            new(27, 15), new(23, 16), new(22, 18), new(20, 21), new(18, 24), new(15, 25),
            new(8, 25), new(5, 24), new(3, 22));
        Shape(g, Color.FromArgb(221, 163, 91), null, new(5, 18), new(11, 18), new(15, 19), new(15, 20), new(5, 20));
        Shape(g, shade, null, new(5, 22), new(9, 23), new(16, 23), new(15, 24), new(8, 24), new(5, 23));
        Shape(g, cream, null, new(19, 15), new(22, 15), new(21, 19), new(18, 23), new(16, 23), new(18, 19));
        Shape(g, coat, outline, new(18, 10), new(14, 8), new(15, 11), new(18, 12));
        Stroke(g, cream, 1, new(15, 9), new(17, 10));
        Fill(g, Color.White, 21, 10, 2, 2);
        Fill(g, outline, 22, 10, 1, 2);
        Fill(g, lead ? red : outline, 26, 13, 2, 2);
        Fill(g, cream, 24, 15, 2, 1);
        Shape(g, cream, outline, new(3, 19), new(1, 16), new(0, 17), new(1, 20), new(3, 21));
        Stroke(g, coat, 2, new(6, 23), new(step == 0 ? 8 : 5, 27), new(step == 0 ? 6 : 3, 30));
        Stroke(g, coat, 2, new(16, 23), new(step == 0 ? 18 : 14, 27), new(step == 0 ? 21 : 15, 30));
        Fill(g, outline, step == 0 ? 5 : 2, 30, 3, 1);
        Fill(g, outline, step == 0 ? 20 : 14, 30, 3, 1);
        Fill(g, red, 12, 18, 2, 6);
        Fill(g, Gold, 13, 21, 3, 3);
        Fill(g, outline, 14, 23, 1, 1);
        g.Restore(state);
    }

    private static void Stroke(Graphics g, Color color, float width, params PointF[] points)
    {
        using var pen = new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        g.DrawLines(pen, points);
    }

    private static void Shape(Graphics g, Color fill, Color? outline, params PointF[] points)
    {
        using var path = new GraphicsPath();
        path.AddPolygon(points);
        using var brush = new SolidBrush(fill);
        g.FillPath(brush, path);
        if (outline is Color color)
        {
            using var pen = new Pen(color) { LineJoin = LineJoin.Round };
            g.DrawPath(pen, path);
        }
    }

    private static void DrawSleigh(Graphics g, bool waving)
    {
        Fill(g, Outline, 3, 12, 12, 11);
        Fill(g, Color.FromArgb(41, 111, 76), 4, 13, 10, 9);
        Fill(g, Gold, 8, 13, 2, 9);
        Fill(g, Gold, 4, 16, 10, 2);
        Fill(g, Outline, 1, 17, 7, 7);
        Fill(g, Color.FromArgb(65, 137, 166), 2, 18, 5, 5);
        Fill(g, Snow, 4, 18, 1, 5);

        Fill(g, Outline, 18, 5, 9, 10);
        Fill(g, Color.FromArgb(244, 188, 143), 19, 7, 7, 6);
        Fill(g, Red, 17, 3, 8, 4);
        Fill(g, Red, 15, 2, 7, 3);
        Fill(g, Snow, 14, 3, 3, 3);
        Fill(g, Snow, 17, 7, 10, 2);
        Fill(g, Outline, 24, 10, 1, 1);
        Fill(g, Color.FromArgb(233, 139, 119), 25, 11, 2, 2);
        Fill(g, Snow, 18, 11, 4, 4);
        Fill(g, Snow, 20, 13, 6, 4);
        Fill(g, Snow, 22, 16, 3, 2);
        Fill(g, Outline, 16, 15, 12, 9);
        Fill(g, Red, 17, 16, 10, 7);
        Fill(g, Snow, 17, 21, 10, 2);
        Fill(g, Outline, 17, 20, 10, 1);
        Fill(g, Gold, 23, 19, 3, 3);
        Fill(g, Outline, 27, 21, 4, 3);
        if (waving)
        {
            Fill(g, Red, 26, 13, 3, 6);
            Fill(g, Snow, 27, 11, 3, 3);
            Fill(g, Color.FromArgb(244, 188, 143), 28, 8, 3, 4);
        }
        else
        {
            Fill(g, Red, 26, 16, 4, 3);
            Fill(g, Snow, 29, 16, 2, 3);
            Fill(g, Color.FromArgb(244, 188, 143), 31, 16, 2, 2);
        }

        using (var body = new SolidBrush(Outline))
        {
            g.FillPolygon(body, [new(0, 20), new(5, 20), new(9, 23), new(28, 23),
                new(32, 18), new(36, 18), new(33, 25), new(28, 28), new(7, 28), new(3, 25)]);
        }
        using (var body = new SolidBrush(Red))
        {
            g.FillPolygon(body, [new(2, 21), new(5, 21), new(9, 24), new(29, 24),
                new(33, 20), new(34, 20), new(31, 25), new(27, 27), new(8, 27), new(5, 25)]);
        }
        Fill(g, Gold, 9, 24, 19, 1);
        Fill(g, Gold, 13, 26, 10, 1);
        Fill(g, Outline, 9, 28, 2, 2);
        Fill(g, Outline, 26, 28, 2, 2);
        Fill(g, Gold, 5, 30, 27, 2);
        Fill(g, Gold, 31, 29, 3, 2);
        Fill(g, Gold, 33, 26, 2, 4);
        Fill(g, Snow, 7, 30, 22, 1);
    }

    private static void Fill(Graphics g, Color color, int x, int y, int width, int height)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, x, y, width, height);
    }

    public static Icon CreateIcon()
    {
        using Bitmap frame = CreateFrame(0, false);
        using var bitmap = new Bitmap(32, 32);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.DrawImage(frame, new Rectangle(0, 2, 32, 28), new Rectangle(0, 0, 37, 32),
                GraphicsUnit.Pixel);
        }

        nint handle = bitmap.GetHicon();
        try
        {
            using Icon temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(nint icon);
}
