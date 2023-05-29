# WoW2Unity
This is just for reference. Consider it outdated, deprecated, unsupported and so on.

For the time being this is just me trying to get this wow.export (https://github.com/Kruithne/wow.export) to Unity importer made by Ryan Zehm to work, ref. https://www.youtube.com/watch?v=1Xo3Bo04wCI

It requires a specific, modified version of wow.export that is available in the description of the above video. Ryan Zehm's version of wow.export is modified to output splat maps suitable for Unity terrain, and there may be other modifications I'm not aware of, but that seems to be the main one at least. And that seems to be the prerequisite for this entire importer to work.

# How to use

Ryan explains how to use the importer in the above video, but I'll just provde a 1-2-3 step guide of how I've been able to use this with some success in my hours of fiddling with this:

1. Clone this repo and open the Unity project. I assume it will work with any Unity 2019.x version, but if you choose to upgrade the project it's at your own risk. Anything here is for that matter. I have no intention of providing any support for any of this :p
2. Download Ryan's modified version of wow.export. You'll find the link in the above Youtube video description. I'm not pasting a direct link here because I don't want to be responsible for that in any way. I used the link for "v004: (current)", which was the latest at the time of writing.
3. Run Ryan's modified wow.export and use it to open your local WoW installation. To get it to work I had to "Scan and repair" my WoW installation in Battle.net first. I never tested the CDN option so I have no idea if that works, and I'm only interested in WoW Classic Era, so I never tested anything else.
4. Configure the "Export Directory" inside wow.export to be the "Exports" folder inside the Unity project.
5. Select the map tile you want to export and make sure to select Ryan's custom "Splat Maps" option as the "Terrain Texture Quality"
6. Click "Export 1 Tiles". At the time of writing I never tested with more than 1 tile.
7. It should run successfully. If it doesn't you're on your own.
8. Click "Export 1 Tiles" again. Ryan talks about an error in his video that happens occasionally, but for me it was 100% reproducible. I always had to click export twice to get all the required files.
9. Go into your Unity project which should now start importing all the assets you've exported.
10. Open the "TestScene" scene, if it's not already open.
11. Select the "TerrainCreator" game object and assign the "Export DATA JSON" file for the "Splatmap Importer" script, if it's not already assigned. The file should be located at: "Assets/Exports/maps/azeroth_ExportData.json".
12. Click "B Parse Export Data", and cross your fingers all goes well. If it does you should have the terrain tile in your scene view, looking "acceptable", with some models placed here and there, in seemingly the correct places. And there should be no errors in the logs. Once in a while I got some missing material file errors etc., but I've done nothing to try and solve those. Things will be nowhere close to perfect, but it's a good starting point at least.

# wow.unity

There is also this much more robust and promising wow.export to Unity project by Breanna Jones: https://github.com/briochie/wow.unity  
But it imports the terrain as tons of individual meshes, at least currently, which is why I wanted a working copy of Ryan's importer as a reference. Based on my short time with both tools I feel like Breanna's importer imports models in a much more robust and usable way, but getting the terrain as a proper Unity terrain is really good. If I'm going to actually use this for something without contributing further to either project then I would use a combination of both. Ryan's importer for the terrain and model placements on the terrain, but Breanna's importer for the actual models. The biggest advantage of Breanna's importer over Ryan's is that it works with the default wow.export, and can use the latest version of it, rather than being restricted to a very old, custom compiled version.

# Why?

My motivation for spending time on any of this is that I want to make a protoype/game hobby project in Unity with gameplay similar to World of Warcraft, but it looking exactly like World of Warcraft is in no way a requirement. It's only if I end up saving time by using existing WoW assets that I will bother with any of this. And as primarily a designer/programmer it's motivating to have high quality ready-made and compatible assets to choose from, so that I only have to focus on the actual game design, which is the part I enjoy. WoW also still look extremely nice to this day. At least the Classic versions.
