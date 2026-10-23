"""Fails (exit 1) when the line coverage of a Cobertura report is below a threshold.

Usage: python3 build/check-coverage.py <cobertura.xml> <minimum-percent>
"""
import sys
import xml.etree.ElementTree as ET

report, minimum = sys.argv[1], float(sys.argv[2])
coverage = float(ET.parse(report).getroot().attrib["line-rate"]) * 100

print(f"Line coverage: {coverage:.1f}% (minimum {minimum:g}%)")
if coverage < minimum:
    print("Coverage gate failed.")
    sys.exit(1)
