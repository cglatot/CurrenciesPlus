using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Umbra.Common;
using Umbra.Widgets;

namespace Umbra.SamplePlugin.Widgets;

/// <summary>
/// A toolbar widget that contains a little menu.
///
/// Refer to the <see cref="SampleWidget"/> class for a simpler example
/// that contains more detailed explanations.
/// </summary>
[ToolbarWidget(
    "SampleWidgetWithAMenu",
    "A Sample Widget with a menu",
    "This is a sample widget from the Umbra.SamplePlugin repository that has a menu."
)]
public class SampleWidgetWithMenu(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    /// <inheritdoc/>
    public override MenuPopup Popup { get; } = new();

    // Services can be fetched from the framework like so.
    // Since toolbar widgets are always instantiated after the framework is
    // initialized, it's safe to fetch services in the constructor.
    private IToastGui ToastGui { get; set; } = Framework.Service<IToastGui>();

    /// <inheritdoc/>
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        // If you don't need any configuration variables, you can return an empty list.
        // This will result the "cog" button to be in a disabled state.
        return [];
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        // Let's add some buttons.
        Popup.AddButton(
            "MyButton",
            label: "A button",
            onClick: () => OnItemClicked("Button 1"),
            iconId: 14u,
            altText: "Alt-Text here"
        );

        // You can also add groups...
        Popup.AddGroup("Group1", "A button group");
        Popup.AddButton("Btn1", "My first button", groupId: "Group1", onClick: () => OnItemClicked("Button 1"), iconId: 14u);
        Popup.AddButton("Btn2", "My second button", groupId: "Group1", onClick: () => OnItemClicked("Button 2"));

        Popup.AddGroup("Group2", "Another button group");
        Popup.AddButton("Btn3", "My third button", groupId: "Group2", onClick: () => OnItemClicked("Button 3"));
        Popup.AddButton("Btn4", "My fourth button", groupId: "Group2", onClick: () => OnItemClicked("Button 4"));
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        // Labels can be updated during runtime as well.
        SetLabel("A sample menu");

        if (!Popup.IsOpen) {
            return;
        }

        // Button states can be updated during runtime by referencing their ids.
        Popup.SetButtonDisabled("Btn3", true);
        Popup.SetButtonIcon("Btn1", 15u);
    }

    private void OnItemClicked(string id)
    {
        // Pull a service from the framework.
        ToastGui.ShowNormal($"You clicked button [{id}]");
    }
}
