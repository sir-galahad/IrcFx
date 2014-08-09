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
	/// Description of User.
	/// </summary>
	public class IrcUser
	{
		public String CurrentNick{get; private set;}
		public String RealName{get; private set;}
		public String UserName{get; private set;}
		public String[] NickNames{get;private set;}
		
		public IrcUser(String realName,String userName,String nick1,String nick2,String nick3)
		{
			RealName=realName;
			UserName=userName;
			NickNames=new String[3];
			NickNames[0]=nick1;
			NickNames[1]=nick2;
			NickNames[2]=nick3;
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
		
		
		public override string ToString()
		{
			return CurrentNick;
		}
	}
}
