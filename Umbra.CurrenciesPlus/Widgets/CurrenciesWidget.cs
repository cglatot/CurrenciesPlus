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
using System.Timers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Umbra.Common;
using Una.Drawing;

namespace Umbra.Widgets;

[ToolbarWidget("Currencies+", "Currencies+", "Same as the Currencies Widget, with some extra features")]
internal sealed partial class CurrenciesWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    private readonly Timer _updateTimer           = new(1000);
    private          byte  _currentGrandCompanyId = 0;

    private readonly Dictionary<uint, Node> _alerts = [];

    /// <inheritdoc/>
    protected override void Initialize()
    {
        SyncTrackedCurrencyOptions();
        HydratePopupMenu();
        UpdateMenuItems(true);

        _updateTimer.Elapsed   += (_, _) => UpdateMenuItems();
        _updateTimer.AutoReset =  true;
        _updateTimer.Start();

        Node.OnClick      += _ => UpdateMenuItems(true);
        Node.OnRightClick += _ => OpenCurrenciesWindow();

        CreateAlertNodes();
    }

    public override string GetInstanceName()
    {
        if (uint.TryParse(GetConfigValue<string>("TrackedCurrency"), out uint customId)) {
            if (CustomCurrencies.TryGetValue(customId, out Currency? customCurrency)) {
                return $"{"Currencies+"} - {customCurrency.Name}";
            }

            return string.IsNullOrEmpty(GetConfigValue<string>("CustomLabel"))
                ? "Currencies+"
                : GetConfigValue<string>("CustomLabel");
        }

        return GetConfigValue<string>("TrackedCurrency") != ""
            ? $"{"Currencies+"} - {Currencies[Enum.Parse<CurrencyType>(GetConfigValue<string>("TrackedCurrency"))].Name}"
            : string.IsNullOrEmpty(GetConfigValue<string>("CustomLabel"))
                ? "Currencies+"
                : GetConfigValue<string>("CustomLabel");
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        Popup.IsDisabled        = !GetConfigValue<bool>("EnableMouseInteraction");
        Popup.UseGrayscaleIcons = GetConfigValue<bool>("DesaturateIcons");

        UpdateCustomIdList();

        var trackedCurrencyId = GetConfigValue<string>("TrackedCurrency");

        if (uint.TryParse(GetConfigValue<string>("TrackedCurrency"), out uint customId)) {
            if (CustomCurrencies.TryGetValue(customId, out Currency? customCurrency)) {
                string customName = GetConfigValue<bool>("ShowName") ? $" {customCurrency.Name}" : "";
                SetLabel($"{GetCustomAmount(customCurrency.Id, GetConfigValue<bool>("ShowCapOnWidget"))}{customName}");
                SetIcon(customCurrency.Icon);
                base.OnUpdate();
                return;
            }
        }

        if (!Enum.TryParse(trackedCurrencyId, out CurrencyType currencyType) || currencyType == 0 || !Currencies.TryGetValue(currencyType, out Currency? currency)) {
            string customLabel = GetConfigValue<string>("CustomLabel");
            string label       = "Currencies+";

            SetLabel(string.IsNullOrEmpty(customLabel) ? label : customLabel);
            SetIcon(null);
            base.OnUpdate();
            return;
        }

        string name = GetConfigValue<bool>("ShowName") ? $" {currency.Name}" : "";
        SetLabel($"{GetAmount(currency.Type, GetConfigValue<bool>("ShowCapOnWidget"))}{name}");
        SetIcon(currency.Icon);

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

        bool alertMode = GetConfigValue<bool>("CurrencyAlertMode");
        if (alertMode) {
            var iconSize = GetConfigValue<int>("IconSize") == 0 ? 24 : GetConfigValue<int>("IconSize");
            var iconGray = GetConfigValue<bool>("DesaturateIcon");

            foreach (Node nodeStyle in _alerts.Values) {
                nodeStyle.Style.Size = new(iconSize, iconSize);
                nodeStyle.Style.ImageGrayscale = iconGray;
                nodeStyle.Style.IsVisible = true;
            }
        }
        if (alertMode && _alerts.TryGetValue(currency.Id, out Node? nodeAlert)) {
            if (currency.Type == CurrencyType.Maelstrom || currency.Type == CurrencyType.TwinAdder || currency.Type == CurrencyType.ImmortalFlames) {

            }
            else if (currency.GroupId == 1) {
                nodeAlert.Style.IsVisible = true;
                // if (GetActualAmount(currency.Type) >= GetConfigValue<int>("HuntThreshold")) nodeAlert.Style.IsVisible = true;
                // else nodeAlert.Style.IsVisible = false;
            }
            else if (currency.GroupId == 2) {

            }
            else if (currency.GroupId == 3) {

            }
            else if (currency.GroupId == 4) {
                if (currency.Type == CurrencyType.SkyBuildersScrips) {

                }
                else {

                }
            }
            else if (currency.GroupId == 5) {

            }
        }

        base.OnUpdate();
    }

    /// <inheritdoc/>
    protected override void OnDisposed()
    {
        _updateTimer.Stop();
        _updateTimer.Dispose();
    }

    private void UpdateMenuItems(bool force = false)
    {
        if (!force && !Popup.IsOpen) return;

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

            string id = $"Currency_{currency.Id}";

            Popup.SetButtonAltLabel(id, GetAmount(currency.Type, GetConfigValue<bool>("ShowCap")));
            Popup.SetButtonVisibility(id, GetConfigValue<bool>($"EnabledCurrency_{currency.Id}"));

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

            Popup.SetButtonAltLabel($"Currency_{currency.Id}", GetAmount(currency.Type, GetConfigValue<bool>("ShowCap")));
            Popup.SetNewColor($"Currency_{currency.Id}", setTextColor);
        }
    }

    private unsafe void OpenCurrenciesWindow()
    {
        UIModule* uiModule = UIModule.Instance();
        if (uiModule == null) return;
        uiModule->ExecuteMainCommand(66);
    }

    private void CreateAlertNodes() {
        bool alertMode = GetConfigValue<bool>("CurrencyAlertMode");
        if (alertMode) {
            foreach (var currency in Currencies.Values) {
                Node node = new() {
                    ClassList = ["alert-node-entry"],
                    InheritTags = true,
                    Style = new() {
                        Size = new(24, 24),
                        Margin = new(0),
                        Padding = new(0),
                        IconId = currency.Icon,
                        IsVisible = false
                    }
                };

                _alerts.Add(currency.Id, node);
                Node.AppendChild(node);
            }
        }
    }
}