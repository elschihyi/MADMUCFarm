using System;
using SQLite;

namespace MadmucFarm
{
	public class Field
	{
		public Field ()
		{
		}
		[PrimaryKey,AutoIncrement]
		public int fieldID{ get; set; }
		public string fieldName{ get; set;}
		public double acre{ get; set; }
		public int farmID{ get; set;}
		public string note{ get; set;}
		public int flag{ get; set;}
		public string timeStamp{ get; set;}
	}
}

