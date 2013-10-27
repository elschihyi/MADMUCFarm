using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace MadmucFarm
{
	public partial class Login : DialogViewController
	{
		public Login () : base (UITableViewStyle.Grouped, null)
		{
			Root = new RootElement ("Login") {};

			var input = new Section(){};
			var userName=new EntryElement ("User Name", "Enter your user name",null);
			var password = new EntryElement ("Password", "Enter your password", null, true);
			input.Add (userName);
			input.Add (password);
			Root.Add (input);
	

			var submit = new Section () { };
			var btnSubmit=new StringElement("Submit",()=>{

				Global.userName=userName.Value;
				Global.password=BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(password.Value))).Replace("-", "").ToLower();
				var sync=new Synchronization();
				if(!Synchronization.hasConnection()){
					new UIAlertView ("Error", "No connection!", null, "Continue").Show ();
					return;
				}

				//add a loading view
				var rec = new RectangleF (new PointF (UIScreen.MainScreen.Bounds.X,  UIScreen.MainScreen.Bounds.Y), UIScreen.MainScreen.Bounds.Size);
				Authenticate authentication=new Authenticate (rec);
				View.Add (authentication);

				int hasUser=-1;
				bool pass=false;
				Task.Factory.StartNew (
					// tasks allow you to use the lambda syntax to pass work
					() => {
					hasUser=sync.hasUser();
					pass=DBConnection.isUser(Global.userName,Global.password);
				}
				).ContinueWith ( 
				                t => {
					authentication.Hide ();
					if(hasUser==0||hasUser==1){
						DBConnection.insertUser(Global.userName,Global.password,hasUser);
						Global.isAdmin=Convert.ToBoolean(hasUser);
						var farm=new SelectFarm();
						this.NavigationController.PushViewController(farm,true);
					}
					else if(pass){
						Global.isAdmin=DBConnection.isAdmin(Global.userName);
						var farm=new SelectFarm();
						this.NavigationController.PushViewController(farm,true);
					}
					else{
						new UIAlertView ("Error", "Wrong UserName or Password!", null, "Continue").Show (); 
					}
				}, TaskScheduler.FromCurrentSynchronizationContext()
				);



			});
			submit.Add (btnSubmit);
			Root.Add (submit);

			/*
			//code to add user
			var addUser=new Section(){};
			var btnAddUser=new StringElement("Adduser",()=>{
				Console.WriteLine("Add user name: "+userName.Value);
				Console.WriteLine("The password is: "+password.Value);
				DBConnection.insertUser(userName.Value,BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(password.Value))).Replace("-", ""),1);
			});
			addUser.Add(btnAddUser);
			Root.Add(addUser);
			//end of code for add user
			*/
		}

	}
}
