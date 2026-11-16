using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace DesktopSleigh;

internal static class SleighArt
{
    public const int Width = 96;
    public const int Height = 32;

    private static readonly Color Outline = Color.FromArgb(65, 36, 35);
    private static readonly Color Red = Color.FromArgb(196, 38, 51);
    private static readonly Color Gold = Color.FromArgb(244, 191, 76);
    private static readonly Color Snow = Color.FromArgb(255, 249, 229);
    private static readonly Color Brown = Color.FromArgb(157, 94, 49);
    private static readonly Color Tan = Color.FromArgb(207, 151, 87);

    public static Bitmap CreateFrame(int step, bool waving)
    {
        var bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.None;

        using (var reins = new Pen(Gold))
        {
            g.DrawLines(reins, [new(29, 17), new(41, 20), new(54, 18), new(69, 20), new(82, 18)]);
        }

        DrawReindeer(g, 40, step, false);
        DrawReindeer(g, 68, 1 - step, true);
        DrawSleigh(g, waving);
        return bitmap;
    }

    private static void DrawReindeer(Graphics g, int x, int step, bool lead)
    {
        void Block(Color color, int left, int top, int width, int height) =>
            Fill(g, color, x + left, top, width, height);

        // Forked antlers and ears remain legible at the smallest pet size.
        Block(Outline, 18, 1, 2, 9);
        Block(Outline, 14, 2, 2, 4);
        Block(Outline, 15, 5, 5, 2);
        Block(Outline, 21, 0, 2, 5);
        Block(Outline, 19, 3, 5, 2);
        Block(Tan, 18, 3, 1, 5);
        Block(Outline, 13, 9, 6, 3);
        Block(Tan, 14, 10, 4, 1);
        Block(Outline, 18, 8, 7, 8);
        Block(Brown, 19, 9, 5, 6);
        Block(Outline, 23, 12, 4, 4);
        Block(Tan, 23, 13, 3, 2);
        Block(lead ? Red : Outline, 26, 12, 2, 3);
        Block(Snow, 22, 10, 2, 2);
        Block(Outline, 23, 10, 1, 1);

        Block(Outline, 15, 14, 6, 10);
        Block(Brown, 16, 15, 4, 8);
        Block(Tan, 19, 17, 1, 5);
        Block(Outline, 3, 17, 15, 8);
        Block(Brown, 4, 18, 13, 6);
        Block(Tan, 5, 18, 10, 2);
        Block(Outline, 1, 15, 3, 5);
        Block(Snow, 1, 15, 2, 2);
        Block(Red, 12, 17, 3, 8);
        Block(Gold, 13, 21, 3, 3);
        Block(Outline, 14, 23, 1, 1);

        int back = step == 1 ? 2 : 5;
        int front = step == 1 ? 19 : 16;
        Block(Outline, 6, 24, 2, 3);
        Block(Outline, Math.Min(6, back), 25, Math.Abs(6 - back) + 2, 2);
        Block(Outline, back, 26, 2, 4);
        Block(Brown, back, 26, 1, 3);
        Block(Outline, back, 29, 4, 2);
        Block(Outline, 15, 24, 2, 3);
        Block(Outline, Math.Min(15, front), 25, Math.Abs(15 - front) + 2, 2);
        Block(Outline, front, 26, 2, 4);
        Block(Brown, front, 26, 1, 3);
        Block(Outline, front, 29, 4, 2);
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
