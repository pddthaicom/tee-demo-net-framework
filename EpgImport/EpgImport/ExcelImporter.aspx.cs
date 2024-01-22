using EpgImport.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EpgImport
{
	public partial class ExcelImporter : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void btnImport_Click(object sender, EventArgs e)
		{
			lblProgress.Text = "";
			if (fileExcel.HasFile)
			{
				string path = Server.MapPath(string.Format("Upload/{0}.xlsx", Path.GetRandomFileName()));
				fileExcel.SaveAs(path);

				int row_affected = 0;
				int count = 0, count_error = 0, count_program = 0, count_epg = 0;
				SqlCommand sql = Database.GetSqlCommand();
				OleDbConnection conn = new OleDbConnection(string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{0}';Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'", path));
				OleDbCommand excel = conn.CreateCommand();
				DataTable table = new DataTable();

				#region Read Excel File
				try
				{
					excel.CommandText = "SELECT * FROM [Template$]";
					conn.Open();
					new OleDbDataAdapter(excel).Fill(table);
					conn.Close();
				}
				catch (Exception ex)
				{
					lblProgress.Text = string.Format("Cannot open excel file : {0} [{1}]", fileExcel.FileName, ex.Message);
					return;
				}
				#endregion

				sql.Parameters.Add("@channel_id", SqlDbType.Int);
				sql.Parameters.Add("@name", SqlDbType.VarChar);
				sql.Parameters.Add("@desc", SqlDbType.VarChar);
				sql.Parameters.Add("@start", SqlDbType.DateTime);
				sql.Parameters.Add("@stop", SqlDbType.DateTime);
				sql.Parameters.Add("@user", SqlDbType.VarChar).Value = "excel_sync";
				sql.Connection.Open();
				foreach (DataRow row in table.Rows)
				{
					// Valid Record
					if (row[0].ToString() == "Y")
					{
						count++;
						#region Read Value and Initialize Sql Parameters
						DateTime start;
						DateTime stop;
						try
						{
							start = DataType.ToDateTime(row[5]);
							start = start.Add(DataType.ToDateTime(row[6]).TimeOfDay);
							stop = DataType.ToDateTime(row[7]);
							stop = stop.Add(DataType.ToDateTime(row[8]).TimeOfDay);
						}
						catch (Exception ex)
						{
							count_error++;
							Console.WriteLine(ex.Message);

							continue;
						}
						sql.Parameters["@channel_id"].Value = Convert.ToInt32(row[1]);
						sql.Parameters["@name"].Value = row[3].ToString();
						sql.Parameters["@desc"].Value = row[4].ToString();
						sql.Parameters["@start"].Value = start;
						sql.Parameters["@stop"].Value = stop;
						#endregion

						#region 1) Insert Program
						#region 1.1) Map Excel Program Name into Main Database (Sync Data)
						sql.CommandText = @"INSERT INTO main_map_excel_program
							SELECT TOP 1 c.channel_id ref_main_channel, mp.program_id, REPLACE(REPLACE(@name, '-', ''), ' ', '') title
							FROM
								main_channel c INNER JOIN
								main_program mp ON c.channel_id = mp.channel_id AND (REPLACE(REPLACE(@name, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(@name, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(@name, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(@name, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
								main_map_excel_program map ON map.channel_id = mp.channel_id AND map.nam_program_title = REPLACE(REPLACE(@name, '-', ''), ' ', '')
							WHERE
								c.channel_id = @channel_id AND
								map.channel_id IS NULL";
						row_affected = sql.ExecuteNonQuery();
						#endregion

						#region 1.2) Import New Excel Program (Not Exists)
						sql.CommandText = @"INSERT INTO main_program(channel_id, nam_program_th, nam_program_en, desc_program_th, desc_program_en, cod_user_cre, cod_user_upd)
							SELECT @channel_id, @name, @name, @desc, @desc, @user, @user
							WHERE NOT EXISTS (SELECT * FROM main_map_excel_program map WHERE map.channel_id = @channel_id AND map.nam_program_title = REPLACE(REPLACE(@name, '-', ''), ' ', ''))";
						row_affected = sql.ExecuteNonQuery();
						#endregion

						#region 1.3) Map Excel Program Name into Main Database (Sync Data)
						sql.CommandText = @"INSERT INTO main_map_excel_program
							SELECT TOP 1 c.channel_id ref_main_channel, mp.program_id, REPLACE(REPLACE(@name, '-', ''), ' ', '') title
							FROM
								main_channel c INNER JOIN
								main_program mp ON c.channel_id = mp.channel_id AND (REPLACE(REPLACE(@name, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(@name, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(@name, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(@name, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
								main_map_excel_program map ON map.channel_id = mp.channel_id AND map.nam_program_title = REPLACE(REPLACE(@name, '-', ''), ' ', '')
							WHERE
								c.channel_id = @channel_id AND
								map.channel_id IS NULL";
						if (row_affected != 0)
						{
							row_affected = sql.ExecuteNonQuery();
							count_program++;
						}
						#endregion
						#endregion

						#region 2) Insert Epg
						#region 2.1) Deactivate Overlapped Program
						sql.CommandText = @"UPDATE me SET me.sta_RecStatus = 'C', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = @user
							FROM
								main_epg me
							WHERE
								me.epg_stop > @start AND
								me.epg_start < @stop AND
								me.channel_id = @channel_id";
						row_affected = sql.ExecuteNonQuery();
						#endregion

						#region 2.2) Update Old EPG (Same Program, Start Time, End Time)
						sql.CommandText = @"UPDATE me SET me.nam_epg_th = LTRIM(RTRIM(@name)), me.nam_epg_en = LTRIM(RTRIM(@name)), me.desc_epg_th = LTRIM(RTRIM(@desc)), me.desc_epg_en = LTRIM(RTRIM(@desc)), me.sta_RecStatus = 'A', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = @user
							FROM
								main_epg me INNER JOIN
								main_map_excel_program map ON me.program_id = map.program_id
							WHERE
								map.channel_id = @channel_id AND
								REPLACE(REPLACE(@name, '-', ''), ' ', '') = map.nam_program_title AND
								me.epg_start = @start AND
								me.epg_stop = @stop";
						row_affected = sql.ExecuteNonQuery();
						#endregion

						#region 2.3) Import Non Existing EPG into Main Database (EPG)
						sql.CommandText = @"INSERT INTO main_epg(channel_id, program_id, nam_epg_th, nam_epg_en, desc_epg_th, desc_epg_en, epg_start, epg_stop, cod_user_cre, cod_user_upd)
							SELECT map.channel_id, map.program_id, @name, @name, @desc, @desc, @start, @stop, @user, @user
							FROM
								main_map_excel_program map
							WHERE
								map.channel_id = @channel_id AND
								REPLACE(REPLACE(@name, '-', ''), ' ', '') = map.nam_program_title AND
								NOT EXISTS (SELECT *
									FROM
										main_epg me INNER JOIN
										main_map_excel_program map ON me.program_id = map.program_id
									WHERE
										map.channel_id = @channel_id AND
										REPLACE(REPLACE(@name, '-', ''), ' ', '') = map.nam_program_title AND
										me.epg_start = @start AND
										me.epg_stop = @stop)";
						row_affected = sql.ExecuteNonQuery();
						if (row_affected != 0) count_epg++;
						#endregion
						#endregion
					}
				}
				sql.Connection.Close();
				lblProgress.Text = string.Format("Found EPG {0} record(s)<br>Found error {3} record(s)<br>Insert new program {1} record(s)<br>Insert new EPG {2} record(s)", count, count_program, count_epg, count_error);
			}
		}
	}
}