using System;
using System.Collections.Generic;
using Umbra.Common;
using Umbra.Markers;
using Umbra.Markers.System;

namespace Umbra.SamplePlugin.Markers;

// World marker factories are used to create and manage markers in the game
// world. These classes must be marked with the [Service] attribute and must
// inherit from the WorldMarkerFactory class.
[Service]
public class SampleMarkerFactory : WorldMarkerFactory
{
    /// <summary>
    /// Defines the unique identifier of this marker type. This needs to be
    /// unique across Umbra and other plugins, so make sure to use a name
    /// that is specific to your plugin.
    /// </summary>
    public override string Id => "SamplePluginMarker";

    /// <summary>
    /// A display name for this marker type. This is the name that will be
    /// visible to the user in the marker list and settings window.
    /// </summary>
    public override string Name => "Sample Plugin Marker";

    /// <summary>
    /// A short description of this marker type. This is the description that
    /// users will see in the marker settings window.
    /// </summary>
    public override string Description => "A sample marker from the Umbra.SamplePlugin repository.";

    /// <summary>
    /// Returns a list of configuration variables that can be set by the user
    /// for this marker type. You can include the default state and fade config
    /// variables by using the `DefaultStateConfigVariables` and
    /// `DefaultFadeConfigVariables` properties.
    /// </summary>
    /// <returns></returns>
    public override List<IMarkerConfigVariable> GetConfigVariables()
    {
        return [
            ..DefaultStateConfigVariables,
            ..DefaultFadeConfigVariables,
        ];
    }

    /// <summary>
    /// Invoke this method every second to update the marker state.
    /// </summary>
    [OnTick(interval: 1000)]
    private void OnUpdate()
    {
        if (!GetConfigValue<bool>("Enabled")) {
            // The marker type is not enabled, so we should remove all markers
            // that we've previously created.
            RemoveAllMarkers();
            return;
        }

        var fadeDist = GetConfigValue<int>("FadeDistance");

        // Create a marker!
        SetMarker(
            new WorldMarker() {
                // A unique key for this specific marker. This is used to identify
                // this specific instance. Make sure to use a unique key for each
                // marker you create.
                Key   = "SamplePluginMarker",

                // In which map should this marker be shown?
                MapId = 4, // 4 = Black Shroud.

                // The world position of the marker.
                Position = new(15, 0, 36), // Bentbranch Meadows Aetheryte.

                // The label that will be shown on the marker.
                Label = "Sample Marker",

                // The sub-label that will be shown on the marker.
                SubLabel = "This is a sample marker",

                // The icon ID of the marker. This is the icon that will be shown
                // on the marker and direction indicator.
                IconId = 14,

                // FadeDistance is a range that defines the two distances at which
                // the marker starts fading and is completely invisible, respectively.
                FadeDistance = new(fadeDist, fadeDist + Math.Max(1, GetConfigValue<int>("FadeAttenuation"))),

                // Whether a direction marker should be shown on the screen if
                // the marker is on the current map but is off-screen.
                ShowOnCompass = GetConfigValue<bool>("ShowOnCompass"),
            }
        );
    }
}
