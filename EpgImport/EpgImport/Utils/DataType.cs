using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EpgImport.Utils
{
	public class DataType
	{
		public static DateTime ToDateTime(object date)
		{
			if (date is DateTime) return (DateTime)date;
			if (date is String || date is string) return DateTime.Parse((string)date);
			throw new Exception(string.Format("Cannot Convert Date Time {0}", date));
		}
	}
}