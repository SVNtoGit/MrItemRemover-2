/*
 * Mr.ItemRemover2 - Created by CodenameGamma - 1/31/11 - For WoW Version 4.0.3
 * 1.6 Update by Bambam922 10/18/13
 * www.thebuddyforum.com
 * This is a free plugin and should not be sold or repackaged.
 * Donations accepted.
 * Version 1.6 for WoW Version 5.4 +
 */

using System.Globalization;
using System.Windows.Forms;
using System;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System.IO;
using Styx.Plugins;
using Styx;
using Styx.Common.Helpers;
using Styx.Common;
using System.Collections.Generic;
using System.Windows.Media;


namespace MrItemRemover2
{

    public partial class MrItemRemover2 : HBPlugin
    {
        // ReSharper disable InconsistentNaming
        const string _name = "Mr.ItemRemover2 1.6.1";
        const string _debug = "Mr.ItemRemover2 DEBUG";
        // ReSharper restore InconsistentNaming

        //Normal Stuff.
        public override string Name { get { return _name; } }
        public override string Author { get { return "CnG & Bambam922"; } }
        public override Version Version { get { return new Version(1, 6, 1); } }
        public override bool WantButton { get { return true; } }
        public override string ButtonText { get { return _name; } }

        public override void OnButtonPress()
        {
            if (!IsInitialized)
            {
                Slog("Not finished initializing");
                return;
            }

            var form = new MrItemRemover2Gui(this);
            form.ShowDialog();
        }

        public static void Slog(string format, params object[] args)
        {
            Slog(Colors.SkyBlue, format, args);
        }

        public static void Slog(Color color, string format, params object[] args)
        {
            Logging.Write(color, "[" + _name + "]: " + format, args);
        }

        public static void Dlog(string format, params object[] args)
        {
            Dlog(Colors.LimeGreen, format, args);
        }

        public static void Dlog(Color color, string format, params object[] args)
        {
            Logging.WriteDiagnostic(color, "[" + _debug + "]: " + format, args);
        }

        //My Crappy Initalise.
        public override void Initialize()
        {
            Lua.Events.AttachEvent("DELETE_ITEM_CONFIRM", DeleteItemConfirmPopup);
            Lua.Events.AttachEvent("MERCHANT_SHOW", SellVenderItems);
            Lua.Events.AttachEvent("LOOT_CLOSED", LootEnded);

            Slog("Initial Loading of Item names.");
            InitialMirLoad();
            MrItemRemover2Settings.Instance.Load();
            PrintSettings();

            _checkTimer.Reset(); //should start the timer 

            IsInitialized = true;
        }

        public override void Dispose()
        {
            Lua.Events.DetachEvent("DELETE_ITEM_CONFIRM", DeleteItemConfirmPopup);
            Lua.Events.DetachEvent("MERCHANT_SHOW", SellVenderItems);
            Lua.Events.DetachEvent("LOOT_CLOSED", LootEnded);

            IsInitialized = false;
            MirSave();

            Dlog("MrItemRemover2 is now disabled.");
        }

        public bool ManualCheckRequested { get; set; }

        private readonly WaitTimer _checkTimer = new WaitTimer(TimeSpan.FromMinutes(MrItemRemover2Settings.Instance.Time));
        private bool EnableCheck { get; set; }
        private bool IsInitialized { get; set; }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        public override void Pulse()
        {
            if (ManualCheckRequested)
            {
                EnableCheck = true;
                ManualCheckRequested = false;
                _checkTimer.Reset();

                Slog("Checking Bags Manually & Reloading Item Lists.");
                CheckForItems();
            }

            else if (!MrItemRemover2Settings.Instance.LootEnable)
            {
                if (_checkTimer.TimeLeft.Ticks <= 0)
                {
                    if (EnableCheck == false)
                    {
                        EnableCheck = true;
                        _checkTimer.Reset();

                        Slog("Enabling Check at {0}", GetTime(DateTime.Now));
                        Dlog("Checktimer has Finished its Total wait of {0} Minutes, Enabling Item Check for next Opportunity", MrItemRemover2Settings.Instance.Time.ToString(CultureInfo.InvariantCulture));
                        Slog("Will Run Next Check At {0}", GetTime(_checkTimer.EndTime));
                    }
                }
            }
            
            if (!Me.Combat && !Me.IsCasting && !Me.IsDead && !Me.IsGhost && EnableCheck)
            {
                Slog("EnableCheck was Passed!");
                if (MrItemRemover2Settings.Instance.EnableOpen)
                {
                    OpenBagItems();
                }
                if (MrItemRemover2Settings.Instance.EnableRemove)
                {
                    CheckForItems();
                }
                EnableCheck = false;
                Slog("Turning off Check Since Done!");
            }
        }

        private void LootEnded(object sender, LuaEventArgs args)
        {
            if (MrItemRemover2Settings.Instance.LootEnable)
            {
                if (EnableCheck == false)
                {
                    EnableCheck = true;
                    Slog("Enabling Check because Loot Ended");
                }
            }
        }
        
        //All items from the TXT Doc are loaded here.
        public List<string> ItemName = new List<string>();
        //Specific items from the TXT Doc are loaded here.
        public List<string> ItemNameSell = new List<string>();
        public List<string> InventoryList = new List<string>();
        public List<string> KeepList = new List<string>();
        public List<string> OpnList = new List<string>();
        public List<string> BagList = new List<string>();

        //file Path for Saving and Loading. 
        private readonly string _removeListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           string.Format(@"Plugins/MrItemRemover2/ItemNameRemoveList.txt"));
        private readonly string _sellListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           string.Format(@"Plugins/MrItemRemover2/ItemNameSellList.txt"));
        private readonly string _keepListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           string.Format(@"Plugins/MrItemRemover2/ItemNameKeepList.txt"));
        private readonly string _opnListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           string.Format(@"Plugins/MrItemRemover2/ItemNameOpnList.txt"));

        public void InitialMirLoad()
        {
            Slog("Initial Loading of Items to Remove List.");
            LoadList(ItemName, _removeListPath);
            Slog("Initial Loading of Items to Sell List.");
            LoadList(ItemNameSell, _sellListPath);
            Slog("Initial Loading of Items to Keep List.");
            LoadList(KeepList, _keepListPath);
            Slog("Initial Loading of Items to Open List.");
            LoadList(OpnList, _opnListPath);
            Slog("Initial Loading Complete!");
        }

        public void MirLoad()
        {
            LoadList(ItemName, _removeListPath);
            LoadList(ItemNameSell, _sellListPath);
            LoadList(KeepList, _keepListPath);
            LoadList(OpnList, _opnListPath);
        }

        public void LoadList(List<string> listToLoad, string filePath)
        {
            listToLoad.Clear();
            try
            {
                using (var read = new StreamReader(Convert.ToString(filePath)))
                {
                    while (read.Peek() >= 0)
                    {
                        listToLoad.Add(Convert.ToString(read.ReadLine()));
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex.Message));
            }
        }

        public void MirSave()
        {
            Slog("Saving All Lists.");

            WriteList(ItemName, _removeListPath);

            WriteList(ItemNameSell, _sellListPath);

            WriteList(KeepList, _keepListPath);

            WriteList(OpnList, _opnListPath);
        }

        public void WriteList(List<string> listName, string filePath)
        {
            try
            {
                var write = new StreamWriter(filePath);
                // ReSharper disable ForCanBeConvertedToForeach
                for (int I = 0; I < listName.Count; I++)
                // ReSharper restore ForCanBeConvertedToForeach
                {
                    write.WriteLine(Convert.ToString(listName[I]));
                }
                write.Close(); //dakkon for this fix.
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex.Message));
            }
        }
    }
}

