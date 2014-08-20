/*
 * Created by SharpDevelop.
 * User: aaron
 * Date: 8/19/2014
 * Time: 6:50 PM
 * 
 * 
 */
using System;
using System.Net.Sockets;
using System.Text;
namespace IrcFx
{
	/// <summary>
	/// a simple buffered reader, because StreamReader doesn't have a non blocking way to call ReadLine()
	/// or even a way to check if ReadLine() would block;
	/// </summary>
	public class BufferedNetworkReader
	{
		NetworkStream NetStream;
		byte[] buffer=new byte[512]; //should be plently as irc message can't be more than 512 bytes
		int head;
		UTF8Encoding codec=new UTF8Encoding();
		public BufferedNetworkReader(NetworkStream stream)
		{
			NetStream=stream;
		}
		public bool ReadyToRead(){
			if(head!=0)return true;
			if(NetStream.DataAvailable)return true;
			return false;
		}
		string GetLine(){
			byte[] temp=new byte[512];
			string line=codec.GetString(buffer);
			if((line.Contains("\r\n"))){
				line=line.Split("\r\n".ToCharArray())[0];
			   	line+="\r\n";
			   	int start=codec.GetByteCount(line);
			   	head=head-start;
			   	Array.ConstrainedCopy(buffer,start,temp,0,head);
			   	buffer=temp;
			   	return line;
			}
			return null;
		}
		public string ReadLine(){
			int bytesread;	
			string line=GetLine();
			if(line!=null){
				return line;
			}else{
				if(NetStream.DataAvailable){
					bytesread=NetStream.Read(buffer,head,512-head);
					head=head+bytesread;
					if(bytesread==0)return null;//server is dead to u	
					return GetLine();
				}
				return null;
			}
		}

	}
}
