/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 11/13/2010
 * Time: 9:18 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
namespace IrcFx
{
	/// <summary>
	/// Description of NetworkInfo.
	/// </summary>
	public class NetworkInfo
	{
		public String NetworkName{get; private set;}
		List<ServerInfo> ServerList=new List<ServerInfo>();
		int currentServer=0;
		int currentIPAddress=0;
		public NetworkInfo(String Name)
		{
		NetworkName=Name;
		}
		public void AddServer(ServerInfo Server)
		{
			ServerList.Add(Server);
		}
		public void AddServer(String name,int port,String password)
		{
			ServerList.Add(new ServerInfo(name,port,password));
		}
		
		public ServerInfo GetServer()
		{
			return ServerList[currentServer];
		}
		
		public String GetServerPassword()
		{
			return ServerList[currentServer].Password;
		}
		
		protected IPAddress GetIPAddress()
		{
			//this method should loop through the address lists
			//i'll need to revist this code later right now it only returns
			//the same ip once
			IPAddress Address;
			if(IPAddress.TryParse(ServerList[currentServer].Name,out Address))
			{return Address;}
			IPHostEntry Ent=Dns.GetHostEntry(ServerList[currentServer].Name);
			if(currentIPAddress>(Ent.AddressList.Length-1)){
				currentServer++;
				if(currentServer>(ServerList.Count-1))
				   {currentServer=0;
					return null;}
				currentIPAddress=0;
				Address=GetIPAddress();
			}else{
				Address=Ent.AddressList[currentIPAddress++];
			}

			return Address;
		}
		public IPEndPoint GetIPEndPoint()
		{
			IPAddress Address=GetIPAddress();
			if(Address==null) return null;
			return new IPEndPoint(Address,ServerList[currentServer].Port);
		}
		public void ResetList()
		{
			currentServer=0;
			currentIPAddress=0;
		}
	}
}
