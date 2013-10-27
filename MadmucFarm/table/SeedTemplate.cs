using System;
using SQLite;

namespace MadmucFarm
{
	public class SeedTemplate
	{
		public SeedTemplate ()
		{
		}

		[AutoIncrement, PrimaryKey]
		public int objectId { set; get;}

		public string templateName { set; get;}
		public string implementedUsed { set; get; }
		public string seedDepth { set; get;}
		public string seedTypes { set; get; }
		public string varietyName { set; get;}
		public string seedRate { set; get;}
		public string seedTreatment { set; get; }
		public string NH3 { set; get; }
		public string _11 { set; get; }

	}
}

