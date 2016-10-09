VRArcTeleporter requires SteamVR so make sure it is imported into the project before importing
this package or there will be missing scripts in the prefab.

VR Arc Teleporter comes with everything you need to quickly get a locomotion system in your VR game.
Attach the ArcTeleporter script to the SteamVR_TrackedController object as is shown in the VRPlayerExample
prefab or in the example scene.

The ArcTeleporter script can be overidden to give custom control over the controls and teleportation rules
or can be used standalone with the in built rule system (which I will expand upon soon).

Thanks for downloading.

Whats new!
0.5
-Fixed mistake with ExamplePlayer prefab in example scene

0.4
-Moved Editor script to an editor folder so the project can build.
-Added slope limit to land on flat for some leniency.
-Added tags list for limiting what can be teleported to based on tag.
-Added boolean to disable the premade controls.
-ArcTeleporter is now completely scaleable like the rest of the steamVR camera rig.

0.3
-Added layers. Make a list of layers you either only want to hit or that you want to ignore by toggle the Ignore Raycast Layers boolean

0.2
-Fixed issue where part of the line renderer would not disable when nothing was hit

0.1
-Initial release