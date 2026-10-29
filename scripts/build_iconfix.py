#!/usr/bin/env python3
"""DMTPRO 2.0 beta icon-only repack. Do not change the application payload."""
from __future__ import annotations
import base64
import hashlib
import io
import os
import struct
from pathlib import Path
import lief
import pefile
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / "dist"
WORK.mkdir(exist_ok=True)
SOURCE = WORK / "original" / "DMTPRO.exe"
ICON = WORK / "DMTPRO_Icon.ico"
TEMP = WORK / "DMTPRO_temp.exe"
OUTPUT = WORK / "DMTPRO_patched.exe"

raw = base64.b64decode((ROOT / "assets" / "dmt-icon192.webp.b64").read_text().strip(), validate=True)
image = Image.open(io.BytesIO(raw)).convert("RGBA").resize((256, 256), Image.Resampling.LANCZOS)
image.save(ICON, format="ICO", sizes=[(16,16),(32,32),(48,48),(64,64),(128,128),(256,256)])
ico = ICON.read_bytes()
count = struct.unpack_from("<H", ico, 4)[0]
entries = [struct.unpack_from("<BBBBHHII", ico, 6+i*16) for i in range(count)]
assert count >= 5

pe = lief.PE.parse(str(SOURCE))
assert pe.has_resources
root = pe.resources
types_before = {n.id for n in root.childs}
assert not ({3,14} & types_before), "An icon already exists: refuse to overwrite or duplicate"
group_bytes = struct.pack("<HHH",0,1,count)
icon_type = lief.PE.ResourceDirectory(3)
for j,e in enumerate(entries):
    w,h,c,r,planes,bpp,size,off=e
    payload = ico[off:off+size]
    rid = j + 1
    icon_id = lief.PE.ResourceDirectory(rid)
    leaf = lief.PE.ResourceData(list(payload))
    leaf.id = 1033
    icon_id.add_child(leaf)
    icon_type.add_child(icon_id)
    group_bytes += struct.pack("<BBBBHHIH",w,h,c,r,planes,bpp,size,rid)
root.add_child(icon_type)
group_type = lief.PE.ResourceDirectory(14)
group_id = lief.PE.ResourceDirectory(1)
leaf = lief.PE.ResourceData(list(group_bytes))
leaf.id = 1033
group_id.add_child(leaf)
group_type.add_child(group_id)
root.add_child(group_type)

cfg = lief.PE.Builder.config_t()
cfg.resources = True
cfg.overlay = True
builder = lief.PE.Builder(pe, cfg)
builder.build()
builder.write(str(TEMP))

original = pefile.PE(str(SOURCE))
patched = pefile.PE(str(TEMP))
old_offset = original.get_overlay_data_start_offset()
tmp_offset = patched.get_overlay_data_start_offset()
assert old_offset is not None and tmp_offset > old_offset
assert patched.sections[-1].Name.startswith(b".rsrc")
new_resource_block = TEMP.read_bytes()[old_offset:tmp_offset]
single_file_bundle = original.get_overlay()
assert single_file_bundle is not None and len(single_file_bundle) > 100_000_000
assert single_file_bundle == TEMP.read_bytes()[tmp_offset:], "Original .NET bundle changed"

# Preserve the absolute offset of the original .NET 8 single-file bundle.
# Place new resource-section data AFTER it rather than moving the bundle.
aligned = ((old_offset+len(single_file_bundle)+511)//512)*512
prefix = TEMP.read_bytes()[:old_offset]
result = bytearray(prefix + single_file_bundle +
                   b"\x00"*(aligned-old_offset-len(single_file_bundle)) +
                   new_resource_block)
struct.pack_into("<I", result, patched.sections[-1].get_file_offset()+20, aligned)
OUTPUT.write_bytes(result)
check = pefile.PE(str(OUTPUT), fast_load=False)
types_after = {e.id for e in check.DIRECTORY_ENTRY_RESOURCE.entries}
assert {3, 10, 14, 16, 24} <= types_after, types_after
assert OUTPUT.read_bytes()[old_offset:old_offset+len(single_file_bundle)] == single_file_bundle
assert check.sections[-1].PointerToRawData == aligned
print("Icon sizes:", count)
print("EXE SHA256:", hashlib.sha256(OUTPUT.read_bytes()).hexdigest())
print("Original .NET bundle bytes preserved:", len(single_file_bundle))
print("Embedded resource types:", sorted(types_after))
