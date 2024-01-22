using System;

namespace EpgImport.Entity.WebService
{
	/// <summary>
	/// Epg Information for EPG Web Service
	/// </summary>
	public class Epg
	{
		/// <summary>
		/// Channel Id
		/// </summary>
		public long ChannelId { get; set; }
		/// <summary>
		/// Channel Name
		/// </summary>
		public string ChannelName { get; set; }
		/// <summary>
		/// Program Id
		/// </summary>
		public long ProgramId { get; set; }
		/// <summary>
		/// Epg Id
		/// </summary>
		public long EpgId { get; set; }
		/// <summary>
		/// Title English
		/// </summary>
		public string TitleEN { get; set; }
		/// <summary>
		/// Title Thai
		/// </summary>
		public string TitleTH { get; set; }
		/// <summary>
		/// Description English
		/// </summary>
		public string DescEN { get; set; }
		/// <summary>
		/// Description Thai
		/// </summary>
		public string DescTH { get; set; }
		/// <summary>
		/// Start Time
		/// </summary>
		public DateTime DateStart { get; set; }
		/// <summary>
		/// End Time
		/// </summary>
		public DateTime DateStop { get; set; }
	}
}