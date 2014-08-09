/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 8/8/2014
 * Time: 5:59 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;
namespace IrcFx
{
	/// <summary>
	/// This class will save the options sent in the ISUPPORT message
	/// in the form of a string,string dictionary
	/// unary options will be keys with null values;
	/// </summary>
	public class IrcISupport
	{
		Dictionary<string, string> options=new Dictionary<string, string>();
		public IrcISupport()
		{
			//make an empty object
		}
		public IrcISupport(IrcMessage mesg){
			//verify that the message is an ISUPPORT message
			short replyCode;		
			if(mesg==null||!(short.TryParse(mesg.Command, out replyCode)) || !(replyCode==5))
			{
				return;
			}
			//populate the dictionary
			char[] delimiter={'='};
			for(int x=1;x<mesg.Parameters.Length-1;x++){
				string[] tmp=mesg.Parameters[x].Split(delimiter);
				if(tmp.Length==1){
					options.Add(tmp[0].ToLower(),"");
				}else{
					options.Add(tmp[0].ToLower(),tmp[1]);
				}
			}
			
		}
		//used to combine two IrcISupport objects;
		public IrcISupport(IrcISupport first,IrcISupport second){
			IrcISupport[] supportarray={first,second};
			foreach(IrcISupport sup in supportarray){
				foreach(string key in sup.Options){
					options.Add(key,sup[key]);
				}
			}
			
		}
		public string this[string s]{
			get{
				if(options.ContainsKey(s)){
					return options[s.ToLower()];}
				else return null;
			}
			private set{}
		}
		//get a list of all the keys
		public string[] Options{
			get{
				string[] result=new string[options.Keys.Count];
				int x=0;
				foreach(string s in options.Keys){
					result[x++]=s;
				}
				return result;
			}
			private set{}
		}
		public IrcISupport Merge(IrcISupport other)
		{
			return new IrcISupport(this,other);
		}
	}
}
