import argparse
import json
import os
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional
import tempfile

ROOT = Path(__file__).resolve().parents[1]
MEASURE_SCRIPT = ROOT / "scripts" / "measure_memory.py"


@dataclass
class Target:
    name: str
    cwd: Path
    template: List[str]
    env: Optional[Dict[str, str]] = None

    def command(self, depth: int) -> List[str]:
        cmd = []
        for token in self.template:
            if token == "{n}":
                cmd.append(str(depth))
            else:
                cmd.append(token)
        return cmd


def ensure_java_compiled() -> None:
    java_dir = ROOT / "java"
    subprocess.run(
        ["javac", "BinaryTrees.java"],
        cwd=java_dir,
        check=True,
    )


def ensure_go_built() -> None:
    go_dir = ROOT / "go"
    env = os.environ.copy()
    env["GOCACHE"] = str(go_dir / ".gocache")
    subprocess.run(
        ["go", "build", "-o", "binarytrees", "main.go"],
        cwd=go_dir,
        env=env,
        check=True,
    )


def ensure_csharp_built() -> None:
    csharp_dir = ROOT / "csharp"
    subprocess.run(
        ["dotnet", "build", "-c", "Release"],
        cwd=csharp_dir,
        check=True,
    )


def ensure_csharp_pool_built() -> None:
    csharp_dir = ROOT / "csharp-pool"
    subprocess.run(
        ["dotnet", "build", "-c", "Release"],
        cwd=csharp_dir,
        check=True,
    )


def ensure_csharp_array_built() -> None:
    csharp_dir = ROOT / "csharp-array"
    subprocess.run(
        ["dotnet", "build", "-c", "Release"],
        cwd=csharp_dir,
        check=True,
    )


def ensure_csharp_unsafe_built() -> None:
    csharp_dir = ROOT / "csharp-unsafe"
    subprocess.run(
        ["dotnet", "build", "-c", "Release"],
        cwd=csharp_dir,
        check=True,
    )


def ensure_csharp_stackonly_built() -> None:
    csharp_dir = ROOT / "csharp-stackonly"
    subprocess.run(
        ["dotnet", "build", "-c", "Release"],
        cwd=csharp_dir,
        check=True,
    )

def ensure_rust_built() -> None:
    rust_dir = ROOT / "rust"
    env = os.environ.copy()
    cargo_bin = Path.home() / ".cargo" / "bin"
    env["PATH"] = f"{cargo_bin}:{env.get('PATH', '')}"
    try:
        subprocess.run(
            ["cargo", "build", "--release"],
            cwd=rust_dir,
            env=env,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            check=True,
            text=True,
        )
    except subprocess.CalledProcessError as exc:
        raise RuntimeError(
            f"cargo build --release 失败：{exc.stderr or exc.stdout}"
        ) from exc


def run_measurement(
    target: Target,
    depth: int,
    python_exec: str,
) -> Dict[str, float]:
    tmp = tempfile.NamedTemporaryFile(delete=False)
    json_file = Path(tmp.name)
    tmp.close()

    cmd = [
        python_exec,
        str(MEASURE_SCRIPT),
        "--cwd",
        str(target.cwd),
        "--json-file",
        str(json_file),
        "--",
    ] + target.command(depth)

    env = os.environ.copy()
    if target.env:
        env.update(target.env)

    subprocess.run(
        cmd,
        text=True,
        env=env,
        check=True,
    )

    if not json_file.exists():
        raise RuntimeError(
            f"测量脚本未写入结果文件，语言={target.name}, 深度={depth}"
        )

    raw = json_file.read_text(encoding="utf-8").strip()
    json_file.unlink(missing_ok=True)
    data = json.loads(raw)
    data["language"] = target.name
    data["depth"] = depth
    return data


def format_table(rows: List[Dict[str, float]]) -> str:
    headers = ["语言", "树深度", "峰值 RSS (MB)"]
    data_rows = [
        [
            row["language"],
            f"{row['depth']}",
            f"{row['peak_mb']:.2f}",
        ]
        for row in rows
    ]

    widths = [len(header) for header in headers]
    for row in data_rows:
        for idx, cell in enumerate(row):
            widths[idx] = max(widths[idx], len(cell))

    def fmt(values: List[str]) -> str:
        return " | ".join(value.ljust(widths[i]) for i, value in enumerate(values))

    separator = "-+-".join("-" * width for width in widths)
    lines = [fmt(headers), separator]
    lines.extend(fmt(row) for row in data_rows)
    return "\n".join(lines)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="批量运行所有语言的 binary-trees 基准。")
    parser.add_argument(
        "--depths",
        type=int,
        nargs="+",
        default=[10, 16],
        help="需要测试的最大树深度列表（默认：10 16）。",
    )
    parser.add_argument(
        "--python",
        default=sys.executable,
        help="Python 解释器路径，用于运行测量脚本。",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    ensure_go_built()
    ensure_java_compiled()
    ensure_csharp_built()
    ensure_csharp_pool_built()
    ensure_csharp_array_built()
    ensure_csharp_unsafe_built()
    ensure_csharp_stackonly_built()
    ensure_rust_built()

    go_cache = ROOT / "go" / ".gocache"
    go_env = {"GOCACHE": str(go_cache)}

    targets = [
        Target(
            name="Go",
            cwd=ROOT / "go",
            template=["./binarytrees", "{n}"],
            env=go_env,
        ),
        Target(
            name="C#",
            cwd=ROOT / "csharp",
            template=["dotnet", "bin/Release/net8.0/BinaryTrees.dll", "{n}"],
        ),
        Target(
            name="C# Pool",
            cwd=ROOT / "csharp-pool",
            template=["dotnet", "bin/Release/net8.0/BinaryTrees.dll", "{n}"],
        ),
        Target(
            name="C# Array",
            cwd=ROOT / "csharp-array",
            template=["dotnet", "bin/Release/net8.0/BinaryTrees.dll", "{n}"],
        ),
        Target(
            name="C# Unsafe",
            cwd=ROOT / "csharp-unsafe",
            template=["dotnet", "bin/Release/net8.0/BinaryTrees.dll", "{n}"],
        ),
        Target(
            name="C# StackOnly",
            cwd=ROOT / "csharp-stackonly",
            template=["dotnet", "bin/Release/net8.0/BinaryTrees.dll", "{n}"],
        ),
        Target(
            name="Java",
            cwd=ROOT / "java",
            template=["java", "BinaryTrees", "{n}"],
        ),
        Target(
            name="Node.js",
            cwd=ROOT / "nodejs",
            template=["node", "main.js", "{n}"],
        ),
        Target(
            name="Python",
            cwd=ROOT / "python",
            template=[sys.executable, "main.py", "{n}"],
        ),
        Target(
            name="Rust",
            cwd=ROOT / "rust",
            template=["./target/release/rust", "{n}"],
        ),
    ]

    results: List[Dict[str, float]] = []
    for depth in args.depths:
        for target in targets:
            measurement = run_measurement(
                target,
                depth,
                args.python,
            )
            results.append(measurement)

    print(format_table(results))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
