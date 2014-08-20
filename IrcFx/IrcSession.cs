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
	//TODO: fix handling of NICK messages, add a channels variable to the quit handler
	
	public delegate void MessageHandler(IrcSession s,IrcUser Sender,string Target,string Text);
	public delegate void ServerReplyHandler(IrcSession s,short code,string data);
	public delegate void DisconnectHandler(IrcSession s);
	public delegate void NamesReceivedHandler(IrcSession s,string channel,IrcNick[] names);
	public delegate void ChannelJoinedEventHandler(IrcSession s,string channel,IrcUser user);
	public delegate void ChannelPartedEventHandler(IrcSession s,string channel,IrcUser user, string message);
	public delegate void UserKicked(IrcSession s,string channel, string kicker,string kickee,string message);
	public delegate void UserQuitHandler(IrcSession s,string[] affectedChannels,IrcUser user,string message);
	public delegate void UserNickChangedHandler(IrcSession s,string[] affectdChannels,string oldNick, string newNick);
	public delegate void ChannelModeChanged(IrcSession s,string channel,IrcUser user, string change);
	public enum ServerReplyCode { RPL_ISUPPORT=5, RPL_NAMREPLY=353,RPL_ENDOFNAMES=366};
	
	/// <summary>
	/// IrcSession is the class responsible for implementing the IRC protocol
	/// </summary>
	public class IrcSession
	{
	public static string channelPrefixChars="#&!+.~";
	public event MessageHandler OnMessage;
	public event MessageHandler OnNotice;
	public event ServerReplyHandler OnServerReply;
	public event DisconnectHandler OnDisconnect;
	public event NamesReceivedHandler OnNamesReceived;
	public event ChannelJoinedEventHandler OnChannelJoined; //for when other users (not us) joine the channel
	public event ChannelPartedEventHandler OnChannelParted; //as above
	public event UserQuitHandler OnUserQuit;
	public event UserKicked OnUserKicked;
	public event UserNickChangedHandler OnUserNickChange;
	public event ChannelModeChanged OnChannelModeChange;
	public Boolean LocalEcho=true;
	IrcUser User;
	IrcNetworkInfo Network;
	Socket Connection;
	Thread ReaderThread;
	public Boolean Connected{get; private set;}
	Queue<IrcMessage> SendQueue=new Queue<IrcMessage>();
	Dictionary<String,IrcChannelNames> channels=new Dictionary<string, IrcChannelNames>();
	public IrcISupport Support{get;private set;}
	Object lockObject=new Object();
	public IrcSession(IrcUser user,IrcNetworkInfo net)
	{
		User=user;
		Network=net;
		Connected=false;
		
	}

	public void Connect()
	{
		IrcMessage mesg;
		IPEndPoint ipe;
		Support=new IrcISupport();
		//have to fix this  yup yup
		while((ipe=Network.GetIPEndPoint())!=null)
		{
			Connection=new Socket(ipe.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
			try{Connection.Connect(ipe);}
			catch{
				continue;
			}
			this.Connected=Connection.Connected;
			break;
		}
		mesg=IrcMessage.GetNickMessage(User,0);
		Connection.Send(mesg.GetBytes());
		mesg=IrcMessage.GetPassMessage(Network.GetServerPassword());
		Connection.Send(mesg.GetBytes());
		mesg=IrcMessage.GetNickMessage(User,0);
		Connection.Send(mesg.GetBytes());
		mesg=IrcMessage.GetPassMessage(Network.GetServerPassword());
		Connection.Send(mesg.GetBytes());
		mesg=IrcMessage.GetUserMessage(User);
		Connection.Send(mesg.GetBytes());
		
		//AddToSendQueue(IrcMessage.GetPongMessage());
		ReaderThread=new Thread(new ThreadStart(ReaderWriter));
		ReaderThread.IsBackground=true;
		ReaderThread.Start();
		
	}
		
	private void AddToSendQueue(IrcMessage mesg)
	{
		lock(lockObject){
			SendQueue.Enqueue(mesg);
			//SendQueue.
		}
	}

	public void JoinChannel(String Channel,String Key)
		{
			IrcMessage mesg=IrcMessage.GetJoinMessage(Channel,Key);
			AddToSendQueue(mesg);
			//mesg=IrcMessage.GetPongMessage(null,null);
			//AddToSendQueue(mesg);
			
		}
	public void LeaveChannel(String Channel,String Key)
		{
			IrcMessage mesg=IrcMessage.GetLeaveMessage(Channel,null);
				
			
		}
		
	public void Msg(String target,String Text)
		{
			IrcMessage mesg=IrcMessage.GetMessage(target,Text);
			AddToSendQueue(mesg);
			if(LocalEcho==true){
				OnMessage(this,User,mesg.Parameters[0],mesg.Parameters[1]);
			}
		}
	public void Action(String target,String text){
		StringBuilder sb=new StringBuilder();
		sb.Append('\x01');
		sb.Append("ACTION ");
		sb.Append(text);
		sb.Append('\x01');
		
		Msg(target,sb.ToString());
	}
	public void Quit(String quitmesg)
	{
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
	private void ReaderWriter()
		{
			
			StreamReader sreader;
			IrcMessage mesg;
			sreader=new StreamReader(new NetworkStream(Connection),Encoding.ASCII);
			BufferedNetworkReader breader=new BufferedNetworkReader(new NetworkStream(Connection));
			while(ReaderThread!=null){
				
				try{
					if(Connection.Poll(0,SelectMode.SelectRead)){
						//Console.WriteLine("{0}",sreader.Peek());
						do{
							string text=null;
							
							//text=sreader.ReadLine();
							text=breader.ReadLine();
							if(text==null) continue;
							/*if(text==null){
								
								Connection.Disconnect(true);
								Network.ResetList();
								//Connection.Shutdown(SocketShutdown.Both);
								//Thread.Sleep(100);
								break;
							}*/		
							//string[] mesgs=text.Split("\n\r".ToCharArray());
							//foreach(string msg in mesgs){
								mesg=new IrcMessage(text);
								ReceivedMessage(mesg);
							//}
						//}while(sreader.Peek()!=-1);
						//}while(((NetworkStream)(sreader.BaseStream)).DataAvailable);
						}while(breader.ReadyToRead());
						       
					}
				}catch(Exception ex){
					Console.WriteLine(ex.Message);
					Connected=false;
					ReaderThread=null;
					break;
				}
				Thread.Sleep(100);
				lock(lockObject){
					while(SendQueue.Count!=0){
						//mesg=null;
						mesg=SendQueue.Dequeue();
						//Console.WriteLine(mesg.Command);
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
			OnDisconnect(this);
			sreader.Dispose();
		}
	private void ReceivedMessage(IrcMessage mesg)
		{
			short replyCode;		
			if(short.TryParse(mesg.Command, out replyCode))
			{
				OnServerReply(this,replyCode,mesg.Parameters[1]);
				HandleServerReply(replyCode,mesg);
			}
			string channelName;
			IrcUser user;
			string message;
			switch(mesg.Command){
				case "PRIVMSG":
					if(OnMessage!=null){
						OnMessage(this,new IrcUser(mesg.Prefix),mesg.Parameters[0],mesg.Parameters[1]);
					}
					//Console.WriteLine(mesg);
					break;
				case "PING":
					//Console.WriteLine(mesg.GetText());
					//Console.WriteLine(mesg.Parameters[0]);
					mesg=IrcMessage.GetPongMessage(User.UserName,mesg.Parameters[0]);
					//Console.WriteLine(mesg.GetText());
					AddToSendQueue(mesg);
					break;
				case "NOTICE":
					if(OnNotice!=null){
						OnNotice(this,new IrcUser(mesg.Prefix),mesg.Parameters[0],mesg.Parameters[1]);
					}
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
					//Console.WriteLine("{0} joined {1}",new IrcUser(mesg.Prefix).CurrentNick,channelName);
					if(OnChannelJoined!=null) OnChannelJoined(this,channelName,user);
					break;
				case "NICK":
					user=new IrcUser(mesg.Prefix);
					string newuser=mesg.Parameters[0];
					Console.WriteLine(mesg.Parameters[0]);
					List<string> nickChannels=new List<string>();
					string[] affectedChannels=FindChannelsAffectedByNick(user.CurrentNick);
					foreach(string cName in affectedChannels){
						channels[cName].ReplaceNick(user.CurrentNick,newuser);
					}
					if(OnUserNickChange!=null){
						OnUserNickChange(this,affectedChannels,user.CurrentNick,newuser);
					}
					break;
				case "PART":
					//Console.WriteLine("PART");
					user=new IrcUser(mesg.Prefix);
					channelName=mesg.Parameters[0];
					this.channels[channelName].RemoveName(user.CurrentNick);
					if(mesg.Parameters.Length>1){message=mesg.Parameters[1];}
					else{message="";}
					
					//Console.WriteLine("{0} left {1}",user.CurrentNick,channelName);
					
					if(OnChannelParted!=null){
						OnChannelParted(this,channelName,user,message);
					}
					
					break;
				case "MODE":
					user=new IrcUser(mesg.Prefix);
					if(channelPrefixChars.Contains(new String(new char[]{mesg.Parameters[0][0]}))){
						channelName=mesg.Parameters[0];
						StringBuilder sb=new StringBuilder(mesg.Parameters[1]);
						if(mesg.Parameters.Length>2){
							sb.Append(' ');
							sb.Append(mesg.Parameters[2]);
							IrcNick nick=channels[channelName][mesg.Parameters[2]];
								bool loseMode=false;
							if(mesg.Parameters[1][0]=='-'){
								loseMode=true;
							}
							nick.ModeChange(mesg.Parameters[1][1],loseMode);
						}
						if(OnChannelModeChange!=null){
							OnChannelModeChange(this,channelName,user,sb.ToString());
						}
					}else{
						Console.WriteLine(mesg.ToString());
					}
					break;
				case "QUIT":
					user=new IrcUser(mesg.Prefix);
					if(mesg.Parameters.Length>0){message=mesg.Parameters[0];}
					else{message="";}
					affectedChannels=FindChannelsAffectedByNick(user.CurrentNick);
					foreach(string cName in affectedChannels ){
						channels[cName].RemoveName(user.CurrentNick);
					}
					//Console.WriteLine("calling");
					if(OnUserQuit!=null){
						OnUserQuit(this,affectedChannels,user,message);
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
					Console.WriteLine("this far");
					string kickee=mesg.Parameters[1];
					Console.WriteLine("calling onkicked");
					if(OnUserKicked!=null){
						OnUserKicked(this,channelName,user.CurrentNick,kickee,message);
					}
					break;
					//channels[]
				default:
					//Console.WriteLine(mesg.GetText());
					break;
			}
		}
	private void HandleServerReply(short replyCode,IrcMessage mesg)
		{
		string channel;
		switch((ServerReplyCode)replyCode){
			case ServerReplyCode.RPL_NAMREPLY: 
				string[] names=mesg.Parameters[3].Split(' ');
				channel=mesg.Parameters[2];
				if(!channels.ContainsKey(channel) || channels[channel].ReceivedEndOfNames){
					channels.Remove(channel);
				   	channels.Add(channel,new IrcChannelNames(channel,Support));
				   }
				channels[channel].AddNames(names);
				//Console.WriteLine("+1");
				break;
			case ServerReplyCode.RPL_ENDOFNAMES:
				
				channel=mesg.Parameters[1];
				channels[channel].SetRecievedEndOfNames();
				if(OnNamesReceived!=null){
					OnNamesReceived(this,channel,channels[channel].GetAllUsers().ToArray());
				}
				break;
			case ServerReplyCode.RPL_ISUPPORT :
				//String preffix
				IrcISupport iSpt=new IrcISupport(mesg);
				if(Support["prefix"]!=null){
					Console.WriteLine("prefix is {0}",Support["prefix"]);
				}
				Support=Support.Merge(iSpt);
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
