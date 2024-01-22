using EpgImport.Entity.Kapook;
using EpgImport.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;

namespace EpgImport
{
	/// <summary>
	/// EPG Data Importer (Kapook)
	/// </summary>
    public partial class KapookImporter : System.Web.UI.Page
    {
		#region Global Variables
		/// <summary>
		/// Web Client Instance
		/// </summary>
		private WebClient client = new WebClient();

		private List<Channel> listChannel = null;
		#endregion

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
			if (listChannel != null) listChannel.Clear();
			GetChannelInfo();
			GetProgramInfo();
        }

		/// <summary>
		/// Get Channel Information
		/// </summary>
		/// <remarks>
		/// Sync channel information from kapook and save into DTV EPG Database
		/// 
		/// Changelog:
		/// 2014-10-15 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		public void GetChannelInfo()
		{
			var data = client.DownloadString("http://sandbox.kapook.com/epg_api/?type=channel");
			listChannel = JsonConvert.DeserializeObject<List<Channel>>(data);
			SqlCommand cmd = Database.GetSqlCommand();

			if (listChannel == null || listChannel.Count == 0) return;
			#region Setup Sql Query (Insert Channel)
			cmd.CommandText = "kapook_channel_upsert";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("@channel_id", SqlDbType.VarChar);
			cmd.Parameters.Add("@nam_channel_th", SqlDbType.NVarChar);
			cmd.Parameters.Add("@nam_channel_en", SqlDbType.NVarChar);
			cmd.Parameters.Add("@cod_user", SqlDbType.NVarChar).Value = "epg_importer";
			cmd.Parameters.Add("@out_ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
			cmd.Parameters.Add("@out_ErrorMessage", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
			#endregion

			#region Read Channel List & Upsert data into database
			cmd.Connection.Open();
			foreach (Channel channel in listChannel)
			{
				string name_th = channel.ChannelNameTH;
				string name_en = channel.ChannelNameEN;

				if (String.IsNullOrEmpty(name_th)) name_th = name_en;
				if (String.IsNullOrEmpty(name_en)) name_en = name_th;

				cmd.Parameters["@channel_id"].Value = channel.ChannelMongoID.Trim();
				cmd.Parameters["@nam_channel_th"].Value = name_th.Trim();
				cmd.Parameters["@nam_channel_en"].Value = name_en.Trim();
				//cmd.ExecuteNonQuery();

				//Debug.WriteLine("GetChannelInfo::Channel:id={0},name_th={1},name_en={2}", new object[] { channel.ChannelMongoID.Trim(), name_th.Trim(), name_en.Trim() });
			}
			cmd.Connection.Close();
			#endregion
		}

		/// <summary>
		/// Get Program Information
		/// </summary>
		/// <remarks>
		/// Sync program information from kapook and save into DTV EPG Database
		/// 1) Read Channel List from Database
		/// 2) Get program information for each channel
		/// 3) Insert program information to database
		/// 
		/// Changelog:
		/// 2014-10-15 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		public void GetProgramInfo()
		{
			SqlCommand cmd = Database.GetSqlCommand();

			if (listChannel == null || listChannel.Count == 0) return;
			#region Setup Sql Query (Insert Channel)
			cmd.CommandText = "kapook_program_upsert";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("@program_id", SqlDbType.VarChar);
			cmd.Parameters.Add("@program_key_id", SqlDbType.VarChar);
			cmd.Parameters.Add("@channel_id", SqlDbType.VarChar);
			cmd.Parameters.Add("@nam_program_th", SqlDbType.NVarChar);
			cmd.Parameters.Add("@nam_program_en", SqlDbType.NVarChar);
			cmd.Parameters.Add("@onair_day", SqlDbType.NVarChar);
			cmd.Parameters.Add("@onair_time_from", SqlDbType.Time);
			cmd.Parameters.Add("@onair_time_to", SqlDbType.Time);
			cmd.Parameters.Add("@onair_date_from", SqlDbType.Date);
			cmd.Parameters.Add("@onair_date_to", SqlDbType.Date);
			cmd.Parameters.Add("@cod_user", SqlDbType.NVarChar).Value = "epg_importer";
			cmd.Parameters.Add("@out_ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
			cmd.Parameters.Add("@out_ErrorMessage", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
			#endregion

			#region Get Program List from each Channel
			foreach (Channel channel in listChannel)
			{
				var channelId = channel.ChannelMongoID.Trim();
				var data = client.DownloadString(String.Format("http://sandbox.kapook.com/epg_api/?type=program&channel_id={0}", channelId));
				var list = JsonConvert.DeserializeObject<List<Program>>(data);

				if (list == null || list.Count == 0) continue;
				cmd.Parameters["@channel_id"].Value = channelId;
				Debug.WriteLine("GetProgramInfo::Channel:id={0},name_en={1},name_th={2},count={3}", new object[] { channelId, channel.ChannelNameEN, channel.ChannelNameTH, list.Count });

				#region Read Channel List & Upsert data into database
				cmd.Connection.Open();
				foreach (Program program in list)
				{
					string name_th = program.ItemNameTH;
					string name_en = program.ItemNameEN;

					if (String.IsNullOrEmpty(name_th)) name_th = name_en;
					if (String.IsNullOrEmpty(name_en)) name_en = name_th;

					cmd.Parameters["@program_id"].Value = program.ItemMongoID.Trim();
					cmd.Parameters["@program_key_id"].Value = program.ItemKeyID.Trim();
					cmd.Parameters["@nam_program_th"].Value = name_th.Trim();
					cmd.Parameters["@nam_program_en"].Value = name_en.Trim();
					cmd.Parameters["@onair_day"].Value = program.ItemOnAirDay.Trim();
					cmd.Parameters["@onair_time_from"].Value = String.IsNullOrWhiteSpace(program.ItemOnAirFromTime) ? DBNull.Value : (object)program.ItemOnAirFromTime.Trim();
					cmd.Parameters["@onair_time_to"].Value = String.IsNullOrWhiteSpace(program.ItemOnAirToTime) ? DBNull.Value : (object)program.ItemOnAirToTime.Trim();
					cmd.Parameters["@onair_date_from"].Value = String.IsNullOrWhiteSpace(program.ItemOnAirFromDate) ? DBNull.Value : (object)program.ItemOnAirFromDate.Trim();
					cmd.Parameters["@onair_date_to"].Value = String.IsNullOrWhiteSpace(program.ItemOnAirToDate) ? DBNull.Value : (object)program.ItemOnAirToDate.Trim();
					cmd.ExecuteNonQuery();

					Debug.WriteLine("GetProgramInfo::Program:id={0},name_th={1},name_en={2}", new object[] { program.ItemMongoID.Trim(), name_th.Trim(), name_en.Trim() });
				}
				cmd.Connection.Close();
				#endregion
			}
			#endregion
		}
    }
}