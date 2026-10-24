"""Checks that every business rule ID (BR-xxx) in docs/01-business-rules.md is referenced by at least one test.

Usage: python3 build/check-rule-traceability.py [--markdown]
Exit code 1 when a rule has no test. With --markdown, prints the rule -> tests table.
"""
import pathlib
import re
import sys

root = pathlib.Path(__file__).resolve().parent.parent
rules = sorted(set(re.findall(r"\b(BR-[A-Z]\d{2})\b", (root / "docs/01-business-rules.md").read_text())))

references: dict[str, set[str]] = {rule: set() for rule in rules}
for test_file in sorted((root / "tests").rglob("*.cs")):
    if "/obj/" in test_file.as_posix() or "/bin/" in test_file.as_posix():
        continue
    text = test_file.read_text()
    # Ranges like "BR-T01–BR-T04" or "BR-L01–L05" reference every rule in between.
    for prefix, start, end in re.findall(r"BR-([A-Z])(\d{2})\s*[–-]\s*(?:BR-[A-Z])?(\d{2})", text):
        for number in range(int(start), int(end) + 1):
            references.setdefault(f"BR-{prefix}{number:02d}", set()).add(test_file.stem)
    for rule in re.findall(r"\b(BR-[A-Z]\d{2})\b", text):
        references.setdefault(rule, set()).add(test_file.stem)

missing = [rule for rule in rules if not references[rule]]

if "--markdown" in sys.argv:
    print("| Rule | Tests |\n|---|---|")
    for rule in rules:
        print(f"| {rule} | {', '.join(sorted(references[rule])) or '**missing**'} |")
else:
    for rule in rules:
        print(f"{rule}: {len(references[rule])} test file(s)")

if missing:
    print(f"Rules without tests: {', '.join(missing)}")
    sys.exit(1)
print(f"All {len(rules)} rules are referenced by tests.", file=sys.stderr)
