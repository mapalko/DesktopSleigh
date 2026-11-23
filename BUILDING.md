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
needed at runtime: the artwork is drawn locally and the bell recording is
embedded in the application. The recording is "Bells Jingling 2" by
DRAGON-STUDIO from Pixabay; see [audio credits and license](Assets/AUDIO-LICENSE.md).

## Build and checks

```powershell
dotnet build .\DesktopSleigh.slnx -c Release
dotnet run --project .\Tests\DesktopSleigh.Checks.csproj -c Release
```

For a live Windows smoke check that briefly shows the team, exercises pause/resume,
and closes it automatically, append `-- --smoke` to the checks command. To
regenerate the preview and icon from the renderer, append `-- --artifacts .`.

## Publish a standalone, portable Windows x64 executable

```powershell
dotnet publish .\DesktopSleigh.csproj -c Release -p:PublishProfile=PortableWinX64
```

The output is `artifacts\portable-win-x64\DesktopSleigh.exe`. It includes the
.NET runtime, native libraries, artwork, bell recording, and debug symbols in
one executable. No .NET installation or installer is needed on the destination
Windows x64 PC.

Keep the accompanying license notices when distributing it. To package the
executable and notices:

```powershell
Compress-Archive -Path .\artifacts\portable-win-x64\* -DestinationPath .\artifacts\DesktopSleigh-win-x64.zip -Force
```

Extract the ZIP to a folder of your choice and run `DesktopSleigh.exe`. The
executable runs without dependency DLLs next to it, but may extract bundled
native libraries into the user's `%TEMP%\.net` cache when needed. That cache
must be writable; it can be recreated if Windows clears temporary files.

Portable describes deployment, not settings storage: preferences still live in
`%LOCALAPPDATA%\DesktopSleigh\settings.json`, not beside the EXE or on a USB drive.
Run on startup remains opt-in and points to the EXE's current location; turn it
off before moving the EXE, then enable it again from the new location if desired.
The executable is unsigned, so Windows may show a SmartScreen warning.
