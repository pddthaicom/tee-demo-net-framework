using EpgSync.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EpgSync
{
	/// <summary>
	/// EPG Sync, Import from Satellite (Program, EPG) and Export to Multi Screen Database (Channel, Program, Episode)
	/// </summary>
	/// <remarks>
	/// Changelog:
	/// 2014-10-29 - Create (Woraphot Chokratanasombat)
	/// 2015-01-30 - Fix Duplicate Program in EPG
	/// 2015-02-04 - Add Warning in subject if no new epg inserted
	/// </remarks>
	class Program
	{
		private static TextWriter wLog = null;
		private static string log = "";
		private static Boolean isWarning = false;

		static void Main(string[] args)
		{
			using (StreamWriter w = File.AppendText("EpgSync.log"))
			{
				wLog = w;
				log = "";
				isWarning = false;
				Log("= EPG Sync : Start =");

				ProcessSyncSatelliteProgram();
				ProcessSyncSatelliteEPG();
				ProcessSyncToMultiScreenDatabase();
				ShowSyncStatistics();

				String subject = string.Format("{0}EPG Sync, Import and Export Batch Status", isWarning ? "[WARNING] " : "");
				SendEmail("woraphotc@thaicom.net;kanjanat@thaicom.net;pitchapornp@thaicom.net", subject, log);

				Log("= EPG Sync : End =" + Environment.NewLine + Environment.NewLine);
				wLog = null;
			}
		}

		/// <summary>
		/// Send Email
		/// </summary>
		/// <param name="email">Email List</param>
		/// <param name="title">Email Title</param>
		/// <param name="message">Email Message</param>
		/// <remarks>
		/// Changelog:
		/// 2014-10-30 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private static void SendEmail(string email, string title, string message)
		{
			string from = "noreply@thaicom.net";
			MailMessage msg = new MailMessage();
			msg.From = new MailAddress(from);
			msg.Subject = title;
			msg.Body = message;
			SmtpClient client = new SmtpClient();

			foreach (string addr in email.Split(';'))
			{
				if (!String.IsNullOrWhiteSpace(addr)) msg.To.Add(new MailAddress(addr)); 
			}
			try
			{
				client.Send(msg);
			}
			catch (Exception ex)
			{
				Log(string.Format("Send Email : Error = '{0}'", ex.Message));
			}
		}

		/// <summary>
		/// Print Log to Console and Log File
		/// </summary>
		/// <remarks>
		/// Changelog:
		/// 2014-10-29 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private static void Log(string message)
		{
			string tmp = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);

			Console.WriteLine(tmp);
			if (wLog != null) wLog.WriteLine(tmp);
			log += tmp + Environment.NewLine;
		}

		/// <summary>
		/// Sync Satellite Program to Main Program
		/// </summary>
		/// <remarks>
		/// Sync Satellite Program to Main Program
		/// 1) Clean Up Program Name (ทดลองออกอากาศ), (Broadcast testing)
		/// 2) Map Satellite Program Name into Main Database (Sync Data)
		/// 3) Import Data From Satellite EPG (On The Air)
		/// 4) Map Satellite Program Name into Main Database (Sync Data)
		/// 
		/// Changelog:
		/// 2014-10-29 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private static void ProcessSyncSatelliteProgram()
		{
			int count = 0;
			SqlCommand cmd = Database.GetSqlCommand();

			#region Sync Data to Multi Screen Database
			Log("== ProcessSyncSatelliteProgram: Start ==");
			cmd.Connection.Open();
			cmd.CommandTimeout = 240;
			#region 1) Clean Up Program Name (ทดลองออกอากาศ), (Broadcast testing), Leading Spaces, Ending Spaces, Fill Blank Program Name, Delete Blank Program Name
			#region 1.1) Clean Up Program Name (Thai) - (ทดลองออกอากาศ)
			cmd.CommandText = @"UPDATE epg_event SET title_th = LEFT(title_th, LEN(title_th) - CHARINDEX('(', REVERSE(title_th))) WHERE title_th LIKE '%(ท%' OR title_th LIKE '%(Broadcast%'";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.1) Clean up program name (Thai) - (ทดลองออกอากาศ) successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.1) Clean up program name (Thai) - (ทดลองออกอากาศ) : Error = {0}", ex.Message));
			}
			#endregion

			#region 1.2) Clean Up Program Name (English) - (Broadcast testing)
			cmd.CommandText = @"UPDATE epg_event SET title_en = LEFT(title_en, LEN(title_en) - CHARINDEX('(', REVERSE(title_en))) WHERE title_en LIKE '%(ท%' OR title_en LIKE '%(Broadcast%'";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.2) Clean up program name (English) - (Broadcast testing) successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.2) Clean up program name (English) - (Broadcast testing) : Error = {0}", ex.Message));
			}
			#endregion

			#region 1.3) Clean Up Program Name (Thai) - Blank
			cmd.CommandText = @"UPDATE epg_event SET title_th = LTRIM(RTRIM(title_th)) WHERE (title_th LIKE ' %' OR title_th LIKE '% ')";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.3) Clean up program name (Thai) - Blank successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.3) Clean up program name (Thai) - Blank : Error = {0}", ex.Message));
			}
			#endregion

			#region 1.4) Clean Up Program Name (English) - Blank
			cmd.CommandText = @"UPDATE epg_event SET title_en = LTRIM(RTRIM(title_en)) WHERE (title_en LIKE ' %' OR title_en LIKE '% ')";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.4) Clean up program name (English) - Blank successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.4) Clean up program name (English) - Blank : Error = {0}", ex.Message));
			}
			#endregion

			#region 1.5) Fill up blank program name (Thai)
			cmd.CommandText = @"UPDATE epg_event SET title_th = title_en WHERE (title_th = '')";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.5) Fill up blank program name (Thai) successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.5) Fill up blank program name (Thai) : Error = {0}", ex.Message));
			}
			#endregion

			#region 1.6) Fill up blank program name (English)
			cmd.CommandText = @"UPDATE epg_event SET title_en = title_th WHERE (title_en = '')";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.6) Fill up blank program name (English) successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.6) Fill up blank program name (English) : Error = {0}", ex.Message));
			}
			#endregion

			#region 1.7) Delete Blank Program Name
			cmd.CommandText = @"DELETE FROM epg_event WHERE title_th = '' AND title_en = ''";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1.7) Delete Blank Program Name successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1.7) Delete Blank Program Name : Error = {0}", ex.Message));
			}
			#endregion
			#endregion

			#region 2) Map Satellite Program Name into Main Database (Sync Data)
			#region 2.1) Map Satellite Program Name into Main Database (Sync Data)
			cmd.CommandText = @"INSERT INTO main_map_epg_event_program
				SELECT DISTINCT * FROM 
				(
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
					UNION
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
					UNION
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')) LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
					UNION
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')) LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
				) A
				WHERE title <> ''";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("2.1) Map satellite program successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("2.1) Map satellite program : Error = {0}", ex.Message));
			}
			#endregion

			#region 2.2) Clean Up Duplicate Mapped Program Record (SAT)
			cmd.CommandText = @"DELETE map
					FROM main_map_epg_event_program map
					WHERE EXISTS 
					(
						SELECT channel_id, nam_program_title, MIN(program_id), COUNT(*) FROM main_map_epg_event_program t WHERE t.channel_id = map.channel_id AND t.nam_program_title = map.nam_program_title AND t.ref_epg_channel = map.ref_epg_channel GROUP BY channel_id, nam_program_title, ref_epg_channel HAVING COUNT(*) > 1 AND MIN(program_id) <> map.program_id
					)";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("2.2) Clean up mapped program successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("2.2) Clean up mapped program : Error = {0}", ex.Message));
			}
			#endregion
			#endregion

			#region 3) Import Program Data From Satellite EPG (On The Air)
			#region 3.1) Import Program Data From Satellite EPG (On The Air)
			cmd.CommandText = @"INSERT INTO main_program(channel_id, nam_program_th, nam_program_en, desc_program_th, desc_program_en, cod_user_cre, cod_user_upd)
				SELECT DISTINCT c.channel_id, LTRIM(RTRIM(e.title_th)), LTRIM(RTRIM(e.title_en)), LTRIM(RTRIM(e.desc_th)), LTRIM(RTRIM(e.desc_en)), 'epg_sync', 'epg_sync'
				FROM
					main_channel c INNER JOIN
					epg_event e ON c.ref_epg = e.channel_id LEFT JOIN
					main_map_epg_event_program map ON map.ref_epg_channel = e.channel_id AND (map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') OR map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''))
				WHERE
					map.channel_id IS NULL AND
					c.sta_RecStatus = 'A' AND
					c.source = 'SAT' AND
					e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP))";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("3.1) Import satellite program successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("3.1) Import satellite program : Error = {0}", ex.Message));
			}
			#endregion

			#region 3.2) Clean Up Duplicate Program Record (On The Air)
			cmd.CommandText = @"DELETE map 
				FROM main_program map
				WHERE EXISTS 
				(
					SELECT channel_id, nam_program_en, MIN(program_id), COUNT(*) FROM main_program t WHERE t.channel_id = map.channel_id AND t.nam_program_en = map.nam_program_en GROUP BY channel_id, nam_program_en HAVING COUNT(*) > 1 AND MIN(program_id) <> map.program_id
				)";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("3.2) Clean up program successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("3.2) Clean up program : Error = {0}", ex.Message));
			}
			#endregion
			#endregion

			#region 4) Map Satellite Program Name into Main Database (Sync Data)
			#region 4.1) Map Satellite Program Name into Main Database (Sync Data)
			cmd.CommandText = @"INSERT INTO main_map_epg_event_program
				SELECT DISTINCT * FROM 
				(
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
					UNION
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
					UNION
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')) LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
					UNION
					SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') title
					FROM
						main_channel c INNER JOIN
						main_program mp ON c.channel_id = mp.channel_id INNER JOIN
						epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')) LEFT JOIN
						main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', '')
					WHERE
						map.channel_id IS NULL AND
						c.sta_RecStatus = 'A' AND
						c.source = 'SAT' AND
						e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
						1 = 1
				) A
				WHERE title <> ''";
			try
			{
				if (count > 0)
				{
					count = cmd.ExecuteNonQuery();
					Log("4.1) Map satellite program successfully " + count + " record(s)");
				}
			}
			catch (Exception ex)
			{
				Log(string.Format("4.1) Map satellite program : Error = {0}", ex.Message));
			}
			#endregion

			#region 4.2) Clean Up Duplicate Mapped Program Record (SAT)
			cmd.CommandText = @"DELETE map
					FROM main_map_epg_event_program map
					WHERE EXISTS 
					(
						SELECT channel_id, nam_program_title, MIN(program_id), COUNT(*) FROM main_map_epg_event_program t WHERE t.channel_id = map.channel_id AND t.nam_program_title = map.nam_program_title AND t.ref_epg_channel = map.ref_epg_channel GROUP BY channel_id, nam_program_title, ref_epg_channel HAVING COUNT(*) > 1 AND MIN(program_id) <> map.program_id
					)";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("4.2) Clean up mapped program successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("4.2) Clean up mapped program : Error = {0}", ex.Message));
			}
			#endregion
			#endregion
			cmd.Connection.Close();
			#endregion
			Log("== ProcessSyncSatelliteProgram: End ==");
		}

		/// <summary>
		/// Sync Satellite EPG to Main EPG
		/// </summary>
		/// <remarks>
		/// Sync Satellite EPG to Main EPG
		/// 1) Sync EPG Information from Satellite EPG
		/// 2) Import Non Existing EPG into Main Database (EPG)
		/// 
		/// Changelog:
		/// 2014-10-29 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private static void ProcessSyncSatelliteEPG()
		{
			int count = 0;
			SqlCommand cmd = Database.GetSqlCommand();

			#region Sync Data to Multi Screen Database
			Log("== ProcessSyncSatelliteEPG : Start ==");
			cmd.Connection.Open();
			cmd.CommandTimeout = 240;
			#region 0) Disable Overlapped EPG Informations (To be Enabled later on if Valid)
			cmd.CommandText = @"UPDATE me SET me.sta_RecStatus = 'C', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = 'sat_sync' FROM
					main_channel c INNER JOIN
					main_epg me ON c.channel_id = me.channel_id
				WHERE
					EXISTS (SELECT * FROM epg_event e WHERE e.channel_id = c.ref_epg AND me.epg_stop > e.start_time AND me.epg_start < e.stop_time AND e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP))) AND
					c.source = 'SAT' AND
					me.sta_RecStatus = 'A'";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("0) Disable satellite epg successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("0) Disable satellite epg : Error = {0}", ex.Message));
			}
			#endregion

			#region 1) Sync EPG Information from Satellite EPG
			cmd.CommandText = @"UPDATE me SET me.nam_epg_th = LTRIM(RTRIM(e.title_th)), me.nam_epg_en = LTRIM(RTRIM(e.title_en)), me.desc_epg_th = LTRIM(RTRIM(e.desc_th)), me.desc_epg_en = LTRIM(RTRIM(e.desc_en)), me.sta_RecStatus = 'A', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = 'sat_sync'
				FROM
					main_channel c INNER JOIN
					main_epg me ON c.channel_id = me.channel_id INNER JOIN
					main_map_epg_event_program map ON me.program_id = map.program_id AND c.ref_epg = map.ref_epg_channel INNER JOIN
					epg_event e ON map.ref_epg_channel = e.channel_id AND (REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') = map.nam_program_title OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = map.nam_program_title) AND e.start_time = me.epg_start AND e.stop_time = me.epg_stop
				WHERE
					e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
					c.source = 'SAT'";
			try
			{
				count = cmd.ExecuteNonQuery();
				Log("1) Sync satellite epg successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("1) Sync satellite epg : Error = {0}", ex.Message));
			}
			#endregion

			#region 2) Import Non Existing EPG into Main Database (EPG)
			cmd.CommandText = @"INSERT INTO main_epg(channel_id, program_id, nam_epg_th, nam_epg_en, desc_epg_th, desc_epg_en, epg_start, epg_stop, cod_user_cre, cod_user_upd)
				SELECT DISTINCT map.channel_id, map.program_id, e.title_th, e.title_en, e.desc_th, e.desc_en, e.start_time, e.stop_time, 'sat_sync', 'sat_sync'--,*
				FROM
					epg_event e INNER JOIN
					main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND (REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') = map.nam_program_title OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = map.nam_program_title) INNER JOIN
					main_channel c ON map.channel_id = c.channel_id AND map.ref_epg_channel = c.ref_epg AND c.sta_RecStatus = 'A' AND c.source = 'SAT'
				WHERE
					e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
					--e.start_time > CONVERT(DATE, CURRENT_TIMESTAMP) AND
					--e.stop_time <= CONVERT(DATE, CURRENT_TIMESTAMP) AND
					NOT EXISTS (SELECT * FROM main_epg t WHERE t.channel_id = map.channel_id AND t.epg_start >= e.start_time AND epg_stop <= e.stop_time AND sta_RecStatus = 'A') AND
					1 = 1
				ORDER BY
					e.stop_time";
			try
			{
				count = cmd.ExecuteNonQuery();
				isWarning = count < 1;
				Log("2) Import satellite epg successfully " + count + " record(s)");
			}
			catch (Exception ex)
			{
				Log(string.Format("2) Import satellite epg : Error = {0}", ex.Message));
			}
			#endregion
			cmd.Connection.Close();
			#endregion
			Log("== ProcessSyncSatelliteEPG : End ==");
		}

		/// <summary>
		/// Sync EPG Data to Multi Screen Database
		/// </summary>
		/// <remarks>
		/// Sync EPG data (channel, program, episode) and save into Multi Screen Database
		/// 1) Sync Main Channel into MultiScreen Channel
		/// 2) Import Main Channel into MultiScreen Channel
		/// 3) Sync Main Program with MultiScreen Program (Update Program Reference)
		/// 4) Import Main Program into MultiScreen Program
		/// 5) Import Main EPG with MultiScreen EPG
		/// 6) Sync Main EPG with MultiScreen EPG
		/// 
		/// Changelog:
		/// 2014-10-29 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private static void ProcessSyncToMultiScreenDatabase()
		{
			int count = 0;
			int provider_id = 0;
			SqlCommand cmd = Database.GetSqlCommand("MS");
			Int32.TryParse(ConfigurationManager.AppSettings["provider_id"], out provider_id);

			#region Sync Data to Multi Screen Database
			Log("== Start ProcessSyncToMultiScreenDatabase : Start ==");
			cmd.Parameters.Add("@provider_id", SqlDbType.Int).Value = provider_id;
			try
			{
				cmd.Connection.Open();
				#region 1) Sync Main Channel into MultiScreen Channel
				cmd.CommandText = @"UPDATE c SET num_order = o.channel_no, id_content_provider = '1', cod_upd = '1', dtm_upd = CURRENT_TIMESTAMP
				FROM
					channel c INNER JOIN
					EPGSERVER.DTVEpg.dbo.main_channel mc ON mc.channel_id = c.id_ref INNER JOIN
					EPGSERVER.DTVEpg.dbo.mst_channel_order o ON mc.channel_id = o.channel_id
				WHERE
					mc.sta_RecStatus = 'A' AND
					o.provider_id = @provider_id AND
					o.sta_RecStatus = 'A'";
				try
				{
					count = cmd.ExecuteNonQuery();
					Log("1) Sync channel successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{ 
					Log(string.Format("1) Sync channel : Error = {0}", ex.Message));
				}
				#endregion

				#region 2) Import Main Channel into MultiScreen Channel
				cmd.CommandText = @"INSERT INTO channel(nam_channel, txt_detail, flg_showchat, num_order, id_content_provider, status, cod_upd, dtm_upd, id_ref)
				SELECT LTRIM(RTRIM(nam_channel_th)) nam_channel, LTRIM(RTRIM(nam_channel_en)) txt_detail, 'N' flg_showchat, o.channel_no, 1, 'A' status, '1' cod_upd, CURRENT_TIMESTAMP dtm_upd, mc.channel_id id_ref
				FROM
					EPGSERVER.DTVEpg.dbo.main_channel mc INNER JOIN
					EPGSERVER.DTVEpg.dbo.mst_channel_order o ON mc.channel_id = o.channel_id
				WHERE
					mc.channel_id NOT IN (SELECT id_ref FROM channel WHERE id_ref IS NOT NULL) AND
					mc.sta_RecStatus = 'A' AND
					o.provider_id = @provider_id AND
					o.sta_RecStatus = 'A'";
				try
				{
					count = cmd.ExecuteNonQuery();
					Log("2) Import channel successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("2) Import channel : Error = {0}", ex.Message));
				}
				#endregion

				/*#region 3) Sync Main Program with MultiScreen Program (Update Program Reference)
				cmd.CommandText = @"UPDATE p SET p.id_ref = mp.program_id, p.dtm_upd = CURRENT_TIMESTAMP
				FROM
					EPGSERVER.DTVEpg.dbo.main_program mp INNER JOIN
					channel c ON mp.channel_id = c.id_ref INNER JOIN
					program p ON c.id_channel = p.id_channel AND p.id_ref IS NULL AND (REPLACE(REPLACE(p.nam_program, '-', ''), ' ', '') COLLATE Thai_CI_AS = REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') COLLATE Thai_CI_AS OR REPLACE(REPLACE(p.nam_program, '-', ''), ' ', '') COLLATE Thai_CI_AS = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') COLLATE Thai_CI_AS)
				WHERE
					mp.program_id NOT IN (SELECT id_ref FROM program WHERE id_ref IS NOT NULL) AND c.id_channel = p.id_channel AND mp.sta_RecStatus = 'A'";
				try
				{
					count = cmd.ExecuteNonQuery();
					Log("3) Sync program reference successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("3) Sync program reference : Error = {0}", ex.Message));
				}
				#endregion*/

				#region 4) Import Main Program into MultiScreen Program
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
					Log("4) Import program successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("4) Import program : Error = {0}", ex.Message));
				}
				#endregion

				#region 5) Disable Overlapped EPG Informations (To be Enabled later on if Valid)
				cmd.CommandText = @"UPDATE me SET me.status = 'C', me.dtm_upd = CURRENT_TIMESTAMP, me.cod_upd = '1'
				FROM
					channel c INNER JOIN
					episode me ON c.id_channel = me.id_channel
				WHERE 
					EXISTS (SELECT * FROM EPGSERVER.DTVEpg.dbo.main_epg e WHERE e.channel_id = c.id_ref AND me.dtm_to > e.epg_start AND me.dtm_from < e.epg_stop AND e.dte_upd > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)))
					AND me.status = 'A'";
				try
				{
					count = cmd.ExecuteNonQuery();
					Log("5) Disable episode successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("5) Disable episode : Error = {0}", ex.Message));
				}
				#endregion

				#region 6) Import Main EPG with MultiScreen EPG
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
					Log("6) Import episode successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("6) Import episode : Error = {0}", ex.Message));
				}
				#endregion

				#region 7) Sync Main EPG with MultiScreen EPG
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
					Log("7) Sync episode successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("7) Sync episode : Error = {0}", ex.Message));
				}
				#endregion

				#region 8) Cleanup Duplicate Data in MultiScreen EPG
				cmd.CommandText = @"DELETE e 
				FROM 
					episode e INNER JOIN
					(SELECT MIN(id_episode) id_episode, id_channel, dtm_from, dtm_to FROM episode WHERE status = 'A' GROUP BY id_channel, dtm_from, dtm_to HAVING COUNT(*) > 1) t ON e.id_channel = t.id_channel AND e.dtm_from = t.dtm_from AND e.dtm_to = t.dtm_to
				WHERE
					e.status = 'A' AND
					e.id_episode <> t.id_episode";
				try
				{
					count = cmd.ExecuteNonQuery();
					Log("8) Cleanup duplicate episode successfully " + count + " record(s)");
				}
				catch (Exception ex)
				{
					Log(string.Format("8) Cleanup duplicate episode : Error = {0}", ex.Message));
				}
				#endregion
			}
			catch (Exception e)
			{
				Log("== ProcessSyncToMultiScreenDatabase : Main Error ==" + e.Message);
			}
			cmd.Connection.Close();
			#endregion
			Log("== ProcessSyncToMultiScreenDatabase : End ==");
		}

		/// <summary>
		/// Show Sync Statistics
		/// </summary>
		/// <remarks>
		/// Changelog:
		/// 2015-02-11 - Create (Woraphot Chokratanasombat)
		/// </remarks>
		private static void ShowSyncStatistics()
		{
			SqlCommand cmd = Database.GetSqlCommand();

			#region Sync Data to Multi Screen Database
			Log("== ShowSyncStatistics: Start ==");
			cmd.Connection.Open();
			cmd.CommandTimeout = 240;
			#region 1) Show Sat Program Max Start Date
			cmd.CommandText = @"SELECT  MAX(start_time) FROM epg_event e INNER JOIN main_channel c ON e.channel_id = c.ref_epg WHERE last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND source = 'SAT'";
			try
			{
				Log("1) Show Sat Program Max Start Date " + cmd.ExecuteScalar());
			}
			catch (Exception ex)
			{
				Log(string.Format("1) Show Sat Program Max Start Date : Error = {0}", ex.Message));
			}
			#endregion

			cmd.Connection.Close();
			#endregion
			Log("== ShowSyncStatistics: End ==");
		}
	}
}
