using System;
using SQLite;
namespace MadmucFarm
{
	public class ChemicalDB
	{
		public ChemicalDB ()
		{
		}

		[AutoIncrement, PrimaryKey]
		public int id { set; get; }
		public string farmName { set; get; }
		public string fieldName { set; get; }
		public string chemicalDate{ set; get;}
		public string implementedUsed { set; get; }
		public string chemicalRates { set; get; }
		public string chemicalTypes{ set; get; }
		public string note { set; get; }
		public int templateIndex { set; get; }
		public bool isSynch { set; get; }
		public string lastUpdate { set; get; }
		public string serverId { set; get; }
	}
}

