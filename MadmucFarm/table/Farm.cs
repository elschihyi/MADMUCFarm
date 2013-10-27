using System;
using SQLite;

namespace MadmucFarm
{
	public class Farm
	{
		public Farm ()
		{
		}

		[PrimaryKey, AutoIncrement]
		public int farmID{ get; set; }
		public String farmName{ get; set; }
		public int rain{ get; set;}
		public string timeStamp{ get; set;}
	}
}

