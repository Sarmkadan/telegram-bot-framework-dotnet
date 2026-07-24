#!/usr/bin/env python3
"""
Wrapper script to run `dotnet test` for the Telegram Bot Framework repository.

Place this file at /home/redrocket/task-factory/aider_buildcmd.py.
It determines the repository root (workdir/telegram-bot-framework-dotnet) and
executes `dotnet test` there, propagating the exit code.
"""

import subprocess
import sys
import pathlib


def main() -> None:
    # Repository root relative to this script
    repo_root = pathlib.Path(__file__).resolve().parent / "workdir" / "telegram-bot-framework-dotnet"

    # Execute `dotnet test` in the repository root
    try:
        result = subprocess.run(
            ["dotnet", "test"],
            cwd=repo_root,
            check=False,
        )
    except FileNotFoundError:
        print("Error: `dotnet` executable not found. Ensure the .NET SDK is installed.", file=sys.stderr)
        sys.exit(1)

    # Propagate the exit code from the test runner
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
