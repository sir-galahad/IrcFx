IrcFx
=====

A frame work in C# to implement Irc clients/bots. Note: this framework as been tested to ensure usability under mono

Currently this project is a bit of a pain to build into a client because of all of the events to handle in the 
IrcMessageHandler class (events here are sadly handled in a Java style using one class to implement multiple handling 
methods, because maintaining a seperate message handler for each type of message was becoming  too unweildly.) The next
thing that I believe i would like to do is to create a default client message handler that can interpret the various
message types and send default strings for them to  the client seperating the messages by source. Possibly different 
streams for each source?

Hopefully the Sample implementation can give you an idea of how to use this code as a client.
