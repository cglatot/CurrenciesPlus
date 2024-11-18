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

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using Umbra.Common;

namespace Umbra.Widgets;

internal partial class CurrenciesWidget
{
    public override CurrencyMenuPopup Popup { get; } = new() { UseGrayscaleIcons = false };

    private void HydratePopupMenu()
    {
        Framework.DalamudFramework.RunOnTick(
            () => {
                Popup?.Clear();
                HydratePopupMenuInternal();
                HydrateCustomCurrencies();
            },
            delayTicks: 1
        );
    }

    private void HydratePopupMenuInternal()
    {
        IEnumerable<Currency> group0Currencies = Currencies.Values.Where(currency => currency.GroupId == 0);

        byte gcId = Player.GrandCompanyId;

        Una.Drawing.Color cappedColor = new(Convert.ToUInt32(GetConfigValue<string>("ThresholdMaxColor"), 16));
        Una.Drawing.Color thresholdColor = new(Convert.ToUInt32(GetConfigValue<string>("ThresholdColor"), 16));
        Una.Drawing.Color weeklyCappedColor = new(Convert.ToUInt32(GetConfigValue<string>("WeeklyCappedColor"), 16));

        bool enableWeeklyColor = GetConfigValue<bool>("EnableWeeklyCapColor");

        foreach (Currency currency in group0Currencies) {
            if (currency.Type == CurrencyType.Maelstrom && gcId != 1) continue;
            if (currency.Type == CurrencyType.TwinAdder && gcId != 2) continue;
            if (currency.Type == CurrencyType.ImmortalFlames && gcId != 3) continue;

            Una.Drawing.Color setTextColor = new("Widget.PopupMenuText");
            if (currency.Type == CurrencyType.Maelstrom || currency.Type == CurrencyType.TwinAdder || currency.Type == CurrencyType.ImmortalFlames) {
                if (GetActualAmount(currency.Type) >= 90000) setTextColor = cappedColor;
                else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("GCSealThreshold")) setTextColor = thresholdColor;
            }


            Popup.AddButton(
                $"Currency_{currency.Id}",
                currency.Name,
                iconId: currency.Icon,
                altText: GetAmount(currency.Type, GetConfigValue<bool>("ShowCap")),
                textColor: setTextColor,
                onClick: () => {
                    var type = currency.Type.ToString();

                    SetConfigValue("TrackedCurrency", GetConfigValue<string>("TrackedCurrency") != type
                        ? currency.Type.ToString()
                        : ""
                    );
                }
            );

            Popup.SetButtonVisibility($"Currency_{currency.Id}", GetConfigValue<bool>($"EnabledCurrency_{currency.Id}"));
        }

        // Add groups.
        Popup.AddGroup("Group_1", I18N.Translate("Widget.Currencies.Group.TheHunt"));
        Popup.AddGroup("Group_2", I18N.Translate("Widget.Currencies.Group.Tomestones"));
        Popup.AddGroup("Group_3", I18N.Translate("Widget.Currencies.Group.PvP"));
        Popup.AddGroup("Group_4", I18N.Translate("Widget.Currencies.Group.CraftingGathering"));
        Popup.AddGroup("Group_5", I18N.Translate("Widget.Currencies.Group.Miscellaneous"));

        foreach (Currency currency in Currencies.Values) {
            if (currency.GroupId == 0) continue;

            Una.Drawing.Color setTextColor = new("Widget.PopupMenuText");
            if (currency.GroupId == 1) {
                if (GetActualAmount(currency.Type) >= 4000) setTextColor = cappedColor;
                else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("HuntThreshold")) setTextColor = thresholdColor;
            }
            else if (currency.GroupId == 2) {
                if (GetActualAmount(currency.Type) >= GetConfigValue<int>("TomeThreshold")) setTextColor = thresholdColor;
                if (currency.Type == CurrencyType.LimitedTomestone && enableWeeklyColor) {
                    int weeklyLimit = GetLimitedCap();
                    int weeklyCount = GetLimitedCurrent();
                    if (weeklyCount == weeklyLimit) {
                        setTextColor = weeklyCappedColor;
                    }
                }
                if (GetActualAmount(currency.Type) >= 2000) setTextColor = cappedColor;
            }
            else if (currency.GroupId == 3) {
                if (GetActualAmount(currency.Type) >= 20000) setTextColor = cappedColor;
                else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("PvPThreshold")) setTextColor = thresholdColor;
            }
            else if (currency.GroupId == 4) {
                if (currency.Type == CurrencyType.SkyBuildersScrips) {
                    if (GetActualAmount(currency.Type) >= 20000) setTextColor = cappedColor;
                    else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("SkybuilderThreshold")) setTextColor = thresholdColor;
                }
                else {
                    if (GetActualAmount(currency.Type) >= 4000) setTextColor = cappedColor;
                    else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("CraftGatherThreshold")) setTextColor = thresholdColor;
                }
            }
            else if (currency.GroupId == 5) {
                if (GetActualAmount(currency.Type) >= 1500) setTextColor = cappedColor;
                else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("BicolorThreshold")) setTextColor = thresholdColor;
            }

            Popup.AddButton(
                $"Currency_{currency.Id}",
                currency.Name,
                iconId: currency.Icon,
                altText: GetAmount(currency.Type, GetConfigValue<bool>("ShowCap")),
                textColor: setTextColor,
                groupId: $"Group_{currency.GroupId}",
                onClick: () => {
                    var type = currency.Type.ToString();

                    SetConfigValue("TrackedCurrency", GetConfigValue<string>("TrackedCurrency") != type
                        ? currency.Type.ToString()
                        : ""
                    );
                }
            );

            Popup.SetButtonVisibility($"Currency_{currency.Id}", GetConfigValue<bool>($"EnabledCurrency_{currency.Id}"));
        }
    }

    private void HydrateCustomCurrencies()
    {
        foreach (Currency currency in CustomCurrencies.Values) {
            if (Popup.HasButton($"CustomCurrency_{currency.Id}")) continue;

            Popup.AddButton(
                $"CustomCurrency_{currency.Id}",
                iconId: currency.Icon,
                label: $"{currency.Name}",
                altText: GetCustomAmount(currency.Id, GetConfigValue<bool>("ShowCap")),
                groupId: "Group_5",
                onClick: () => {
                    var type  = currency.Id.ToString();
                    string newId = GetConfigValue<string>("TrackedCurrency") != type ? type : "";

                    SetConfigValue("TrackedCurrency", newId);
                }
            );
        }
    }
}