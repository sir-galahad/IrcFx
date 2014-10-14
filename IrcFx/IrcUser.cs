/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 11/13/2010
 * Time: 9:07 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace IrcFx
{
	/// <summary>
	/// a classs to represent the local user and remote users for messages that give a nick+host
	/// </summary>
	public class IrcUser
	{
		public String CurrentNick{get;set;}
		public String RealName{get; private set;}
		public String UserName{get; private set;}
		public String[] NickNames{get;private set;}
		int NickNumber=0;
		public IrcUser(String realName,String userName,params string[] nicks)
		{
			RealName=realName;
			UserName=userName;
			
			NickNames=new String[3];
			NickNames=nicks;
			CurrentNick=NickNames[0];
		}
		public IrcUser(string mesgdata)
		{
			string[] data;
			data=mesgdata.Split("!".ToCharArray());
			if(data.Length==1){UserName=data[0];}
			else {UserName=data[1];}
			NickNames=new String[1];
			NickNames[0]=data[0];
			CurrentNick=NickNames[0];
		}
		public string GetNextNick(){
			if(NickNumber>=NickNames.Length){
				return null;
			}
			Console.WriteLine(NickNames[NickNumber]);
			return NickNames[NickNumber++];
		}
		public override string ToString()
		{
			return CurrentNick;
		}
	}
}
