using System;
using SQLite;

namespace MadmucFarm
{
	public class HarvestData
	{
		[PrimaryKey, AutoIncrement]
		public int DbID { get; set; }

		public String DbField { get ; set; }
		public DateTime DbDate { get; set; }
		public string DbImplement { get; set; }
		public int DbYield { get; set; }
		public int DbMoisture { get; set; }
		public string DbBin { get; set; } 
		public string DbNotes { get; set; }
		public string DbTimeStamp { get; set; }
		public Boolean DbUpdate { get; set;} 


	}
}

