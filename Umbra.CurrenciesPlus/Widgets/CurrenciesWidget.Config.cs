/* Umbra | (c) 2024 by Una              ____ ___        ___.
 * Licensed under the terms of AGPL-3  |    |   \ _____ \_ |__ _______ _____
 *                                     |    |   //     \ | __ \\_  __ \\__  \
 * https://github.com/una-xiv/umbra    |    |  /|  Y Y  \| \_\ \|  | \/ / __ \_
 *                                     |______//__|_|  /____  /|__|   (____  /
 *     Umbra is free software: you can redistribute  \/     \/             \/
 *     it and/or modify it under the terms of the GNU Affero General Public
 *     License as published by the Free Software Foundation, either version 3
 *     of the License, or (at your option) any later version.
 *
 *     Umbra UI is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU Affero General Public License for more details.
 */

using System.Collections.Generic;
using Umbra.Common;

namespace Umbra.Widgets;

internal partial class CurrenciesWidget
{
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        Precache();
        Dictionary<string, string> trackedSelectOptions = new() { { "", "None" } };

        foreach (Currency currency in Currencies.Values)
            trackedSelectOptions.Add(currency.Type.ToString(), currency.Name);

        return [
            new SelectWidgetConfigVariable(
                "TrackedCurrency",
                I18N.Translate("Widget.Currencies.Config.TrackedCurrency.Name"),
                I18N.Translate("Widget.Currencies.Config.TrackedCurrency.Description"),
                "",
                trackedSelectOptions,
                true
            ),
            new StringWidgetConfigVariable(
                "CurrencySeparator",
                I18N.Translate("Widget.Currencies.Config.CurrencySeparator.Name"),
                I18N.Translate("Widget.Currencies.Config.CurrencySeparator.Description"),
                ".",
                1
            ),
            new StringWidgetConfigVariable(
                "CustomCurrencyIds",
                I18N.Translate("Widget.Currencies.Config.CustomCurrencyIds.Name"),
                I18N.Translate("Widget.Currencies.Config.CustomCurrencyIds.Description"),
                ""
            ),
            new BooleanWidgetConfigVariable(
                "EnableMouseInteraction",
                I18N.Translate("Widget.Currencies.Config.EnableMouseInteraction.Name"),
                I18N.Translate("Widget.Currencies.Config.EnableMouseInteraction.Description"),
                true
            ),
            new StringWidgetConfigVariable(
                "CustomLabel",
                I18N.Translate("Widget.Currencies.Config.CustomWidgetLabel.Name"),
                I18N.Translate("Widget.Currencies.Config.CustomWidgetLabel.Description"),
                "",
                32
            ) { Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance") },
            new BooleanWidgetConfigVariable(
                "ShowName",
                I18N.Translate("Widget.Currencies.Config.ShowName.Name"),
                I18N.Translate("Widget.Currencies.Config.ShowName.Description"),
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance") },
            new BooleanWidgetConfigVariable(
                "ShowCapOnWidget",
                I18N.Translate("Widget.Currencies.Config.ShowCapOnWidget.Name"),
                I18N.Translate("Widget.Currencies.Config.ShowCapOnWidget.Description"),
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance") },
            ..DefaultToolbarWidgetConfigVariables,
            ..SingleLabelTextOffsetVariables,
            new BooleanWidgetConfigVariable(
                "CurrencyAlertMode",
                "Currency Alert Mode",
                "YOU MUST RESTART UMBRA WHEN YOU CHANGE THIS. This will change the behaviour of the main widget. It will show all the currencies that have passed the set thresholds. Recommended to also change 'Icon Only'.",
                false
            ) { Category = "Threshold Settings" },
            new BooleanWidgetConfigVariable(
                "ApplyToWidgetText",
                "Also Apply Colours to Toolbar Text",
                "Check this to also apply the colours to the widget text in the toolbar. Uncheck to only colourise the text in the popup menu.",
                true
            ) { Category = "Threshold Settings" },
            new StringWidgetConfigVariable(
                "ThresholdColor",
                "Threshold Colour",
                "Choose the color you want the text to be beyond the threshold limits. This must be given in the format 0xaaBBGGRR. Default is 0xFF52ABFF",
                "0xFF52ABFF",
                10
            ) { Category = "Threshold Settings" },
            new StringWidgetConfigVariable(
                "ThresholdMaxColor",
                "Capped Colour",
                "Choose the color you want the text to be when capped. This must be given in the format 0xaaBBGGRR. Default is 0xFF6565FC",
                "0xFF6565FC",
                10
            ) { Category = "Threshold Settings" },
            new BooleanWidgetConfigVariable(
                "ShowWeeklyProgress",
                "Show Limited Tomestone Weekly Progress",
                "This cannot be used at the same time as the option above. This will only show your current progress in the weekly tomes. Once you reach the weekly cap, this will be hidden.",
                true
            ) { Category = "Threshold Settings" },
            new StringWidgetConfigVariable(
                "WeeklyCappedColor",
                "Weekly Tomestone Capped Colour",
                "Choose the color you want the text to be when you have capped the weekly limit. You also need to check . This must be given in the format 0xaaBBGGRR. Default is 0xFF79E4EA",
                "0xFF79E4EA",
                10
            ) { Category = "Threshold Settings" },
            new BooleanWidgetConfigVariable(
                "EnableWeeklyCapColor",
                "Enable the Weekly Cap Colour",
                "Check this to enable the Weekly Tomestome Cap colour, as set above.",
                true
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "GCSealThreshold",
                "Grand Company Seal Threshold",
                "Set a threshold warning for Grand Company seals. The text will highlight orange above this value.",
                75000,
                0,
                90000
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "HuntThreshold",
                "The Hunt Threshold",
                "Set a threshold warning for The Hunt currencies. The text will highlight orange above this value.",
                3000,
                0,
                4000
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "TomeThreshold",
                "Tomestone Threshold",
                "Set a threshold warning for your Tomestones. The text will highlight orange above this value.",
                1500,
                0,
                2000
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "PvPThreshold",
                "PvP Threshold",
                "Set a threshold warning for your PvP currencies. The text will highlight orange above this value.",
                15000,
                0,
                20000
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "CraftGatherThreshold",
                "Crafter / Gather Threshold",
                "Set a threshold warning for your Crafter / Gatherer scrips. The text will highlight orange above this value.",
                3000,
                0,
                4000
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "SkybuilderThreshold",
                "Skybuilder Scrips Threshold",
                "Set a threshold warning for your Skybuilder scrips. The text will highlight orange above this value.",
                15000,
                0,
                20000
            ) { Category = "Threshold Settings" },
            new IntegerWidgetConfigVariable(
                "BicolorThreshold",
                "Bicolor Gems Threshold",
                "Set a threshold warning for your Bicolor Gems. The text will highlight orange above this value.",
                1200,
                0,
                1500
            ) { Category = "Threshold Settings"},
            new BooleanWidgetConfigVariable(
                "DesaturateIcons",
                I18N.Translate("Widget.Currencies.Config.DesaturateIcons.Name"),
                I18N.Translate("Widget.Currencies.Config.DesaturateIcons.Description"),
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new BooleanWidgetConfigVariable(
                "ShowCap",
                I18N.Translate("Widget.Currencies.Config.ShowCap.Name"),
                I18N.Translate("Widget.Currencies.Config.ShowCap.Description"),
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            ..GetEnabledCurrenciesVariables()
        ];
    }

    private static List<IWidgetConfigVariable> GetEnabledCurrenciesVariables()
    {
        List<IWidgetConfigVariable> variables = [];

        foreach (var currency in Currencies.Values) {
            variables.Add(new BooleanWidgetConfigVariable($"EnabledCurrency_{currency.Id}", currency.Name, null, true) {
                Category = I18N.Translate("Widget.Currencies.Config.EnabledCurrencyGroup")
            });
        }

        return variables;
    }
}