#!/usr/bin/env python3
"""Check that Blender review part IDs cannot silently replace runtime part IDs."""

import json
import re
from pathlib import Path
import sys


ROOT = Path(__file__).resolve().parents[1]
RUNTIME_PATH = ROOT / "Assets/Resources/WispFamily20261112/catalog.json"
DESIGN_PATH = ROOT / "Docs/Design/WISP_Parts/PART_CATALOG.json"
MAP_PATH = ROOT / "Docs/Design/WISP_Parts/ID-MAPPING.json"


def read_json(path: Path):
    with path.open(encoding="utf-8") as stream:
        return json.load(stream)


def main() -> int:
    runtime = read_json(RUNTIME_PATH)
    design = read_json(DESIGN_PATH)
    mapping = read_json(MAP_PATH)
    runtime_ids = {part["id"] for part in runtime["assets"]}
    design_by_id = {part["id"]: part for part in design["parts"]}
    entries = mapping["entries"]
    keys = [entry["stableKey"] for entry in entries]
    errors = []

    if len(keys) != len(set(keys)):
        errors.append("stableKey values must be unique across both namespaces")

    runtime_entries = [entry for entry in entries if entry["namespace"] == "runtime"]
    design_entries = [entry for entry in entries if entry["namespace"] == "designReview"]
    for entry in runtime_entries:
        part_id = entry["catalogId"]
        if part_id not in runtime_ids:
            errors.append(f"runtime mapping references missing catalog ID: {part_id}")
        source = entry.get("source")
        if source and not (ROOT / source).is_file():
            errors.append(f"runtime mapping references missing source: {source}")
        if entry["stableKey"] != f"runtime:{part_id}":
            errors.append(f"runtime stableKey does not match catalog ID: {part_id}")
        blender_source = entry.get("blenderSource")
        if blender_source and not (ROOT / blender_source).is_file():
            errors.append(f"runtime mapping references missing Blender source: {blender_source}")

    for entry in design_entries:
        catalog_id = entry["catalogId"]
        part = design_by_id.get(catalog_id)
        if part is None:
            errors.append(f"design mapping references missing catalog ID: {catalog_id}")
            continue
        if part["name"] != entry.get("partName"):
            errors.append(f"design name changed without updating mapping: {catalog_id}")
        if entry["stableKey"] != f"designReview:{entry['partName']}":
            errors.append(f"design stableKey does not match review name: {catalog_id}")

    listed_candidates = set(mapping["pendingVisualApproval"])
    expected_candidates = {
        f"designReview:{part['name']}"
        for part in design["parts"]
        if part.get("design_status") == "PENDING_PD"
    }
    if listed_candidates != expected_candidates:
        errors.append(
            "pendingVisualApproval must list every PENDING_PD Blender part exactly "
            f"(missing={sorted(expected_candidates-listed_candidates)}, "
            f"extra={sorted(listed_candidates-expected_candidates)})"
        )
    mapped_keys = set(keys)
    if not listed_candidates <= mapped_keys:
        errors.append("every pending design candidate must have a namespaced mapping entry")

    catalog_by_id = {part["id"]: part for part in runtime["assets"]}
    selectable_candidates = {
        "VIS_B02_SKYWING", "VIS_B03_CARGO_PACK", "VIS_L02_BASTION_TRACK",
        "VIS_L03_TARANTULAN", "VIS_L04_DRAFT_HOVER",
        "VIS_H01_RADOME", "VIS_H02_LONGSIGHT", "VIS_H03_SWIFTFIN",
        "VIS_H04_BULWARK", "VIS_H05_NEEDLE", "VIS_H06_LANCET",
        "VIS_C01_STANDARD", "VIS_C02_LIGHTFRAME", "VIS_C03_BULWARK_CORE",
        "VIS_C04_ARC_CORE",
    }
    if not selectable_candidates <= set(catalog_by_id):
        errors.append(f"missing selectable visual candidates: {sorted(selectable_candidates-set(catalog_by_id))}")
    for part_id in selectable_candidates & set(catalog_by_id):
        asset_path = ROOT / "Assets/Resources" / (catalog_by_id[part_id]["resource"] + ".json")
        if not asset_path.is_file():
            errors.append(f"runtime catalog resource is missing: {part_id}")
        if f"runtime:{part_id}" not in mapped_keys:
            errors.append(f"selectable visual candidate lacks a stable ID mapping: {part_id}")

    # Unity resolves imported assets by the GUID in each .meta file. A unique
    # runtime catalog ID is insufficient if a newly exported asset reuses a GUID.
    candidate_meta_guids = {}
    for part_id in selectable_candidates & set(catalog_by_id):
        asset_path = ROOT / "Assets/Resources" / (catalog_by_id[part_id]["resource"] + ".json")
        meta_path = Path(str(asset_path) + ".meta")
        if not meta_path.is_file():
            errors.append(f"selectable visual candidate has no Unity .meta file: {part_id}")
            continue
        match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", meta_path.read_text(encoding="utf-8"), re.MULTILINE)
        if not match:
            errors.append(f"selectable visual candidate has an invalid Unity GUID: {part_id}")
            continue
        candidate_meta_guids[part_id] = match.group(1).lower()

    guid_paths = {}
    for meta_path in (ROOT / "Assets").rglob("*.meta"):
        try:
            match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", meta_path.read_text(encoding="utf-8", errors="ignore"), re.MULTILINE)
        except OSError:
            continue
        if match:
            guid_paths.setdefault(match.group(1).lower(), []).append(meta_path)
    for part_id, guid in candidate_meta_guids.items():
        paths = guid_paths.get(guid, [])
        if len(paths) != 1:
            errors.append(f"selectable visual candidate Unity GUID is not unique: {part_id} ({len(paths)} assets)")

    if errors:
        for error in errors:
            print(f"FAIL {error}")
        return 1
    print(
        "PASS "
        f"{len(runtime_entries)} runtime IDs, {len(design_entries)} review IDs, "
        f"{len(listed_candidates)} pending Blender candidates, "
        f"{len(candidate_meta_guids)} selectable Unity GUIDs unique; namespaces remain distinct"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
