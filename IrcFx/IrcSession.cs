 /*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 11/13/2010
 * Time: 9:06 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
namespace IrcFx
{
	public enum ServerReplyCode {RPL_WELCOME=1, RPL_ISUPPORT=5, RPL_NAMREPLY=353,RPL_ENDOFNAMES=366,ERR_NOTREGISTERED=451};
	
	/// <summary>
	/// IrcSession is the class responsible for implementing the IRC protocol
	/// </summary>
	public class IrcSession{
		public static string channelPrefixChars="#&!+.~";

		IrcMessageHandler MessageHandler;
		public Boolean LocalEcho=true;

		IrcUser User;
		IrcNetworkInfo Network;
		Socket Connection;
		Thread ReaderThread;
		public bool Connected{get; private set;}
		Queue<IrcMessage> SendQueue=new Queue<IrcMessage>();
		Dictionary<String,IrcChannelNames> channels=new Dictionary<string, IrcChannelNames>();
		public IrcISupport Support{get;private set;}
		Object lockObject=new Object();
	

		public IrcSession(IrcUser user,IrcNetworkInfo net,IrcMessageHandler messageHandler)
		{
			User=user;
			Network=net;
			Connected=false;
			MessageHandler=messageHandler;

		}

		public void Connect(){
			IPEndPoint ipe;
			Support=new IrcISupport();
			while((ipe=Network.GetIPEndPoint())!=null)
			{
				Connection=new Socket(ipe.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
				try{Connection.Connect(ipe);}
				catch{
					continue;
				}
				break;
			}
			ReaderThread=new Thread(new ThreadStart(ReaderWriter));
			ReaderThread.IsBackground=true;
			ReaderThread.Start();
			Register();
			//wait for the registration to go through
			//if it doesn't go through Register() will be called again by
			//the server reply handler
			while(true){
				lock(lockObject){
					if(Connected==true)break;
				}
				Thread.Sleep(100);
			}
		}	
	
		private void Register(){
			string nick=User.GetNextNick();
			IrcMessage mesg;
			mesg=IrcMessage.GetPassMessage(Network.GetServerPassword());
			AddToSendQueue(mesg);
			mesg=IrcMessage.GetNickMessage(nick);
			AddToSendQueue(mesg);
			mesg=IrcMessage.GetUserMessage(User);
			AddToSendQueue(mesg);
		}
	
		private void AddToSendQueue(IrcMessage mesg){
			lock(lockObject)
				SendQueue.Enqueue(mesg);
		}
	
		public void JoinChannel(String Channel,String Key){
			IrcMessage mesg=IrcMessage.GetJoinMessage(Channel,Key);
			AddToSendQueue(mesg);
		}
	
		public void LeaveChannel(String Channel,String message){
			IrcMessage mesg=IrcMessage.GetPartMessage(Channel,message);
			AddToSendQueue(mesg);
		}
		
		public void Msg(String target,String Text){

			IrcMessage mesg=IrcMessage.GetMessage(target,Text);
			if(mesg==null) return;

			AddToSendQueue(mesg);
			if(LocalEcho==true&&MessageHandler!=null)
				MessageHandler.OnChatMessage(this,User,mesg.Parameters[0],mesg.Parameters[1]);
		}
		
		public void Action(String target,String text){
			StringBuilder sb=new StringBuilder();
			sb.Append('\x01');
			sb.Append("ACTION ");
			sb.Append(text);
			sb.Append('\x01');
			Msg(target,sb.ToString());
		}
		
		public void SetChannelMode(string channel, string ModesToSet,string ModeData, bool Unset){
			IrcMessage mesg=IrcMessage.GetChannelModeMessage(channel,ModesToSet,ModeData,Unset);
			Console.WriteLine(mesg.ToString());
			AddToSendQueue(mesg);
		}
		
		public void SetUserMode(string ModesToSet,bool Unset){
			IrcMessage mesg=IrcMessage.GetUserModeMessage(User.CurrentNick,ModesToSet,Unset);
			AddToSendQueue(mesg);
		}
		
		public void Quit(String quitmesg){
			AddToSendQueue(IrcMessage.GetQuitMessage(quitmesg));
			if(Thread.CurrentThread!=ReaderThread){
				Thread.Sleep(500);
			}
		}
	
		public List<IrcNick> GetChannelUsers(String channel){
			if(channels.ContainsKey(channel)){
				return channels[channel].GetAllUsers();
			}
			return null;
		}
	
		private void ReaderWriter(){
			IrcMessage mesg;
			BufferedNetworkReader lineReader=new BufferedNetworkReader(new NetworkStream(Connection));
			while(ReaderThread!=null){
				try{
					if(lineReader.ReadyToRead){
						string text=null;
						text=lineReader.ReadLine();
						mesg=new IrcMessage(text);
						ReceivedMessage(mesg);
					}
				}catch(Exception ex){
					Console.WriteLine(ex.Message);
					Connected=false;
					ReaderThread=null;
					break;
				}
				Thread.Sleep(25);
				lock(lockObject){
					while(SendQueue.Count!=0){
						mesg=SendQueue.Dequeue();
						try{Connection.Send(mesg.GetBytes());}
						catch(Exception ex){
							Console.WriteLine(ex.Message);
							Connected=false;
							ReaderThread=null;
							break;
						}
					}
				}				
				if(Connection.Connected==false){
					Connected=false;
					ReaderThread=null;
					break;
				}
			}
			if(MessageHandler!=null)MessageHandler.OnDisconnect(this);
		}
		
		private void ReceivedMessage(IrcMessage mesg){
			short replyCode;
			//if mesg is a server reply code then we have a specific handler for that
			if(short.TryParse(mesg.Command, out replyCode)){
				if(MessageHandler!=null) MessageHandler.OnServerReply(this,replyCode,mesg.Parameters[1]);
				HandleServerReply(replyCode,mesg);
			}
			string channelName;
			IrcUser user;
			string message;
			switch(mesg.Command){
				case "PRIVMSG":
					if(MessageHandler!=null)
						MessageHandler.OnChatMessage(this,new IrcUser(mesg.Prefix),mesg.Parameters[0],mesg.Parameters[1]);
					break;
				case "PING":
					mesg=IrcMessage.GetPongMessage(User.UserName,mesg.Parameters[0]);
					AddToSendQueue(mesg);
					break;
				case "NOTICE":
					if(MessageHandler!=null)
						MessageHandler.OnNotice(this,new IrcUser(mesg.Prefix),mesg.Parameters[0],mesg.Parameters[1]);
					break;
				case "JOIN":
					channelName=mesg.Parameters[0];
					user=new IrcUser(mesg.Prefix);
					if(user.CurrentNick==this.User.CurrentNick) return;
					if(!this.channels.ContainsKey(channelName)){
						this.channels.Add(channelName,new IrcChannelNames(channelName,Support));
						this.channels[channelName].SetRecievedEndOfNames();
					}
					this.channels[channelName].AddName(new IrcUser(mesg.Prefix).CurrentNick);
					if(MessageHandler!=null) MessageHandler.OnUserJoinedChannel(this,channelName,user);
					break;
				case "NICK":
					user=new IrcUser(mesg.Prefix);
					string newuser=mesg.Parameters[0];
					string[] affectedChannels=FindChannelsAffectedByNick(user.CurrentNick);
					foreach(string cName in affectedChannels)
						channels[cName].ReplaceNick(user.CurrentNick,newuser);
					if(user.CurrentNick==User.CurrentNick) User.CurrentNick=newuser;
					if(MessageHandler!=null)
						MessageHandler.OnUserNickChanged(this,affectedChannels,user.CurrentNick,newuser);
					break;
				case "PART":
					user=new IrcUser(mesg.Prefix);
					channelName=mesg.Parameters[0];
					this.channels[channelName].RemoveName(user.CurrentNick);
					if(mesg.Parameters.Length>1){message=mesg.Parameters[1];}
					else{message="";}
					if(MessageHandler!=null){
						MessageHandler.OnUserPartedChannel(this,channelName,user,message);
					}					
					break;
				case "MODE":
					user=new IrcUser(mesg.Prefix);
					IrcNick nick=null;
					if(channelPrefixChars.Contains(new String(new char[]{mesg.Parameters[0][0]}))){
						channelName=mesg.Parameters[0];
						StringBuilder sb=new StringBuilder(mesg.Parameters[1]);
						if(mesg.Parameters.Length>2){
							sb.Append(' ');
							sb.Append(mesg.Parameters[2]);
							if(channels[channelName].ContainsNick(mesg.Parameters[2]))
								nick=channels[channelName][mesg.Parameters[2]];
							bool loseMode=false;
							if(mesg.Parameters[1][0]=='-'){
								loseMode=true;
							}
							if(mesg.Parameters[1][1]=='v' || mesg.Parameters[1][1]=='o')
								nick.ModeChange(mesg.Parameters[1][1],loseMode);
						}
						if(MessageHandler!=null){
							MessageHandler.OnChannelModeChanged(this,channelName,user,sb.ToString());
						}
					}else{
						StringBuilder sb=new StringBuilder();
						for(int x=1;x<mesg.Parameters.Length;x++){
							sb.Append(mesg.Parameters[x]);
							sb.Append(' ');
						}
						if(MessageHandler!=null)
							MessageHandler.OnUserModeChanged(this,user,sb.ToString().Trim());
					}
					break;
				case "QUIT":
					user=new IrcUser(mesg.Prefix);
					if(mesg.Parameters.Length>0){message=mesg.Parameters[0];}
					else{message="";}
					affectedChannels=FindChannelsAffectedByNick(user.CurrentNick);
					foreach(string cName in affectedChannels )
						channels[cName].RemoveName(user.CurrentNick);
					if(MessageHandler!=null)
						MessageHandler.OnUserQuit(this,affectedChannels,user,message);
					if(user.CurrentNick==User.CurrentNick){
						Connection.Shutdown(SocketShutdown.Both);
						if(MessageHandler!=null){MessageHandler.OnDisconnect(this);}
					}
					break;
				case "KICK":
					user=new IrcUser(mesg.Prefix);
					if(mesg.Parameters.Length>2){
						message=mesg.Parameters[2];
					}else{
						message="";
					}
					channelName=mesg.Parameters[0];
					string kickee=mesg.Parameters[1];
					this.channels[channelName].RemoveName(kickee);
					if(MessageHandler!=null){
						MessageHandler.OnUserKicked(this,channelName,user.CurrentNick,kickee,message);
					}
					break;
				default:
					//Console.WriteLine(mesg.GetText());
					break;
			}
		}
	
		private void HandleServerReply(short replyCode,IrcMessage mesg){
			string channel;
			switch((ServerReplyCode)replyCode){
				case ServerReplyCode.RPL_WELCOME:
					string[] data = mesg.Parameters[1].Split(' ');
					User.CurrentNick=data[data.Length-1].Split('!')[0];
					lock(lockObject){
						Connected=true;
					}
					break;
				case ServerReplyCode.RPL_NAMREPLY: 
					string[] names=mesg.Parameters[3].Split(' ');
					channel=mesg.Parameters[2];
					if(!channels.ContainsKey(channel) || channels[channel].ReceivedEndOfNames){
						channels.Remove(channel);
					   	channels.Add(channel,new IrcChannelNames(channel,Support));
					   }
					channels[channel].AddNames(names);
					break;
				case ServerReplyCode.RPL_ENDOFNAMES:		
					channel=mesg.Parameters[1];
					channels[channel].SetRecievedEndOfNames();
					if(MessageHandler!=null)
						MessageHandler.OnNamesReceived(this,channel,channels[channel].GetAllUsers().ToArray());
					break;
				case ServerReplyCode.RPL_ISUPPORT :
					IrcISupport iSpt=new IrcISupport(mesg);
					Support=Support.Merge(iSpt);
					break;
				case ServerReplyCode.ERR_NOTREGISTERED:
					Register();
					break;
				default:
					//Console.WriteLine(mesg.ToString());
					break;
			}	
		}
	
		private string[] FindChannelsAffectedByNick(string nick){
			if(nick==null)return null;
			List<string> channelNames=new List<string>();
			foreach(IrcChannelNames channel in channels.Values){
				if(channel.ContainsNick(nick)){
					channelNames.Add(channel.Name);
				}
			}
			return channelNames.ToArray();
		}	
	}
}
