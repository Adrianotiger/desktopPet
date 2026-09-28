param(
    [string]$FrameworkPath = "${env:ProgramFiles(x86)}/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2",
    [string]$MSBuildPath,
    [string[]]$Platforms = @('x64', 'x86'),
    [int[]]$Scales = @(1, 3),
    [int[]]$Displays = @()
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
if (-not $MSBuildPath) {
    $vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
    $MSBuildPath = & $vswhere -latest -products '*' -find MSBuild/**/Bin/MSBuild.exe | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'Visual Studio MSBuild was not found.' }
if (-not (Test-Path "$FrameworkPath/System.dll")) { throw 'Provide .NET Framework 4.7.2 reference assemblies through -FrameworkPath.' }
$compiler = Join-Path (Split-Path $MSBuildPath -Parent) 'Roslyn/csc.exe'
$output = Join-Path ([IO.Path]::GetTempPath()) ('DesktopPet-motion-tests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output | Out-Null
Add-Type -AssemblyName System.Windows.Forms
if ($Displays.Count -eq 0) { $Displays = @(0..([Windows.Forms.Screen]::AllScreens.Length - 1)) }
Write-Output "Isolated test output: $output"
foreach ($platform in $Platforms) {
    if ($platform -notin @('x64', 'x86')) { throw "Unsupported platform: $platform" }
    $runtime = Join-Path $output $platform
    & $MSBuildPath "$repo/src/DesktopPet_Portable.csproj" /nologo /v:minimal /p:Configuration=Release "/p:Platform=$platform" "/p:FrameworkPathOverride=$FrameworkPath" "/p:OutputPath=$runtime/" "/p:IntermediateOutputPath=$output/obj-$platform/" /p:GenerateManifests=false /p:DocumentationFile= /p:CodeAnalysisRuleSet=
    if ($LASTEXITCODE -ne 0) { throw "Portable $platform build failed." }
    & $compiler /nologo "/platform:$platform" /r:System.Drawing.dll "/r:$FrameworkPath/System.Numerics.dll" "/r:$runtime/System.Numerics.Vectors.dll" "/out:$runtime/MotionTests.exe" "$repo/tests/MotionTests.cs" "$repo/src/dotNet/MotionTrack.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Motion test compilation failed.' }
    & "$runtime/MotionTests.exe"
    if ($LASTEXITCODE -ne 0) { throw 'Motion tests failed.' }
    & $compiler /nologo "/platform:$platform" /r:System.Drawing.dll /r:System.Windows.Forms.dll "/out:$runtime/RuntimeMotionTests.exe" "$repo/tests/RuntimeMotionTests.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Runtime test compilation failed.' }
    Copy-Item "$repo/src/Resources/animations.xml" "$runtime/motion-fixture.xml"
    foreach ($scale in $Scales) {
        foreach ($display in $Displays) {
            & "$runtime/RuntimeMotionTests.exe" $scale $display
            if ($LASTEXITCODE -ne 0) { throw "Runtime tests failed: $platform, scale $scale, display $display." }
        }
    }
}
