/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 8/21/2014
 * Time: 5:25 PM
 * 
 * 
 */
using System;

namespace IrcFx
{
	/// <summary>
	/// An all null but overridable implementation of a IIrcMessageHandler
	/// </summary>

	public class IrcMessageHandlerAdapter:IrcMessageHandler{

		public IrcMessageHandlerAdapter(){
		}
		
		public virtual void OnChatMessage(IrcSession s,IrcUser Sender,string Target,string Text){}
		public virtual void OnNotice(IrcSession s,IrcUser Sender,string Target, string Text){}
		public virtual void OnServerReply(IrcSession s,short code,string data){}
		public virtual void OnDisconnect(IrcSession s){}
		public virtual void OnNamesReceived(IrcSession s,string channel,IrcNick[] names){}
		public virtual void OnUserJoinedChannel(IrcSession s,string channel,IrcUser user){}
		public virtual void OnUserPartedChannel(IrcSession s,string channel,IrcUser user, string message){}
		public virtual void OnUserKicked(IrcSession s,string channel, string kicker,string kickee,string message){}
		public virtual void OnUserQuit(IrcSession s,string[] affectedChannels,IrcUser user,string message){}
		public virtual void OnUserNickChanged(IrcSession s,string[] affectdChannels,string oldNick, string newNick){}
		public virtual void OnChannelModeChanged(IrcSession s,string channel,IrcUser user, string change){}
		public virtual void OnUserModeChanged(IrcSession s,IrcUser user, string change){}
	}
}
