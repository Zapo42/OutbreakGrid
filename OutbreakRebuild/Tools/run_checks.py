"""Run production C# checks sequentially; these are not native Unity or Windows player tests."""
import argparse
import glob
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


def executable(value, label):
    """Reject missing explicit tools instead of silently switching to a different backend."""
    found = shutil.which(value) if value else None
    if not found:
        raise ValueError(label + " executable not found: " + str(value))
    return str(Path(found).resolve())


def manifest_sources(project):
    """Use the same explicit Compile includes as MSBuild for the optional Mono backend."""
    document = ET.parse(project).getroot()
    if document.findtext("./PropertyGroup/EnableDefaultCompileItems") != "false":
        raise ValueError("Mono requires explicit Compile items: " + str(project))
    sources = set()
    for item in document.findall("./ItemGroup/Compile"):
        # Fail on unsupported MSBuild constructs rather than skipping authoritative sources.
        if set(item.attrib) != {"Include"} or "$" in item.attrib["Include"]:
            raise ValueError("Unsupported Mono Compile item in " + str(project))
        for pattern in item.attrib["Include"].split(";"):
            matches = glob.glob(str(project.parent / pattern), recursive=True)
            if not matches:
                raise ValueError("Compile include has no sources: " + pattern)
            sources.update(str(Path(path).resolve()) for path in matches)
    if not sources:
        raise ValueError("No Compile sources in " + str(project))
    return sorted(sources)


def manifest_defines(project, mode):
    """Read conditional input symbols from the project instead of maintaining a second list."""
    symbols = ""
    for item in ET.parse(project).getroot().findall("./PropertyGroup/DefineConstants"):
        condition = item.get("Condition")
        if condition and condition != "'$(InputMode)' == '" + mode + "'":
            if re.fullmatch(r"'\$\(InputMode\)' == '[^']+'", condition):
                continue
            raise ValueError("Unsupported Mono symbol condition: " + condition)
        symbols = item.text or ""
    return symbols


def run(command):
    print("RUN " + shlex.join([str(part) for part in command]), flush=True)
    subprocess.run(command, cwd=ROOT, check=True)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, epilog="Default reports/examples are retained in a printed temporary directory, never tracked Examples. --output explicitly selects their destination. Mono uses project Compile manifests and rejects unsupported items.")
    parser.add_argument("--dotnet", help=".NET SDK executable (8 or later); preferred when found on PATH")
    parser.add_argument("--compiler", help="Mono C# compiler; selecting this or --mono requests the Mono backend")
    parser.add_argument("--mono", help="Mono runtime executable")
    parser.add_argument("--output", type=Path, help="report/example destination; default: a retained temporary directory")
    args = parser.parse_args(argv)
    try:
        if args.dotnet and (args.compiler or args.mono):
            raise ValueError("Choose --dotnet or the Mono options, not both")
        use_mono = bool(args.compiler or args.mono) or not (args.dotnet or shutil.which("dotnet"))
        if use_mono:
            compiler = executable(args.compiler or "mcs", "C# compiler")
            mono = executable(args.mono or "mono", "Mono runtime")
        else:
            dotnet = executable(args.dotnet or "dotnet", ".NET")
            sdks = subprocess.check_output([dotnet, "--list-sdks"], text=True)
            if not any(int(major) >= 8 for major in re.findall(r"^(\d+)\.", sdks, re.MULTILINE)):
                raise ValueError("A .NET SDK version 8 or later is required")
        # Resolve tools before making output folders, so missing-tool checks have no side effects.
        output = args.output.expanduser().resolve() if args.output else Path(tempfile.mkdtemp(prefix="OutbreakChecks-"))
        output.mkdir(parents=True, exist_ok=True)
        print("Check output: " + str(output), flush=True)
        run([sys.executable, ROOT / "Tools/check_project_config.py"])
        projects = [("FoundationChecks", None, str(output)), ("WindowsDialogChecks", None, str(output / "FOUNDATION_CHECKS.txt")), ("UiChecks", "Legacy", str(output / "FOUNDATION_CHECKS.txt")), ("UiChecks", "NewOnly", str(output / "FOUNDATION_CHECKS.txt"))]
        with tempfile.TemporaryDirectory(prefix="OutbreakCheckBinaries-") as temporary:
            for name, mode, argument in projects:
                project = ROOT / "Tests" / (name + ".csproj")
                if use_mono:
                    folder = Path(temporary) / (name + (mode or ""))
                    folder.mkdir()
                    # Core fixture lookup is relative to the host executable, not its working directory.
                    shutil.copytree(ROOT / "Tests/Fixtures", folder / "Fixtures")
                    binary = folder / (name + ".exe")
                    command = [compiler, "-langversion:latest", "-warnaserror+", "-r:System.Runtime.Serialization.dll", "-out:" + str(binary)]
                    symbols = manifest_defines(project, mode)
                    if symbols:
                        command.append("-define:" + symbols.replace(";", ","))
                    run(command + manifest_sources(project))
                    run([mono, binary, argument])
                else:
                    # These projects share obj state; complete each build and run before starting the next.
                    command = [dotnet, "run", "--project", project, "--no-launch-profile"]
                    if mode:
                        command.append("-p:InputMode=" + mode)
                    run(command + ["--", argument])
        print("PASS all hosted checks. Native Unity, native dialogs and Windows player builds were NOT RUN.", flush=True)
        return 0
    except ValueError as error:
        parser.error(str(error))
    except (OSError, subprocess.CalledProcessError) as error:
        print("FAIL " + str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
