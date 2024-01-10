using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress;
using RCE_ADMIN.Interface;
using RCE_ADMIN.WebSockets;
using System.Diagnostics;
using DevExpress.Utils;
using System.Drawing;

namespace RCE_ADMIN
{
    public partial class Form1 : XtraForm
    {
        public static Settings Settings;
        public static BarStaticItem Status;
        public static RichTextBox Console;
        public static BarStaticItem Counter;
        public static DataGridView Players;
        public Form1()
        {
            InitializeComponent();
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this, new Point(e.X, e.Y));
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Settings = Settings.Read();
            Status = toolStripStatusLabelRight;
            Console = richTextBoxConsole;
            Counter = toolStripStatusLabelCounter;
            Players = dataGridViewPlayers;
            textBoxAddress.Text = Settings.ServerAddress;
            textBoxPort.Text = Settings.ServerPort;
            textBoxPassword.Text = Settings.ServerPassword;
            eventsWebhookUrl.Text = Settings.EventWebhookUrl;
            killfeedsWebhookUrl.Text = Settings.KillFeedWebhookUrl;
            inGameName.Text = Settings.InGameName;
            ServerConsole.Disable();
        }
        public void CopyFromDT(int i)
        {
            try
            {
                if (Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    Clipboard.SetText(Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString());
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
        public string GetFromDT(int i)
        {
            try
            {
                if (Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    return Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString();
                }
                else
                {
                    XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return null;
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (WebSocketsWrapper.IsConnected())
                WebSocketsWrapper.Disconnect();
        }
        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.Properties.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }
        public void save_settings()
        {
            Settings.Write(new Settings(textBoxAddress.Text, textBoxPort.Text, textBoxPassword.Text, eventsWebhookUrl.Text, killfeedsWebhookUrl.Text, inGameName.Text));
            Settings = Settings.Read();
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            save_settings();
        }
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Connect();
        }
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Disconnect();
        }
        private void buttonCommand_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.SendCommand(textBoxCommand.Text);
            textBoxCommand.Text = "";
        }
        private void buttonBroadcast_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send($"global.say {broadcastMessageBox.Text}");
            broadcastMessageBox.Text = "";
        }
        private void textBoxCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((ConsoleKey)e.KeyChar == ConsoleKey.Enter)
            {
                WebSocketsWrapper.SendCommand(textBoxCommand.Text);
                textBoxCommand.Text = "";
            }
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            ServerConsole.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void copyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyFromDT(1);
            XtraMessageBox.Show(string.Format("Gamertag {0} Has Been Copied To Your Clipboard!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void kickPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("kick {0}", GetFromDT(1)));
        }

        private void banPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("banid {0}", GetFromDT(1)));
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Process.Start("https://prnt.sc/Qc-uU0PHiPx8");
            XtraMessageBox.Show(string.Format("Go To Your Servers Console On GPortal{0}Press CTRL, SHIFT & I To Open Inspect Element{0}Click On The ngapi/ Requests Until You Find The One With The Sameish Output At The Screenshot We Just Opened{0}Follow The Screenshot For The RCON Password!", Environment.NewLine), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void checkButton1_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkButton2_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkButton4_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkButton3_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("VIPID {0}", GetFromDT(1)));
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveVIP {0}", GetFromDT(1)));
        }

        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("ModeratorID {0}", GetFromDT(1)));
        }

        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveModerator {0}", GetFromDT(1)));
        }

        private void addToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("AdminID {0}", GetFromDT(1)));
        }

        private void removeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveAdmin {0}", GetFromDT(1)));
        }

        private void addToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("OwnerID {0}", GetFromDT(1)));
        }

        private void removeToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveOwner {0}", GetFromDT(1)));
        }

        private void broadcastMessageBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((ConsoleKey)e.KeyChar == ConsoleKey.Enter)
            {
                WebSocketsWrapper.Send($"global.say {broadcastMessageBox.Text}");
                broadcastMessageBox.Text = "";
            }
        }
        public static void give_item_to_player(string player, string item, int amount = 1)
        {
            WebSocketsWrapper.Send(string.Format("inventory.giveto {0} {1} {2}", player, item, amount));
        }
        public static void teleport_to(string player)
        {
            WebSocketsWrapper.Send(string.Format("global.teleport {0} {1}", Settings.InGameName, player));
        }
        public static void teleport_here(string player)
        {
            WebSocketsWrapper.Send(string.Format("global.teleport2me {0} (0,0,0)", player));
        }
        public static void give_item_to_all(string item)
        {
            WebSocketsWrapper.Send(string.Format("inventory.giveall {0}", item));
        }

        private void boneClubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bone.club");
        }

        private void compoundBowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bow.compound");
        }

        private void huntingBowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bow.hunting");
        }

        private void crossbowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "crossbow");
        }

        private void flameThrowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flamethrower");
        }

        private void beancanGrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grenade.beancan");
        }

        private void f1GrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grenade.f1");
        }

        private void smokeGrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grenade.smoke");
        }

        private void boneKnifeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "knife.bone");
        }

        private void combatKnifeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "knife.combat");
        }

        private void m249ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lmg.m249");
        }

        private void longSwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "longsword");
        }

        private void maceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mace");
        }

        private void macheteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "machete");
        }

        private void grenadeLauncherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "multiplegrenadelauncher");
        }

        private void paddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "paddle");
        }

        private void eokaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.eoka");
        }

        private void m92ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.m92");
        }

        private void nailgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.nailgun");
        }

        private void pythonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.python");
        }

        private void revolverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.revolver");
        }

        private void semiAutoPistolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.semiauto");
        }

        private void assaultRifleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.ak");
        }

        private void boltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.bolt");
        }

        private void l96ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.l96");
        }

        private void lR300ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.lr300");
        }

        private void m339ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.m39");
        }

        private void semiAutoRifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.semiauto");
        }

        private void dataGridViewPlayers_Click(object sender, EventArgs e)
        {
            MainForm_MouseClick(sender, (MouseEventArgs)e);
        }
        private void rocketLauncherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rocket.launcher");
        }
        private void salvagedCleaverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "salvaged.cleaver");
        }
        private void salvagedSwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "salvaged.sword");
        }
        private void doubleBarrelShotgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.double");
        }

        private void pumpShotgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.pump");
        }

        private void spas12ShotgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.shas12");
        }

        private void waterpipeShotguunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.waterpipe");
        }

        private void customSMGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smg.2");
        }

        private void mP5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smg.mp5");
        }

        private void thompsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smg.thompson");
        }

        private void stoneSpearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "spear.stone");
        }

        private void woodenSpearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "spear.wooden");
        }

        private void spearguunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "speargun");
        }

        private void flashlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.flashlight");
        }

        private void holoSightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.holosight");
        }
        private void laserSightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.lasersight");
        }
        private void muzzleBoostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.muzzleboost");
        }
        private void muzzleBreakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.muzzlebreak");
        }
        private void silencerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.silencer");
        }
        private void simpleHandmadeSigntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.simplesight");
        }
        private void xscopeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.small.scope");
        }

        private void concreteBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.concrete");
        }

        private void metalBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.metal");
        }

        private void sandBagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.sandbags");
        }

        private void stoneBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.stone");
        }

        private void woodBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.wood");
        }

        private void smallWoodenWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.wood.cover");
        }

        private void barbedWoodenBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.woodwire");
        }

        private void buuildingPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "building.planner");
        }

        private void toolCupboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cupboard.tool");
        }

        private void metaldoubleDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.double.hinged.metal");
        }

        private void armouredDoubleDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.double.hinged.toptier");
        }

        private void wooddenDoubleDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.double.hinged.wood");
        }

        private void singleMetalDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.hinged.metal");
        }

        private void armouredDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.hinged.toptier");
        }

        private void woodenDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.hinged.wood");
        }

        private void floorGrillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.grill");
        }

        private void ladderHtachToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.ladder.hatch");
        }

        private void triangleFloorGrillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.triangle.grill");
        }

        private void triangleLadderHatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.triangle.ladder.hatch");
        }

        private void highExternalStoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gates.external.high.stone");
        }

        private void highExternalWoodGateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gates.external.high.wood");
        }

        private void ladderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ladder.wooden.wall");
        }

        private void codeLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lock.code");
        }

        private void keyLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lock.key");
        }

        private void hoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shutter.metal.embrasure.a");
        }

        private void metalVerticalEmbrasureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shutter.metal.embrasure.b");
        }

        private void woodShuttersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shutter.wood.a");
        }

        private void highExternalWoodenWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.external.high");
        }

        private void highExternalStoneWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.external.high.stone");
        }

        private void prisonCellWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.cell");
        }

        private void prisonCellGateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.cell.gate");
        }

        private void chainLinkFenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.fence");
        }

        private void chainLinkGateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.fence.gate");
        }

        private void garageDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.garagedoor");
        }

        private void nettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.netting");
        }

        private void woodenShopFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.shopfront");
        }

        private void metalShopFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.shopfront.metal");
        }

        private void metalWindowBarsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.window.bars.metal");
        }

        private void reinforcedGlassWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.window.bars.toptier");
        }

        private void strengthenedGlasWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.window.glass.reinforced");
        }

        private void woodenWatchTowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "watchtower.wood");
        }

        private void largeWacerCatcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.catcher.large");
        }

        private void smallWaterCatcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.catcher.small");
        }

        private void barbequeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bbq");
        }

        private void botaBagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "botabag");
        }

        private void bedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bed");
        }

        private void smallFurnaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "furnace");
        }

        private void fridgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fridge");
        }

        private void smallFishTrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fishtrap.small");
        }

        private void stoneFirePlaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fireplace.stone");
        }

        private void dropBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "dropbox");
        }

        private void composerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "composter");
        }

        private void chairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chair");
        }

        private void campFireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "campfire");
        }

        private void largeWoodenBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "box.wooden.large");
        }

        private void smallWoodenBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "box.wooden.small");
        }

        private void repairBenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "box.repair.bench");
        }

        private void largeFuurnaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "furnace.large");
        }

        private void hitchTrouughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hitchtroughcombo");
        }

        private void kayakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "kayak");
        }

        private void lanternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lantern");
        }

        private void mailboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mailbox");
        }

        private void mixingTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "Mixing Table");
        }

        private void largePlanterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "planter.large");
        }

        private void smallPlanterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "planter.small");
        }

        private void researchTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "research.table");
        }

        private void bearRuugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rug.bear");
        }

        private void rugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rug");
        }

        private void salvaagedShelvesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shelves");
        }

        private void sleepingBagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sleepingbag");
        }

        private void workbenchLevel3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "workbench3");
        }

        private void workbechLevel2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "workbench2");
        }

        private void workbenchLevel1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "workbench1");
        }

        private void waterPuurifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.purifier");
        }

        private void waterBarrelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.barrel");
        }

        private void vendingMachineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "vending.machine");
        }

        private void tuunaCanLampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tunalight");
        }

        private void tableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "table");
        }

        private void smallStashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stash.small");
        }

        private void smallOilRefineryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "small.oil.refinery");
        }

        private void highQualityMetalOreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hq.metal.ore", 100);
        }

        private void horseDungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsedung");
        }

        private void gunPowderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gunpowder", 1000);
        }

        private void fertilizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fertilizer", 1000);
        }

        private void animalFatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fat.animal", 1000);
        }

        private void explosivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "explosives", 100);
        }

        private void dieselFuelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diesel_barrel");
        }

        private void crudeOilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "crude.oil");
        }

        private void clothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cloth", 1000);
        }

        private void charcoalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "charcoal", 1000);
        }

        private void cCTVCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cctv.camera");
        }

        private void emptyCanOfTunaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.tuna.empty");
        }

        private void emptyCanOfBeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.beans.empty");
        }

        private void boneFragmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bone.fragments", 1000);
        }

        private void leatherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "leather", 1000);
        }

        private void lowGradeFuelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lowgradefuel", 500);
        }

        private void metalFragmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.fragments", 1000);
        }

        private void metalOreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.ore", 1000);
        }

        private void highQualityMetalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.refined", 100);
        }

        private void plantFiberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "plantfiber", 1000);
        }

        private void scrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scrap", 1000);
        }

        private void wolfSkuullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "skull.wolf", 1000);
        }

        private void stoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stones", 1000);
        }

        private void sulfurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sulfur", 1000);
        }

        private void sulfurOreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sulfur.ore", 1000);
        }

        private void targetingComputerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "targeting.computer");
        }

        private void woodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood", 1000);
        }

        private void vestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.vest");
        }

        private void skirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.skirt");
        }

        private void ponchoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.poncho");
        }

        private void pantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.pants");
        }

        private void halterneckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.helterneck");
        }

        private void bootsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.boots");
        }

        private void boneArmourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bone.armor.suit");
        }

        private void frogBootsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "boots.frog");
        }

        private void buucketHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bucket.helmet");
        }

        private void glovesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.gloves");
        }

        private void headWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.headwrap");
        }

        private void shirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.shirt");
        }

        private void shoesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.shoes");
        }

        private void trousersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.trousers");
        }

        private void coffeeCanHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "coffeecan.helmet");
        }

        private void boneHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deer.skull.mask");
        }

        private void finsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.fins");
        }

        private void maskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.mask");
        }

        private void tankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.tank");
        }

        private void wetSuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.wetsuit");
        }

        private void beenieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.beenie");
        }

        private void boonieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.boonie");
        }

        private void candleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.candle");
        }

        private void minerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.miner");
        }

        private void wolfHedressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.wolf");
        }

        private void hazmatSuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hazmatsuit");
        }

        private void helmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "heavy.plate.helmet");
        }

        private void jacketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "heavy.plate.jacket");
        }

        private void pantsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "heavy.plate.pants");
        }

        private void hoodieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hoodie");
        }

        private void roadsignArmorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.armor.roadsign");
        }

        private void woodenArmorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.armor.wood");
        }

        private void saddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.saddle");
        }

        private void doubleSaddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.saddle.double");
        }

        private void saddleBagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.saddlebag");
        }

        private void basicShoesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.shoes.basic");
        }

        private void highQualityShoesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.shoes.advanced");
        }

        private void jacketToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jacket");
        }

        private void snowJacketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jacket.snow");
        }

        private void lumberjackHoodieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lumberjack hoodie");
        }

        private void balaclavaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mask.balaclava");
        }

        private void bandanaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mask.bandana");
        }

        private void metalFacemaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.facemask");
        }

        private void metalChestPlateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.plate.torso");
        }

        private void pantsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pants");
        }

        private void shortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pants.shorts");
        }

        private void riotHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "riot.helmet");
        }

        private void glovesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsign.gloves");
        }

        private void jacketToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsign.jacket");
        }

        private void kiltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsign.kilt");
        }

        private void shirtToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shirt.collared");
        }

        private void bootsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shoes.boots");
        }

        private void tacticalGlovesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tactical.gloves");
        }

        private void tShirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tshirt.long");
        }

        private void longsleeveTShirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tshirt");
        }

        private void tankTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shirt.tanktop");
        }

        private void helmetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood.armor.helmet");
        }

        private void jacketToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood.armor.jacket");
        }

        private void pantsToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood.armor.pants");
        }

        private void salvagedAceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "axe.salvaged");
        }

        private void waterBuucketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bucket.water");
        }

        private void chainsawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chainsaw");
        }

        private void satchelChargeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "explosive.satchel");
        }

        private void timedExplosiveChargeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "explosive.timed");
        }

        private void fishingRodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fishingrod.handmade");
        }

        private void flareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flare");
        }

        private void flashlightToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flashlight.held");
        }

        private void hammerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hammer");
        }

        private void salvagedHammerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hammer.salvaged");
        }

        private void hatchetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hatchet");
        }

        private void salvagedIcepickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "icepick.salvaged");
        }

        private void jackhammerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jackhammer");
        }

        private void pickaxeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pickaxe");
        }

        private void rFTransmitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rf.detonator");
        }

        private void rockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rock");
        }

        private void stonePickaxeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stone.pickace");
        }

        private void stoneHatchetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stonehatchet");
        }

        private void suupplySignalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "supply.signal");
        }

        private void binocuularsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tool.binoculars");
        }

        private void torchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "torch");
        }

        private void antiRadiationPillsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "antiradpills");
        }

        private void bandageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bandage");
        }

        private void largeMedKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "largemedkit");
        }

        private void medicalSyringeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "syringe.medical");
        }

        private void blueBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.blue.berry");
        }

        private void cornCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.corn");
        }

        private void greenBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.green.berry");
        }

        private void hedmpCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.hemp");
        }

        private void potatoCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.potato");
        }

        private void pumpkinCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.pumpkin");
        }

        private void redBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.red.berry");
        }

        private void whiteBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.white.berry");
        }

        private void yellowBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.yellow.berry");
        }

        private void blueBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.blue.berry");
        }

        private void cornToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.corn");
        }

        private void greenBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.green.berry");
        }

        private void hempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.hemp");
        }

        private void potatoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.potato");
        }

        private void pumpkinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.pumpkin");
        }

        private void redBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.red.berry");
        }

        private void whiteBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.white.berry");
        }

        private void ywllowBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.yellow.berry");
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "radiationresisttea");
        }

        private void advancedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "radiationresisttea.advanced");
        }

        private void pureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "radiationresisttea.pure");
        }

        private void basicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "oretea");
        }

        private void advancedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "oretea.advanced");
        }

        private void puureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "oretea.pure");
        }

        private void basicToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scraptea");
        }

        private void advancedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scraptea.advanced");
        }

        private void pureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scraptea.pure");
        }

        private void basicToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "woodtea");
        }

        private void advancedToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "woodtea.advanced");
        }

        private void pureToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "woodtea.pure");
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "red.berry");
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "green.berry");
        }

        private void ywllowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "yellow.berry");
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "blue.berry");
        }

        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "white.berry");
        }

        private void rawToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wolfmeat.raw");
        }

        private void cookedToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wolfmeat.cooked");
        }

        private void burnedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wolfmeat.burned");
        }

        private void rawToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "meat.boar");
        }

        private void cookedToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "meat.pork.cooked");
        }

        private void buurnedToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "meat.pork.burned");
        }

        private void rawToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "humanmeat.raw");
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "humanmeat.cooked");
        }

        private void buurnedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "humanmeat.burned");
        }


        private void toolStripMenuItem6_Click_1(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsemeat.cooked");
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsemeat.raw");
        }

        private void buurnedToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsemeat.burned");
        }

        private void rawToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deermeat.raw");
        }

        private void cookedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deermeat.cooked");
        }

        private void buurnedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deermeat.burned");
        }

        private void rawToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chicken.raw");
        }

        private void cookedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chicken.cooked");
        }

        private void burnedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chicken.burned");
        }

        private void buurnedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bearmeat.burned");
        }

        private void cookedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bearmeat.cooked");
        }

        private void rawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bearmeat");
        }

        private void anchovyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.anchovy");
        }

        private void catfishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.catfish");
        }

        private void cookedToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.cooked");
        }

        private void herringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.herring");
        }

        private void minnorwsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.minnows");
        }

        private void rawToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.raw");
        }

        private void salmonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.salmon");
        }

        private void sardineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.sardine");
        }

        private void smallSharkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.smallshark");
        }

        private void smallTrouutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.troutsmall");
        }

        private void ywlloePerchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.yellowperch");
        }

        private void basicToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "healingtea");
        }

        private void advancedToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "healingtea.advanced");
        }

        private void puureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "healingtea.pure");
        }

        private void basicToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "maxhealthtea");
        }

        private void advancedToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "maxhealthtea.advanced");
        }

        private void pureToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "maxhealthtea.pure");
        }

        private void appleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "apple");
        }

        private void blackRaspbeerriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "black.raspberries");
        }

        private void bluueBerriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "blueberries");
        }

        private void cactusFleshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cactusflesh");
        }

        private void canOfBeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.beans");
        }

        private void canOfTuunaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.tuna");
        }

        private void chocholateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chocholate");
        }

        private void cornToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "corn");
        }

        private void granolaBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "granolabar");
        }

        private void grubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grub");
        }

        private void picklesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jar.pickle");
        }

        private void mushroomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mushroom");
        }

        private void potatoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "potato");
        }

        private void pumpkinToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pumpkin");
        }

        private void smallWaterBottleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smallwaterbottle");
        }

        private void waterJugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "waterjug");
        }

        private void wormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "worm");
        }

        private void gLBuckshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.grenadelauncher.buckshot");
        }

        private void hEGrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.grenadelauncher.he");
        }

        private void mmSmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.grenadelauncher.smoke");
        }

        private void handmadeShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.handmade.shell");
        }

        private void nailguunNailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.nailgun.nails");
        }

        private void pistolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.pistol");
        }

        private void incendiaryPistolBulletToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.pistol.fire");
        }

        private void highVelocityPistolBluuuetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.pistol.hv");
        }

        private void rifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle");
        }

        private void explosive556RifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle.explosive");
        }

        private void hV556RifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle.hv");
        }

        private void inccccccendiary556RifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle.incendiary");
        }

        private void rocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rocket.basic");
        }

        private void incendiaryRocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rocket.fire");
        }

        private void highVelocityRocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rocket.hv");
        }

        private void guageBuckshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.shotgun");
        }

        private void guuageIncendiaryShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.shotgun.fire");
        }

        private void guageSluugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.shotgun.slug");
        }

        private void boneArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.bone");
        }

        private void fireArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.fire");
        }

        private void highVelocityArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.hv");
        }

        private void woodenArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.wooden");
        }

        private void speargunSpearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "speargun.spear");
        }

        private void flameTurretToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flameturret");
        }

        private void shotgunTrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "guntrap");
        }

        private void woodenFloorSpikesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "spikes.floor");
        }

        private void snapTrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "trap.bear");
        }

        private void homemadeLandmineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "trap.landmine");
        }

        private void redToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "keycard_red");
        }

        private void greenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "keycard_green");
        }

        private void blueToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "keycard_blue");
        }

        private void doorKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.key");
        }

        private void guitarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fun.guitar");
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "note");
        }

        private void fuseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fuse");
        }

        private void gearsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gears");
        }

        private void metalBladeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metalblade");
        }

        private void metalPipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metalpipe");
        }

        private void metalSpringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metalspring");
        }

        private void propaneTankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "propanetank");
        }

        private void rifleBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "riflebody");
        }

        private void roadsignToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsigns");
        }

        private void ropeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rope");
        }

        private void semiAutomaticBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "semibody");
        }

        private void sewingKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sewingkit");
        }

        private void sheetMetalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sheetmetal");
        }

        private void sMGBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smgbody");
        }

        private void tarpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tarp");
        }

        private void techTrshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "techparts");
        }

        private void auutoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "autoturret");
        }

        private void ceilingLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ceilinglight");
        }

        private void computerStationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "computerstation");
        }

        private void andSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.andswitch");
        }

        private void audioAlarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.audioalarm");
        }

        private void largeBatteryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.battery.rechargeable.large");
        }

        private void mediumBatteryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.battery.rechargeable.medium");
        }

        private void smallBatteryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.battery.rechargeable.small");
        }

        private void blockerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.blocker");
        }

        private void buuttonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.button");
        }

        private void couunterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.counter");
        }

        private void doorControllerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.doorcontroller");
        }

        private void flasherLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.flasherlight");
        }

        private void smallGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.fuelgenerator.small");
        }

        private void hBHFSensorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.hbhfsensor");
        }

        private void heaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.heater");
        }

        private void igniterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.igniter");
        }

        private void laserDetectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.laserdetector");
        }

        private void oRSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.orswitch");
        }

        private void pressurePadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.pressurepad");
        }

        private void randSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.random.switch");
        }

        private void rFBroadcasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.rf.broadcaster");
        }

        private void rFReceiverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.rf.receiver");
        }

        private void sirenLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.sirenlight");
        }

        private void largeSolarPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.solarpanel.large");
        }

        private void splitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.splitter");
        }

        private void sprinklerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.sprinkler");
        }

        private void switchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.switch");
        }

        private void teslaCoilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.teslacoil");
        }

        private void timerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.timer");
        }

        private void xORSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.xorswitch");
        }

        private void branchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electrical.branch");
        }

        private void combinerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electrical.combiner");
        }

        private void memoryCellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electrical.memorycell");
        }

        private void elevatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "elevator");
        }

        private void fluidCombinerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fluid.combiner");
        }

        private void fluidSplitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fluid.splitter");
        }

        private void fluidSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fluid.switch");
        }

        private void windTurbineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "generator.wind.scrap");
        }

        private void hoseToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hosetool");
        }

        private void poweredWaterPurifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "powered.water.purifier");
        }

        private void rFPagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rf_pager");
        }

        private void searchLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "searchlight");
        }

        private void storageMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "storage.monitor");
        }

        private void reactiveTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "target.reactive");
        }

        private void waterPuumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "waterpump");
        }

        private void wireToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wiretool");
        }

        private void teleportToYouToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.InGameName))
            {
                teleport_here(GetFromDT(1));
            }
            else
                XtraMessageBox.Show(string.Format("Cant Teleport To {0} As You Have Not Set Your In Game Name!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void teleporToThemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.InGameName))
            {
                teleport_to(GetFromDT(1));
            }
            else
                XtraMessageBox.Show(string.Format("Cant Teleport To {0} As You Have Not Set Your In Game Name!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void hazzyMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hazmatsuit");
            give_item_to_player(GetFromDT(1), "smg.mp5");
            give_item_to_player(GetFromDT(1), "weapon.mod.holosight");
            give_item_to_player(GetFromDT(1), "weapon.mod.lasersight");
            give_item_to_player(GetFromDT(1), "ammo.pistol", 128);
            give_item_to_player(GetFromDT(1), "syringe.medical", 6);
            give_item_to_player(GetFromDT(1), "bandage", 10);
            give_item_to_player(GetFromDT(1), "jackhammer");
            give_item_to_player(GetFromDT(1), "chainsaw");
            give_item_to_player(GetFromDT(1), "lowgradefuel", 50);
        }

        private void fullKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.ak");
            give_item_to_player(GetFromDT(1), "weapon.mod.holosight");
            give_item_to_player(GetFromDT(1), "weapon.mod.lasersight");
            give_item_to_player(GetFromDT(1), "ammo.rifle", 128);
            give_item_to_player(GetFromDT(1), "syringe.medical", 6);
            give_item_to_player(GetFromDT(1), "bandage", 10);
            give_item_to_player(GetFromDT(1), "metal.facemask");
            give_item_to_player(GetFromDT(1), "metal.plate.torso");
            give_item_to_player(GetFromDT(1), "tactical.gloves");
            give_item_to_player(GetFromDT(1), "pants");
            give_item_to_player(GetFromDT(1), "hoodie");
            give_item_to_player(GetFromDT(1), "roadsign.kilt");
            give_item_to_player(GetFromDT(1), "shoes.boots");
            give_item_to_player(GetFromDT(1), "jackhammer");
            give_item_to_player(GetFromDT(1), "chainsaw");
            give_item_to_player(GetFromDT(1), "lowgradefuel", 50);
        }
    }
}
