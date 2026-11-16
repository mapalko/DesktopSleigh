using System.Diagnostics;
using System.Media;
using Microsoft.Win32;

namespace DesktopSleigh;

internal sealed class SleighForm : SleighWindow
{
    private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupRegistryName = "DesktopSleigh";
    private readonly SleighSettings settings = SleighSettings.Load();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 25 };
    private readonly SleighMotion team = new();
    private readonly ContextMenuStrip menu = new();
    private readonly NotifyIcon tray;
    private readonly Icon sleighIcon = SleighArt.CreateIcon();
    private Stream soundStream;
    private readonly SoundPlayer sound;
    private readonly ToolStripMenuItem pauseItem = new("Pause sleigh");
    private readonly ToolStripMenuItem soundItem = new("Sleigh bells");
    private readonly ToolStripMenuItem taskbarItem = new("Avoid the taskbar");
    private readonly ToolStripMenuItem screenMenu = new("Screen");
    private readonly ToolStripMenuItem startupItem = new("Run on startup");
    private Rectangle screenBounds;
    private double lastTick;
    private bool paused;
    private bool soundFailed;

    public SleighForm()
    {
        Text = "DesktopSleigh";
        Icon = sleighIcon;
        soundStream = SleighBells.OpenStream(settings.VolumeDb);
        sound = new SoundPlayer(soundStream);

        BuildMenu();
        ContextMenuStrip = menu;
        tray = new NotifyIcon
        {
            Icon = sleighIcon,
            Text = "DesktopSleigh - right-click for options",
            ContextMenuStrip = menu,
            Visible = true
        };
        tray.DoubleClick += (_, _) => RingBells();
        MouseClick += SleighClicked;
        timer.Tick += (_, _) => TickSleigh();
        SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
        UpdateScreen();
        timer.Start();
    }

    private void SleighClicked(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            RingBells();
        }
    }

    private void BuildMenu()
    {
        menu.Items.Add(new ToolStripMenuItem("DesktopSleigh") { Enabled = false });
        menu.Items.Add("Ring the bells!", null, (_, _) => RingBells());
        pauseItem.CheckOnClick = true;
        pauseItem.Click += (_, _) =>
        {
            paused = pauseItem.Checked;
            pauseItem.Text = paused ? "Resume sleigh" : "Pause sleigh";
        };
        menu.Items.Add(pauseItem);

        soundItem.Checked = settings.SoundEnabled;
        soundItem.CheckOnClick = true;
        soundItem.Click += (_, _) =>
        {
            settings.SoundEnabled = soundItem.Checked;
            if (!settings.SoundEnabled)
            {
                sound.Stop();
            }
            settings.Save();
        };
        menu.Items.Add(soundItem);

        var volumeMenu = new ToolStripMenuItem("Bell volume");
        AddChoice(volumeMenu, "Full", 0, () => settings.VolumeDb, SetVolume);
        AddChoice(volumeMenu, "Softer (-3 dB)", -3, () => settings.VolumeDb, SetVolume);
        AddChoice(volumeMenu, "Barely audible (-30 dB)", -30, () => settings.VolumeDb, SetVolume);
        menu.Items.Add(volumeMenu);
        menu.Items.Add(new ToolStripSeparator());

        var speedMenu = new ToolStripMenuItem("Sleigh speed");
        AddChoice(speedMenu, "Sleepy", 16, () => settings.Speed, value => settings.Speed = value);
        AddChoice(speedMenu, "Leisurely", 28, () => settings.Speed, value => settings.Speed = value);
        AddChoice(speedMenu, "Places to be", 48, () => settings.Speed, value => settings.Speed = value);
        menu.Items.Add(speedMenu);

        var sizeMenu = new ToolStripMenuItem("Sleigh size");
        AddChoice(sizeMenu, "Tiny", 24, () => settings.Size, value => settings.Size = value);
        AddChoice(sizeMenu, "Small", 32, () => settings.Size, value => settings.Size = value);
        AddChoice(sizeMenu, "Large", 48, () => settings.Size, value => settings.Size = value);
        menu.Items.Add(sizeMenu);

        taskbarItem.Checked = settings.AvoidTaskbar;
        taskbarItem.CheckOnClick = true;
        taskbarItem.Click += (_, _) =>
        {
            settings.AvoidTaskbar = taskbarItem.Checked;
            UpdateScreen();
            settings.Save();
        };
        menu.Items.Add(taskbarItem);
        menu.Items.Add(screenMenu);
        menu.Opening += (_, _) =>
        {
            PopulateScreens();
            RefreshStartupItem();
        };
        menu.Items.Add(new ToolStripSeparator());
        startupItem.Click += (_, _) => ToggleStartup();
        menu.Items.Add(startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit DesktopSleigh", null, (_, _) => Close());
    }

    private void RefreshStartupItem()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath);
            startupItem.Checked = key?.GetValue(StartupRegistryName) is string command
                && string.Equals(command, $"\"{Application.ExecutablePath}\"", StringComparison.OrdinalIgnoreCase);
            startupItem.Enabled = true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or System.Security.SecurityException)
        {
            startupItem.Enabled = false;
            MessageBox.Show($"Couldn't read DesktopSleigh's startup option.\n\n{exception.Message}",
                "DesktopSleigh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ToggleStartup()
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(StartupRegistryPath);
            if (startupItem.Checked)
            {
                key.DeleteValue(StartupRegistryName, throwOnMissingValue: false);
            }
            else
            {
                key.SetValue(StartupRegistryName, $"\"{Application.ExecutablePath}\"", RegistryValueKind.String);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or System.Security.SecurityException)
        {
            MessageBox.Show($"Couldn't change DesktopSleigh's startup option.\n\n{exception.Message}",
                "DesktopSleigh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        RefreshStartupItem();
    }

    private void AddChoice(ToolStripMenuItem parent, string label, int value,
        Func<int> getValue, Action<int> setValue)
    {
        var item = new ToolStripMenuItem(label) { Checked = getValue() == value };
        item.Click += (_, _) =>
        {
            setValue(value);
            foreach (ToolStripMenuItem sibling in parent.DropDownItems)
            {
                sibling.Checked = sibling == item;
            }
            UpdateScreen();
            settings.Save();
        };
        parent.DropDownOpening += (_, _) => item.Checked = getValue() == value;
        parent.DropDownItems.Add(item);
    }

    private void SetVolume(int volumeDb)
    {
        Stream replacement = SleighBells.OpenStream(volumeDb);
        sound.Stop();
        sound.Stream = replacement;
        soundStream.Dispose();
        soundStream = replacement;
        settings.VolumeDb = volumeDb;
    }

    private void PopulateScreens()
    {
        while (screenMenu.DropDownItems.Count > 0)
        {
            screenMenu.DropDownItems[0].Dispose();
        }

        Screen current = SelectedScreen();
        foreach (Screen screen in Screen.AllScreens)
        {
            var item = new ToolStripMenuItem(
                $"{screen.DeviceName}{(screen.Primary ? " (primary)" : "")}")
            {
                Checked = screen.DeviceName == current.DeviceName
            };
            item.Click += (_, _) =>
            {
                settings.ScreenName = screen.DeviceName;
                UpdateScreen();
                settings.Save();
            };
            screenMenu.DropDownItems.Add(item);
        }
    }

    private Screen SelectedScreen() =>
        Screen.AllScreens.FirstOrDefault(screen => screen.DeviceName == settings.ScreenName)
        ?? Screen.PrimaryScreen
        ?? Screen.AllScreens.First();

    private void UpdateScreen()
    {
        bool firstScreen = screenBounds.IsEmpty;
        double oldLength = EdgePath.Length(screenBounds.Width - Width, screenBounds.Height - Height);
        double progress = oldLength > 0 ? team.Distance / oldLength : 0;
        Screen screen = SelectedScreen();
        screenBounds = settings.AvoidTaskbar ? screen.WorkingArea : screen.Bounds;
        int extent = settings.Size * SleighArt.Width / SleighArt.Height;
        ClientSize = new Size(extent, extent);
        double newLength = EdgePath.Length(screenBounds.Width - Width, screenBounds.Height - Height);
        double distance = firstScreen ? Math.Max(0, screenBounds.Width - Width) : progress * newLength;
        team.Distance = distance;
        UpdateTeam();
    }

    private void TickSleigh()
    {
        double now = clock.Elapsed.TotalSeconds;
        double elapsed = Math.Clamp(now - lastTick, 0, 0.1);
        lastTick = now;
        if (team.Tick(elapsed, paused, settings.Speed))
        {
            PlayBells();
        }

        UpdateTeam();
    }

    private void UpdateTeam()
    {
        double width = screenBounds.Width - Width;
        double height = screenBounds.Height - Height;
        bool stepping = !paused && team.IsMoving
            && (int)(team.WalkingDistance / (settings.Size * 0.16)) % 2 == 1;
        UpdateSleigh(EdgePath.AtCornerHop(team.Distance, width, height, Width, Width),
            screenBounds, settings.Size, stepping, team.IsWaving);
    }

    private void RingBells()
    {
        PlayBells();
        team.BeginWave();
        UpdateTeam();
    }

    private void PlayBells()
    {
        if (!settings.SoundEnabled || soundFailed)
        {
            return;
        }

        try
        {
            sound.Load();
            sound.Play();
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException
            or System.TimeoutException)
        {
            soundFailed = true;
            tray.ShowBalloonTip(5000, "The sleigh bells are quiet",
                $"Bell audio is unavailable. The sleigh will keep moving.\n{exception.Message}",
                ToolTipIcon.Warning);
        }
    }

    private void DisplaySettingsChanged(object? sender, EventArgs e) => QueueScreenUpdate();

    private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) => QueueScreenUpdate();

    private void QueueScreenUpdate()
    {
        if (IsHandleCreated && !IsDisposed && !Disposing)
        {
            BeginInvoke((Action)(() =>
            {
                if (!IsDisposed && !Disposing)
                {
                    UpdateScreen();
                }
            }));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Stop();
            SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;
            tray.Visible = false;
            tray.Dispose();
            timer.Dispose();
            menu.Dispose();
            sound.Dispose();
            soundStream.Dispose();
            sleighIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
