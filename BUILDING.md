# Running and building DesktopSleigh

## Run from source

Windows only. The `net10.0-windows` target is inherited from DesktopDuck; this
fork does not add a newer .NET requirement.

To build or run from source, install the
[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0):

```powershell
dotnet run --project .\DesktopSleigh.csproj
```

A framework-dependent executable needs the **.NET 10 Desktop Runtime**, not the
SDK. A **self-contained** executable (see the publish command below) needs no
separate .NET installation.

## Controls and settings

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
```

For a live Windows smoke check that briefly shows the team, exercises pause/resume,
and closes it automatically, append `-- --smoke` to the checks command. To
regenerate the preview and icon from the renderer, append `-- --artifacts .`.

## Publish a self-contained executable

```powershell
dotnet publish .\DesktopSleigh.csproj -c Release -r win-x64 --self-contained true
```

Use `win-arm64` instead of `win-x64` for ARM64 Windows. Run the published
`DesktopSleigh.exe` from the publish directory.
