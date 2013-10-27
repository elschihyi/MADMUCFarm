using System;
using SQLite;

namespace MadmucFarm
{
	public class SoilTestData
	{
			[PrimaryKey, AutoIncrement]
			public int DbID { get; set; }

			public String DbField { get ; set; }
			public string DbNotes { get; set; }
			public string DbTimeStamp { get; set; }
			public Boolean DbUpdate { get; set;} 

	}
}

