/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 8/21/2014
 * Time: 5:07 PM
 * 
 * 
 */
using System;

namespace IrcFx
{
	/// <summary>
	/// Unfortunatly we've reached the point where we've implemented too many messages for them to be properly
	/// served by delegates/events as such from now on we're going to register for all messages with a single class
	/// the if you need to only register for only a couple of messages then you probably want IrcMessageAdapter.
	/// </summary>
	public interface IrcMessageHandler
	{
		void OnChatMessage(IrcSession s,IrcUser Sender,string Target,string Text);
		void OnNotice(IrcSession s,IrcUser Sender,string Target,string Text);
		void OnServerReply(IrcSession s,short code,string data);
		void OnDisconnect(IrcSession s);
		void OnNamesReceived(IrcSession s,string channel,IrcNick[] names);
		void OnUserJoinedChannel(IrcSession s,string channel,IrcUser user);
		void OnUserPartedChannel(IrcSession s,string channel,IrcUser user, string message);
		void OnUserKicked(IrcSession s,string channel, string kicker,string kickee,string message);
		void OnUserQuit(IrcSession s,string[] affectedChannels,IrcUser user,string message);
		void OnUserNickChanged(IrcSession s,string[] affectdChannels,string oldNick, string newNick);
		void OnChannelModeChanged(IrcSession s,string channel,IrcUser user, string change);
		void OnUserModeChanged(IrcSession s,IrcUser user,string change);
	}
}
