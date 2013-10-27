using System;
using SQLite;

namespace MadmucFarm
{
	public class User
	{
		public User ()
		{
		}

		[PrimaryKey, AutoIncrement]
		public int userID{ get; set;}
		[Unique]
		public String userName{ get; set;}
		public String password{ get; set;}
		public bool isAdmin{ get; set;}

	}
}

