/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 11/11/2010
 * Time: 7:24 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
namespace IrcFx
{
	/// <summary>
	/// Description of Message.
	/// </summary>
	public class IrcMessage
	{
		public String Command{get;private set;}
		public String[] Parameters{get; private set;}
		String Text="";	
		public String Prefix{get;private set;}
		public IrcMessage(String Cmd,String[] Params)
		{
			Command=Cmd;
			Parameters=Params;
			Prefix="";
			int Trailing=0;
				
			//IRC messages are only allowed to have spaces in the trailing param
			//and there can be only one! (one trailing param, as many spaces as you want
			//the trailing param must be the last one in the list as well
			Text+=Cmd;
			foreach(String S in Params)
			{
				if(S.Contains(" ")){
					Trailing++;
					Text+=" :";
					Text+=S;
				}else{
					Text+=" ";
					Text+=S;
				}
			}
			if(Trailing>1) throw new Exception("Too Many trailing params(should be 1 or 0)");
			//newline/carridge return to signify the end of message
			Text+="\r\n";
		}
		
		//this constructor is intended to be used on received messages
		public IrcMessage(String data)
		{
			int iter=0;
			int trailing=-1;
			int paramstart;
			int paramcount=0;
			String[] Params;
			Text=data;
			Params=Text.Split(" ".ToCharArray());
			if(Params[iter][0]==':'){
				Prefix=Params[0].Substring(1);
				iter++;
			}
			Command=Params[iter];
			iter++;
			paramstart=iter;
			while(iter<Params.Length)
			{
				paramcount++;
				if(Params[iter]=="")
				{
					Params[iter]=" ";
				}
				if(Params[iter][0]==':'){
					trailing=iter;
					break;
				}
				iter++;
			}
			Parameters=new String[paramcount];
			iter=paramstart;
			while((iter-paramstart)<paramcount&&iter!=trailing)
			{
				
				Parameters[iter-paramstart]=Params[iter];
				iter++;
			}
			iter--;
			if(trailing>0){
				iter++;
				Parameters[iter-paramstart]=Params[trailing].Substring(1);
				trailing++;
				while(trailing<Params.Length)
				{
					Parameters[iter-paramstart]+=" ";
					Parameters[iter-paramstart]+=Params[trailing];
					trailing++;
				}
			}
			//complicated but it removes the \r\n from the last parameter
			String end="\r\n";
			Parameters[iter-paramstart]=Parameters[iter-paramstart].TrimEnd(end.ToCharArray());
		}
		
		public String GetText()
		{
			return Text;
		}
		
		public override string ToString()
		{
			return string.Format("[IrcMessage Text={0}]", Text);
		}

		public byte[] GetBytes()
		{
			return Encoding.UTF8.GetBytes(Text);
		}
		
		//a bunch of static methods to create standard irc messages
		
		public static IrcMessage GetPassMessage(String Password)
		{
			String[] args=new String[1];
			args[0]=Password;
			return new IrcMessage("PASS",args);
		}
		public static IrcMessage GetUserMessage(IrcUser User)
		{
			String[] args=new String[4];
			args[0]=User.UserName;
			args[1]="0";
			args[2]="*";
			args[3]=User.RealName;
			return new IrcMessage("USER",args);
		}
		public static IrcMessage GetNickMessage(IrcUser User,int nicktotry)
		{
			String[] args=new String[1];
			args[0]=User.NickNames[nicktotry];
			return new IrcMessage("NICK",args);
		}
		
		//use null for key if no key is needed
		public static IrcMessage GetJoinMessage(String Channel,String Key)
		{
			int argcount=2;
			if(Key==null)argcount=1;
			String[] args=new String[argcount];
			args[0]=Channel;
			if(Key!=null)args[1]=Key;
			return new IrcMessage("JOIN",args);
		}
		public static IrcMessage GetLeaveMessage(String Channel,String PartMessage)
		{
			String[] args=new String[2];
			args[0]=Channel;
			args[1]=PartMessage;
			return new IrcMessage("PART",args);
		}
		public static IrcMessage GetMessage(String target,String text)
			
		{
			String[] args=new String[2];
			args[0]=target;
			//make sure emoticons or other one word strings starting with ':' don't get interpreted wrongly
			//(IRC uses " :" to denote a multi word string)
			if(text[0]==':'){
				text=text+" ";
			}
			args[1]=text;
			return new IrcMessage("PRIVMSG",args);
		}
		public static IrcMessage GetPongMessage(string source, string target)
		{
			string[] args;
			if(source==null||target==null){
				args=new String[1];
			}
			else args=new String[2];
			byte currentarg=0;
			if(source!=null){
				args[currentarg]=source;
				currentarg++;
			}
			if(target!=null)
				args[currentarg]=":"+target;
			return new IrcMessage("PONG",args);
		}
		public static IrcMessage GetQuitMessage(string quitmesg)
		{
			string[] args=new String[1];
			args[0]=quitmesg;
			return new IrcMessage("QUIT",args);
		}
			
			
	}
}
