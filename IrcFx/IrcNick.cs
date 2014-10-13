/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 1/17/2013
 * Time: 10:25 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
namespace IrcFx
{
	/// <summary>
	/// a class to represent an Irc nick
	/// </summary>
	public class IrcNick : IComparable<IrcNick>
	{
		static char[] legalchars=null;
		public string Nick{get; private set;}
		public string FlagCharacters{get;private set;}
	 	string NickModeCharacters; //modes you would see attached to a nick like @ or +
	 	string ModesCharacters;//modes as they show up in the MODE command like o or v
	 	public string CurrentMode{get;private set;}
		
	 	static IrcNick(){
			//create our list of legal nick characters
			legalchars=new Char[(0x7d-0x30)];
			int offset=0;
			for(int x=0x30;x<=0x7d;x++){
				
				legalchars[(x-0x30)-offset]=(char)x;
				if(x==0x40)offset=1;
			}
		}
		
		//static method to test the legality of a given char for a nick
		static bool IsLegalChar(char nickChar){
			foreach(char c in legalchars){
				if(c==nickChar){return true;}
			}
			return false;
		}
		
		public IrcNick(String rawNick,IrcISupport support){
			//set up supported modes prefix string is in the form of 
			//"(ov)@+
			String tmp=support["prefix"]; 
			tmp=tmp.TrimStart('('); //remove leading open paren leaving "ov)@+"
			String [] tmps=tmp.Split(')'); //split to "ov" and "@+"
			ModesCharacters=tmps[0];
			NickModeCharacters=tmps[1];
			RawNick=rawNick;
			int x=0;
			if(IsLegalChar(rawNick[0])){
				x=0;
				CurrentMode="";
			}
			else{
				x=1;
				CurrentMode+=rawNick[0];//current mode will use the NickModeCharacters
			}
			Nick=rawNick.Substring(x);
		}
		
		public IrcNick(string nick,IrcISupport support,string currentmode):this(nick,support){
			CurrentMode=currentmode;
		}
		
		public void ModeChange(char modeChar,bool losesMode){	
			char nickModeChar=NickModeCharacters[ModesCharacters.IndexOf(modeChar)];
			if(losesMode==true){
				while(CurrentMode.Contains(new String(new char[]{nickModeChar}))){
					CurrentMode=CurrentMode.Remove(CurrentMode.IndexOf(nickModeChar),1);
				}
			}else{
				if(CurrentMode.Contains(new String(new char[]{nickModeChar}))){
				   	return;
				}
				CurrentMode=CurrentMode+nickModeChar;
			}
		}
		
		public string RawNick{
			get{
				//char nickMod='\0';//the char that will prefix our nick will go here
				StringBuilder sb=new StringBuilder();
				foreach(char tmp in NickModeCharacters){
					if(CurrentMode.Contains(new String(new char[]{tmp}))){
						sb.Append(tmp);
						break;
					}
				}
				sb.Append(Nick);
				return sb.ToString();
			}
			private set{}//raw nick will contain @/+ or any other prefixing chars
		}
		
		public int CompareTo(IrcNick other){
			return Nick.CompareTo(other.Nick);
		}
		
		public override string ToString(){
			return string.Format(Nick);
		}

	}
}
