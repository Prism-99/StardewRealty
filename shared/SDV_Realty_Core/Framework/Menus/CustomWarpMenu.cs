using CustomMenuFramework.Controls;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using CustomMenuFramework.Menus;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace SDV_Realty_Core.Framework.Menus
{
    internal class CustomWarpMenu : CustomMenu
    {
        private string hoverText = "";
        private int selectedControl = -1;
        private int hoverControl = -1;
        private List<ICustomOption> controls = new();
        private List<OptionsElement> labels = new();
        private IModDataService _modDataService;
        private Action? onSave;
        public CustomWarpMenu(int x, int y, IModDataService _modDataService, Action? onSave = null) : base(x, y, 800, 550, true, true)
        {
            this._modDataService = _modDataService;
            this.onSave = onSave;
            //int width = 800;
            //int height = 550;
            int labelHeight = 48;
            int labelTopPad = 13;
            int labelLeft = 150;
            int controlHeight = 48;
            int controlTopPad = 22;
            LabelOption.LabelSize labelSize = LabelOption.LabelSize.dialogue;
            int baseControlId = 1000;
            for (int index = 0; index < 9; index += 2)
            {
                int entryIndex = (index / 2);
                var entry = _modDataService.GetCustomMiniMapEntry(entryIndex);
                if (entry != null)
                {
                    string destination = entry.Warp.ToMap;
                    if (Game1.locationData.TryGetValue(destination, out var data))
                        destination = TokenParser.ParseText(data.DisplayName ?? destination);

                    controls.Add(new LabelOption($"{entry.DisplayName}: {destination} ({entry.Warp.ToX},{entry.Warp.ToY})", new Rectangle(labelLeft, labelTopPad + labelHeight * index, 150, labelHeight), labelSize, -1, entryIndex + 300));
                }
                else
                {
                    controls.Add(new LabelOption(I18n.MM_Open(), new Rectangle(labelLeft, labelTopPad + labelHeight * index, 20, labelHeight), labelSize, -1, entryIndex + 300));
                }
                ButtonOption clearButton = new ButtonOption(I18n.MM_Clear(), 10, controlTopPad + labelHeight * index, HandleClearButton, entryIndex, entryIndex + 400);
                if (entryIndex > 0)
                    clearButton.upNeighborID = 100 + entryIndex - 1;
                clearButton.downNeighborID = 100 + entryIndex;
                controls.Add(clearButton);
                ButtonOption setButton = new ButtonOption(I18n.MM_Set(), 10, controlTopPad + controlHeight * (index + 1), HandleSetButton, entryIndex, entryIndex + 100);
                setButton.upNeighborID = 400 + entryIndex;
                if (entryIndex < 4)
                    setButton.downNeighborID = 400 + entryIndex + 1;
                setButton.rightNeighborID = 200 + entryIndex;
                controls.Add(setButton);
                TextEntryOption textBox = new TextEntryOption("", new Rectangle(140, controlTopPad + controlHeight * (index + 1), 150, 20), index, null, entryIndex + 200);
                textBox.leftNeighborID = 100 + entryIndex;
                if (entryIndex > 0)
                    textBox.upNeighborID = 200 + entryIndex-1;
                if (entryIndex < 4)
                    textBox.downNeighborID = 200 + entryIndex + 1;
                controls.Add(textBox);
                string currentLocation = TokenParser.ParseText(Game1.currentLocation.GetData()?.DisplayName) ?? Game1.currentLocation.Name;
                controls.Add(new LabelOption($"{currentLocation} ({Game1.player.Tile.X},{Game1.player.Tile.Y})", new Rectangle(290, labelTopPad + labelHeight * (index + 1), 150, labelHeight), labelSize, index));
            }
            ;

            SetControls(controls, labels, 400);
        }
        private void SaveEdits()
        {
            if (onSave != null)
                onSave();
        }
        private void HandleClearButton(int option)
        {
            if (TryGetControl(option + 300, out var control))
            {
                if (control is LabelOption label)
                {
                    label.Text = "Open";
                    SaveEdits();
                }
                _modDataService.DeleteMiniMapCustomEntry(option);
                SaveEdits();
            }
        }
        private void HandleSetButton(int option)
        {
            if (TryGetControl(option + 300, out ICustomOption? control))
            {
                if (control is LabelOption label)
                {
                    if (TryGetControl(option + 200, out ICustomOption? labelname))
                    {
                        if (labelname is TextEntryOption entry)
                        {
                            string currentLocation = TokenParser.ParseText(Game1.currentLocation.GetData()?.DisplayName) ?? Game1.currentLocation.Name;
                            label.Text = $"{entry.textBox.Text}: {currentLocation} ({Game1.player.Tile.X},{Game1.player.Tile.Y})";

                            _modDataService.InsertCustomMiniMapEntry(option, new ServiceInterfaces.GUI.IMiniMapService.MiniMapEntry
                            {
                                DisplayName = entry.textBox.Text,
                                Warp = new MapWarp
                                {
                                    ToMap = Game1.currentLocation.NameOrUniqueName,
                                    ToX = (int)Game1.player.Tile.X,
                                    ToY = (int)Game1.player.Tile.Y
                                }
                            });

                            SaveEdits();
                        }
                    }
                }
            }
        }
    }
}
