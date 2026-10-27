# Data provenance and attribution

The code in `Tools/city_atlas.py` is project tooling. Data files have their upstream terms independently of that code.

## OpenStreetMap-derived samples

The additional Midtown Manhattan, Chicago Loop, Philadelphia residential, Irvine/Woodbridge and Naperville survey windows were downloaded from the official Overpass endpoint (`https://overpass-api.de/api/interpreter`). They and their derived maps, measurements and reference-calibrated generation outputs retain **© OpenStreetMap contributors**, ODbL 1.0 attribution. Each profile records its query bounds, raw-response SHA256 and OSM base timestamp. These are survey windows, not official district boundaries. Vinohrady remains an unsuccessful query and is not included as measured data.

West Oakland and Manhattan: **© OpenStreetMap contributors**, available under the [Open Database License (ODbL) 1.0](https://opendatacommons.org/licenses/odbl/1-0/). See [OpenStreetMap copyright and attribution](https://www.openstreetmap.org/copyright). Their derived GeoJSON, block geometry, measurement databases and game terrain templates retain this notice and the source provenance; redistribution of adapted databases must follow ODbL requirements.

West Oakland upstream extract: <https://github.com/gboeing/osmnx/blob/main/tests/input_data/West-Oakland.osm.bz2>. This is a historical extract, with feature edits through 2016, not a fresh survey.

Manhattan graph: <https://github.com/pysal/momepy/blob/main/momepy/datasets/nyc_graph.graphml>. Its metadata records OSMnx 1.2.2 and creation on 2022-09-10. The graph does not provide building or land-use mapping.

OSMnx citation: Boeing, G. (2025), *Modeling and Analyzing Urban Networks and Amenities with OSMnx*, Geographical Analysis 57(4), 567–577. <https://doi.org/10.1111/gean.70009>.

## Momepy sample data

Bubeneč streets/buildings are adapted from <https://github.com/pysal/momepy/blob/main/momepy/datasets/bubenec.gpkg>. Only the published streets/buildings layers are used; synthetic tessellation is not treated as district boundaries. The upstream repository does not specify the geometry lineage in this file, so the atlas does not label it as OpenStreetMap. Momepy's repository BSD notice follows and also covers the Manhattan graph's distribution through that project.

Copyright (c) 2018-2021, Martin Fleischmann and PySAL Developers.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.
* Neither the name of the copyright holder nor the names of its contributors
  may be used to endorse or promote products derived from this software without
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
