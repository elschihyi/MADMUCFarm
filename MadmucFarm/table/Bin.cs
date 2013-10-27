using System;
using SQLite;

namespace MadmucFarm
{
	public class Bin
	{
		public Bin ()
		{
		}
		public int binID{ get; set;}
		public int binSize{ get; set;}
		public int bushel{ get; set;}
		public string crop{get;set;}
		public string moister{ get; set;}
		public int flag{ get; set;}
		public string timeStamp{ get; set;}
	}
}

