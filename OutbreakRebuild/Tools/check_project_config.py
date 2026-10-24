"""Check this foundation's documented Unity module baseline before packaging or logic checks.""" # Keep configuration checks distinct from native package resolution.
import json # Parse the actual project manifest rather than relying on source-code compilation.
from pathlib import Path # Resolve the delivered project independently from the current working directory.
root = Path(__file__).resolve().parents[1] # Locate the exact source package being checked.
dependencies = json.loads((root / "Packages" / "manifest.json").read_text())["dependencies"] # Inspect the production dependency declarations.
required = {"com.unity.modules.audio", "com.unity.modules.imageconversion", "com.unity.modules.imgui", "com.unity.modules.jsonserialize"} # Pin the verified built-in baseline used by the editor/runtime workflow.
missing = required - set(dependencies) # Catch disabled AudioListener and serialization/image support before delivery.
unsupported = {name for name in dependencies if name.startswith("com.unity.modules.") and name not in required} # Require a documentation review before adding another built-in module name to this foundation.
if missing or unsupported: raise SystemExit(f"Unity module baseline mismatch: missing={sorted(missing)}, unreviewed={sorted(unsupported)}") # Reject missing modules and phantom package identities.
if any(dependencies[name] != "1.0.0" for name in required): raise SystemExit("This foundation uses the editor-bundled 1.0.0 module declarations.") # Keep version declarations consistent with the recovered working project baseline.
editor_lines=(root / "ProjectSettings" / "ProjectVersion.txt").read_text().splitlines()
editor_version=next((line.partition(":")[2].strip() for line in editor_lines if line.startswith("m_EditorVersion:")), "")
if editor_version != "6000.6.3f1": raise SystemExit("Project must target the requested Unity editor 6000.6.3f1.")
print("PASS Unity 6000.6.3f1 target and documented Unity module baseline; actual Unity Package Manager resolution remains unverified.") # State exactly what this local check establishes.
