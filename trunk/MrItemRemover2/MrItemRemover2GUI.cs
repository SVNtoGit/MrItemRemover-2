using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Styx;
using Styx.WoWInternals.WoWObjects;

namespace MrItemRemover2
{
    public partial class MrItemRemover2Gui : Form
    {
        private readonly string _goldImangePathName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                                  string.Format(@"Plugins/MrItemRemover2/Gold2.bmp"));

        private readonly string _refreshImangePathName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                                  string.Format(@"Plugins/MrItemRemover2/ref.bmp"));

        public MrItemRemover2Gui(MrItemRemover2 controller)
        {
            Controller = controller;
            InitializeComponent();
        }
        
        public MrItemRemover2 Controller { get; private set; }

        public static void Slog(string format, params object[] args)
        {
            MrItemRemover2.Slog(format, args);
        }
        public static void Dlog(string format, params object[] args)
        {
            MrItemRemover2.Dlog(format, args);
        }

        private void MrItemRemover2GUI_Load(object sender, EventArgs e)
        {
            var refresh = new Bitmap(_refreshImangePathName);
            var goldImg = new Bitmap(_goldImangePathName);
            GoldBox.Image = goldImg;
            resf.Image = refresh;
            MrItemRemover2Settings.Instance.Load();
            SellList.Items.Clear();
            RemoveList.Items.Clear();
            ProtectedList.Items.Clear();
            GrayItems.Checked = MrItemRemover2Settings.Instance.DeleteAllGray;
            WhiteItems.Checked = MrItemRemover2Settings.Instance.DeleteAllWhite;
            GreenItems.Checked = MrItemRemover2Settings.Instance.DeleteAllGreen;
            BlueItems.Checked = MrItemRemover2Settings.Instance.DeleteAllBlue;
            SellGray.Checked = MrItemRemover2Settings.Instance.SellGray;
            //ApplyAll.Checked = MrItemRemover2Settings.Instance.ApplyAll;
            SellWhite.Checked = MrItemRemover2Settings.Instance.SellWhite;
            SellGreen.Checked = MrItemRemover2Settings.Instance.SellGreen;
            SellBlue.Checked = MrItemRemover2Settings.Instance.SellBlue;
            SellSoulbound.Checked = MrItemRemover2Settings.Instance.SellSoulbound;
            CombineItems.Checked = MrItemRemover2Settings.Instance.CombineItems;
            EnableRemove.Checked = MrItemRemover2Settings.Instance.EnableRemove;
            EnableOpen.Checked = MrItemRemover2Settings.Instance.EnableOpen;
            EnableSell.Checked = MrItemRemover2Settings.Instance.EnableSell;
            RemoveQItems.Checked = MrItemRemover2Settings.Instance.DeleteQuestItems;
            LootEnable.Checked = MrItemRemover2Settings.Instance.LootEnable;
            GoldGrays.Text = MrItemRemover2Settings.Instance.GoldGrays.ToString(CultureInfo.InvariantCulture);
            SilverGrays.Text = MrItemRemover2Settings.Instance.SilverGrays.ToString(CultureInfo.InvariantCulture);
            CopperGrays.Text = MrItemRemover2Settings.Instance.CopperGrays.ToString(CultureInfo.InvariantCulture);
            Time.Value = MrItemRemover2Settings.Instance.Time;
            foreach (string itm in Controller.ItemName)
            {
                RemoveList.Items.Add(itm);
            }
            foreach (string itm in Controller.ItemNameSell)
            {
                SellList.Items.Add(itm);
            }
            foreach (string itm in Controller.KeepList)
            {
                ProtectedList.Items.Add(itm);
            }

            foreach (WoWItem bagItem in StyxWoW.Me.BagItems)
            {
                if (bagItem.IsValid && bagItem.BagSlot != -1 && !MyBag.Items.Contains(bagItem.Name))
                {
                    MyBag.Items.Add(bagItem.Name);
                }
            }
        }

        private void AddToBagList_Click(object sender, EventArgs e)
        {
            if (InputAddToBagItem.Text != null)
            {
                MyBag.Items.Add(InputAddToBagItem.Text);
                Slog("Added {0} to Inventory List", InputAddToBagItem.Text);
            }
        }

        private void AddToSellList_Click(object sender, EventArgs e)
        {
            if (MyBag.SelectedItems[0] != null)
            {
                SellList.Items.Add(MyBag.SelectedItem);
                Controller.ItemNameSell.Add(MyBag.SelectedItem.ToString());
            }
        }

        private void AddToRemoveList_Click(object sender, EventArgs e)
        {
            if (MyBag.SelectedItems[0] != null)
            {
                RemoveList.Items.Add(MyBag.SelectedItem);
                Controller.ItemName.Add(MyBag.SelectedItem.ToString());
            }
        }

        private void AddToProtList_Click(object sender, EventArgs e)
        {
            if (MyBag.SelectedItems[0] != null)
            {
                ProtectedList.Items.Add(MyBag.SelectedItem);
                Controller.KeepList.Add(MyBag.SelectedItem.ToString());
            }
        }

        private void RemoveSellItem_Click(object sender, EventArgs e)
        {
            if (SellList.SelectedItem != null)
            {
                Slog("{0} Removed", SellList.SelectedItem.ToString());
                Controller.ItemNameSell.Remove(SellList.SelectedItem.ToString());
                SellList.Items.Remove(SellList.SelectedItem);
            }
        }

        private void RemoveRemoveItem_Click(object sender, EventArgs e)
        {
            if (RemoveList.SelectedItem != null)
            {
                Slog("{0} Removed", RemoveList.SelectedItem.ToString());
                Controller.ItemName.Remove(RemoveList.SelectedItem.ToString());
                RemoveList.Items.Remove(RemoveList.SelectedItem);
            }
        }

        private void RemoveProtectedItem_Click(object sender, EventArgs e)
        {
            if (ProtectedList.SelectedItem != null)
            {
                Slog("{0} Removed", ProtectedList.SelectedItem.ToString());
                Controller.KeepList.Remove(ProtectedList.SelectedItem.ToString());
                ProtectedList.Items.Remove(ProtectedList.SelectedItem);
            }
        }

        public void SettingsDebug()
        {
            Dlog("Enable Removing  = {0}", EnableRemove.Checked);
            Dlog("Enable Selling   = {0}", EnableSell.Checked);
            Dlog("Enable Opening   = {0}", EnableOpen.Checked);
            Dlog("Remove Grays     = {0}", GrayItems.Checked);
            Dlog("Remove Whites    = {0}", WhiteItems.Checked);
            Dlog("Remove Greens    = {0}", GreenItems.Checked);
            Dlog("Remove Blues     = {0}", BlueItems.Checked);
            Dlog("Sell Grays       = {0}", SellGray.Checked);
            Dlog("Sell Whites      = {0}", SellWhite.Checked);
            Dlog("Sell Greens      = {0}", SellGreen.Checked);
            Dlog("Sell Blues       = {0}", SellBlue.Checked);
            Dlog("Sell Soulbound   = {0}", SellSoulbound.Checked);
            Dlog("Check After Loot = {0}", LootEnable.Checked);
            Dlog("Gold Value       = {0}", GoldGrays.Text);
            Dlog("Silver Value     = {0}", SilverGrays.Text);
            Dlog("Copper Value     = {0}", CopperGrays.Text);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Controller.MirSave();
            MrItemRemover2Settings.Instance.Save();
            SettingsDebug();
            Close();
        }

        private void GrayItems_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.DeleteAllGray = GrayItems.Checked;
        }

        private void WhiteItems_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.DeleteAllWhite = WhiteItems.Checked;
        }

        private void GreenItems_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.DeleteAllGreen = GreenItems.Checked;
        }

        private void BlueItems_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.DeleteAllBlue = BlueItems.Checked;
        }

        private void RemoveQItems_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.DeleteQuestItems = RemoveQItems.Checked;
        }

        private void SellGray_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.SellGray = SellGray.Checked;
        }

        /* private void ApplyAll_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.ApplyAll = ApplyAll.Checked;
        }*/

        private void SellGreen_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.SellGreen = SellGreen.Checked;
        }

        private void SellWhite_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.SellWhite = SellWhite.Checked;
        }

        private void EnableSell_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.EnableSell = EnableSell.Checked;
        }

        private void EnableRemove_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.EnableRemove = EnableRemove.Checked;
        }

        private void Time_ValueChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.Time = int.Parse(Time.Value.ToString(CultureInfo.InvariantCulture));
        }

        private void GoldGrays_TextChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.GoldGrays = int.Parse(GoldGrays.Text);
        }

        private void SilverGrays_TextChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.SilverGrays = int.Parse(SilverGrays.Text);
        }

        private void CopperGrays_TextChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.CopperGrays = int.Parse(CopperGrays.Text);
        }

        private void EnableOpen_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.EnableOpen = EnableOpen.Checked;
        }

        private void resf_Click(object sender, EventArgs e)
        {
            MyBag.Items.Clear();
            foreach (WoWItem bagItem in StyxWoW.Me.BagItems)
            {
                if (bagItem.BagSlot != -1 && !MyBag.Items.Contains(bagItem.Name))
                {
                    MyBag.Items.Add(bagItem.Name);
                }
            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            Controller.ManualCheckRequested = true;
        }

        private void LootEnable_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.LootEnable = LootEnable.Checked;
        }

        private void SellList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {
        }

        private void SellBlue_CheckedChanged_1(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.SellBlue = SellBlue.Checked;
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void SellSoulbound_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.SellSoulbound = SellSoulbound.Checked;
        }

        private void CombineItems_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.CombineItems = CombineItems.Checked;
        }

        private void RSDrinks_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.RSDrinks = CombineItems.Checked;
        }

        private void RSFood_CheckedChanged(object sender, EventArgs e)
        {
            MrItemRemover2Settings.Instance.RSFood = CombineItems.Checked;
        }
    }
}