Version 1.5.01


- Additional demo scenes  http://kripto289.com/AssetStore/WaterSystem/DemoScenes_1.5/
- My email is "kripto289@gmail.com"
- Discord channel https://discord.gg/GUUZ9D96Uq (you can get all new changes/fixes/features in the discord channel. The asset store version will only receive major updates)







-----------------------------------  WATER FIRST STEPS ------------------------------------------------------------------------------------------------------------

               1) Right click in hierarchy -> Effects -> Water system
               2) See the description of each setting: just click the help box with the symbol "?" or go over the cursor to any setting to see a text description. 

--------------------------------------------------------------------------------------------------------------------------------------------------------------------








-----------------------------------  DEMO SCENE CORRECT SETTINGS -----------------------------------------------------------------------------------------------
1) Import "cinemachine" (for camera motion) and "post processing"
Window -> Package Manager -> click button bellow "packages" tab -> select "All Packages" or "Packages: Unity registry" -> Cinemachine -> "Install"
----------------------------------------------------------------------------------------------------------------------------------------------------------------





----------------------------------- USING FLOWING EDITOR --------------------------------------------------------------------------------------------------
1) Click the "Flowmap Painter" button
2) Set the "Flowmap area position" and "Area Size" parameters. You must draw flowmap in this area!
3) Press and hold the left mouse button to draw on the flowmap area.
4) Use the "control" (ctrl) button + left mouse to erase mode.
5) Use the mouse wheel to change the brush size.
6) Press the "Save All" button.
7) All changes will be saved in the folder "Assets\KriptoFX\WaterSystem\WaterResources\Resources\SavedData\WaterID", so be careful and don't remove it.
You can see the current waterID under section "water->rendering tab". It's look like a "Water unique ID : Beach Rocky.M8W3ER5V"
--------------------------------------------------------------------------------------------------------------------------------------------------------------



----------------------------------- USING FLUIDS SIMULATION -------------------------------------------------------------------------------------------------
Fluids simulation calculate dynamic flow around static objects only.
1) draw the flow direction on the current flowmap (use flowmap painter)
2) save flowmap
3) press the button "Bake Fluids Obstacles"
-------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------- USING SHORELINE EDITOR --------------------------------------------------------------------------------------------------
1) Click the "Edit mode" button
2) Click the "Add Wave" button OR you can add waves to the mouse cursor position using the "Insert" key button. For removal, select a wave and press the "Delete" button.
3) You can use move/rotate/scale as usual for any other game object. 
4) Save all changes.
5) All changes will be saved in the folder "Assets\KriptoFX\WaterSystem\WaterResources\Resources\SavedData\WaterID", so be careful and don't remove it.
You can see the current waterID under section "water->rendering tab". It's look like a "Water unique ID : Beach Rocky.M8W3ER5V"
-------------------------------------------------------------------------------------------------------------------------------------------------------------



----------------------------------- USING RIVER SPLINE EDITOR -----------------------------------------------------------------------------------------------
1) In this mode, a river mesh is generated using splines (control points).
Press the button "Add River" and left click on your ground and set the starting point of your river
2) Press 
SHIFT + LEFT click to add a new point.
Ctrl + Left click deletes the selected point.
Use "scale tool" (or R button) to change the river width
3) A minimum of 3 points is required to create a river. Place the points approximately at the same distance and avoid strong curvature of the mesh 
(otherwise you will see red intersections gizmo and artifacts)
4) Press "Save Changes"
--------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------- USING ADDITIONAL FEATURES ---------------------------------------------------------------------------------------------------------
1) You can use the "water depth mask" feature (used for example for ignoring water rendering inside a boat). 
Just create a mesh mask and use shader "KriptoFX/Water/KW_WaterHoleMask"
2) For buoyancy, add the script "KW_Buoyancy" to your object with rigibody. 
3) For compatibility with third-party assets (Enviro/Azure/WeatherMaker/Atmospheric height fog/Volumetric fog and mist2/etc) use WaterSystem -> Rendering -> Third-party fog support
----------------------------------------------------------------------------------------------------------------------------------------------------------------



----------------------------------- WATER API --------------------------------------------------------------------------------------------------------------------------
1) To get the water position/normal (for example for bouyancy) use follow code:

var waterSurfaceData = WaterSystem.GetWaterSurfaceData(position); //Used for water physics. It check all instances. 
if (waterSurfaceData.IsActualDataReady) //checking if the surface data is ready. Since I use asynchronous updating, the data may be available with a delay, so the first frame can be null. 
{
    var waterPosition = waterSurfaceData.Position;
    var waterNormal = waterSurfaceData.Normal;
} 
2) if you want to manually synchronize the time for all clients over the network, use follow code:
_waterInstance.UseNetworkTime = true;
_waterInstance.NetworkTime = ...  //your time in seconds

3) WaterInstance.IsWaterRenderingActive = true/false;   //You can manually control the rendering of water (software occlusion culling)
4) WaterInstance.WorldSpaceBounds   //world space bounds of the current quadtree mesh/custom mesh/river 
5) WaterInstance.IsCameraUnderwater() //check if the current rendered camera intersect underwater volume
6) WaterInstance.IsPositionUnderWater(position) or WaterInstance.IsSphereUnderWater(position, radius) //Check if the current world space position/sphere is under water. 
For example, you can detect if your character enters the water to like triggering a swimming state.
7) Example of adding shoreline waves in realtime

var data =  WaterInstance.Settings.ShorelineWaves = ScriptableObject.CreateInstance<ShorelineWavesScriptableData>();
for(int i = 0; I < 100; i++)
{
  var wave = new ShorelineWave(typeID: 0, position, rotation, scale, timeOffset, flip);
  wave.UpdateMatrix();
  data.Waves.Add(wave);
}
WaterInstance.Settings.ShorelineWaves = data;
----------------------------------------------------------------------------------------------------------------------------------------------------------------






Other resources: 
Galleon https://sketchfab.com/Harry_L
Shark https://sketchfab.com/Ravenloop
Pool https://sketchfab.com/aurelien_martel









/////////////////////////////////// Release notes ///////////////////////////////////////////////////////
Release Notes 1.5.01

Before updating, remove the old version.

If you want to save water profiles and other settings, then do not delete the "Assets folder\KriptoFX\WaterSystem\WaterResources\Resources\SavedData"
Release Notes 1.5.01

New:

-Implemented large-scale optimizations for multiple water instances. Currently, all effects (global waves simulation, SSR reflection, buoyancy, dynamic waves, volumetric lightings, caustic, shoreline, ocean foam, etc.) are calculated once instead of per water instance. Exceptions include planar reflection and fluids simulation.

-added new fft waves generation (with lod system for the storm waves)
-added fft waves scaling mode
-added local/global wind settings
-added ocean foam rendering
-added dynamic waves interaction from meshes
-added volumetric lighting caustic parameters
-added volumetric lighting rendering using temporal reprojection instead of blur(bilateral/gaussian blur was removed)
-added volumetric lighting intensity settings for dir/additional lights
-added caustic rendering for additional lights (point/spot)
-added infinite caustic rendering without cascades
-added underwater half line effect
-added underwater physical approximated refraction and internal reflection (using SSR)
-added transform scale (mesh size property removed)
-added stencil mask (for example, you can look out the porthole, see the demo scene "UnderwaterShip")
-added wide-angle camera rendering for "unity recorder" and 360 degrees cubemaps (water -> rendering tab -> wide-angle toogle)
-Added new underwater bubbles/dust effects
-Added new water decals (lit/unlit) and effect (duckweed/blood)
-Added new particles decals (foam trails, etc)
-Added aura2 support
-Added underwater environment lighting attenuation depending on the depth and spot/point lights.

-Added the new water/underwater rendering using transparent queue (Water-> Rendering tab -> Transparent sorting priority)
(before it rendered only before or after transparent). Right now all third-party fog rendering (like enviro3) can work correctly and you can render transparent effects before/after water/underwater pass.

-added experimental "curved world plugin" support
(you need to open the file "KWS_PlatformSpecificHelpers.cginc" and uncomment this line "#define CURVED_WORLDS".
By default used "#define CURVEDWORLD_BEND_TYPE_LITTLEPLANET_Y"  for other modes you need to replace this line)


Improvements:

-improved waves aliasing/filtering
-improved refraction artefacts (outside the viewport)
-optimized quadtree rendering for non-squared meshes
-optimized quadtree mesh rendering for multiple cameras
-changed the logic of saving profiles for multiple instances


API changes:
-Optimized buoyancy API for mutliple points (WaterSustem.GetWaterSurfaceDataArray)
-Added new feature "WaterSystem.UseNetworkBuoyancy", it allow you to render buoyancy height data without cameras
-Added the new API for third-party shaders (hlsl/shadergraph)
Custom shaders using:
#pragma multicompile  KWS_USE_VOLUMETRIC_LIGHT
#include "Assets/KriptoFX/WaterSystem/WaterResources/Shaders/Resources/Common/KWS_SharedAPI.cginc"
Shadergraph using
Create node -> subgraph -> KWS_API_exampleNode


Fixes:
-fixed mac M1 compiler errors
-fixed xbox compiller errors
-fixed ps5 compiller errors
-fixed incorrect buoyancy height
-fixed rain time overflow
-fixed finite mesh quadtree patch holes
-fixed some warnings relative to textures (depth/color etc)
-fixed vr rendering issues
-fixed caustic dispersion
-fixed multiple camera rendering
-fixed spline editor bugs
-fixed incorrect river buoyancy
-fixed incorrect custom importer and incorrect GUID of flowmaps
-fixed incorrect spline bounding box
-fixed dynamic waves for multiple cameras
-fixed HDRI skybox encoding
-fixed quadtree mesh rendering with multiple cameras and different settings (far/fov, etc)

-Numerous other fixes and improvements





Before updating, remove the old version and the folder "streaming assets/WaterSystemData"
Sometimes unity tries to use the cache of old shaders instead of new ones. if you will have some shaders errors, try to remove the "Library/ShaderCache" folder from the project and restart unity.

1.4.04
-fixed finite mesh performance issue
-fixed some issues with native/third party fog
-fixed ps5 support
-fixed buoyancy issue in build
-fixed caustic rendering with multiple water bodies
-fixed some macos issues

1.4.03

-fixed vulcan build issue
-fixed multipass VR rendering
-fixed underwater far clip issue
-fixed mesh flickering
-fixed underwater horizont flickering



1.4.0b2 (for unity 2021.3x+ and HDRP 12+)

- Added instanced quadtree mesh rendering for Infinite/Finite mesh. (faster than tessellation with the same mesh detailing))
- Added automatic mesh quality relative to wind speed
- Added VR support
- Added multiple reflection stack (cubemap + planar layers + SSR)
For example, you can use planar layers for important objects (characters, mountains, trees, etc) and SSR reflection for other scene. This helps to avoid SSR artifacts.
- Improved SSR reflection (added temporal reprojection for hole filling and border stretching)
- Added new shoreline rendering for water surface waves + foam particles
Several times faster and takes up ~5 times less VRAM.
- Added infinite shoreline rendering with stacking feature (without artifacts of waves intersection).
- Added new shoreline API, you can change waves at runtime. (WaterInstance.Settings.ShorelineWavesScriptableData, see the readme for details)
- Added wireframe mode for rivers
- Added "enable mesh rendering" feature. It allows you to render only foam particles/caustic without water mesh.
- Added profiles for common water settings and water features (you can separately save shoreline/flowmap/fluids/spline profiles)
- Added infinite ocean mesh stretching
- Added beta version of the foam rendering (using depth buffer)
- Added simple buoyancy reaction for rivers
- Improved/changed editor GUI
- Added underwater queue (before/after transparent). Right now you can use transparent shaders, but without underwater fog.
- CG Allocation optimisations
- Optimized keyword variants (x10 times)
- Added new API
-WaterInstance.ForceUpdateWaterSettings //You must invoke this method every time you change any water parameter.
-WaterInstance.IsWaterRenderingActive   //You can manually control the rendering of water (software occlusion culling)
-WaterInstance.WorldSpaceBounds   //world space bounds of the current quadtree mesh/custom mesh/river (size relative to wind)
-WaterInstance.IsCameraUnderwater //must be obvious 🙂
-WaterInstance.IsPositionUnderWater/WaterInstance.IsSphereUnderWater  //Check if the current world space position/sphere is under water. For example, you can detect if your character enters the water to like triggering a swimming state.
-WaterSystem.GetWaterSurfaceData // Get world space water position/normal at point. Used for water physics. It check all instances, instead of the old method.
-WaterInstance.Settings.ShorelineWavesScriptableData)
- 100500 minor fixes and optimizations

1.3.0
-added river splines
-added performance profiles
-added third-party fog support for Expanse, Time Of Day, Enviro, Atmospheric height fog
-added BC7/BC6 texture compression for flowmap/fluids data
-added actual video helpers from the cloud server
-added waves limitation at the boundaries for custom meshes
-added network time

-optimized multiple cameras rendering (will be rendered only visible water layer)
-optimized screen space reflection x2 times
-optimized water mesh vertices count (~x3 less vertices)
-optimized tesselation quality
-optimized foam rendering

-improved water GUI
-improved anisotropic filtering for cubemap reflection
-improved fluids simulation quality (and fixed some artifacts when moving)
-improved subsurface scattering
-improved mesh detailing for finite/infinite meshes
-improved underwater quality
-improved SSR reflection quality on MacOS

-fixed multiple cameras (overlay)
-fixed rtHandles with incorrect screen size for underwater/caustic/lighting
-fixed dynamic waves bug with incorrect positions
-fixed dynamic waves flickering and incorrect normals
-fixed caustic overlight (flickering)
-fixed shoreline editor bugs
-fixed volumetric bilateral blur with far distance
-fixed incorrect fluids/caustic baking on unity 2019.4+
-fixed pink water on macos
-fixed incorrect tesselation on macos
-fixed prefab view
-fixed black borders with volumetric lighting and finite meshes
-fixed bounding box calculation relative to wind speed (incorrect underwater/water culling)
-fixed incorrect wave displacement with finite/custom meshes and rotation
-fixed visual artifacts of side surfaces (finite meshes)
-fixed planar reflection aspect ratio
-fixed full-screen mode + inspector errors
-fixed some artifacts in refraction mode by IOR
-fixed fluids prebaked simulation
-fixed black skybox reflection
-fixed incorrect shoreline baking rendering
-river editor improvements
-fixed black artifacts with of finite box
-fixed transparent big waves
-fixed memory leak/errors in hdrp 2022.1x
-fixed mesh normals (right now custom meshes/rivers should have correct normals with any rotation)
-fixed IOR refraction of the side faces of finite box (for example for aquariums)
-fixed errors with incorrect shader paths on macos
-fixed incorrect sun reflection in editor camera
-fixed broken custom mesh rendering in play mode.
-fixed incorrect cubemap reflection exposure with custom volume layers

-removed offscreen rendering (according to tests, this did not give a performance boost)
-removed underwater texture resolution and used fullscreen size instead (according to tests, this did not give a performance boost)
-removed "cubemap clear flag" and forced default cubemap behaviour to standard(non planar) reflection probe. You can select planar/non planar reflection behaviour.

1.2.0
-Refactored ~90% shaders/scripts.
-Optimized CG.allocation
-Optimized caustic rendering
-Optimized volumetric lighting
-Optimized buoyancy physics (and added new public API)
-Optimized foam rendering
-Optimized water shader
-Optimisations for 4k resolution rendering

-Added occlusion culling for water mesh/underwater pass
-Added assembly definition and namespace "KWS"
-Added SRP RTHandleSystem (multiple camera rendering with one RenderTexture)

-New feature: Custom meshes (partial support with some limitations)
-New feature: Multiple meshes rendering (partial support with some limitations)
-New feature: Multiple camera rendering
-New feature: refraction dispersion control
-New feature: bilateral volumetric blur
-New feature: sun reflection cloudiness/strength
-New feature: anisotropic reflection
-New feature: SSPR for metal (with some limitations)

1.0.0.a
Fixed errors on HDRP12+
Fixed editor camera FOV

1.0.0 First release