/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 1/14/2013
 * Time: 12:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
namespace IrcFx
{
	/// <summary>
	/// This class probably should only be used in the IrcFx internals
	/// it's perpose is simply to add a flag to a collection to state weather
	/// we're still waiting for RPL_ENDOFNAMES
	/// </summary>
	internal class IrcChannelNames
	{
		IrcISupport Support;
		private string name;
		private bool receivedEndOfNames=false;
		private Dictionary<string,IrcNick> users=new Dictionary<string,IrcNick>();
		public IrcChannelNames(String channelName,IrcISupport sup)
		{
			Support=sup;
			name=channelName;
			
		}
		public string Name{get{return name;}private set{}}
		public bool ReceivedEndOfNames{get{return receivedEndOfNames;}private set{}}
		public void AddName(string userName)
		{
			IrcNick nick=new IrcNick(userName,Support);
			users.Add(nick.Nick,nick);
		}
		public void AddNames(string[] userNames)
		{
			foreach(string user in userNames){
				IrcNick nick=new IrcNick(user,Support);
				users.Add(nick.Nick,nick);
			}
		}
		public void SetRecievedEndOfNames()
		{
			receivedEndOfNames=true;
		}
		public void RemoveName(string userName)
		{
			//IrcNick nick=new IrcNick(userName);
			if(users.ContainsKey(userName)){
				users.Remove(userName);
			}else{
				Console.WriteLine("user removal Failed!");
			}
		}
		public List<IrcNick> GetAllUsers()
		{
			List<IrcNick> list=new List<IrcNick>();
			list.AddRange((IEnumerable<IrcNick>)users.Values);
			//foreach(IrcNick n in users.Values){
			 
			//}
			return list;
		}
		public bool ContainsNick(string nick){
			return users.ContainsKey(nick);
		}
		
		public void ReplaceNick(string oldnick,string newnick){
			if(!users.ContainsKey(oldnick)){return;}
			IrcNick nicktoadd= new IrcNick(newnick,Support,this.users[oldnick].CurrentMode);
			
			users.Remove(oldnick);
			users.Add(newnick,nicktoadd);    
		}
		public IrcNick this[string name]{
			get{
				return users[name];
			}
			private set{}
		}
	}
}
