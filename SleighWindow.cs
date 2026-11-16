using System.Drawing.Drawing2D;

namespace DesktopSleigh;

internal class SleighWindow : Form
{
    private readonly Bitmap standingFrame = CreateCanvasFrame(0, false);
    private readonly Bitmap walkingFrame = CreateCanvasFrame(1, false);
    private readonly Bitmap wavingFrame = CreateCanvasFrame(0, true);
    private int rotation;
    private bool isStepping;
    private bool isWaving;

    public SleighWindow()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
        StartPosition = FormStartPosition.Manual;
    }

    public void UpdateSleigh(EdgePose pose, Rectangle bounds, int size, bool stepping,
        bool waving = false)
    {
        int extent = size * SleighArt.Width / SleighArt.Height;
        if (ClientSize.Width != extent || ClientSize.Height != extent)
        {
            ClientSize = new Size(extent, extent);
        }
        Location = new Point(bounds.Left + (int)Math.Round(pose.X),
            bounds.Top + (int)Math.Round(pose.Y));
        RotateFlipType turn = ((pose.Rotation - rotation + 360) % 360) switch
        {
            90 => RotateFlipType.Rotate90FlipNone,
            180 => RotateFlipType.Rotate180FlipNone,
            270 => RotateFlipType.Rotate270FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone
        };
        if (turn != RotateFlipType.RotateNoneFlipNone)
        {
            standingFrame.RotateFlip(turn);
            walkingFrame.RotateFlip(turn);
            wavingFrame.RotateFlip(turn);
        }
        rotation = pose.Rotation;
        isStepping = stepping;
        isWaving = waving;
        Invalidate();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams parameters = base.CreateParams;
            parameters.ExStyle |= 0x08000000 | 0x00000080; // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW
            return parameters;
        }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x0021) // WM_MOUSEACTIVATE
        {
            message.Result = 3; // MA_NOACTIVATE
            return;
        }

        base.WndProc(ref message);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Bitmap frame = isWaving ? wavingFrame : isStepping ? walkingFrame : standingFrame;
        Graphics graphics = e.Graphics;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.DrawImage(frame, ClientRectangle);

        if (!TopMost)
        {
            TopMost = true;
        }
    }

    private static Bitmap CreateCanvasFrame(int step, bool waving)
    {
        // Rotate the pixel grid, not the graphics transform, to avoid fractional-pixel clipping.
        using Bitmap sprite = SleighArt.CreateFrame(step, waving);
        var canvas = new Bitmap(SleighArt.Width, SleighArt.Width);
        using Graphics graphics = Graphics.FromImage(canvas);
        graphics.DrawImageUnscaled(sprite, 0, SleighArt.Width - SleighArt.Height);
        return canvas;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            standingFrame.Dispose();
            walkingFrame.Dispose();
            wavingFrame.Dispose();
        }

        base.Dispose(disposing);
    }
}
