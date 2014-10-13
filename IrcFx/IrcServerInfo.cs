/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 11/11/2010
 * Time: 6:49 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace IrcFx
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class IrcServerInfo
	{
		public String Name{get; private set;}
		public int Port{get;private set;}
		public String Password{get; private set;}
		public IrcServerInfo(string name,int port,string password)
		{
			Name=name;
			Port=port;
			Password=password;
		}

	}
}