# DesktopSleigh

Santa's sleigh, pulled by two pixel-art reindeer, takes a trip around the edges of
your Windows desktop. The reindeer trot together, reins attached, while Santa
rides behind them with a sleigh full of presents. Every 30 seconds the team
pauses, Santa waves, and the sleigh bells ring.

![Santa and two reindeer, drawn by DesktopSleigh's native renderer](Assets/sleigh-preview.png)

## Run

Requires Windows and the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
dotnet run --project .\DesktopSleigh.csproj
```

Left-click the team or double-click its tray icon to ring the bells and wave.
Right-click either for the native options menu: pause/resume, sound and volume,
speed, three sizes, screen selection, taskbar avoidance, run on startup, and exit.
The team stays together when turning corners; the original duck flock's
catch-up stops are now Santa's wave-and-bell stops.

Settings are saved separately from DesktopDuck in
`%LOCALAPPDATA%\DesktopSleigh\settings.json`. Startup registration also uses its
own `DesktopSleigh` entry. No images, audio downloads, or third-party packages are
needed at runtime: the artwork and short bell sound are generated locally.

## Build and checks

```powershell
dotnet build .\DesktopSleigh.slnx -c Release
dotnet run --project .\Tests\DesktopSleigh.Checks.csproj -c Release
dotnet publish .\DesktopSleigh.csproj -c Release -r win-x64 --self-contained true
```

For a live Windows smoke check that briefly shows the team, exercises pause/resume,
and closes it automatically, append `-- --smoke` to the checks command. To
regenerate the preview and icon from the renderer, append `-- --artifacts .`.

Use `win-arm64` instead of `win-x64` for ARM64 Windows. Run the published
`DesktopSleigh.exe` from the publish directory.

## Credits

A festive fork of [Steve Syfuhs' DesktopDuck](https://github.com/SteveSyfuhs/DesktopDuck),
retaining its transparent, non-activating desktop-window behavior, screen-edge
path, and native tray controls. The Santa/reindeer artwork and synthesized bells
replace the duck artwork and recording. Licensed under the [MIT license](LICENSE);
the original copyright notice is retained.
