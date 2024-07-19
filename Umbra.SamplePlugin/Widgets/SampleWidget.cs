using System.Collections.Generic;
using Umbra.Widgets;

namespace Umbra.SamplePlugin.Widgets;

[ToolbarWidget(
    // A unique ID for this type of widget. Don't change this once it's set,
    // because it is used to associate saved settings with this widget type.
    "SampleWidget",

    // The display name of this widget.
    "A Sample Widget",

    // A brief description of this widget that the user can see in the
    // "Add Widget" window.
    "This is a sample widget from the Umbra.SamplePlugin repository."
)]
public class SampleWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    /// <summary>
    /// Defines the popup of this widget. Setting a value will make the widget
    /// interactive and will render the popup node when the widget is clicked.
    /// </summary>
    public override WidgetPopup? Popup { get; } = null;

    /// <summary>
    /// Returns a list of configuration variables that can be set by the user
    /// for instances of this widget.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            new BooleanWidgetConfigVariable(
                "Decorate",
                "Decorate the widget",
                "Whether to decorate the widget with a background and border.",
                true
            )
        ];
    }

    /// <summary>
    /// This method is invoked once an instance of this widget has been added
    /// to the toolbar. You can use this method to pull in any data or perform
    /// one-time operations that are required for the widget to function.
    /// </summary>
    protected override void Initialize()
    {
        SetLabel("A sample widget");
        SetLeftIcon(14);
    }

    /// <summary>
    /// This method is invoked on every tick of the game loop. You can use this
    /// method to update the state of the widget based on the current game state
    /// or user configuration.
    /// </summary>
    protected override void OnUpdate()
    {
        // Set the widget to be ghosted if the user has disabled decoration.
        // Note that we do this in the update method rather than the initialize
        // method to allow the user to change the setting at runtime.
        SetGhost(!GetConfigValue<bool>("Decorate"));
    }
}
