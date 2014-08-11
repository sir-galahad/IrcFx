/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 11/13/2010
 * Time: 9:04 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using IrcFx;
using System.Threading;
namespace Sample
{
	class Sample
	{
		IrcSession mySession;
		//string channel="##csharp";
		string channel="##programming";
		//string channel="##testroom";
		public static void Main(string[] args)
		{
			Sample client=new Sample();
			client.Run();
		}
		
		public void Run(){
			
			Console.WriteLine("Welcome to sIRC");
			Console.WriteLine("This progam is intended to be an example of how to use IrcFx");
			IrcNetworkInfo mynet=new IrcNetworkInfo("bleh");
			mynet.AddServer("irc.freenode.net",6667,"double9");
			IrcUser me=new IrcUser("Aaron H Davis","gala","galah","Sir_galahad","bleh");
			mySession=new IrcSession(me,mynet);
			mySession.OnMessage+=new MessageHandler(mesghandle);
			mySession.OnNotice+=new MessageHandler((s,se,t,m)=>{Console.WriteLine("-*{0}*- {1}",se,m);});
			mySession.OnServerReply+=new ServerReplyHandler(rplyhandle);
			mySession.OnDisconnect+=new DisconnectHandler(Reconnect);
			/*mySession.OnNamesReceived+=new NamesReceivedHandler((s,c,a)=>{
			                                                    	foreach(IrcNick n in a){ 
			                                                    		Console.WriteLine(n);
			                                                    	}
			                                                    });
			                                                   */
			mySession.OnChannelJoined+=new ChannelJoinedEventHandler((s,c,u)=>Console.WriteLine("{0} joined {1} ({2})",u.CurrentNick,c,
			                                                                                    mySession.GetChannelUsers(channel).Count));
			mySession.OnUserQuit+=new UserQuitHandler((s,c,u,m)=>{Console.WriteLine("{0} quit [{1}]",u.CurrentNick,m);});
			mySession.OnUserKicked+=new UserKicked((s,c,u,k,m)=>{Console.WriteLine("{0} kicked by {1} [{2}]",k,u,m);});
			mySession.OnChannelModeChange+=new ChannelModeChanged( (s,c,u,m)=>{Console.WriteLine("{0} set mode to {1}",u,m);});
			mySession.Connect();
			if(mySession.Connected==true)
				Console.WriteLine("Connected");
			else
			{
				Console.WriteLine("Connection Failed!!");
				return;
			}
			mySession.JoinChannel(channel,null);
			//IrcMessage test=IrcMessage.GetUserMessage(me);
			while(true)
			{
				string bleh=Console.ReadLine();
				HandleLocalInput(bleh);
			}
			
		}
		public void mesghandle(IrcSession s,IrcUser sender,string target,string text)
		{
			char[] trimOut=new Char[1];
			string cmd=null;;
			trimOut[0]='\x0001';
			if(text[0]==1 && text[text.Length-1]==1){
				text=text.Trim(trimOut);
				cmd=text.Split(' ')[0];
				text=text.Remove(0,cmd.Length);
				text=text.Trim();
			}
			if(cmd!=null && cmd=="ACTION"){
				Console.WriteLine("*{0} {1}*",sender.CurrentNick,text);
			}
			else{
				Console.WriteLine("<{0}> {1}",sender.CurrentNick,text);
			}
		}
		
		public void rplyhandle(IrcSession s,short code,string data)
		{
			Console.WriteLine("{1}Server: {0}",data,code);
		}
		
		public void Reconnect(IrcSession s)
		{
			//s.Quit("nou");
			Console.WriteLine("Press any key to continue...");
			//Console.ReadKey();
			Environment.Exit(0);
		}
		public void HandleLocalInput(string input){
			char[] seperator=new Char[1];
			seperator[0]=' ';
			input=input.Trim();
			string[] args=input.Split(seperator);
			switch(args[0]){
				case "":
					break;
				case "/quit":
					mySession.Quit("gala out!");
					//while(mySession.Connected){Thread.Sleep(1000);}
					//Environment.Exit(0);
					break;
				case "/msg":
					if(args.Length<3){break;}
					String text=args[2];
					if(args.Length>3){
						for(int x=3;x<args.Length;x++){
							text=String.Concat(text," ");
							text=String.Concat(text,args[x]);
						}
					}
					mySession.Msg(args[1],text);
					break;
				case "/list":
					foreach(IrcNick nick in mySession.GetChannelUsers(channel)){
						Console.Write("{0} ",nick.RawNick);
					}
					Console.WriteLine("");
					break;
				case "/me":
					if(args.Length<2){break;}
					text=args[1];
					if(args.Length>1){
						for(int x=2;x<args.Length;x++){
							text=String.Concat(text," ");
							text=String.Concat(text,args[x]);
						}
					}
					mySession.Action(channel,text);
					break;
				default:
					if(input[0]=='/'){break;}
					//if(input[0]==':')
					mySession.Msg(channel,input);
					break;
			}
		}
	}
}