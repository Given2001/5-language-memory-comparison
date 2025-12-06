#!/usr/bin/env python3
"""Install or download dependencies for all language implementations."""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def run(cmd: list[str], cwd: Path) -> None:
    print(f"[bootstrap] {' '.join(cmd)} (cwd={cwd})")
    subprocess.run(cmd, cwd=cwd, check=True)


def main() -> int:
    run(["python3", "-m", "pip", "install", "-r", "requirements.txt"], ROOT / "python")
    run(["npm", "install"], ROOT / "nodejs")
    run(["go", "mod", "download"], ROOT / "go")
    run(["cargo", "fetch"], ROOT / "rust")
    run(["dotnet", "restore"], ROOT / "csharp")
    run(["dotnet", "restore"], ROOT / "csharp-pool")
    run(["dotnet", "restore"], ROOT / "csharp-array")
    run(["dotnet", "restore"], ROOT / "csharp-unsafe")
    run(["dotnet", "restore"], ROOT / "csharp-stackonly")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
