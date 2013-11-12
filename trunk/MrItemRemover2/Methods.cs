﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Styx;
using Styx.CommonBot.Frames;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace MrItemRemover2
{
    public partial class MrItemRemover2
    {
        public void SellVenderItems(object sender, LuaEventArgs args)
        {
            if (MerchantFrame.Instance.IsVisible && MrItemRemover2Settings.Instance.EnableSell)
            {
                foreach (WoWItem item in Me.BagItems)
                {
                    if (!MrItemRemover2Settings.Instance.SellSoulbound)
                    {
                        if (!item.IsSoulbound && !KeepList.Contains(item.Name))
                        {
                            if (item.Quality == WoWItemQuality.Poor && MrItemRemover2Settings.Instance.SellGray)
                            {
                                Slog("Selling Gray Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (item.Quality == WoWItemQuality.Common && MrItemRemover2Settings.Instance.SellWhite)
                            {
                                Slog("Selling White Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (item.Quality == WoWItemQuality.Uncommon && MrItemRemover2Settings.Instance.SellGreen)
                            {
                                Slog("Selling Green Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (item.Quality == WoWItemQuality.Rare && MrItemRemover2Settings.Instance.SellBlue)
                            {
                                Slog("Selling Blue Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (ItemNameSell.Contains(item.Name))
                            {
                                Slog("Item Matched List Selling {0}", item.Name);
                                item.UseContainerItem();
                            }
                        }
                    }

                    if (MrItemRemover2Settings.Instance.SellSoulbound)
                    {
                        if (!KeepList.Contains(item.Name))
                        {
                            if (item.Quality == WoWItemQuality.Poor && MrItemRemover2Settings.Instance.SellGray)
                            {
                                Slog("Selling Gray Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (item.Quality == WoWItemQuality.Common && MrItemRemover2Settings.Instance.SellWhite)
                            {
                                Slog("Selling White Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (item.Quality == WoWItemQuality.Uncommon && MrItemRemover2Settings.Instance.SellGreen)
                            {
                                Slog("Selling Green Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (item.Quality == WoWItemQuality.Rare && MrItemRemover2Settings.Instance.SellBlue)
                            {
                                Slog("Selling Blue Item {0}", item.Name);
                                item.UseContainerItem();
                            }
                            if (ItemNameSell.Contains(item.Name))
                            {
                                Slog("Item Matched List Selling {0}", item.Name);
                                item.UseContainerItem();
                            }
                        }
                    }
                }
            }
        }

        private static void DeleteItemConfirmPopup(object sender, LuaEventArgs args)
        {
            string itemNamePopUp = args.Args[0].ToString();

            if (Me.CurrentTarget != null)
            {
                Slog("Clicking Yes to Comfirm {0}'s Removal From Inventory", itemNamePopUp);
                Lua.DoString("RunMacroText(\"/click StaticPopup1Button1\");");
            }
        }

        public void PrintSettings()
        {
            Dlog("Mr.ItemRemover2 Settings");
            Dlog("------------------------------------------");
            foreach (var setting in MrItemRemover2Settings.Instance.GetSettings())
            {
                string key = setting.Key;
                var value = setting.Value;
                Dlog(string.Format("{0} - {1}", key, value));
            }
            Dlog("------------------------------------------");
        }

        public void OpenBagItems()
        {
            MirSave();
            MirLoad();
            LoadList(OpnList, _opnListPath);

            // NB: Since we will be modifying the Me.BagItems list indirectly through WoWclient directives,
            // we can't use it as our iterator--we must make a copy, instead.
            List<WoWItem> itemsToVisit = Me.BagItems.ToList();
            foreach (WoWItem item in itemsToVisit)
            {
                if ((item == null) || !item.IsValid)
                {
                    continue;
                }

                if (OpnList.Contains(item.Name))
                {
                    //probally not needed, but still user could be messing with thier inventory.
                    Slog("{0} Found Open Item", item.Name);
                    item.Interact();
                    Thread.Sleep(600);
                }
            }
        }

        public void CheckForItems()
        {
            //Reload item lists
            MirSave();
            MirLoad();

            //Added to Make sure our list matches what we are looking for. 
            LoadList(ItemName, _removeListPath);

            // NB: Since we will be modifying the Me.BagItems list indirectly through WoWclient directives,
            // we can't use it as our iterator--we must make a copy, instead.
            List<WoWItem> itemsToVisit = Me.BagItems.ToList();
            foreach (WoWItem item in itemsToVisit)
            {
                // Uncomment this to have quest items printed to log. DIAGNOSTIC.

                if ((item == null) || !item.IsValid)
                {
                    continue;
                }

                bool isQuestItem = IsQuestItem(item);
                if (BagList.Contains(item.Entry.ToString(CultureInfo.InvariantCulture)))
                {
                    Slog("{0} is a bag, ignoring.", item.Name);
                    return;
                }
                //if item name Matches whats in the text file / the internal list (after load)
                if (ItemName.Contains(item.Name) && !KeepList.Contains(item.Name))
                {
                    //probally not needed, but still user could be messing with thier inventory.
                    //Printing to the log, and Deleting the Item.
                    Slog("{0} Found Removing Item", item.Name);
                    item.PickUp();
                    Lua.DoString("DeleteCursorItem()");
                    //a small Sleep, might not be needed. 
                    Thread.Sleep(600);
                }

                if (MrItemRemover2Settings.Instance.DeleteQuestItems && item.ItemInfo.BeginQuestId != 0 &&
                    !KeepList.Contains(item.Name))
                {
                    Slog("{0}'s Began a Quest. Removing", item.Name);
                    item.PickUp();
                    Lua.DoString("DeleteCursorItem()");
                }

                //Process all Gray Items if enabled. 
                if (MrItemRemover2Settings.Instance.DeleteAllGray && item.Quality == WoWItemQuality.Poor && !KeepList.Contains(item.Name) && !BagList.Contains(item.Name))
                {
                    //Gold Format, goes in GXX SXX CXX 
                    string goldString = MrItemRemover2Settings.Instance.GoldGrays.ToString(CultureInfo.InvariantCulture);
                    int goldValue = goldString.ToInt32() * 10000;
                    string silverString = MrItemRemover2Settings.Instance.SilverGrays.ToString(CultureInfo.InvariantCulture);
                    int silverValue = silverString.ToInt32() * 100;
                    string copperString = MrItemRemover2Settings.Instance.CopperGrays.ToString(CultureInfo.InvariantCulture);
                    int copperValue = copperString.ToInt32();

                    //slog("Value of input sell string - " + (goldValue + silverValue + copperValue));

                    if (item.BagSlot != -1 && !isQuestItem && item.ItemInfo.SellPrice <= (goldValue + silverValue + copperValue))
                    {
                        Slog("{0}'s Item Quality was Poor and only worth {1} copper. Removing.", item.Name, item.ItemInfo.SellPrice);
                        Lua.DoString("ClearCursor()");
                        item.PickUp();
                        Lua.DoString("DeleteCursorItem()");
                        Thread.Sleep(300);
                    }
                }

                //Process all White Items if enabled.
                if (MrItemRemover2Settings.Instance.DeleteAllWhite && item.Quality == WoWItemQuality.Common && !KeepList.Contains(item.Name) && !BagList.Contains(item.Name))
                {
                    if (item.BagSlot != -1 && !isQuestItem)
                    {
                        Slog("{0}'s Item Quality was Common. Removing.", item.Name);
                        Lua.DoString("ClearCursor()");
                        item.PickUp();
                        Lua.DoString("DeleteCursorItem()");
                        Thread.Sleep(300);
                    }
                }

                //Process all Green Items if enabled.
                if (MrItemRemover2Settings.Instance.DeleteAllGreen && item.Quality == WoWItemQuality.Uncommon && !KeepList.Contains(item.Name) && !BagList.Contains(item.Name))
                {
                    if (item.BagSlot != -1 && !isQuestItem)
                    {
                        Slog("{0}'s Item Quality was Uncommon. Removing.", item.Name);
                        Lua.DoString("ClearCursor()");
                        item.PickUp();
                        Lua.DoString("DeleteCursorItem()");
                        Thread.Sleep(300);
                    }
                }

                //Process all Blue Items if enabled.
                if (MrItemRemover2Settings.Instance.DeleteAllBlue && item.Quality == WoWItemQuality.Rare && !KeepList.Contains(item.Name) && !BagList.Contains(item.Name))
                {
                    if (item.BagSlot != -1 && !isQuestItem)
                    {
                        Slog("{0}'s Item Quality was Rare. Removing.", item.Name);
                        Lua.DoString("ClearCursor()");
                        item.PickUp();
                        Lua.DoString("DeleteCursorItem()");
                        Thread.Sleep(300);
                    }
                }
            }
        }

        public string GetTime(DateTime input)
        {
            int hour = input.Hour;
            int min = input.Minute;
            int sec = input.Second;

            string timeInString = (hour < 10) ? "0" + hour.ToString(CultureInfo.InvariantCulture) : hour.ToString(CultureInfo.InvariantCulture);
            timeInString += ":" + ((min < 10) ? "0" + min.ToString(CultureInfo.InvariantCulture) : min.ToString(CultureInfo.InvariantCulture));
            // ReSharper disable RedundantAssignment
            return timeInString += ":" + ((sec < 10) ? "0" + sec.ToString(CultureInfo.InvariantCulture) : sec.ToString(CultureInfo.InvariantCulture));
            // ReSharper restore RedundantAssignment
        }

        private bool IsQuestItem(WoWItem item)
        {
            if ((item == null) || !item.IsValid)
            {
                return false;
            }

            string luaCommand = string.Format("return GetContainerItemQuestInfo({0},{1});", item.BagIndex + 1,
                                              item.BagSlot + 1);
            bool isQuestItem =
                Lua.GetReturnVal<bool>(luaCommand, 0) // item is quest item?
                || (Lua.GetReturnVal<int>(luaCommand, 1) > 0); // item begins a quest?

            return isQuestItem;
        }
    }
}