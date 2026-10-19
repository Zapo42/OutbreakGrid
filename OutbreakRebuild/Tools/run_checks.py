"""Compile and execute the delivered C# foundation; never substitute a Python simulation.""" # Keep the validation target explicit.
import argparse # Accept an installed Mono compiler and runtime.
from pathlib import Path # Locate the project relative to this script.
import shutil # Discover standard Mono executables.
import subprocess # Propagate real compiler and test failures.
import sys # Run configuration checks with the same Python interpreter as this tool.
import tempfile # Keep disposable binaries outside Unity Assets.
parser = argparse.ArgumentParser(description=__doc__) # Document what the runner actually executes.
parser.add_argument("--compiler", default=shutil.which("mcs")) # Use an installed C# compiler unless explicitly supplied.
parser.add_argument("--mono", default=shutil.which("mono")) # Use an installed Mono runtime unless explicitly supplied.
args = parser.parse_args() # Read supported toolchain inputs.
if not args.compiler or not args.mono: parser.error("Install Mono or use dotnet run --project Tests/FoundationChecks.csproj -- Examples.") # Fail honestly when no C# execution toolchain exists.
root = Path(__file__).resolve().parents[1] # Keep fresh ZIP extraction independent of the current folder.
subprocess.run([sys.executable, str(root / "Tools" / "check_project_config.py")], check=True) # Reject missing or unreviewed module declarations before reporting C# logic success.
with tempfile.TemporaryDirectory(prefix="OutbreakFoundation-") as folder: # Avoid shipping an accidental test executable as a Windows game.
    libraries = [] # Keep exact assembly references in dependency order.
    for name in ["Contracts", "Core", "Persistence"]: # Compile the real layers separately so dependency direction is enforced.
        library = str(Path(folder) / ("Outbreak." + name + ".dll")) # Match the production assembly identities used by Unity.
        sources = sorted(str(p) for p in (root / "Assets" / "Outbreak" / name).glob("*.cs")) # Compile the delivered source for this layer only.
        subprocess.run([args.compiler, "-langversion:latest", "-warnaserror+", "-target:library", "-r:System.Runtime.Serialization.dll", "-out:" + library] + ["-r:" + path for path in libraries] + sources, check=True) # Give a layer only its allowed earlier dependencies.
        libraries.append(library) # Make the successfully compiled layer available to later layers.
    shutil.copytree(root / "Tests" / "Fixtures", Path(folder) / "Fixtures") # Supply independently recorded prior-release continuation to the same test executable.
    binary = str(Path(folder) / "FoundationChecks.exe") # Name the host test process separately from the player.
    subprocess.run([args.compiler, "-langversion:latest", "-warnaserror+", "-r:System.Runtime.Serialization.dll", "-out:" + binary] + ["-r:" + path for path in libraries] + [str(root / "Tests" / "Program.cs"), str(root / "Tests" / "HumanReactionChecks.cs"), str(root / "Tests" / "MovementChecks.cs"), str(root / "Tests" / "InfectedEscapeChecks.cs"), str(root / "Tests" / "ShelterChecks.cs"), str(root / "Tests" / "ShelterRecoveryChecks.cs"), str(root / "Tests" / "ScenarioSetupChecks.cs"), str(root / "Tests" / "ZombieWanderChecks.cs"), str(root / "Tests" / "DoorDamageChecks.cs"), str(root / "Tests" / "DoorHearingChecks.cs"), str(root / "Tests" / "ZombieSoundChecks.cs"), str(root / "Tests" / "HumanWarningChecks.cs"), str(root / "Tests" / "WarningAttractionChecks.cs"), str(root / "Tests" / "DoorBracingChecks.cs"), str(root / "Tests" / "WarningShelterChecks.cs"), str(root / "Tests" / "DoorRepairChecks.cs"), str(root / "Tests" / "HungerChecks.cs"), str(root / "Tests" / "FoodSharingChecks.cs"), str(root / "Tests" / "ShelfMemoryChecks.cs"), str(root / "Assets" / "Outbreak" / "Unity" / "MovementPresentation.cs")], check=True) # Link the checks against actual production assemblies and treat all warnings as failures.
    subprocess.run([args.mono, binary, str(root / "Examples")], check=True) # Execute production logic and retain evidence with loadable sample documents.
    platform = str(Path(folder) / "WindowsDialogChecks.exe") # Keep platform signature checks separate from a game executable.
    subprocess.run([args.compiler, "-langversion:latest", "-warnaserror+", "-define:UNITY_STANDALONE_WIN", "-out:" + platform, str(root / "Assets" / "Outbreak" / "Unity" / "FileDialogs.cs"), str(root / "Tests" / "WindowsDialogChecks.cs")], check=True) # Compile the actual Windows Open and Save bindings without a Unity substitute.
    subprocess.run([args.mono, platform, str(root / "Examples" / "FOUNDATION_CHECKS.txt")], check=True) # Validate the native record layout without pretending to open a Windows dialog on this host.
    for mode, symbols in [("Legacy", "UNITY_EDITOR,ENABLE_LEGACY_INPUT_MANAGER"), ("NewOnly", "UNITY_EDITOR,ENABLE_INPUT_SYSTEM")]: # Check real handlers with both compatible input and the documented incompatible mode.
        ui_binary = str(Path(folder) / ("UiChecks" + mode + ".exe")) # Keep host test executables distinct from a compiled Unity player.
        ui_sources = [root / "Assets" / "Outbreak" / "Unity" / "FoundationApp.cs", root / "Assets" / "Outbreak" / "Unity" / "MovementPresentation.cs", root / "Assets" / "Outbreak" / "Unity" / "FileDialogs.cs", root / "Assets" / "Outbreak" / "Editor" / "InputSetup.cs", root / "Assets" / "Outbreak" / "Editor" / "InputDiagnostics.cs", root / "Tests" / "UiHarness.cs"] # Compile actual UI, pause-safe dispatch and diagnostic handlers with an explicitly scoped API double.
        subprocess.run([args.compiler, "-langversion:latest", "-warnaserror+", "-define:" + symbols, "-out:" + ui_binary] + ["-r:" + path for path in libraries] + [str(path) for path in ui_sources], check=True) # Reject syntax and handler integration errors in either input configuration.
        subprocess.run([args.mono, ui_binary, str(root / "Examples" / "FOUNDATION_CHECKS.txt")], check=True) # Record command and setup behavior without claiming native Unity mouse or layout verification.
