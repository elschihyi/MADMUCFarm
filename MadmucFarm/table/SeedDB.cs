using System;
using SQLite;

namespace MadmucFarm
{
	public class SeedDB
	{
		public SeedDB ()
		{
		}

		[AutoIncrement,PrimaryKey]
		public int id { set; get; }


		public string farmName{ set; get;}
		public string fieldName{ set; get;}
		//public int fieldId{ set; get;}
		public string seedDate { set; get;}
		public string implementedUsed { set; get; }
		public float seedDepth { set; get;}
		public string seedTypes { set; get; }
		public string varietyName { set; get;}
		public int seedRate { set; get;}
		public string seedTreatment { set; get; }
		public int NH3 { set; get; }
		public int _11 { set; get; }
		public string notes { set; get;}
		public int templateIndex { set; get;}
		public string lastUpdate { set; get; }
		public bool isSynch { set; get;}

	}
}

