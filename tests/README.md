# Motion regression checks

These Windows desktop checks use the stock, embedded eSheep animation XML. They
create an isolated temporary runtime and configuration; they do not read or modify
an installed pet's configuration. Interactive checks briefly show test pets and a
topmost support window. Do not run them on a locked/headless desktop.

## Run

Prerequisites: Visual Studio Build Tools with desktop MSBuild and Roslyn, NuGet,
and the .NET Framework 4.7.2 targeting pack.

```powershell
nuget restore src/DesktopPet_Portable.sln
powershell -NoProfile -File tests/run-motion-tests.ps1
```

If reference assemblies are supplied through the
`Microsoft.NETFramework.ReferenceAssemblies.net472` NuGet package instead of an
installed targeting pack, pass its `build/.NETFramework/v4.7.2` directory using
`-FrameworkPath`. `-MSBuildPath` can select a particular desktop MSBuild executable.

The default matrix is Release x64/x86, scale 1/3, and every connected display.
`-Platforms`, `-Scales`, and `-Displays` accept arrays when invoked directly from
PowerShell. The script prints the temporary output location and retains it for
inspection. Package restore is a separate prerequisite; the test runner itself
does not download packages or access an installed pet.

## Coverage

- Removed-display indices in XML expressions and pet bounds, relocation from a
  removed monitor, and display-change recovery callbacks after disposal.
- Interpolation endpoints, direction changes, negative desktop coordinates, window
  resize transforms, and timestamp-based drag velocity.
- Real WinForms movement between animation ticks, immediate drag positioning,
  unchanged stock walking distance and sprite cadence, and idle timer shutdown.
- Window location events with the high-rate timer disabled, support-window
  closure, real window landing, wall rebound without a stopped vertical step,
  taskbar landing, and equal flight trajectories at different render cadences.
- Lazy sprite mirroring, respawn resets, child initialization, concurrent pets,
  and disposal while dragging, including timer and hook cleanup.

The 8 ms timer is a scheduling request, not a frame-rate guarantee. Presentation
uses integer window positions and remains subject to the Windows message loop.
The existing XML still controls sprite frames and animation timing. The temporary
1 ms timer-resolution request is released when motion stops or the pet is disposed;
stationary window attachment uses events with a 100 ms polling fallback. Timer
resolution can affect power use, especially on older Windows versions.

These checks are not a certification of all pets, mixed-DPI transitions, Windows
versions, or Store packaging. Additional manual checks: drag across monitors,
release while the cursor is outside the pet, close/minimize the supporting window,
and exercise a pet with child animations. Test long sessions and battery use
separately before making power-consumption claims.

## Validation on this change

The Portable Release x64/x86 matrix passed using stock eSheep at scales 1 and 3
on two connected displays. Both architectures also passed the pure motion tests.
The .NET Framework 4.8 desktop/Store source project compiled for x64 and x86 after
restoring its individually referenced legacy NuGet versions, supplying the locally
installed Windows SDK with `ReferencePath`, and disabling the Store-copy
`PostBuildEvent`. This is a source compile check; assembly-resolution warnings
remain and Store packaging/installation were not validated.
