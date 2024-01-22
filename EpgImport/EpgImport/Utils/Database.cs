using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace EpgImport.Utils
{
	/// <summary>
	/// Database Utilities
	/// </summary>
	public class Database
	{
		/// <summary>
		/// Connection String for EPG Database
		/// </summary>
		private static string _connectionStringEPG = ConfigurationManager.ConnectionStrings["epgdb"].ConnectionString;
		private static string _connectionStringMS = ConfigurationManager.ConnectionStrings["fbbdb"].ConnectionString;

		public static SqlCommand GetSqlCommand(string type = "EPG")
		{
			switch (type)
			{
				case "MS":
					return new SqlConnection(_connectionStringMS).CreateCommand();
			}
			return new SqlConnection(_connectionStringEPG).CreateCommand();
		}

		public static void ExecuteCommand(SqlCommand cmd)
		{
			if (cmd == null || String.IsNullOrWhiteSpace(cmd.CommandText)) return;

			if (cmd.Connection == null) return;
			if (cmd.Connection.State == System.Data.ConnectionState.Closed) cmd.Connection.Open();
			cmd.ExecuteNonQuery();
		}
	}
}