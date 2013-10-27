using System;
using System.IO;
using SQLite;
using System.Collections.Generic;



namespace MadmucFarm
{
	public class DBConnection
	{


		public DBConnection ()
		{
		}

		public static SQLiteConnection initialDB(){

			string dbName="db_MadmucFarm.db";
			SQLiteConnection db = new SQLiteConnection (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), dbName));
			//SQLiteConnection db = new SQLiteConnection (dbName);
			db.CreateTable<User> ();
			db.CreateTable<Farm> ();
			db.CreateTable<Field> ();
			db.CreateTable<Bin> ();
			db.CreateTable<RainGauge> ();
			return db;
		}
		//***********************************
		public static void dropBinTable(){
			var db =initialDB();
			db.Query<Bin> ("drop table Bin");
			db.Close ();
			db.Dispose ();
		}


		public static void updateBinMoister(int binID,string moister){
			var db = initialDB ();
			db.Query<Bin> ("update Bin set moister ='"+moister+"' where binID="+binID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateBinCrop(int binID,string crop){
			var db = initialDB ();
			db.Query<Bin> ("update Bin set crop ='"+crop+"' where binID="+binID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateBinBushel(int binID,int bushel){
			var db = initialDB ();
			db.Query<Bin> ("update Bin set bushel ="+bushel+" where binID="+binID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateBinSize(int binID,int binSize){
			var db = initialDB ();
			db.Query<Bin> ("update Bin set binSize ="+binSize+" where binID="+binID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateBinTimeStamp(int binID,string timeStamp){
			var db = initialDB ();
			db.Query<Bin> ("update Bin set timeStamp ="+timeStamp+" where binID="+binID);
			db.Close ();
			db.Dispose ();
		}

		public static IEnumerable<Bin> getBin(int binNum){
			var db = initialDB();

			var result= db.Query<Bin> ("select * from Bin where binID="+binNum);
			db.Close ();
			db.Dispose ();
			return result;
		}

		public static IEnumerable<Bin> getAllBin(){
			var db = initialDB();

			var result= db.Query<Bin> ("select * from Bin");
			db.Close ();
			db.Dispose ();
			return result;
		}

		public static void updateBinFlag(int binID,int flag){
			var db = initialDB ();
			db.Query<Bin> ("update Bin set flag ="+flag+" where binID="+binID);
			db.Close ();
			db.Dispose ();
		}

		public static void insertBin(){
			var db = initialDB ();
			for (int i=1; i<71; i++) {
				db.Query<Bin> ("insert into Bin(binID,binSize,bushel,crop,flag,timeStamp) Values (" + i + ",0,0,'',0,'0')");
			}
			db.Close ();
			db.Dispose ();
		}

		public static string getBinTimeStamp(int binID){
			var db = initialDB ();
			var result=	db.Query<Bin> ("select timeStamp from Bin where binID="+binID);
			db.Close ();
			db.Dispose ();
			if (result.Count > 0)
				return result [0].timeStamp;
			return null;
		}
		//--------------------------------------------------------------------
		public static void dropFieldTable(){
			var db =initialDB();
			db.Query<Field> ("drop table Field");
			db.Close ();
			db.Dispose ();
		}

		public static void updateNote(int fieldID,string note){
			var db = initialDB ();
			db.Query<Field> ("update Field set note ='"+note+"' where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateAcre(int fieldID,int acre){
			var db = initialDB ();
			db.Query<Field> ("update Field set acre ="+acre+" where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
		}

		public static string getFieldNote(int fieldID){
			var db = initialDB();
			var result= db.Query<Field> ("select note from Field where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
			return result [0].note;
		}

		public static double getFieldAcre(int fieldID){
			var db = initialDB();
			var result= db.Query<Field> ("select acre from Field where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
			return result [0].acre;
		}

		public static IEnumerable<Field> getAllFields(int farmID) {
			var db = initialDB();
			var result= db.Query<Field> ("select * from Field where farmID="+farmID);
			db.Close ();
			db.Dispose ();
			return result;
			//return result;
		}

		public static void insertField(string fieldName,double arce,int farmID,string note){
			var db = initialDB();
			db.Query<Field> ("insert into Field(fieldName,acre,farmID,note,flag,timeStamp) Values ('" + fieldName + "',"+arce+","+farmID+",'"+note+"',1,'0')");
			db.Close ();
			db.Dispose ();
		}

		public static void updateFieldFlag(int fieldID,int flag){
			var db = initialDB ();
			db.Query<Field> ("update Field set flag ="+flag+" where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateFieldTimeStamp(int fieldID,string timeStamp){
			var db = initialDB ();
			db.Query<Field> ("update Field set timeStamp ='"+timeStamp+"' where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateField(int fieldID,string fieldName,int acre,string note,string timeStamp){
			var db = initialDB ();
			db.Query<Field> ("update Field set timeStamp ='"+timeStamp+"' where fieldID="+fieldID);
			db.Query<Field> ("update Field set fieldName ='"+fieldName+"' where fieldID="+fieldID);
			db.Query<Field> ("update Field set acre ="+acre+" where fieldID="+fieldID);
			db.Query<Field> ("update Field set note ='"+note+"' where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
		}

		public static int getFieldID(string farmName,string fieldName){
			var db = initialDB();
			var farmID = getFarmID (farmName);
			var result=db.Query<Field> ("select fieldID from Field where fieldName='"+fieldName+"' and farmID="+farmID);
			db.Close ();
			db.Dispose ();
			if (result.Count == 1)
				return result [0].fieldID;
			else
				return -1;
		}

		public static string getFieldTimeStamp(int fieldID){
			var db = initialDB ();
			var result=	db.Query<Field> ("select timeStamp from Field where fieldID="+fieldID);
			db.Close ();
			db.Dispose ();
			if (result.Count > 0)
				return result [0].timeStamp;
			return null;
		}

		//---------------------------------------------------------------------------------------------------
		public static void dropRainGaugeTable(){
			var db =initialDB();
			db.Query<RainGauge> ("drop table RainGauge");
			db.Close ();
			db.Dispose ();
		}

		public static void insertRainGuage(int farmID,int rain,string theDate,string timeStamp){
			var db =initialDB();
			RainGauge nrg= new RainGauge();
			nrg.farmID=farmID;
			nrg.rain=rain;
			nrg.theDate=DateTime.ParseExact(theDate, "dd/MM/yyyy", null);
			nrg.flag = 0;
			nrg.timeStamp=timeStamp;
			db.Insert(nrg);
			db.Close ();
			db.Dispose ();
		}

		public static IEnumerable<RainGauge> getRainDetails(int farmID) {
			var db = initialDB();
			var result= db.Query<RainGauge> ("select * from RainGauge where farmID="+farmID);
			db.Close ();
			db.Dispose ();
			return result;
			//return result;
		}

		public static IEnumerable<RainGauge> getAllRainGuage() {
			var db = initialDB();
			var result= db.Query<RainGauge> ("select * from RainGauge ");
			db.Close ();
			db.Dispose ();
			return result;
			//return result;
		}

		public static void updateRainGuageFlag(int rainGaugeID,int flag){
			var db = initialDB ();
			db.Query<RainGauge> ("update RainGauge set flag ="+flag+" where rainGuageID="+rainGaugeID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateRainGuageFlagByFarmID(int farmID,int flag){
			var db = initialDB ();
			var result= db.Query<RainGauge> ("select * from RainGauge where farmID="+farmID);
			foreach (RainGauge rainGuage in result) {
				if(rainGuage.theDate.Year==DateTime.Now.Year&&rainGuage.theDate.Month==DateTime.Now.Month&&rainGuage.theDate.Day==DateTime.Now.Day)
					db.Query<RainGauge> ("update RainGauge set flag ="+flag+" where rainGuageID="+rainGuage.rainGuageID);
			}
			db.Close ();
			db.Dispose ();
		}

		public static void updateRainGuageRain(int rainGaugeID,int rain){

			var db = initialDB ();
			db.Query<RainGauge> ("update RainGauge set rain ="+rain+" where rainGuageID="+rainGaugeID);
			db.Close ();
			db.Dispose ();
		}

		public static void updateRainGuageTimeStamp(int rainGuageID,string timeStamp){
			var db = initialDB ();
			db.Query<RainGauge> ("update RainGauge set timeStamp ='"+timeStamp+"' where rainGuageID="+rainGuageID);
			db.Close ();
			db.Dispose ();
		}

		public static int getRainGuage(int rainGuageID){
			var db = initialDB();
			var result= db.Query<RainGauge> ("select * from RainGauge where rainGuageID="+rainGuageID);
			db.Close ();
			db.Dispose ();
			return result[0].rain;
		}


		public static int getRainGuageID (string farmName, string theDate){
			var db = initialDB();
			var farmID = getFarmID (farmName);
			var results=db.Query<RainGauge> ("select * from RainGauge where farmID="+farmID);
			int rainGuageID = -1;
			foreach (RainGauge theRainGuage in results) {
				if (theRainGuage.theDate.ToShortDateString() == theDate) {
					rainGuageID = theRainGuage.rainGuageID;
					return rainGuageID;
				}
			}
			db.Close ();
			db.Dispose ();
			return rainGuageID;
			
		}

		public static string getRainGuageTimeStamp(int rainGuageID){
			var db = initialDB ();
			var result=	db.Query<RainGauge> ("select timeStamp from RainGauge where rainGuageID="+rainGuageID);
			db.Close ();
			db.Dispose ();
			if (result.Count > 0)
				return result [0].timeStamp;
			return null;
		}

		//------------------------------------------------------------------------------------------------
		public static void dropFarmTable(){
			var db =initialDB();
			db.Query<Farm> ("drop table Farm");
			db.Close ();
			db.Dispose ();
		}

		public static void setRain(int farmID,int rain){
			var db = initialDB ();
			db.Query<Farm> ("update Farm set rain ="+rain+" where farmID="+farmID);
		}
		public static int getRain(int farmID){
			var db = initialDB ();
			var result=db.Query<Farm> ("select rain from Farm where farmID="+farmID);
			db.Close ();
			db.Dispose ();
			return result [0].rain;
		}

		public static void updateRain(int farmID,int rain){
			var db = initialDB ();
			int oldRain = getRain (farmID);
			db.Query<Farm> ("update Farm set rain ="+(rain+oldRain)+" where farmID="+farmID);

			//have today's rainGuage
			var rainGuages = DBConnection.getAllRainGuage ();
			foreach(RainGauge rainguage in rainGuages){
				if (DateTime.Now.ToShortDateString () == rainguage.theDate.ToShortDateString ()&&rainguage.farmID==farmID) {
					rainguage.rain += rain;
					rainguage.flag = 1;
					db.Update (rainguage);
					return;
				}
			}

			//insert new raindetail
			RainGauge nrg= new RainGauge();
			nrg.farmID=farmID;
			nrg.rain=rain;
			nrg.theDate=DateTime.Now;
			nrg.flag = 1;
			nrg.timeStamp="0";
			db.Insert(nrg);
			db.Close ();
			db.Dispose ();
			//db.Query<RainGauge> ("insert into RainGuage(farmID,rain,theDate) Values ('" + farmID + "',"+rain+","+DateTime.Now+")");
		}


		public static int getFarmID(String farmName){
			var db = initialDB();
			var result=db.Query<Farm> ("select farmID from Farm where farmName='"+farmName+"'");
			db.Close ();
			db.Dispose ();
			if (result.Count == 1)
				return result [0].farmID;
			else
				return -1;
		}

		public static void insertFarm(String farmName,string timeStamp){
			var db = initialDB();
			db.Query<Farm> ("insert into Farm(farmName,rain,timeStamp) Values ('" + farmName + "',0,'"+timeStamp+"')");
			db.Close ();
			db.Dispose ();
		}

		public static IEnumerable<Farm> getAllFarms() {
			var db = initialDB();
			var result= db.Query<Farm> ("select * from Farm");
			db.Close ();
			db.Dispose ();
			return result;
			//return result;
		}

		public static string getFarmName(int farmID){
			var db = initialDB();
			var result= db.Query<Farm> ("select * from Farm where farmID="+farmID);
			db.Close ();
			db.Dispose ();
			return result[0].farmName;
		}
		//-------------------------------------------------------------------------
		public static void dropUserTable(){
			var db =initialDB();
			db.Query<User> ("drop table User");
			db.Close ();
			db.Dispose ();
		}

		public static void insertUser(String userName,String password,int isAdmin){
			var db =initialDB();
			db.Query<User> ("delete from User where userName='" + userName + "'");
			db.Query<User> ("insert into User (userName,password,isAdmin) Values ('" + userName + "','" + password + "',"+isAdmin+");");
			db.Close ();
			db.Dispose ();
		}

		public static bool isUser(String userName,String password){
			var db =initialDB();
			var result = db.Query<User> ("Select * from User where userName='"+userName+"' and password='"+password+"';");
			db.Close ();
			db.Dispose ();
			//result is IEnumerable<User> type
			if(result.Count==1){
				return true;
			}
			else {
				return false;
			}
		}

		public static bool isAdmin(String userName){
			var db =initialDB();
			var result = db.Query<User> ("Select isAdmin from User where userName='"+userName+"';");
			db.Close ();
			db.Dispose ();
			//result is IEnumerable<User> type
			if(result.Count==1&&result[0].isAdmin){
				return true;
			}
			else {
				return false;
			}
		}

	}
}

