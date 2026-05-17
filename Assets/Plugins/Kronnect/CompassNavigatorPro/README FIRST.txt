*****************************************
*         COMPASS NAVIGATOR PRO 3       *
*    (C) Copyright 2024-2026 Kronnect   * 
*              README FILE              *
*****************************************


How to use this asset
---------------------

Thanks for purchasing Compass Navigator Pro!

Using Compass Navigator Pro is very easy! Please take a moment to read the Quick Start Guide located in the Documentation folder.



Help & Support Forum
--------------------

Check the Documentation folder for detailed instructions.
Have any question or issue?
* Support-Web: https://kronnect.com/docs/compass/
* Support-Discord: https://discord.gg/EH2GMaM
* Email: contact@kronnect.com
* Twitter: @Kronnect



Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Compass Navigator Pro be eventually available on the Asset Store.



Other Cool Assets!
------------------

Check our other assets on the Asset Store publisher page:
https://assetstore.unity.com/publishers/15018



Version history
---------------

Version 5.1 B2
- Added new events: OnCompassBarIconCreated, OnIndicatorCreated, OnMiniMapIconCreated
- Key methods are now virtual for easier customization through subclassing
- Custom editors now support inherited classes
- New documentation: https://kronnect.com/docs/compass/code-customization

Version 5.1 B1
- POI inspector now warns when indicator features are enabled per-POI but disabled globally on the Compass component

Version 5.0
- Added support for multiple compass instances
- Added support for split screen mode (including demo scene)

Version 4.0
- Minimum Unity version required is now Unity 2022.3.24
- Internal fixes and minor optimizations

Version 3.9.3
- POI: added on-screen indicator prefab override per POI

Version 3.9.2
- Added seamless support for legacy and new input system
- [Fix] Demo scene URP rendering fixes

Version 3.9.1
- [Fix] Fixed building issue related to the CompassProPOI gizmo icon

Version 3.9
- Minimap: added safety check in case the follow moves outside the captured area to force a new snapshot
- Improved support for camera tilt and different mini-map snapshot frequency options

Version 3.8
- Added support for orthographic camera to offscreen indicators
- Minimap displacement is now smoother when in continuous mode using lower resolutions
- Snapshot Frequency can now be configured when mini-map camera projection is set to perspective

Version 3.7
- API: added POISetVisited method to CompassNavigatorPro component
- API: added SetVisited method to CompassProPOI component

Version 3.6
- Added "Compass Bar Orientation" property: choose between camera (default) or follow direction
- Added "Scale Speed" property to screen indicators

Version 3.5.1
- Player icon orientation now always match the follow rotation
- [Fix] Fixed title vertical position issue

Version 3.5
- Added On-Screen Indicators Far Distance fade

Version 3.4
- Added "Take World Snapshot" option to Compass Inspector (can be found in mini-map / maximized mode section)
- Added "Show Distance" option to off-screen indicators (enable it globally in compass inspector and disable per POI if needed)
- [Fix] Fixed aspect ratio positioning issue in radar mode

Version 3.3
- Added "Text Height" setting for the POI distance text shown in the compass bar
- [Fix] Fixed an POI position issue in radar mode

Version 3.2
- POI inspector: added multiple POI editing capability

Version 3.1
- Scan effect optimization: POI detection no longer relies on physics
- URP: added warning in the Compass inspector to change SSAO source from Depth Normals to Depth if used.
- Fixes

Version 3.0
- New feature: "Area of Interest". Improves performance when using hundreds or thousands of POIs.
- New feature: added "Scan" effect with customizable options in inspector and new API
- API: added Scan() method to perform a visual scan effect
- POI: added "Show Distance Text" option
- Better handling of POI id in prefabs
- CHANGE! All events are now UnityEvents and can be wired in the Compass inspector




