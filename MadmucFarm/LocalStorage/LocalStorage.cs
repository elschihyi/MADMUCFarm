using System;
using SQLite;
using System.IO;
using MadmucFarm;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MadmucFarm
{
	public class LocalStorage
	{

		private static LocalStorage localStorageManager;

		private LocalStorage ()
		{
		}

		public static LocalStorage getLocalStorageManager(){

			if(localStorageManager == null){

				localStorageManager = new LocalStorage ();
			}

			return localStorageManager;
		}

		public SQLiteConnection getLocalDB () {

			//			string dbName = "Farm";
			//			var doc = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			//			string dbPath = Path.Combine (doc,dbName);
			//			SQLiteConnection db = new SQLiteConnection (dbPath);
			//			return db;
			//
			var db = DBConnection.initialDB ();
			return db;
		}

		public void createTable () {
			var db = getLocalDB ();
			db.CreateTable<SeedDB> ();
			db.CreateTable<ChemicalDB> ();
			db.CreateTable<SeedTemplate> ();
			db.CreateTable<ChemicalTemplate>();

		}

		public void deleteObject (string farmName, string fieldName ) {
			var db = getLocalDB ();

			var seed = from s in db.Table<SeedDB> () where s.farmName == farmName && s.fieldName == fieldName select s;

			foreach (var s in seed) {

					//	db.Delete<SeedDB> (s.id);
			db.Delete (s);


			}


		}

		public void cleanTemplates (int name){
			var db = getLocalDB ();

			switch (name) {
				// case 1 cleans SeedTemplates
				case 1:
				var count = db.Table<SeedTemplate> ().Count ();
				if (count > 0) {
					db.DropTable<SeedTemplate> ();
					db.CreateTable<SeedTemplate> ();
				}

				break;
				case 2:
				var count1 = db.Table<ChemicalTemplate> ().Count ();
				if (count1 > 0) {
					db.DropTable<ChemicalTemplate> ();
					db.CreateTable<ChemicalTemplate> ();
				}

				break;
			}

		}

		public  SeedDB loadSeed (string farmName, string fieldName){

			var db = getLocalDB ();

			var history = from s in db.Table<SeedDB>() where s.farmName == farmName && s.fieldName == fieldName select s;

			if(history.Count() > 0){
				return history.First ();
			}
			return null;

		}

		public TableQuery<ChemicalDB> loadChemical (string farmName, string fieldName){

			var db = getLocalDB ();
			var history = from c in db.Table<ChemicalDB>() where c.farmName == farmName && c.fieldName == fieldName select c;

			if(history.Count() > 0){
				return history;
			}
			return null;
		}

		public Dictionary<string,SeedTemplate> loadSeedTemplate ()
		{
			var db = getLocalDB ();
			var seedTemplates = from t in db.Table<SeedTemplate>() select t;
			Dictionary<string,SeedTemplate> templates = new Dictionary<string, SeedTemplate>();


			foreach (var t in seedTemplates) {
				templates.Add (t.templateName,t);
			}

			return templates;
		}

		public Dictionary<string,ChemicalTemplate> loadChemicalTemplate ()
		{
			var db = getLocalDB ();
			var chemicalTemplates = from t in db.Table<ChemicalTemplate>() select t;
			Dictionary<string,ChemicalTemplate> templates = new Dictionary<string, ChemicalTemplate>();


			foreach (var t in chemicalTemplates) {
				templates.Add (t.templateName,t);
			}

			return templates;
		}

		public bool insertSeedHistory(SeedDB s){

			LocalStorage.getLocalStorageManager().deleteObject(s.farmName,s.fieldName);

			var db = getLocalDB ();
			//sychSeed (s);

			if (db.Insert (s) != 0) {
				db.Close ();
				return true;
			} else {
				db.Close ();
				return false;
			}
		}

		public bool insertChemicalHistory(ChemicalDB c){

			var db = getLocalDB ();
			if(c.id == 0 && c.isSynch == true  ){
				// no id but synch
				var items = from chemical in db.Table<ChemicalDB> () where chemical.serverId == c.serverId select chemical;
				foreach (var chemical in items){
					db.Delete (chemical);
				}

				db.Insert (c);
				db.Close ();

			}else if (c.id == 0 && c.isSynch == false){
			//no id no synch
				db.Insert (c);
				db.Close ();


			}else if ( c.id !=0 &&  c.serverId ==  null){
			// has id but nerver synch
				var items = from chemical in db.Table<ChemicalDB> () where chemical.id == c.id select chemical;
				foreach(var chemical in items){
					db.Delete (chemical);
				}

				db.Insert (c);
				db.Close ();

			}else if (c.id !=0 && c.serverId !=  null ){
			// has id,  synched , need update
				var items = from chemical in db.Table<ChemicalDB> () where chemical.serverId == c.serverId select chemical;
				foreach (var chemical in items){
					db.Delete (chemical);
				}
				db.Insert (c);
				db.Close ();
			}
			return true;
		}

		public void sychSeed(SeedDB s){
			// 1. upload 
			uploadSynch (s);
			// 2. download
			downloadSynch ("Seed");
		}

		public void sychAllUnsychSeed(){


			var db = getLocalDB ();
			var seeds = from sd in db.Table<SeedDB>() select sd;
			if (seeds != null) {
				if (seeds.Count () > 0) {
					SeedDB s = new SeedDB ();
					uploadSynch (s);
					downloadSynch ("Seed");
				} else {
			
					var seedInit = WebRequestManager.getWebRequestManager ().downloadAllSeed ();

					foreach (var seed in seedInit) {

						db.Insert (seed);
					}
					db.Close ();
				}
			}
		}

		public void sychChemical(ChemicalDB c){
			uploadSynch (c);
			downloadSynch ("Chemical");
		}
		public void sychAllUnsychChemical(){

			var db = getLocalDB ();
			var chemicals = from ch in db.Table<ChemicalDB> () select ch;
			if (chemicals != null) {
				if (chemicals.Count () > 0) {
					ChemicalDB c = new ChemicalDB ();
					uploadSynch (c);
					downloadSynch ("Chemical");
				} else {
					var chemicalInit = WebRequestManager.getWebRequestManager ().downloadAllChemical ();

					foreach (var chemical in chemicalInit) {
						db.Insert (chemical);
					}
					db.Close ();
				}

			}
		}

		public void uploadSynch (Object obj){
			if(obj is SeedDB){

				var seed = (SeedDB)obj;

				if (seed.farmName != null) {
					string updateTime = "";
					updateTime = WebRequestManager.getWebRequestManager ().uploadObject (seed).First();
					seed.lastUpdate = updateTime;
					seed.isSynch = true;
					insertSeedHistory (seed);
				} else {

					var seedUnsynch = from s in getLocalDB ().Table<SeedDB> () where s.isSynch == false select s;
					foreach (var seedU in seedUnsynch)
					{
						string updTime = "";
						updTime = WebRequestManager.getWebRequestManager ().uploadObject (seedU).First();
						seedU.lastUpdate = updTime;
						seedU.isSynch = true;
						insertSeedHistory (seedU);
					}
				}

			}

			if (obj is ChemicalDB){

				Console.Out.WriteLine ("Chemical obj");
				var chemical = (ChemicalDB)obj;

				if (chemical.farmName != null) {

					if(chemical.serverId == null){
						var rtn = WebRequestManager.getWebRequestManager ().uploadObject (chemical);
						var db = getLocalDB ();
						ChemicalDB c = new ChemicalDB ();
						c.chemicalDate = chemical.chemicalDate;
						c.chemicalRates = chemical.chemicalRates;
						c.chemicalTypes = chemical.chemicalTypes;
						c.farmName = chemical.farmName;
						c.fieldName = chemical.fieldName;
						c.id = chemical.id;
						c.implementedUsed = chemical.implementedUsed;
						c.note = chemical.note;
						c.templateIndex = chemical.templateIndex;
						c.isSynch = true;
						c.lastUpdate = rtn[1];
						c.serverId = rtn [0];

						db.Delete (chemical);

						db.Close ();
						insertChemicalHistory (c);

					}else{
						var rtn = WebRequestManager.getWebRequestManager ().uploadObject (chemical);
						chemical.isSynch = true;
						chemical.lastUpdate = rtn[1];
						chemical.serverId = rtn [0];
						insertChemicalHistory (chemical);
					}

				} else {
					var chemicalUnsych = from c in getLocalDB ().Table<ChemicalDB> () where c.isSynch == false select c;
					foreach(var chemicalU in chemicalUnsych){

						if (chemicalU.serverId == null) {
							var rtn = WebRequestManager.getWebRequestManager ().uploadObject (chemicalU);
							var db = getLocalDB ();
							ChemicalDB cu = new ChemicalDB ();
							cu = chemicalU;

							cu.chemicalDate = chemicalU.chemicalDate;
							cu.chemicalRates = chemicalU.chemicalRates;
							cu.chemicalTypes = chemicalU.chemicalTypes;
							cu.farmName = chemicalU.farmName;
							cu.fieldName = chemicalU.fieldName;
							cu.id = chemicalU.id;
							cu.implementedUsed = chemicalU.implementedUsed;
							cu.note = chemicalU.note;
							cu.templateIndex = chemicalU.templateIndex;
							cu.lastUpdate = rtn [1];
							cu.serverId = rtn [0];
							cu.isSynch = true;

							db.Delete (chemicalU);
							db.Close ();
							insertChemicalHistory (cu);
						} else {
							var rtn = WebRequestManager.getWebRequestManager ().uploadObject (chemicalU);
							chemicalU.isSynch = true;
							chemicalU.serverId = rtn [0];
							chemicalU.lastUpdate = rtn [1];
							insertChemicalHistory (chemicalU);
						}

					}
				}

			}
		}

		public void downloadSynch (string type){
			List<SynchCheck> checkList = new List<SynchCheck> ();
			List<Object> unsychList = new List<Object> ();

			if (type == "Seed"){

				var seedAll = from sd in getLocalDB().Table<SeedDB>() select sd;

				foreach(var seed in seedAll){
					SynchCheck sdsych = new SynchCheck ();
					sdsych.farmName = seed.farmName;
					sdsych.fieldName = seed.fieldName;
					sdsych.update = seed.lastUpdate;
					sdsych.type = "seed";

					checkList.Add (sdsych);
				}
				 
				unsychList = WebRequestManager.getWebRequestManager ().downloadObject (checkList);

				if(unsychList != null){
					foreach(var seed in unsychList){

						insertSeedHistory ((SeedDB)seed);
					}
				}

			}

			if (type == "Chemical"){
				var chemicalAll = from c in getLocalDB ().Table<ChemicalDB> () select c;

				foreach(var chemical in chemicalAll){
					SynchCheck chemicalSynch = new SynchCheck ();
					chemicalSynch.farmName = chemical.farmName;
					chemicalSynch.fieldName = chemical.fieldName;
					chemicalSynch.update = chemical.lastUpdate;
					chemicalSynch.type = "chemical";
					chemicalSynch.chemicalServerId = chemical.serverId;
					checkList.Add (chemicalSynch);
				}

				unsychList = WebRequestManager.getWebRequestManager ().downloadObject (checkList);
				if(unsychList!=null&&unsychList.Count > 0){

					foreach(var chemical in unsychList){
						insertChemicalHistory ((ChemicalDB)chemical);
					}

				}
			}

		}

	}

	public class SynchCheck
	{
		public string farmName{ set; get;}
		public string fieldName { set; get; }
		public string update { set; get; }
		public string type { set; get;}
		public string chemicalServerId { set; get; }
	}
}

