using EpgImport.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EpgImport
{
	/// <summary>
	/// Multi Screen Data Exporter
	/// </summary>
	public partial class MultiScreenExport : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(Request["auto"]))
			{
				ProcessSyncToMultiScreenDatabase();
			}
		}

		/// <summary>
		/// Sync EPG Data to Multi Screen Database
		/// </summary>
		/// <remarks>
		/// Sync EPG data (channel, program, episode) and save into Multi Screen Database
		/// 
		/// Changelog:
		/// 2014-10-28 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		protected void btnSync_Click(object sender, EventArgs e)
		{
			ProcessSyncToMultiScreenDatabase();
		}

		/// <summary>
		/// Sync EPG Data to Multi Screen Database
		/// </summary>
		/// <remarks>
		/// Sync EPG data (channel, program, episode) and save into Multi Screen Database
		/// 1) Import Main Channel into MultiScreen Channel
		/// 2) Sync Main Program with MultiScreen Program (Update Program Reference)
		/// 3) Import Main Program into MultiScreen Program
		/// 4) Import Main EPG with MultiScreen EPG
		/// 5) Sync Main EPG with MultiScreen EPG
		/// 
		/// Changelog:
		/// 2014-10-29 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private void ProcessSyncToMultiScreenDatabase()
		{
			int count = 0;
			SqlCommand cmd = Database.GetSqlCommand("MS");

			#region Sync Data to Multi Screen Database
			lblStatus.Text = "";
			pnlStatus.Update();
			cmd.Connection.Open();
			#region 1) Import Main Channel into MultiScreen Channel
			cmd.CommandText = @"INSERT INTO channel(nam_channel, txt_detail, flg_showchat, status, cod_upd, dtm_upd, id_ref)
				SELECT LTRIM(RTRIM(nam_channel_th)) nam_channel, LTRIM(RTRIM(nam_channel_en)) txt_detail, 'N' flg_showchat, 'A' status, '1' cod_upd, CURRENT_TIMESTAMP dtm_upd, mc.channel_id id_ref
				FROM
					EPGSERVER.DTVEpg.dbo.main_channel mc
				WHERE
					mc.channel_id NOT IN (SELECT id_ref FROM channel WHERE id_ref IS NOT NULL) AND
					mc.sta_RecStatus = 'A'";
			try
			{
				count = cmd.ExecuteNonQuery();
				lblStatus.Text += "1) Import channel successfully " + count + " record(s)<br>";
			}
			catch (Exception ex)
			{
				lblStatus.Text += "1) Error: Import channel " + ex.Message + "<br>";
			}
			pnlStatus.Update();
			#endregion

			#region 2) Sync Main Program with MultiScreen Program (Update Program Reference)
			cmd.CommandText = @"UPDATE p SET p.id_ref = mp.program_id, p.dtm_upd = CURRENT_TIMESTAMP
				FROM
					EPGSERVER.DTVEpg.dbo.main_program mp INNER JOIN
					channel c ON mp.channel_id = c.id_ref INNER JOIN
					program p ON c.id_channel = p.id_channel AND p.id_ref IS NULL AND (REPLACE(REPLACE(p.nam_program, '-', ''), ' ', '') COLLATE Thai_CI_AS = REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') COLLATE Thai_CI_AS OR REPLACE(REPLACE(p.nam_program, '-', ''), ' ', '') COLLATE Thai_CI_AS = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') COLLATE Thai_CI_AS)
				WHERE
					mp.program_id NOT IN (SELECT id_ref FROM program WHERE id_ref IS NOT NULL) AND
					mp.sta_RecStatus = 'A'";
			try
			{
				count = cmd.ExecuteNonQuery();
				lblStatus.Text += "2) Sync program reference successfully " + count + " record(s)<br>";
			}
			catch (Exception ex)
			{
				lblStatus.Text += "2) Sync program reference : Error = " + ex.Message + "<br>";
			}
			pnlStatus.Update();
			#endregion

			#region 3) Import Main Program into MultiScreen Program
			cmd.CommandText = @"INSERT INTO program(id_channel, nam_program, txt_detail, url_image, url_image2, status, cod_upd, dtm_upd, id_ref)
				SELECT c.id_channel, LTRIM(RTRIM(mp.nam_program_th)), LTRIM(RTRIM(mp.desc_program_th)), '', '', 'A', '1', CURRENT_TIMESTAMP, mp.program_id
				FROM
					EPGSERVER.DTVEpg.dbo.main_program mp INNER JOIN
					channel c ON mp.channel_id = c.id_ref
				WHERE
					mp.program_id NOT IN (SELECT id_ref FROM program WHERE id_ref IS NOT NULL)";
			try
			{
				count = cmd.ExecuteNonQuery();
				lblStatus.Text += "3) Import program successfully " + count + " record(s)<br>";
			}
			catch (Exception ex)
			{
				lblStatus.Text += "3) Import program : Error = " + ex.Message + "<br>";
			}
			pnlStatus.Update();
			#endregion

			#region 4) Import Main EPG with MultiScreen EPG
			cmd.CommandText = @"INSERT INTO episode(id_channel, id_program, nam_episode, txt_detail, url_image, url_image2, url_image_mobile, url_image_thumb, dtm_from, dtm_to, status, dtm_upd, cod_upd, id_ref)
				SELECT c.id_channel, p.id_program, me.nam_epg_th, me.desc_epg_th, '', '', '', '', me.epg_start, me.epg_stop, me.sta_RecStatus, CURRENT_TIMESTAMP, '1', me.epg_id
				FROM
					EPGSERVER.DTVEpg.dbo.main_epg me INNER JOIN
					channel c ON me.channel_id = c.id_ref INNER JOIN
					program p ON me.program_id = p.id_ref
				WHERE
					me.epg_id NOT IN (SELECT id_ref FROM episode WHERE id_ref IS NOT NULL) AND
					me.sta_RecStatus = 'A'";
			try
			{
				count = cmd.ExecuteNonQuery();
				lblStatus.Text += "Import episode successfully " + count + " record(s)<br>";
			}
			catch (Exception ex)
			{
				lblStatus.Text += "Error: Import episode " + ex.Message + "<br>";
			}
			pnlStatus.Update();
			#endregion

			#region 5) Sync Main EPG with MultiScreen EPG
			cmd.CommandText = @"UPDATE e SET dtm_from = me.epg_start, dtm_to = me.epg_stop, status = me.sta_RecStatus, dtm_upd = CURRENT_TIMESTAMP
				FROM
					EPGSERVER.DTVEpg.dbo.main_epg me INNER JOIN
					episode e ON e.id_ref = me.epg_id
				WHERE
					me.dte_upd > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
					me.dte_upd <> me.dte_cre";
			try
			{
				count = cmd.ExecuteNonQuery();
				lblStatus.Text += "Sync episode successfully " + count + " record(s)<br>";
			}
			catch (Exception ex)
			{
				lblStatus.Text += "Error: Sync episode " + ex.Message + "<br>";
			}
			pnlStatus.Update();
			#endregion
			cmd.Connection.Close();
			#endregion
		}
	}
}