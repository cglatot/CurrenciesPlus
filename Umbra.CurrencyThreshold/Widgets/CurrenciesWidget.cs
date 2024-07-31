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
using System.Drawing;
using System.Timers;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina;
using Umbra.Common;

namespace Umbra.Widgets;

[ToolbarWidget("CurrenciesThreshold", "Currencies Threshold", "Same as the Currencies Widget, with threshold settings")]
internal sealed partial class CurrenciesWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    private readonly Timer _updateTimer           = new(1000);
    private          byte  _currentGrandCompanyId = 0;
    private          bool? _useGrayscaleIcon;

    /// <inheritdoc/>
    protected override void Initialize()
    {
        SyncTrackedCurrencyOptions();
        HydratePopupMenu();

        _updateTimer.Elapsed   += (_, _) => UpdateMenuItems();
        _updateTimer.AutoReset =  true;
        _updateTimer.Start();

        Node.OnClick      += _ => UpdateMenuItems();
        Node.OnRightClick += _ => OpenCurrenciesWindow();
    }

    public override string GetInstanceName()
    {
        if (uint.TryParse(GetConfigValue<string>("TrackedCurrency"), out uint customId)) {
            if (CustomCurrencies.TryGetValue(customId, out Currency? customCurrency)) {
                return $"{I18N.Translate("Widget.Currencies.Name")} - {customCurrency.Name}";
            }

            return string.IsNullOrEmpty(GetConfigValue<string>("CustomLabel"))
                ? I18N.Translate("Widget.Currencies.Name")
                : GetConfigValue<string>("CustomLabel");
        }

        return GetConfigValue<string>("TrackedCurrency") != ""
            ? $"{I18N.Translate("Widget.Currencies.Name")} - {Currencies[Enum.Parse<CurrencyType>(GetConfigValue<string>("TrackedCurrency"))].Name}"
            : string.IsNullOrEmpty(GetConfigValue<string>("CustomLabel"))
                ? I18N.Translate("Widget.Currencies.Name")
                : GetConfigValue<string>("CustomLabel");
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        Popup.IsDisabled = !GetConfigValue<bool>("EnableMouseInteraction");

        SetGhost(!GetConfigValue<bool>("Decorate"));
        Popup.UseGrayscaleIcons = GetConfigValue<bool>("DesaturateIcons");

        UpdateCustomIdList();

        Node.QuerySelector("#Label")!.Style.TextOffset = new(0, GetConfigValue<int>("TextYOffset"));

        var trackedCurrencyId = GetConfigValue<string>("TrackedCurrency");
        var useGrayscaleIcon  = GetConfigValue<bool>("DesaturateIcon");

        if (uint.TryParse(GetConfigValue<string>("TrackedCurrency"), out uint customId)) {
            if (CustomCurrencies.TryGetValue(customId, out Currency? customCurrency)) {
                if (GetConfigValue<string>("IconLocation") == "Left") {
                    SetLeftIcon(GetConfigValue<bool>("ShowIcon") ? customCurrency.Icon : null);
                    SetRightIcon(null);
                } else {
                    SetLeftIcon(null);
                    SetRightIcon(GetConfigValue<bool>("ShowIcon") ? customCurrency.Icon : null);
                }

                if (_useGrayscaleIcon != useGrayscaleIcon) {
                    _useGrayscaleIcon = useGrayscaleIcon;

                    foreach (var node in Node.QuerySelectorAll(".icon")) {
                        node.Style.ImageGrayscale = useGrayscaleIcon;
                    }
                }

                string customName = GetConfigValue<bool>("ShowName") ? $" {customCurrency.Name}" : "";
                SetLabel($"{GetCustomAmount(customCurrency.Id)}{customName}");

                return;
            }
        }

        if (!Enum.TryParse(trackedCurrencyId, out CurrencyType currencyType) || currencyType == 0 || !Currencies.TryGetValue(currencyType, out Currency? currency)) {
            string customLabel = GetConfigValue<string>("CustomLabel");
            string label       = I18N.Translate("Widget.Currencies.Name");

            SetLabel(string.IsNullOrEmpty(customLabel) ? label : customLabel);
            SetLeftIcon(null);
            SetRightIcon(null);
            return;
        }

        if (GetConfigValue<string>("IconLocation") == "Left") {
            SetLeftIcon(GetConfigValue<bool>("ShowIcon") ? currency.Icon : null);
            SetRightIcon(null);
        } else {
            SetLeftIcon(null);
            SetRightIcon(GetConfigValue<bool>("ShowIcon") ? currency.Icon : null);
        }

        if (_useGrayscaleIcon != useGrayscaleIcon) {
            _useGrayscaleIcon = useGrayscaleIcon;

            foreach (var node in Node.QuerySelectorAll(".icon")) {
                node.Style.ImageGrayscale = useGrayscaleIcon;
            }
        }

        string name = GetConfigValue<bool>("ShowName") ? $" {currency.Name}" : "";
        SetLabel($"{GetAmount(currency.Type)}{name}");

        Una.Drawing.Color setTextColor = new("Widget.PopupMenuText");
        if (GetConfigValue<bool>("ApplyToWidgetText")) {
            Una.Drawing.Color cappedColor = new(Convert.ToUInt32(GetConfigValue<string>("ThresholdMaxColor"), 16));
            Una.Drawing.Color thresholdColor = new(Convert.ToUInt32(GetConfigValue<string>("ThresholdColor"), 16));
            Una.Drawing.Color weeklyCappedColor = new(Convert.ToUInt32(GetConfigValue<string>("WeeklyCappedColor"), 16));

            bool enableWeeklyColor = GetConfigValue<bool>("EnableWeeklyCapColor");

            if (currency.Type == CurrencyType.Maelstrom || currency.Type == CurrencyType.TwinAdder || currency.Type == CurrencyType.ImmortalFlames) {
                if (GetActualAmount(currency.Type) >= 90000) setTextColor = cappedColor;
                else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("GCSealThreshold")) setTextColor = thresholdColor;
            }
            else if (currency.GroupId == 1) {
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
        }
        Node.QuerySelector("#Label")!.Style.Color = setTextColor;
    }

    /// <inheritdoc/>
    protected override void OnDisposed()
    {
        _updateTimer.Stop();
        _updateTimer.Dispose();
    }

    private void UpdateMenuItems()
    {
        if (!Popup.IsOpen) return;

        byte gcId = Player.GrandCompanyId;

        if (gcId != _currentGrandCompanyId) {
            _currentGrandCompanyId = gcId;
            HydratePopupMenu();
            return;
        }

        foreach (var currency in Currencies.Values) {
            if (currency.Type == CurrencyType.Maelstrom && gcId != 1) continue;
            if (currency.Type == CurrencyType.TwinAdder && gcId != 2) continue;
            if (currency.Type == CurrencyType.ImmortalFlames && gcId != 3) continue;

            Una.Drawing.Color cappedColor = new(Convert.ToUInt32(GetConfigValue<string>("ThresholdMaxColor"), 16));
            Una.Drawing.Color thresholdColor = new(Convert.ToUInt32(GetConfigValue<string>("ThresholdColor"), 16));
            Una.Drawing.Color weeklyCappedColor = new(Convert.ToUInt32(GetConfigValue<string>("WeeklyCappedColor"), 16));
            Una.Drawing.Color setTextColor = new("Widget.PopupMenuText");

            bool enableWeeklyColor = GetConfigValue<bool>("EnableWeeklyCapColor");
            
            if (currency.Type == CurrencyType.Maelstrom || currency.Type == CurrencyType.TwinAdder || currency.Type == CurrencyType.ImmortalFlames) {
                if (GetActualAmount(currency.Type) >= 90000) setTextColor = cappedColor;
                else if (GetActualAmount(currency.Type) >= GetConfigValue<int>("GCSealThreshold")) setTextColor = thresholdColor;
            }
            else if (currency.GroupId == 1) {
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

            Popup.SetButtonAltLabel($"Currency_{currency.Id}", GetAmount(currency.Type));
            Popup.SetNewColor($"Currency_{currency.Id}", setTextColor);
        }
    }

    private unsafe void OpenCurrenciesWindow()
    {
        UIModule* uiModule = UIModule.Instance();
        if (uiModule == null) return;
        uiModule->ExecuteMainCommand(66);
    }
}
