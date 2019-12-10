/*
 * Convert.exe, Converts mission formats between TIE, XvT and XWA
 * Copyright (C) 2005- Michael Gaisser (mjgaisser@gmail.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL (License.txt) was not distributed
 * with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * VERSION: 1.5
 */

/* CHANGELOG
* v1.5, 190513
* [UPD] Source file split to create .Designer.cs
* [UPD] general cleaning
* [NEW] BoP functionality [JB]
* [UPD] TIE2XvT goal points only if goal exists [JB]
* [UPD] text individual char/byte writes replaced with byte arrays
* [FIX] offset in TIE2XvT EndOfMissionMessages [JB]
* [UPD] Read/WriteInt() replaced with BinaryReader/Writer calls, several variables subsequently changed from int to short
* [NEW] TIE questions converted to appropriate pre/post-mission text [JB]
* [NEW] TIE Global Bonus Goals converted to 2nd set of XvT Secondary goals [JB]
* [NEW] Combat Engagement capability [JB]
* [NEW] helper function to convert XvT Designations to XWA format [JB]
* [NEW] additional backdrop added to XWA missions [JB]
* [UPD] remove multiple player check for XWA Combat missions [JB]
* [NEW] XWA Order Speed [JB]
* [FIX] XWA order wait times adjusted due to different multipliers [JB]
* [UPD] added backdrops now randomized
* [FIX] corrected XWA message delay [JB]
* [UPD] XWA GlobalGoals now do all teams [JB]
* [FIX] offset error in XWA GG Prim T1
* [NEW] 2nd briefing converted to XWA [JB]
* [UPD] Briefings 3-8 properly skipped when converting MP missions [JB]
*/

using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Idmr.Conversions;
using Idmr.Conversions.Converters;

namespace Idmr.Converter
{
	/// <summary>
	/// X-wing series mission converter (TIE -> XvT, TIE -> XWA, XvT -> XWA)
	/// </summary>
	public partial class MainForm : System.Windows.Forms.Form
	{
		
		
		protected static XWingVsTieConverter XWingVsTieConverter { get; set; }
		public MainForm()
		{
			XWingVsTieConverter = new XWingVsTieConverter();

			InitializeComponent();
			
		}

		[STAThread]
		static void Main(string[] Args) 
		{
			Application.Run(new MainForm());
		}

		#region Check boxes
		void ChkXvT2CheckedChanged(object sender, EventArgs e)
		{
			chkXWA.Checked = !chkXvT2.Checked;
		}
		
		void ChkXWACheckedChanged(object sender, EventArgs e)
		{
			chkXvT2.Checked = !chkXWA.Checked;
			chkXvtBop.Visible = !chkXWA.Checked; //[JB] Updated to hide BoP checkbox.
		}
		#endregion

		#region buttons
		void CmdExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		void CmdExistClick(object sender, EventArgs e)
		{
			opnExist.ShowDialog();
		}
		
		void CmdSaveClick(object sender, EventArgs e)
		{
			savConvert.ShowDialog();
		}
		
		void OpnExistFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtExist.Text = opnExist.FileName;
			FileStream Test;
			Test = File.OpenRead(txtExist.Text);
			lblPlayer.Visible = false;
			optImp.Visible = false;
			optReb.Visible = false;
			chkXvtCombat.Visible = false; //[JB] Added
			chkXvtBop.Visible = false;
			int d = Test.ReadByte();
			switch (d)
			{
				case 255:
					lblType.Text = "TIE";
					chkXvT2.Enabled = true;
					chkXWA.Enabled = true;
					lblPlayer.Visible = true;
					optImp.Checked = true;
					optImp.Visible = true;
					optReb.Visible = true;
					chkXvtBop.Visible = chkXvT2.Checked;
					break;
				case 12:
					lblType.Text = "XvT";
					chkXvT2.Checked = false;
					chkXWA.Checked = true;
					chkXvT2.Enabled = false;
					chkXWA.Enabled = false;
					chkXvtCombat.Visible = true;  //[JB] Added
					break;
				case 14:
					lblType.Text = "BoP";
					chkXvT2.Checked = false;
					chkXWA.Checked = true;
					chkXvT2.Enabled = false;
					chkXWA.Enabled = false;
					chkXvtCombat.Visible = true;  //[JB] Added
					break;
				case 18:
					MessageBox.Show("Cannot convert existing XWA missions!", "Error");
					txtExist.Text = "";
					lblType.Text = "XWA";
					break;
				default:
					MessageBox.Show("Invalid file", "Error");
					txtExist.Text = "";
					lblType.Text = "";
					break;
			}
			Test.Close();
		}
		
		void SavConvertFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtSave.Text = savConvert.FileName;
		}
		
		void CmdConvertClick(object sender, EventArgs e)
		{
			var operationResultSuccess = false;
			if (txtExist.Text == "" | txtSave.Text == "") { return; }
			var fromFileName = txtExist.Text;
			var toFileName = txtSave.Text;

			if (lblType.Text == "TIE")
			{
				if (chkXvT2.Checked == true) { TIE2XvT(); }
				if (chkXWA.Checked == true) { TIE2XWA(txtExist.Text, txtSave.Text); }
			}
			if (lblType.Text == "XvT" || lblType.Text == "BoP") 
			{ 
				operationResultSuccess = XWingVsTieConverter.Convert(fromFileName, toFileName, Conversions.GameType.XWA);
			}

			MessageBox.Show("Conversion completed", "Finished");
		}
		#endregion


	}
}
