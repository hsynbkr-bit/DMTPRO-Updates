#!/usr/bin/env python3
"""Read-only DMTPRO .NET single-file bundle inspection. No application code is run."""
from __future__ import annotations
import hashlib, json, os, pathlib, struct, sys, zlib

SIG=bytes.fromhex("8b1202b96a612038727b930214d7a03213f5b9e6efae3318ee3b2dce24b36aae")
NEEDLES=["updat","releases/latest","api.github.com","dmtpro_app_win_x64.zip","version.json","checkforupdate","autoupdate","github"]
def read_path(buf, pos):
    n=0
    for i in range(5):
        val=buf[pos]; pos+=1
        n |= (val & 127) << (7*i)
        if not (val & 128): break
    else: raise ValueError("invalid 7-bit path length")
    if n>2048: raise ValueError("suspicious path")
    return buf[pos:pos+n].decode("utf-8"),pos+n
def unpack(exe):
    buf=pathlib.Path(exe).read_bytes()
    hit=buf.find(SIG)
    if hit<8: raise ValueError("not a .NET single-file bundle")
    offset,=struct.unpack_from("<q",buf,hit-8)
    if not 0<offset<len(buf)-64: raise ValueError("invalid header offset")
    major,minor,n=struct.unpack_from("<IIi",buf,offset)
    if major not in (1,2,6) or n<1 or n>2000: raise ValueError("unexpected header "+str((major,minor,n)))
    bid,pos=read_path(buf,offset+12)
    if major>=2:pos+=40
    entries=[]
    for i in range(n):
        off,size=struct.unpack_from("<qq",buf,pos);pos+=16
        packed=0
        if major>=6:packed,=struct.unpack_from("<q",buf,pos);pos+=8
        typ=buf[pos];pos+=1
        name,pos=read_path(buf,pos)
        if not (off>=0 and size>=0 and 0<=off+ (packed if packed else size) <=len(buf)):raise ValueError("invalid payload")
        entries.append(dict(name=name,offset=off,size=size,packed=packed,type=typ))
    return buf,major,entries

def main(exe):
    buf,major,entries=unpack(exe)
    findings=[]
    for e in entries:
        name=e["name"]
        if not (name.lower().endswith(".dll") and ("dmt" in name.lower() or name.lower()=="dm tpro.dll".replace(" ",""))):
            continue
        packed=e["packed"]; data=buf[e["offset"]:e["offset"]+(packed or e["size"])]
        if packed: data=zlib.decompress(data,-15)
        if len(data)!=e["size"]:raise ValueError("payload size mismatch "+name)
        detected=[]
        for kw in NEEDLES:
            if kw.encode() in data.lower() or kw.encode("utf-16le") in data.lower(): detected.append(kw)
        findings.append({"assembly":name,"sha256":hashlib.sha256(data).hexdigest(),"bytes":len(data),"indicators":detected})
    summary={"bundle_version":major,"bundle_entries":len(entries),"app_assemblies":findings,
             "possible_updater_logic":any("updat" in x["indicators"] or "checkforupdate" in x["indicators"] or "autoupdate" in x["indicators"] for x in findings)}
    print(json.dumps(summary,ensure_ascii=False,indent=2))
    if os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(os.environ["GITHUB_STEP_SUMMARY"],"a",encoding="utf8") as f:
            f.write("\n## DMTPRO 2.0 managed bundle static indicators\n\n" +
                    "\x60\x60\x60json\n"+json.dumps(summary,ensure_ascii=False,indent=2)+"\n\x60\x60\x60\n")
    return 0

if __name__=="__main__":
    sys.exit(main(sys.argv[1]))
