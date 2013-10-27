using System;
using SQLite;

namespace MadmucFarm
{
	public class ChemicalTemplate
	{
		public ChemicalTemplate ()
		{
		}

		[AutoIncrement,PrimaryKey]
		public int objectId { set; get; }

		public string templateName{ set; get;}
		public string implementedUsed { set; get; }
		public string chemicalRates { set; get; }
		public string chemicalTypes{ set; get; }
	}
}

