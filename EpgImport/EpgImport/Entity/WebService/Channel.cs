
namespace EpgImport.Entity.WebService
{
	/// <summary>
	/// Channel Information for EPG Web Service
	/// </summary>
	public class Channel
	{
		/// <summary>
		/// Channel Id
		/// </summary>
		public long ChannelId { get; set; }
		/// <summary>
		/// Channel Number
		/// </summary>
		public long ChannelNo { get; set; }
		/// <summary>
		/// Channel Name
		/// </summary>
		public string ChannelName { get; set; }
		/// <summary>
		///  Channel Logo
		/// </summary>
		public string ChannelLogo { get; set; }
	}
}