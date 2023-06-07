import subprocess
from pathlib import Path
from typing import Tuple, List


def lint(linter: Path, sources: Path) -> Tuple[int, List[Path]]:
    """
    Lint translations files to ensure their content is valid resx
    """
    errors = []
    for file in sources.glob("*.resx"):
        code = subprocess.call(f'{linter} "{file}"', shell=True)
        if code != 0:
            errors.append(file)

    return (int(bool(len(errors))), errors)
