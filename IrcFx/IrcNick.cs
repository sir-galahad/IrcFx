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
	/// Description of IrcNick.
	/// </summary>
	public class IrcNick : IComparable<IrcNick>
	{
		static char[] legalchars=null;
		public string RawNick{get; private set;}//raw nick will contain @/+ or any other prefixing chars
		public string Nick{get; private set;}
		public string FlagCharacters{get;private set;}
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
		public IrcNick(String rawNick)
		{
			StringBuilder sbuilder=new StringBuilder();
			RawNick=rawNick;
			int x;
			for(x=0;;x++){
				if(IsLegalChar(rawNick[x])){
					//nicks we receive from the server should only have non-legal chars before
					//the nick proper
					break;
				}else{
					sbuilder.Append(rawNick[x]);
				}
			}
			Nick=rawNick.Substring(x);
			FlagCharacters=sbuilder.ToString();
		}
		public int CompareTo(IrcNick other)
		{
			return Nick.CompareTo(other.Nick);
		}
		
		public override string ToString()
		{
			return string.Format(Nick);
		}

	}
}
