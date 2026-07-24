#!/usr/bin/env python3
"""
Utility build script for the Telegram Bot Framework repository.

Running this script will execute `dotnet test` in the repository root,
allowing you to run all unit tests from a single command.

If additional build steps are required, extend the `main` function
accordingly.
"""

import subprocess
import sys
import pathlib


def main() -> None:
    # Determine the repository root (the directory containing this script)
    repo_root = pathlib.Path(__file__).resolve().parent

    # Execute `dotnet test` in the repository root
    try:
        result = subprocess.run(
            ["dotnet", "test"],
            cwd=repo_root,
            check=False,
        )
    except FileNotFoundError as exc:
        print("Error: `dotnet` executable not found. Ensure the .NET SDK is installed.", file=sys.stderr)
        sys.exit(1)

    # Propagate the exit code from the test runner
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
