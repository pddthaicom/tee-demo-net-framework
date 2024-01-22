using EpgImport.Entity.WebService;
using EpgImport.Utils;
using MySql.Data.MySqlClient;
using SQLHelp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;

namespace EpgImport
{
	/// <summary>
	/// EPG Web Service
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class EPGService : System.Web.Services.WebService
	{
        /*
		/// <summary>
		/// Get Channel List
		/// </summary>
		/// <param name="providerId">Provider Id</param>
		/// <returns>Channel List</returns>
		[WebMethod]
		public Channel[] GetChannelList(int providerId)
		{
			List<Channel> list = new List<Channel>();
			SqlCommand cmd = Database.GetSqlCommand();
			DataTable dt = new DataTable();

			cmd.CommandText = @"SELECT ord.channel_no, ord.channel_id, c.nam_channel_th channel_name, c.img_url
				FROM
					mst_channel_order ord INNER JOIN 
					main_channel c ON ord.channel_id = c.channel_id
				WHERE
					ord.provider_id = @provider_id AND
					ord.sta_RecStatus = 'A'
				ORDER BY ord.channel_no";
			cmd.Parameters.Add("@provider_id", SqlDbType.Int).Value = providerId;
			cmd.Connection.Open();
			new SqlDataAdapter(cmd).Fill(dt);
			cmd.Connection.Close();
			foreach (DataRow dr in dt.Rows)
			{
				Channel item = new Channel();

				item.ChannelId = (long)dr["channel_id"];
				item.ChannelNo = (int)dr["channel_no"];
				item.ChannelName = (string)dr["channel_name"];
				item.ChannelLogo = dr["img_url"].ToString();
				list.Add(item);
			}
			return list.ToArray();
		}

		/// <summary>
		/// Get Epg List
		/// </summary>
		/// <param name="providerId">Provider Id</param>
		/// <returns>Epg List</returns>
		[WebMethod]
		public Epg[] GetEpgList(int providerId)
		{
			List<Epg> list = new List<Epg>();
			SqlCommand cmd = Database.GetSqlCommand();
			DataTable dt = new DataTable();

			cmd.CommandText = @"SELECT c.channel_id, c.nam_channel_en channel_name, epg.program_id, epg.epg_id, epg.nam_epg_th title_th, epg.nam_epg_en title_en, epg.desc_epg_th desc_th, epg.desc_epg_en desc_en, epg.epg_start start_time, epg.epg_stop stop_time
					FROM
						mst_channel_order ord INNER JOIN
						main_channel c ON ord.channel_id = c.channel_id INNER JOIN
						main_epg epg ON c.channel_id = epg.channel_id
					WHERE
						ord.provider_id = @provider_id AND
						ord.sta_RecStatus = 'A' AND
						epg.sta_RecStatus = 'A' AND
						epg.epg_stop > CAST(CURRENT_TIMESTAMP AS date) AND
						epg.epg_start < DATEADD(DAY, @day, CAST(CURRENT_TIMESTAMP AS date))
					ORDER BY ord.channel_no, epg.epg_start";
			cmd.Parameters.Add("@provider_id", SqlDbType.Int).Value = providerId;
			cmd.Parameters.Add("@day", SqlDbType.Int).Value = 7;
			cmd.Connection.Open();
			new SqlDataAdapter(cmd).Fill(dt);
			cmd.Connection.Close();
			foreach (DataRow dr in dt.Rows)
			{
				Epg item = new Epg();

				item.ChannelId = (int)dr["channel_id"];
				item.ChannelName = (string)dr["channel_name"];
				item.ProgramId = (long)dr["program_id"];
				item.EpgId = (long)dr["epg_id"];
				item.TitleEN = (string)dr["title_en"];
				item.TitleTH = (string)dr["title_th"];
				item.DescEN = (string)dr["desc_en"];
				item.DescTH = (string)dr["desc_th"];
				item.DateStart = (DateTime)dr["start_time"];
				item.DateStop = (DateTime)dr["stop_time"];
				list.Add(item);
			}

			return list.ToArray();
		}

        /// <summary>
		/// Get Epg List By Channel ID
		/// </summary>
		/// <param name="providerId">Provider Id</param>
		/// <returns>Epg List</returns>
		[WebMethod]
        public Epg[] GetEpgListByChannelId(int providerId,int channelId)
        {
            List<Epg> list = new List<Epg>();
            SqlCommand cmd = Database.GetSqlCommand();
            DataTable dt = new DataTable();

            cmd.CommandText = @"SELECT c.channel_id, c.nam_channel_en channel_name, epg.program_id, epg.epg_id, epg.nam_epg_th title_th, epg.nam_epg_en title_en, epg.desc_epg_th desc_th, epg.desc_epg_en desc_en, epg.epg_start start_time, epg.epg_stop stop_time
					FROM
						mst_channel_order ord INNER JOIN
						main_channel c ON ord.channel_id = c.channel_id INNER JOIN
						main_epg epg ON c.channel_id = epg.channel_id
					WHERE
						ord.provider_id = @provider_id AND
	                    ord.channel_id = @channel_id AND
						ord.sta_RecStatus = 'A' AND
						epg.sta_RecStatus = 'A' AND
						epg.epg_stop > CAST(CURRENT_TIMESTAMP AS date) AND
						epg.epg_start < DATEADD(DAY, @day, CAST(CURRENT_TIMESTAMP AS date))
					ORDER BY ord.channel_no, epg.epg_start";
            cmd.Parameters.Add("@provider_id", SqlDbType.Int).Value = providerId;
            cmd.Parameters.Add("@channel_id", SqlDbType.Int).Value = channelId;
            cmd.Parameters.Add("@day", SqlDbType.Int).Value = 7;
            cmd.Connection.Open();
            new SqlDataAdapter(cmd).Fill(dt);
            cmd.Connection.Close();
            foreach (DataRow dr in dt.Rows)
            {
                Epg item = new Epg();

                item.ChannelId = (int)dr["channel_id"];
                item.ChannelName = (string)dr["channel_name"];
                item.ProgramId = (long)dr["program_id"];
                item.EpgId = (long)dr["epg_id"];
                item.TitleEN = (string)dr["title_en"];
                item.TitleTH = (string)dr["title_th"];
                item.DescEN = (string)dr["desc_en"];
                item.DescTH = (string)dr["desc_th"];
                item.DateStart = (DateTime)dr["start_time"];
                item.DateStop = (DateTime)dr["stop_time"];
                list.Add(item);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get Epg List By Date
        /// </summary>
        /// <param name="providerId">Provider Id</param>
        /// <returns>Epg List</returns>
        [WebMethod]
        public Epg[] GetEpgListByDate(int providerId,string startDate,string stopDate)
        {
            List<Epg> list = new List<Epg>();
            SqlCommand cmd = Database.GetSqlCommand();
            DataTable dt = new DataTable();

            DateTime startDateTime = Convert.ToDateTime(startDate);
            DateTime stopDateTime = Convert.ToDateTime(stopDate);

            cmd.CommandText = @"SELECT c.channel_id, c.nam_channel_en channel_name, epg.program_id, epg.epg_id, epg.nam_epg_th title_th, epg.nam_epg_en title_en, epg.desc_epg_th desc_th, epg.desc_epg_en desc_en, epg.epg_start start_time, epg.epg_stop stop_time
					FROM
						mst_channel_order ord INNER JOIN
						main_channel c ON ord.channel_id = c.channel_id INNER JOIN
						main_epg epg ON c.channel_id = epg.channel_id
					WHERE
						ord.provider_id = @provider_id AND
						ord.sta_RecStatus = 'A' AND
						epg.sta_RecStatus = 'A' AND
						epg.epg_stop > CAST(convert(datetime, '" + startDate + @"', 120) AS date) AND
						epg.epg_start < DATEADD(DAY, @day, CAST(convert(datetime, '" + stopDate + @"', 120) AS date))
					ORDER BY ord.channel_no, epg.epg_start";
            cmd.Parameters.Add("@provider_id", SqlDbType.Int).Value = providerId;
            cmd.Parameters.Add("@day", SqlDbType.Int).Value = 7;
            cmd.Connection.Open();
            new SqlDataAdapter(cmd).Fill(dt);
            cmd.Connection.Close();
            foreach (DataRow dr in dt.Rows)
            {
                Epg item = new Epg();

                item.ChannelId = (int)dr["channel_id"];
                item.ChannelName = (string)dr["channel_name"];
                item.ProgramId = (long)dr["program_id"];
                item.EpgId = (long)dr["epg_id"];
                item.TitleEN = (string)dr["title_en"];
                item.TitleTH = (string)dr["title_th"];
                item.DescEN = (string)dr["desc_en"];
                item.DescTH = (string)dr["desc_th"];
                item.DateStart = (DateTime)dr["start_time"];
                item.DateStop = (DateTime)dr["stop_time"];
                list.Add(item);
            }

            return list.ToArray();
        }
        */

        /// <summary>
		/// Get Epg List
		/// </summary>
		/// <param name="providerId">Provider Id</param>
		/// <returns>Epg List</returns>
		[WebMethod]
        public Epg[] GetEpgList(int providerId)
        {
            List<Epg> list = new List<Epg>();
            DataTable dt = new DataTable();

            try
            {
                SQLHelper sqlHelper = new SQLHelper(ConfigurationManager.ConnectionStrings["epgdb"].ConnectionString);

                MySqlCommand cmd = new MySqlCommand("usp_get_episode_by_provider");
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@id_provider", MySqlDbType.Int32).Value = providerId;
                cmd.Parameters.Add("@add_days", MySqlDbType.Int32).Value = 7;
                cmd.Parameters.Add("@o_return", MySqlDbType.Int32).Direction = ParameterDirection.Output;

                dt = sqlHelper.ExecuteDataTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    Epg item = new Epg();

                    item.ChannelId = Convert.ToInt32(dr["ChannelId"]);
                    item.ChannelName = Convert.ToString(dr["ChannelName"]);
                    item.ProgramId = Convert.ToInt32(dr["ProgramId"]);
                    item.EpgId = Convert.ToInt64(dr["EpgId"]);
                    item.TitleEN = Convert.ToString(dr["TitleEN"]);
                    item.TitleTH = Convert.ToString(dr["TitleTH"]);
                    item.DescEN = Convert.ToString(dr["DescEN"]);
                    item.DescTH = Convert.ToString(dr["DescTH"]);
                    item.DateStart = Convert.ToDateTime(dr["DateStart"]);
                    item.DateStop = Convert.ToDateTime(dr["DateStop"]);
                    list.Add(item);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list.ToArray();
        }


        /// <summary>
		/// Get Epg List By Channel ID
		/// </summary>
		/// <param name="providerId">Provider Id</param>
		/// <returns>Epg List</returns>
		[WebMethod]
        public Epg[] GetEpgListByChannelId(int providerId, int channelId)
        {
            List<Epg> list = new List<Epg>();
            DataTable dt = new DataTable();

            try
            {
                SQLHelper sqlHelper = new SQLHelper(ConfigurationManager.ConnectionStrings["epgdb"].ConnectionString);

                MySqlCommand cmd = new MySqlCommand("usp_get_episode_by_provider_and_channel");
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@id_provider", MySqlDbType.Int32).Value = providerId;
                cmd.Parameters.Add("@id_channel", MySqlDbType.Int32).Value = channelId;
                cmd.Parameters.Add("@add_days", MySqlDbType.Int32).Value = 7;
                cmd.Parameters.Add("@o_return", MySqlDbType.Int32).Direction = ParameterDirection.Output;

                dt = sqlHelper.ExecuteDataTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    Epg item = new Epg();

                    item.ChannelId = Convert.ToInt32(dr["ChannelId"]);
                    item.ChannelName = Convert.ToString(dr["ChannelName"]);
                    item.ProgramId = Convert.ToInt32(dr["ProgramId"]);
                    item.EpgId = Convert.ToInt64(dr["EpgId"]);
                    item.TitleEN = Convert.ToString(dr["TitleEN"]);
                    item.TitleTH = Convert.ToString(dr["TitleTH"]);
                    item.DescEN = Convert.ToString(dr["DescEN"]);
                    item.DescTH = Convert.ToString(dr["DescTH"]);
                    item.DateStart = Convert.ToDateTime(dr["DateStart"]);
                    item.DateStop = Convert.ToDateTime(dr["DateStop"]);
                    list.Add(item);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list.ToArray();
        }


        /// <summary>
		/// Get Channel List
		/// </summary>
		/// <param name="providerId">Provider Id</param>
		/// <returns>Channel List</returns>
		[WebMethod]
        public Channel[] GetChannelList(int providerId)
        {
            List<Channel> list = new List<Channel>();
            DataTable dt = new DataTable();

            try
            {
                SQLHelper sqlHelper = new SQLHelper(ConfigurationManager.ConnectionStrings["epgdb"].ConnectionString);

                MySqlCommand cmd = new MySqlCommand("usp_get_channel_list_by_provider_id");
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@id_provider", MySqlDbType.Int32).Value = providerId;
                cmd.Parameters.Add("@o_return", MySqlDbType.Int32).Direction = ParameterDirection.Output;

                dt = sqlHelper.ExecuteDataTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    Channel item = new Channel();

                    item.ChannelId = Convert.ToInt32(dr["id_channel"]);
                    item.ChannelNo = Convert.ToInt32(dr["no_channel"]);
                    item.ChannelName = Convert.ToString(dr["name_channel_th"]);
                    item.ChannelLogo = Convert.ToString(dr["image_url"]);
                    list.Add(item);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get Epg List By Date
        /// </summary>
        /// <param name="providerId">Provider Id</param>
        /// <returns>Epg List</returns>
        [WebMethod]
        public Epg[] GetEpgListByDate(int providerId, string startDate, string stopDate)
        {
            List<Epg> list = new List<Epg>();
            DataTable dt = new DataTable();

            DateTime startDateTime = Convert.ToDateTime(startDate);
            DateTime stopDateTime = Convert.ToDateTime(stopDate);

            try
            {
                SQLHelper sqlHelper = new SQLHelper(ConfigurationManager.ConnectionStrings["epgdb"].ConnectionString);

                MySqlCommand cmd = new MySqlCommand("usp_get_episode_by_provider_and_startstop");
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@id_provider", MySqlDbType.Int32).Value = providerId;
                cmd.Parameters.Add("@episode_start", MySqlDbType.DateTime).Value = startDateTime;
                cmd.Parameters.Add("@episode_stop", MySqlDbType.DateTime).Value = stopDateTime; 
                cmd.Parameters.Add("@o_return", MySqlDbType.Int32).Direction = ParameterDirection.Output;

                dt = sqlHelper.ExecuteDataTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    Epg item = new Epg();

                    item.ChannelId = Convert.ToInt32(dr["id_channel"]);
                    item.ChannelName = Convert.ToString(dr["name_channel_th"]);
                    item.ProgramId = Convert.ToInt32(dr["id_program"]);
                    item.EpgId = Convert.ToInt64(dr["id_episode"]);
                    item.TitleEN = Convert.ToString(dr["name_episode_en"]);
                    item.TitleTH = Convert.ToString(dr["name_episode_th"]);
                    item.DescEN = Convert.ToString(dr["desc_episode_en"]);
                    item.DescTH = Convert.ToString(dr["desc_episode_th"]);
                    item.DateStart = Convert.ToDateTime(dr["episode_start"]);
                    item.DateStop = Convert.ToDateTime(dr["episode_stop"]);
                    list.Add(item);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list.ToArray();
        }
    }
}
