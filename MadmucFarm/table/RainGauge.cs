using System;
using SQLite;

namespace MadmucFarm
{
	public class RainGauge
	{

		public RainGauge ()
		{
		}

		[PrimaryKey, AutoIncrement]
		public int rainGuageID{ get; set; }
		public int farmID{ get; set; }
		public int rain{ get; set;}
		public DateTime theDate { set; get;}
		public int flag{ get; set;}
		public string timeStamp{ get; set;}
	}
}
