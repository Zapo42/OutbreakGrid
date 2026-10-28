"""Build a replay-verified district library and connected mixed-city examples."""
import html
import json
import argparse

import city_atlas as atlas
import generate_district_city as districts
import generate_seeded_city as streets


def main():
    root=atlas.ROOT/"CityReferences/districts";output=root/"maps"
    catalogue=json.loads(districts.CATALOGUE.read_text());entries=[]
    for kind,spec in catalogue["districts"].items():
        for seed in (214,215):
            recipe=districts.district_recipe(kind,seed)
            result=districts.generate_district(recipe)
            replay=districts.generate_district(json.loads(json.dumps(recipe)))
            if streets.canonical(result[0])!=streets.canonical(replay[0]):raise AssertionError("District replay diverged")
            stem=districts.save(result,output);metadata=result[1]
            entries.append({"stem":stem,"name":spec["name"],"kind":kind,"category":spec["category"],"seed":seed,
                            "buildings":len(metadata["buildings"]),"coverage":metadata["actual_building_coverage"],
                            "reference_id":spec["reference_id"],"use_counts":metadata["use_counts"]})
    for seed in (214,215):
        recipe=districts.city_recipe(["dense-urban","business-core","terrace-residential","garden-suburb"],seed)
        result=districts.compose_city(recipe);replay=districts.compose_city(json.loads(json.dumps(recipe)))
        if streets.canonical(result[0])!=streets.canonical(replay[0]):raise AssertionError("City replay diverged")
        stem=districts.save(result,output)
        entries.append({"stem":stem,"name":"Connected mixed-district city","kind":"mixed-city","category":"mixed","seed":seed,
                        "buildings":len(result[1]["buildings"]),"districts":len(result[1]["districts"]),"use_counts":result[1]["use_counts"]})
    atlas.write_json(root/"examples.json",{"generator_version":districts.VERSION,"examples":entries})
    build_index(root,entries)


def build_index(root,entries):
    output=root/"maps"
    cards=[]
    for entry in entries:
        stem=entry["stem"];svg=(output/(stem+".svg")).read_text()
        counts=', '.join(f'{n} {use}' for use,n in sorted(entry["use_counts"].items()))
        cards.append(f'<article data-category="{entry["category"]}"><h2>{html.escape(entry["name"])} · seed {entry["seed"]}</h2><p>{entry["buildings"]} buildings · {html.escape(counts)}</p>{svg}<p><a href="maps/{stem}.obmap2">Game map</a> · <a href="maps/{stem}.recipe.json">Complete recipe</a> · <a href="maps/{stem}.generation.json">Parcels and districts</a></p></article>')
    (root/"index.html").write_text('''<!doctype html><meta charset="utf-8"><title>OutbreakGrid procedural district library</title><style>body{background:#101920;color:#e2eaf0;font:16px system-ui;max-width:1500px;margin:24px auto;padding:0 20px}p{line-height:1.5}a{color:#8ac8ff}.cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(380px,1fr));gap:20px}article{background:#1c2a35;padding:18px;border-radius:10px}h2{font-size:20px}svg{width:100%;height:350px}button{margin:5px;padding:10px}</style><h1>Procedural districts · dense urban, suburbs, residential and business</h1><p>Nine district types, each at seeds 214 and 215, plus two connected mixed-city examples. All use cardinal streets and a 5-metre cell scale. Same complete recipe and bundled reference snapshots produce identical maps.</p><p>New OpenStreetMap calibration samples: Midtown Manhattan, Chicago Loop, Philadelphia rowhouses, Irvine/Woodbridge and Naperville. Urban apartments use the existing published Prague/Bubeneč sample; business campuses are explicitly designed. These generated maps are fictional adaptations, not exact geographic replicas.</p><p>Interior colours: purple apartments; light blue terrace homes; green detached homes; gold retail; blue offices; pink hotels. Colours, building uses and storeys are placement hints in metadata; the game currently loads unpopulated 2D terrain. No NPC jobs, inventories, upper-floor simulation or vehicle lanes are added.</p><p>Mixed cities keep district identities and connect neighbouring districts with physical road portals. Every generated map is checked for connected roads, reachable entrances and room/floor budgets.</p><nav>'''+''.join(f'<button onclick="filterMaps(\'{kind}\')">{name}</button>' for kind,name in [('all','All'),('urban','Dense urban'),('residential','Residential'),('suburban','Suburbs'),('business','Business'),('mixed','Mixed cities')])+'''</nav><div class="cards">'''+''.join(cards)+'''</div><script>function filterMaps(category){document.querySelectorAll('article').forEach(card=>card.hidden=category!=='all'&&card.dataset.category!==category)}</script><p>Read <a href="README.md">README</a> for use and <a href="../NOTICE.md">NOTICE</a> for source terms.</p>''')


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--index-only",action="store_true")
    args=parser.parse_args()
    if args.index_only:
        root=atlas.ROOT/"CityReferences/districts"
        build_index(root,json.loads((root/"examples.json").read_text())["examples"])
    else:main()
