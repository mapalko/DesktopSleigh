using System.Buffers.Binary;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;
using DesktopSleigh;

namespace DesktopSleigh.Checks;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        CheckMotion();
        CheckBells();
        CheckRendering();
        if (args.Length == 1 && args[0] == "--smoke")
        {
            CheckFormLifecycle();
        }
        if (args.Length == 2 && args[0] == "--artifacts")
        {
            WriteArtifacts(args[1]);
        }
        Console.WriteLine("Passed: travel, pause/resume, periodic/manual waves, bell PCM/volume, "
            + "animation frames, all sizes/rotations, and a complete screen-edge lap.");
    }

    private static void CheckMotion()
    {
        var motion = new SleighMotion { Distance = 100 };
        motion.Tick(0.5, false, 28);
        Require(motion.Distance == 114 && motion.IsMoving, "The team must move at the selected speed.");
        motion.Tick(5, true, 28);
        Require(motion.Distance == 114 && !motion.IsMoving, "Pause must freeze the team.");

        motion.BeginWave();
        motion.Tick(0.5, false, 28);
        Require(motion.IsWaving && !motion.IsMoving && motion.Distance == 114,
            "Santa's wave must stop the entire connected team.");
        motion.Tick(10, true, 28);
        Require(motion.IsWaving, "Pausing must also freeze a wave.");
        motion.Tick(1, false, 28);
        Require(!motion.IsWaving, "A wave must end after its hold time.");
        motion.Tick(0.5, false, 48);
        Require(motion.Distance == 138 && motion.IsMoving, "The team must resume after waving.");

        var periodic = new SleighMotion();
        Require(!periodic.Tick(29, false, 16), "The automatic stop must not trigger early.");
        Require(periodic.Tick(1, false, 16) && periodic.IsWaving,
            "The team must stop and request bells after 30 seconds of travel.");
        Require(!periodic.Tick(0.25, false, 16), "Bells must trigger only once per automatic stop.");
        periodic.BeginWave();
        periodic.Tick(SleighMotion.WaveSeconds, false, 16);
        Require(!periodic.IsWaving, "Repeated manual waves must reset, not accumulate, the hold.");
    }

    private static void CheckBells()
    {
        using Stream full = SleighBells.OpenStream();
        using Stream soft = SleighBells.OpenStream(-30);
        byte[] wave = ReadBytes(full);
        byte[] quiet = ReadBytes(soft);
        Require(wave.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && wave.AsSpan(8, 8).SequenceEqual("WAVEfmt "u8)
            && wave.AsSpan(36, 4).SequenceEqual("data"u8), "Bells must be a valid PCM WAV.");
        Require(BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(4)) == wave.Length - 8
            && BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(40)) == wave.Length - 44,
            "WAV lengths must match the generated samples.");
        Require(BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(24)) == 22050
            && wave.Length == 44144, "Bells must contain exactly one second of mono 16-bit PCM.");
        int peak = 0;
        int quietPeak = 0;
        for (int i = 44; i < wave.Length; i += 2)
        {
            int sample = BinaryPrimitives.ReadInt16LittleEndian(wave.AsSpan(i));
            int quietSample = BinaryPrimitives.ReadInt16LittleEndian(quiet.AsSpan(i));
            peak = Math.Max(peak, Math.Abs(sample));
            quietPeak = Math.Max(quietPeak, Math.Abs(quietSample));
            Require(Math.Abs(quietSample - sample * Math.Pow(10, -1.5)) <= 1,
                "The volume setting must attenuate every sample.");
        }
        Require(peak > 1000 && peak < short.MaxValue && quietPeak > 0,
            "Bells must be audible without clipping, even at the quiet setting.");
        full.Position = 0;
        using var player = new SoundPlayer(full);
        player.Load();
    }

    private static void CheckRendering()
    {
        using Bitmap standing = SleighArt.CreateFrame(0, false);
        using Bitmap trotting = SleighArt.CreateFrame(1, false);
        using Bitmap waving = SleighArt.CreateFrame(0, true);
        for (int deer = 0; deer < 4; deer++)
        {
            Require(CountDifferent(standing, trotting, new Rectangle(40 + deer * 28, 23, 28, 9)) > 10,
                $"Reindeer {deer + 1} must animate its legs.");
        }
        Require(CountDifferent(standing, waving, new Rectangle(26, 8, 8, 12)) > 5,
            "Santa must visibly raise his hand.");
        Require(CountDifferent(standing, trotting, new Rectangle(0, 0, 37, 32)) == 0,
            "The sleigh and rider must not distort while the reindeer trot.");
        Require(standing.GetPixel(37, 0).A == 0 && standing.GetPixel(0, 0).A == 0,
            "The sprite must retain its transparent background.");

        foreach (int size in new[] { 24, 32, 48 })
        {
            using var window = new SleighWindow();
            foreach (int rotation in new[] { 0, 90, 180, 270 })
            {
                window.UpdateSleigh(new(0, 0, rotation), new(0, 0, 1920, 1080), size, true);
                using Bitmap actual = Render(window);
                int extent = size * SleighArt.Width / SleighArt.Height;
                Require(actual.Size == new Size(extent, extent),
                    "The rotation canvas must accommodate the full team at every size.");
                using var source = new Bitmap(SleighArt.Width, SleighArt.Width);
                using (Graphics g = Graphics.FromImage(source))
                {
                    g.Clear(Color.Magenta);
                    g.DrawImageUnscaled(trotting, 0, SleighArt.Width - SleighArt.Height);
                }
                source.RotateFlip(rotation switch
                {
                    90 => RotateFlipType.Rotate90FlipNone,
                    180 => RotateFlipType.Rotate180FlipNone,
                    270 => RotateFlipType.Rotate270FlipNone,
                    _ => RotateFlipType.RotateNoneFlipNone
                });
                using var expected = new Bitmap(extent, extent);
                using (Graphics g = Graphics.FromImage(expected))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(source, new Rectangle(0, 0, extent, extent));
                }
                int differences = CountDifferent(actual, expected, new Rectangle(Point.Empty, actual.Size));
                Require(differences == 0,
                    $"The native paint must not clip or stretch the team at size {size}, rotation {rotation}.");
            }

            var bounds = new Rectangle(-1920, 40, 1280, 720);
            int canvas = size * SleighArt.Width / SleighArt.Height;
            double width = bounds.Width - canvas;
            double height = bounds.Height - canvas;
            double lap = EdgePath.Length(width, height);
            for (double distance = -canvas; distance <= lap + canvas; distance += 2)
            {
                EdgePose pose = EdgePath.AtCornerHop(distance, width, height, canvas, canvas);
                window.UpdateSleigh(pose, bounds, size, false);
                Require(bounds.Contains(window.Bounds),
                    "The full team must stay on the selected screen, including negative-origin screens and corners.");
            }
        }
    }

    private static void WriteArtifacts(string root)
    {
        string assets = Path.Combine(root, "Assets");
        Directory.CreateDirectory(assets);
        using (Icon icon = SleighArt.CreateIcon())
        using (Stream output = File.Create(Path.Combine(assets, "sleigh.ico")))
        {
            icon.Save(output);
        }

        using var sheet = new Bitmap(1024, 320);
        using (Graphics g = Graphics.FromImage(sheet))
        using (var title = new Font("Segoe UI", 22, FontStyle.Bold, GraphicsUnit.Pixel))
        using (var caption = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            g.Clear(Color.FromArgb(30, 43, 53));
            g.FillRectangle(Brushes.WhiteSmoke, 0, 160, sheet.Width, 160);
            for (int row = 0; row < 2; row++)
            {
                using Bitmap frame = SleighArt.CreateFrame(row, row == 1);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(frame, new Rectangle(24, row * 160 + 16, frame.Width * 4, frame.Height * 4));
                Brush text = row == 0 ? Brushes.WhiteSmoke : Brushes.DarkSlateGray;
                g.DrawString(row == 0 ? "DesktopSleigh" : "A wave from Santa", title, text, 672, row * 160 + 22);
                g.DrawString("Native pixel art, enlarged 4x", caption, text, 672, row * 160 + 60);
                g.DrawImage(frame, new Rectangle(672, row * 160 + 100, frame.Width, frame.Height));
                g.DrawString("Actual size", caption, text, 848, row * 160 + 108);
            }
        }
        sheet.Save(Path.Combine(assets, "sleigh-preview.png"), ImageFormat.Png);
    }

    private static void CheckFormLifecycle()
    {
        using var form = new SleighForm();
        using var closeTimer = new System.Windows.Forms.Timer { Interval = 500 };
        ContextMenuStrip menu = form.ContextMenuStrip
            ?? throw new InvalidOperationException("The native options menu is missing.");
        ToolStripMenuItem pause = menu.Items.OfType<ToolStripMenuItem>()
            .Single(item => item.Text == "Pause sleigh");
        foreach (string text in new[] { "Ring the bells!", "Sleigh bells", "Bell volume",
            "Sleigh speed", "Sleigh size", "Avoid the taskbar", "Screen", "Run on startup",
            "Exit DesktopSleigh" })
        {
            Require(menu.Items.OfType<ToolStripMenuItem>().Any(item => item.Text == text),
                $"The native menu must preserve the {text} control.");
        }
        bool shown = false;
        bool pauseWorked = false;
        bool resumeWorked = false;
        Point start = default;
        bool moved = false;
        form.Shown += (_, _) =>
        {
            shown = true;
            start = form.Location;
            closeTimer.Start();
        };
        closeTimer.Tick += (_, _) =>
        {
            closeTimer.Stop();
            moved = form.Location != start;
            pause.PerformClick();
            pauseWorked = pause.Checked && pause.Text == "Resume sleigh";
            pause.PerformClick();
            resumeWorked = !pause.Checked && pause.Text == "Pause sleigh";
            form.Close();
        };
        Application.Run(form);
        Require(shown && moved && pauseWorked && resumeWorked && form.IsDisposed,
            "The live form must animate, handle pause/resume, and close cleanly.");
        Console.WriteLine("Passed: live WinForms startup, timer-driven motion, native menu, pause/resume, clean exit.");
    }

    private static Bitmap Render(SleighWindow window)
    {
        var image = new Bitmap(window.Width, window.Height);
        window.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
        return image;
    }

    private static int CountDifferent(Bitmap first, Bitmap second, Rectangle area)
    {
        int count = 0;
        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                if (first.GetPixel(x, y) != second.GetPixel(x, y))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static byte[] ReadBytes(Stream source)
    {
        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
