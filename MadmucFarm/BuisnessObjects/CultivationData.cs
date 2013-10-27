using System;
using SQLite; 

namespace MadmucFarm
{
	public class CultivationData
	{
			[PrimaryKey, AutoIncrement]
			public int DbID { get; set; }

			public String DbField { get ; set; }
			public DateTime DbDate { get; set; }
			public string DbImplement { get; set; }
			public int DbDepth { get; set; }
			public string DbNotes { get; set; }
			public string DbTimeStamp { get; set; } 
			public Boolean DbUpdate { get; set;} 
	}
}

