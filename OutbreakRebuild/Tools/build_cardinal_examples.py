"""Build the aligned references, six seeded presets and two repeat-seed comparisons."""
import html
import json
from pathlib import Path

import city_atlas as atlas
import generate_reference_city as references
import generate_seeded_city as cities


def main():
    output=atlas.ROOT/"CityReferences/cardinal"
    source=atlas.ROOT/"CityReferences/atlas"
    entries=[]
    for identity in ("west-oakland","manhattan","prague-bubenec"):
        geography=json.loads((source/(identity+".geojson")).read_text())
        profile=json.loads((source/(identity+".profile.json")).read_text())
        document,metadata,artwork=references.generate(profile["sample"],geography["features"],profile["provenance"],214,orientation="cardinal")
        stem=identity+"-214-cardinal"
        folder=output/"aligned"
        atlas.write_json(folder/(stem+".obmap2"),document)
        atlas.write_json(folder/(stem+".generation.json"),metadata)
        (folder/(stem+".svg")).write_text(artwork)
        entries.append({"folder":"aligned","stem":stem,"label":identity+" — aligned reference", "buildings":len(metadata["buildings"]),"rotation_degrees":metadata["grid"]["rotation_degrees"]})
        print(stem,len(metadata["buildings"]),"buildings; rotation",round(metadata["grid"]["rotation_degrees"],2))
    examples=[(name,214) for name in cities.PRESETS]+[("compact-grid",215),("suburban-dead-ends",215)]
    for name,seed in examples:
        settings=cities.preset_settings(name,seed)
        profile=cities.profile_for(settings["reference_id"],source)
        document,metadata,artwork,recipe=cities.generate_city(settings,profile)
        # Replay the saved JSON representation, including all resolved defaults.
        replay,_,_,_=cities.generate_city(json.loads(json.dumps(recipe))["settings"],profile)
        if cities.canonical(document)!=cities.canonical(replay):raise AssertionError("Recipe replay changed map bytes")
        stem=document["id"];folder=output/"layouts"
        atlas.write_json(folder/(stem+".obmap2"),document)
        atlas.write_json(folder/(stem+".generation.json"),metadata)
        atlas.write_json(folder/(stem+".recipe.json"),recipe)
        (folder/(stem+".svg")).write_text(artwork)
        entries.append({"folder":"layouts","stem":stem,"label":name+" — seed "+str(seed),"buildings":len(metadata["buildings"]),"settings":settings})
        print(stem,len(metadata["buildings"]),"buildings; exact replay passed")
    atlas.write_json(output/"examples.json",{"generator_version":references.GENERATOR_VERSION,"examples":entries})
    cards=[]
    for entry in entries:
        base=entry["folder"]+"/"+entry["stem"]
        artwork=(output/(base+".svg")).read_text()
        recipe=f' · <a href="{base}.recipe.json">Replay recipe</a>' if entry["folder"]=="layouts" else ''
        cards.append(f'<article><h2>{html.escape(entry["label"])}</h2><p>{entry["buildings"]} generated buildings</p>{artwork}<p><a href="{base}.obmap2">Game map</a>{recipe} · <a href="{base}.generation.json">Details</a></p></article>')
    (output/"index.html").write_text('''<!doctype html><meta charset="utf-8"><title>OutbreakGrid cardinal city generator</title><style>body{background:#101920;color:#e2eaf0;font:16px system-ui;max-width:1500px;margin:24px auto;padding:0 20px}p{line-height:1.5}a{color:#8ac8ff}.cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(330px,1fr));gap:20px}article{background:#1c2a35;padding:18px;border-radius:10px}h2{font-size:20px}svg{width:100%;height:360px}body.hide-grid .grid{display:none}button{padding:10px}</style><h1>Cardinal cities · replayable seeds and settings</h1><p>The first three examples rotate a reference layout's dominant street axes toward game north/south and east/west. Secondary angles remain; geographic north is rotated. The following maps generate entirely new, strictly cardinal street networks.</p><p>Six presets: compact grid, long blocks, mixed town, town centre, suburban loops and suburban dead ends. The suburban presets are designed examples, not newly downloaded geographic surveys. Repeat a recipe for identical map bytes; change the seed to change streets and lots.</p><p>Controls: block counts, block dimensions, variation, connecting loops, dead-end branches, local/arterial road widths, sidewalks, building density, setbacks, open space and centre density. Coverage is a placement target; actual buildings depend on available space. No NPC or traffic behavior is changed.</p><button onclick="document.body.classList.toggle('hide-grid')">Show / hide 5-metre grid</button><p>Extract this pack before opening the HTML. Load its .obmap2 files using Load Map in the game.</p><div class="cards">'''+''.join(cards)+'''</div><p>Data provenance and terms: <a href="../NOTICE.md">NOTICE</a>. Original geographic source files are kept separately in the atlas folder. Native Unity rendering remains unverified.</p>''')


if __name__=="__main__":main()
