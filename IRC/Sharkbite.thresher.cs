/*
 * Thresher IRC client library
 * Copyright (C) 2002 Aaron Hunter <thresher@sharkbite.org>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * 
 * See the gpl.txt file located in the top-level-directory of
 * the archive of this library for complete text of license.
*/

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

#if SSL
using Org.Mentalis.Security.Ssl;
#endif

namespace Sharkbite.Irc
{
    public class ChannelModeInfo
    {

        private ModeAction action;
        private ChannelMode mode;
        private string parameter;

        public ModeAction Action { get { return action; } set { action = value; } }
        public ChannelMode Mode { get { return mode; } set { mode = value; } }
        public string Parameter { get { return parameter; } set { parameter = value; } }

        public override string ToString() { return string.Format("Action={0} Mode={1} Parameter={2}", Action, Mode, Parameter); }

        internal static ChannelModeInfo[] ParseModes(string[] tokens, int start)
        {
            //This nice piece of code was contributed by Klemen �avs.
            //25 October 2003
            ArrayList modeInfoArray = new ArrayList();
            int i = start;
            while (i < tokens.Length)
            {
                ChannelModeInfo modeInfo = new ChannelModeInfo();
                int parmIndex = i + 1;
                for (int j = 0; j < tokens[i].Length; j++)
                {

                    while (j < tokens[i].Length && tokens[i][j] == '+')
                    {
                        modeInfo.Action = ModeAction.Add;
                        j++;
                    }
                    while (j < tokens[i].Length && tokens[i][j] == '-')
                    {
                        modeInfo.Action = ModeAction.Remove;
                        j++;
                    }
                    if (j == 0) { throw new Exception(); }
                    else if (j < tokens[i].Length)
                    {
                        switch (tokens[i][j])
                        {
                            case 'o':
                            case 'h':
                            case 'v':
                            case 'b':
                            case 'e':
                            case 'I':
                            case 'k':
                            case 'O':
                                modeInfo.Mode = Rfc2812Util.CharToChannelMode(tokens[i][j]);
                                modeInfo.Parameter = tokens[parmIndex++];
                                break;
                            case 'l':
                                modeInfo.Mode = Rfc2812Util.CharToChannelMode(tokens[i][j]);
                                if (modeInfo.Action == ModeAction.Add)
                                {
                                    modeInfo.Parameter = tokens[parmIndex++];
                                }
                                else
                                {
                                    modeInfo.Parameter = "";
                                }
                                break;
                            default:
                                modeInfo.Mode = Rfc2812Util.CharToChannelMode(tokens[i][j]);
                                modeInfo.Parameter = "";
                                break;
                        }

                    }
                    modeInfoArray.Add(modeInfo.MemberwiseClone());
                }
                i = parmIndex;
            }

            ChannelModeInfo[] modes = new ChannelModeInfo[modeInfoArray.Count];
            for (int k = 0; k < modeInfoArray.Count; k++) { modes[k] = (ChannelModeInfo)modeInfoArray[k]; }
            return modes;
        }
    }

    public abstract class CommandBuilder
    {
        // Buffer to hold commands 
        private StringBuilder commandBuffer;
        //Containing conenction instance
        private Connection connection;

        internal const char SPACE = ' ';
        internal const string SPACE_COLON = " :";
        internal const int MAX_COMMAND_SIZE = 512;
        internal const char CtcpQuote = '\u0001';

        internal CommandBuilder(Connection connection)
        {
            this.connection = connection;
            commandBuffer = new StringBuilder(MAX_COMMAND_SIZE);
        }

        internal Connection Connection { get { return connection; } }
        internal StringBuilder Buffer { get { return commandBuffer; } }

        internal void SendMessage(string type, string target, string message)
        {
            commandBuffer.Append(type);
            commandBuffer.Append(SPACE);
            commandBuffer.Append(target);
            commandBuffer.Append(SPACE_COLON);
            commandBuffer.Append(message);
            connection.SendCommand(commandBuffer);
        }

        internal void ClearBuffer() { commandBuffer.Remove(0, commandBuffer.Length); }

        internal string[] BreakUpMessage(string message, int maxSize)
        {
            int pieces = (int)Math.Ceiling((float)message.Length / (float)maxSize);
            string[] parts = new string[pieces];
            for (int i = 0; i < pieces; i++)
            {
                int start = i * maxSize;
                if (i == pieces - 1)
                    parts[i] = message.Substring(start);
                else
                    parts[i] = message.Substring(start, maxSize);
            }
            return parts;
        }
    }

    public class Connection
    {
        public event RawMessageReceivedEventHandler OnRawMessageReceived;
        public event RawMessageSentEventHandler OnRawMessageSent;


#if SSL
		private SecureTcpClient client;
#else
        private TcpClient client;
#endif

        private readonly Regex propertiesRegex;
        private Listener listener;
        private Sender sender;
        private CtcpListener ctcpListener;
        private CtcpSender ctcpSender;
        private CtcpResponder ctcpResponder;
        private bool ctcpEnabled;
        private bool dccEnabled;
        private Thread socketListenThread;
        private StreamReader reader;
        private DateTime timeLastSent;
        //Connected and registered with IRC server
        private bool registered;
        //TCP/IP connection established with IRC server
        private bool connected;
        private bool handleNickFailure;
        private ArrayList parsers;
        private ServerProperties properties;
        private Encoding encoding;

        internal StreamWriter writer; //Access is internal for testing
        internal ConnectionArgs connectionArgs;

        internal Connection(ConnectionArgs args)
        {
            connectionArgs = args;
            sender = new Sender(this);
            listener = new Listener();
            timeLastSent = DateTime.Now;
            EnableCtcp = true;
            EnableDcc = true;
            TextEncoding = Encoding.Default;
        }

        public Connection(ConnectionArgs args, bool enableCtcp, bool enableDcc)
        {
            propertiesRegex = new Regex("([A-Z]+)=([^\\s]+)", RegexOptions.Compiled | RegexOptions.Singleline);
            registered = false;
            connected = false;
            handleNickFailure = true;
            connectionArgs = args;
            parsers = new ArrayList();
            sender = new Sender(this);
            listener = new Listener();
            RegisterDelegates();
            timeLastSent = DateTime.Now;
            EnableCtcp = enableCtcp;
            EnableDcc = enableDcc;
            TextEncoding = Encoding.Default;
        }

        public Connection(Encoding textEncoding, ConnectionArgs args, bool enableCtcp, bool enableDcc)
            : this(args, enableCtcp, enableDcc)
        {
            TextEncoding = textEncoding;
        }

        public Encoding TextEncoding { get { return encoding; } set { encoding = value; } }

        public bool Registered { get { return registered; } }
        public bool Connected { get { return connected; } }

        public bool HandleNickTaken { get { return handleNickFailure; } set { handleNickFailure = value; } }

        public string Name { get { return connectionArgs.Nick + "@" + connectionArgs.Hostname; } }

        public bool EnableCtcp
        {
            get
            {
                return ctcpEnabled;
            }
            set
            {
                if (value && !ctcpEnabled)
                {
                    ctcpListener = new CtcpListener(this);
                    ctcpSender = new CtcpSender(this);
                }
                else if (!value)
                {
                    ctcpListener = null;
                    ctcpSender = null;
                }
                ctcpEnabled = value;
            }
        }

        public bool EnableDcc { get { return dccEnabled; } set { dccEnabled = value; } }

        public CtcpResponder CtcpResponder
        {
            get
            {
                return ctcpResponder;
            }
            set
            {
                if (value == null && ctcpResponder != null)
                {
                    ctcpResponder.Disable();
                }
                ctcpResponder = value;
            }
        }

        public TimeSpan IdleTime { get { return DateTime.Now - timeLastSent; } }
        public Sender Sender { get { return sender; } }
        public Listener Listener { get { return listener; } }
        public CtcpSender CtcpSender { get { return ctcpSender; } }

        public CtcpListener CtcpListener
        {
            get
            {
                if (ctcpEnabled)
                    return ctcpListener;
                else
                    return null;
            }
        }

        public ConnectionArgs ConnectionData { get { return connectionArgs; } }
        public ServerProperties ServerProperties { get { return properties; } }

        private bool CustomParse(string line)
        {
            foreach (IParser parser in parsers)
            {
                if (parser.CanParse(line))
                {
                    parser.Parse(line);
                    return true;
                }
            }
            return false;
        }

        private void KeepAlive(string message) { sender.Pong(message); }

        private void MyNickChanged(UserInfo user, string newNick)
        {
            if (connectionArgs.Nick == user.Nick) { connectionArgs.Nick = newNick; }
        }
        private void OnRegistered()
        {
            registered = true;
            listener.OnRegistered -= new RegisteredEventHandler(OnRegistered);
        }
        private void OnNickError(string badNick, string reason)
        {
            //If this is our initial connection attempt
            if (!registered && handleNickFailure)
            {
                NameGenerator generator = new NameGenerator();
                string nick;
                do
                {
                    nick = generator.MakeName();
                }
                while (!Rfc2812Util.IsValidNick(nick) || nick.Length == 1);
                //Try to reconnect
                Sender.Register(nick);
            }
        }
        private void OnReply(ReplyCode code, string info)
        {
            if (code == ReplyCode.RPL_BOUNCE) //Code 005
            {
                if (properties == null) { properties = new ServerProperties(); }

                MatchCollection matches = propertiesRegex.Matches(info);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        properties.SetProperty(match.Groups[1].ToString(), match.Groups[2].ToString());
                    }
                }
                //Extract ones we are interested in
                ExtractProperties();
            }
        }
        private void ExtractProperties()
        {
			// This seems to've been unused.
			// TODO: Remove this
		}
        private void RegisterDelegates()
        {
            listener.OnPing += new PingEventHandler(KeepAlive);
            listener.OnNick += new NickEventHandler(MyNickChanged);
            listener.OnNickError += new NickErrorEventHandler(OnNickError);
            listener.OnReply += new ReplyEventHandler(OnReply);
            listener.OnRegistered += new RegisteredEventHandler(OnRegistered);
        }

#if SSL
		private void ConnectClient( SecureProtocol protocol )   
		{
			lock ( this ) 
			{
				if( connected ) 
				{
					throw new Exception("Connection with IRC server already opened.");
				}
				Debug.WriteLineIf( Rfc2812Util.IrcTrace.TraceInfo,"[" + Thread.CurrentThread.Name +"] Connection::Connect()");
			
					SecurityOptions options = new SecurityOptions( protocol );
					options.Certificate = null;
					options.Entity = ConnectionEnd.Client;
					options.VerificationType = CredentialVerification.None;
					options.Flags = SecurityFlags.Default;
					options.AllowedAlgorithms = SslAlgorithms.SECURE_CIPHERS;
					client = new SecureTcpClient( options );		
					client.Connect( connectionArgs.Hostname, connectionArgs.Port );
			
				connected = true;
				writer = new StreamWriter( client.GetStream(), TextEncoding );
				writer.AutoFlush = true;
				reader = new StreamReader(  client.GetStream(), TextEncoding );
				socketListenThread = new Thread(new ThreadStart( ReceiveIRCMessages ) );
				socketListenThread.Name = Name;
				socketListenThread.Start();		
				sender.RegisterConnection( connectionArgs );
			}
		}
#endif

        internal void ReceiveIRCMessages()
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Connection::ReceiveIRCMessages()");
            string line;
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Connection::ReceiveIRCMessages() rec'd:" + line);

                        if (CustomParse(line)) { continue; }
						
                        if (DccListener.IsDccRequest(line) && dccEnabled)
                        {
                            DccListener.DefaultInstance.Parse(this, line);
                        }
                        else if (CtcpListener.IsCtcpMessage(line) && ctcpEnabled)
                        {
                            ctcpListener.Parse(line);
                        }
                        else
                        {
                            listener.Parse(line);
                        }
                        if (OnRawMessageReceived != null) { OnRawMessageReceived(line); }
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                        break;
                    }
                }
            }
            catch (IOException e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Connection::ReceiveIRCMessages() IO Error while listening for messages " + e);
                listener.Error(ReplyCode.ConnectionFailed, "Connection to server unexpectedly failed.");
            }
            client.Close();
            registered = false;
            connected = false;
            listener.Disconnected();
        }
        internal void SendCommand(StringBuilder command)
        {
            try
            {
                writer.WriteLine(command.ToString());
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Connection::SendCommand() sent= " + command);
                timeLastSent = DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Connection::SendCommand() exception=" + e);
            }
            if (OnRawMessageSent != null) { OnRawMessageSent(command.ToString()); }
            command.Remove(0, command.Length);
        }
        internal void SendAutomaticReply(StringBuilder command)
        {
            try
            {
                writer.WriteLine(command.ToString());
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Connection::SendAutomaticReply() message=" + command);
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Connection::SendAutomaticReply() exception=" + e);
            }
            command.Remove(0, command.Length);
        }


#if SSL
		public void Connect() 
		{
			Debug.WriteLineIf( Rfc2812Util.IrcTrace.TraceInfo,"Connecting over clear socket");
			ConnectClient( SecureProtocol.None );
		}
		public void SecureConnect() 
		{
			Debug.WriteLineIf( Rfc2812Util.IrcTrace.TraceInfo,"Connecting over encrypted socket");
			ConnectClient( SecureProtocol.Tls1 );
		}
#else

        public void Connect()
        {
            lock (this)
            {
                if (connected)
                {
                    throw new Exception("Connection with IRC server already opened.");
                }
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Connection::Connect()");
                client = new TcpClient();
                client.Connect(connectionArgs.Hostname, connectionArgs.Port);
                connected = true;
                writer = new StreamWriter(client.GetStream(), TextEncoding);
                writer.AutoFlush = true;
                reader = new StreamReader(client.GetStream(), TextEncoding);
                socketListenThread = new Thread(new ThreadStart(ReceiveIRCMessages));
                socketListenThread.Name = Name;
                socketListenThread.Start();
                sender.RegisterConnection(connectionArgs);
            }
        }
#endif

        public void Disconnect(string reason)
        {
            lock (this)
            {
                if (!connected) { throw new Exception("Not connected to IRC server."); }
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Connection::Disconnect()");
                listener.Disconnecting();
                sender.Quit(reason);
                listener.Disconnected();
                //Thanks to Thomas for this next block
                if (socketListenThread.Join(TimeSpan.FromSeconds(1)) == false)
                    socketListenThread.Abort();
            }
        }

        public override string ToString() { return this.Name; }
        public void AddParser(IParser parser) { parsers.Insert(0, parser); }
        public void RemoveParser(IParser parser) { parsers.Remove(parser); }
    }

    public struct ConnectionArgs
    {
        private string realName;
        private string nickName;
        private string userName;
        private string modeMask;
        private string hostname;
        private int port;
        private string serverPassword;

        public ConnectionArgs(string name, string hostname)
        {
            realName = name;
            nickName = name;
            userName = name;
            modeMask = "4";
            this.hostname = hostname;
            port = 6667;
            serverPassword = "*";
        }

        public string Hostname { get { return hostname; } set { hostname = value; } }
        public string ModeMask { get { return modeMask; } set { modeMask = value; } }
        public string Nick { get { return nickName; } set { nickName = value; } }
        public int Port { get { return port; } set { port = value; } }
        public string RealName { get { return realName; } set { realName = value; } }
        public string UserName { get { return userName; } set { userName = value; } }
        public string ServerPassword { get { return serverPassword; } set { serverPassword = value; } }
    }

    #region Delegates.cs

    public delegate void ReplyEventHandler(ReplyCode code, string message);
    public delegate void ErrorMessageEventHandler(ReplyCode code, string message);
    public delegate void AwayEventHandler(string nick, string awayMessage);
    public delegate void InviteSentEventHandler(string nick, string channel);
    public delegate void NickErrorEventHandler(string badNick, string reason);
    public delegate void PingEventHandler(string message);
    public delegate void RegisteredEventHandler();
    public delegate void DisconnectingEventHandler();
    public delegate void DisconnectedEventHandler();
    public delegate void PublicNoticeEventHandler(UserInfo user, string channel, string notice);
    public delegate void PrivateNoticeEventHandler(UserInfo user, string notice);
    public delegate void JoinEventHandler(UserInfo user, string channel);
    public delegate void ActionEventHandler(UserInfo user, string channel, string description);
    public delegate void PrivateActionEventHandler(UserInfo user, string description);
    public delegate void PublicMessageEventHandler(UserInfo user, string channel, string message);
    public delegate void NickEventHandler(UserInfo user, string newNick);
    public delegate void PrivateMessageEventHandler(UserInfo user, string message);
    public delegate void TopicEventHandler(UserInfo user, string channel, string newTopic);
    public delegate void TopicRequestEventHandler(string channel, string topic);
    public delegate void PartEventHandler(UserInfo user, string channel, string reason);
    public delegate void QuitEventHandler(UserInfo user, string reason);
    public delegate void InviteEventHandler(UserInfo user, string channel);
    public delegate void KickEventHandler(UserInfo user, string channel, string kickee, string reason);
    public delegate void NamesEventHandler(string channel, string[] nicks, bool last);
    public delegate void ListEventHandler(string channel, int visibleNickCount, string topic, bool last);
    public delegate void IsonEventHandler(string nicks);
    public delegate void WhoEventHandler(UserInfo user, string channel, string ircServer, string mask,
    int hopCount, string realName, bool last);
    public delegate void WhoisEventHandler(WhoisInfo whoisInfo);
    public delegate void WhowasEventHandler(UserInfo user, string realName, bool last);
    public delegate void UserModeChangeEventHandler(ModeAction action, UserMode mode);
    public delegate void UserModeRequestEventHandler(UserMode[] modes);
    public delegate void ChannelModeRequestEventHandler(string channel, ChannelModeInfo[] modes);
    public delegate void ChannelModeChangeEventHandler(UserInfo who, string channel, ChannelModeInfo[] modes);
    public delegate void ChannelListEventHandler(string channel, ChannelMode mode, string item, UserInfo who, long whenSet, bool last);
    public delegate void CtcpReplyEventHandler(string command, UserInfo who, string reply);
    public delegate void CtcpRequestEventHandler(string command, UserInfo who);
    public delegate void CtcpPingReplyEventHandler(UserInfo who, string timestamp);
    public delegate void CtcpPingRequestEventHandler(UserInfo who, string timestamp);
    public delegate void DccChatRequestEventHandler(DccUserInfo dccUserInfo);
    public delegate void ChatSessionOpenedEventHandler(DccChatSession session);
    public delegate void ChatSessionClosedEventHandler(DccChatSession session);
    public delegate void ChatMessageReceivedEventHandler(DccChatSession session, string message);
    public delegate void ChatRequestTimeoutEventHandler(DccChatSession session);
    public delegate void DccSendRequestEventHandler(DccUserInfo dccUserInfo, string fileName, int size, bool turbo);
    public delegate void FileTransferTimeoutEventHandler(DccFileSession session);
    public delegate void FileTransferStartedEventHandler(DccFileSession session);
    public delegate void FileTransferInterruptedEventHandler(DccFileSession session);
    public delegate void FileTransferCompletedEventHandler(DccFileSession session);
    public delegate void FileTransferProgressEventHandler(DccFileSession session, int bytesSent);
    public delegate void DccGetRequestEventHandler(DccUserInfo dccUserInfo, string fileName, bool turbo);
    public delegate void RawMessageReceivedEventHandler(string message);
    public delegate void RawMessageSentEventHandler(string message);
    public delegate void VersionEventHandler(string versionInfo);
    public delegate void MotdEventHandler(string message, bool last);
    public delegate void TimeEventHandler(string time);
    public delegate void InfoEventHandler(string message, bool last);
    public delegate void AdminEventHandler(string message);
    public delegate void LusersEventHandler(string message);
    public delegate void LinksEventHandler(string mask, string hostname, int hopCount, string serverInfo, bool done);
    public delegate void StatsEventHandler(StatsQuery queryType, string message, bool done);
    public delegate void KillEventHandler(UserInfo user, string nick, string reason);

    #endregion

    #region Enums.cs

    public enum ModeAction : int
    {
        Add = 43, //+
        Remove = 45 //-
    };

    public enum UserMode : int
    {
        Away = 97, //a
        Wallops = 119, //w
        Invisible = 105, //i
        Operator = 111, //o
        Restricted = 114, //r
        LocalOperator = 79, //O
        ServerNotices = 115 //s
    };

    public enum ChannelMode : int
    {
        ChannelCreator = 79, //O
        ChannelOperator = 111, //o
        HalfChannelOperator = 104, //h
        Voice = 118, //v
        Anonymous = 97, //a
        InviteOnly = 105, //i
        Moderated = 109, //m
        NoOutside = 110, //n
        Quiet = 113, //q
        Private = 112, //p
        Secret = 115, //s
        ServerReop = 114, //r
        TopicSettable = 116, //t
        Password = 107, //k
        UserLimit = 108, //l
        Ban = 98, //b
		
        Exception = 101, //e

        Invitation = 73//I
    };

    public enum StatsQuery : int
    {
        Connections = 108, //l
        CommandUsage = 109, //m
        Operators = 111, //o
        Uptime = 117, //u
    };

    public enum MircColor
    {
        White = 0,
        Black = 1,
        Blue = 2,
        Green = 3,
        LightRed = 4,
        Brown = 5,
        Purple = 6,
        Orange = 7,
        Yellow = 8,
        LightGreen = 9,
        Cyan = 10,
        LightCyan = 11,
        LightBlue = 12,
        Pink = 13,
        Grey = 14,
        LightGrey = 15,
        Transparent = 99
    };
    #endregion

    public class Identd
    {
        private static TcpListener listener;
        private static bool running;
        private static object lockObject;
        private static string username;
        private const string Reply = " : USERID : UNIX : ";
        private const int IdentdPort = 113;

        static Identd()
        {
            running = false;
            lockObject = new object();
        }

        //Declare constructor private so it cannot be instatiated.
        private Identd() { }

        public static void Start(string userName)
        {
            lock (lockObject)
            {
                if (running == true)
                {
                    throw new Exception("Identd already started.");
                }
                running = true;
                username = userName;
                Thread socketThread = new Thread(new ThreadStart(Identd.Run));
                socketThread.Name = "Identd";
                socketThread.Start();
            }
        }
        System.Net.IPAddress ipAddress = System.Net.Dns.Resolve("localhost").AddressList[0];

        public static bool IsRunning() { lock (lockObject) { return running; } }

        public static void Stop()
        {
            lock (lockObject)
            {
                if (running == true)
                {
                    listener.Stop();
                    Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Identd::Stop()");
                    listener = null;
                    running = false;
                }
            }
        }
        private static void Run()
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Identd::Run()");
            try
            {
                listener = new TcpListener(IdentdPort);
                listener.Start();

            loop:
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        //Read query
                        StreamReader reader = new StreamReader(client.GetStream());
                        string line = reader.ReadLine();
                        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Identd::Run() received=" + line);

                        //Send back reply
                        StreamWriter writer = new StreamWriter(client.GetStream());
                        writer.WriteLine(line.Trim() + Reply + username);
                        writer.Flush();

                        //Close connection with client
                        client.Close();
                    }
                    catch (IOException ioe)
                    {
                        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Identd::Run() exception=" + ioe);
                    }
                    goto loop;
                }
            }
            catch (Exception)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Identd::Run() Identd stopped");
            }
            finally
            {
                running = false;
            }
        }

    }

    public interface IParser
    {
        bool CanParse(string line);
        void Parse(string message);
    }

    public class Listener
    {
        public event ReplyEventHandler OnReply;
        public event ErrorMessageEventHandler OnError;
        public event AwayEventHandler OnAway;
        public event InviteSentEventHandler OnInviteSent;
        public event NickErrorEventHandler OnNickError;
        public event PingEventHandler OnPing;
        public event RegisteredEventHandler OnRegistered;
        public event DisconnectingEventHandler OnDisconnecting;
        public event DisconnectedEventHandler OnDisconnected;
        public event PublicNoticeEventHandler OnPublicNotice;
        public event PrivateNoticeEventHandler OnPrivateNotice;
        public event JoinEventHandler OnJoin;
        public event PublicMessageEventHandler OnPublic;
        public event ActionEventHandler OnAction;
        public event PrivateActionEventHandler OnPrivateAction;
        public event NickEventHandler OnNick;
        public event PrivateMessageEventHandler OnPrivate;
        public event TopicEventHandler OnTopicChanged;
        public event TopicRequestEventHandler OnTopicRequest;
        public event PartEventHandler OnPart;
        public event QuitEventHandler OnQuit;
        public event InviteEventHandler OnInvite;
        public event KickEventHandler OnKick;
        public event NamesEventHandler OnNames;
        public event ListEventHandler OnList;
        public event IsonEventHandler OnIson;
        public event WhoEventHandler OnWho;
        public event WhoisEventHandler OnWhois;
        public event WhowasEventHandler OnWhowas;
        public event UserModeChangeEventHandler OnUserModeChange;
        public event UserModeRequestEventHandler OnUserModeRequest;
        public event ChannelModeRequestEventHandler OnChannelModeRequest;
        public event ChannelModeChangeEventHandler OnChannelModeChange;
        public event ChannelListEventHandler OnChannelList;
        public event VersionEventHandler OnVersion;
        public event MotdEventHandler OnMotd;
        public event TimeEventHandler OnTime;
        public event InfoEventHandler OnInfo;
        public event AdminEventHandler OnAdmin;
        public event LusersEventHandler OnLusers;
        public event LinksEventHandler OnLinks;
        public event StatsEventHandler OnStats;
        public event KillEventHandler OnKill;

        private const string PING = "PING";
        private const string ERROR = "ERROR";
        private const string NOTICE = "NOTICE";
        private const string JOIN = "JOIN";
        private const string PRIVMSG = "PRIVMSG";
        private const string NICK = "NICK";
        private const string TOPIC = "TOPIC";
        private const string PART = "PART";
        private const string QUIT = "QUIT";
        private const string INVITE = "INVITE";
        private const string KICK = "KICK";
        private const string MODE = "MODE";
        private const string KILL = "KILL";
        private const string ACTION = "\u0001ACTION";
        private readonly char[] Separator = { ' ' };
        private readonly Regex channelPattern;
        private readonly Regex replyRegex;

        private Hashtable whoisInfos;

        public Listener()
        {
            channelPattern = new Regex("([#!+&]\\w+)", RegexOptions.Compiled | RegexOptions.Singleline);
            replyRegex = new Regex("^:([^\\s]*) ([\\d]{3}) ([^\\s]*) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        internal void Parse(string message)
        {
            string[] tokens = message.Split(Separator);
            if (tokens[0] == PING)
            {
                if (OnPing != null)
                {
                    tokens[1] = RemoveLeadingColon(tokens[1]);
                    OnPing(CondenseStrings(tokens, 1));
                }
            }
            else if (tokens[0] == NOTICE)
            {
                if (OnPrivateNotice != null)
                {
                    OnPrivateNotice(
                        UserInfo.Empty,
                        CondenseStrings(tokens, 2));
                }
            }
            else if (tokens[0] == ERROR)
            {
                tokens[1] = RemoveLeadingColon(tokens[1]);
                Error(ReplyCode.IrcServerError, CondenseStrings(tokens, 1));
            }
            else if (replyRegex.IsMatch(message)) { ParseReply(tokens); }
            else { ParseCommand(tokens); }
        }

        internal void Disconnecting()
        {
            if (OnDisconnecting != null) { OnDisconnecting(); }
        }

        internal void Disconnected()
        {
            if (OnDisconnected != null) { OnDisconnected(); }
        }

        internal void Error(ReplyCode code, string message)
        {
            if (OnError != null) { OnError(code, message); }
        }

        private void ParseCommand(string[] tokens)
        {
            //Remove colon user info string
            tokens[0] = RemoveLeadingColon(tokens[0]);
            switch (tokens[1])
            {
                case NOTICE:
                    tokens[3] = RemoveLeadingColon(tokens[3]);
                    if (Rfc2812Util.IsValidChannelName(tokens[2]))
                    {
                        if (OnPublicNotice != null)
                        {
                            OnPublicNotice(
                                Rfc2812Util.UserInfoFromString(tokens[0]),
                                tokens[2],
                                CondenseStrings(tokens, 3));
                        }
                    }
                    else
                    {
                        if (OnPrivateNotice != null)
                        {
                            OnPrivateNotice(
                                Rfc2812Util.UserInfoFromString(tokens[0]),
                                CondenseStrings(tokens, 3));
                        }
                    }
                    break;
                case JOIN:
                    if (OnJoin != null) { OnJoin(Rfc2812Util.UserInfoFromString(tokens[0]), RemoveLeadingColon(tokens[2])); }
                    break;
                case PRIVMSG:
                    tokens[3] = RemoveLeadingColon(tokens[3]);
                    if (tokens[3] == ACTION)
                    {
                        if (Rfc2812Util.IsValidChannelName(tokens[2]))
                        {
                            if (OnAction != null)
                            {
                                int last = tokens.Length - 1;
                                tokens[last] = RemoveTrailingQuote(tokens[last]);
                                OnAction(Rfc2812Util.UserInfoFromString(tokens[0]), tokens[2], CondenseStrings(tokens, 4));
                            }
                        }
                        else
                        {
                            if (OnPrivateAction != null)
                            {
                                int last = tokens.Length - 1;
                                tokens[last] = RemoveTrailingQuote(tokens[last]);
                                OnPrivateAction(Rfc2812Util.UserInfoFromString(tokens[0]), CondenseStrings(tokens, 4));
                            }
                        }
                    }
                    else if (channelPattern.IsMatch(tokens[2]))
                    {
                        if (OnPublic != null) { OnPublic(Rfc2812Util.UserInfoFromString(tokens[0]), tokens[2], CondenseStrings(tokens, 3)); }
                    }
                    else
                    {
                        if (OnPrivate != null) { OnPrivate(Rfc2812Util.UserInfoFromString(tokens[0]), CondenseStrings(tokens, 3)); }
                    }
                    break;
                case NICK:
                    if (OnNick != null)
                    {
                        OnNick(Rfc2812Util.UserInfoFromString(tokens[0]), RemoveLeadingColon(tokens[2]));
                    }
                    break;
                case TOPIC:
                    if (OnTopicChanged != null)
                    {
                        tokens[3] = RemoveLeadingColon(tokens[3]);
                        OnTopicChanged(
                            Rfc2812Util.UserInfoFromString(tokens[0]), tokens[2], CondenseStrings(tokens, 3));
                    }
                    break;
                case PART:
                    if (OnPart != null)
                    {
                        OnPart(
                            Rfc2812Util.UserInfoFromString(tokens[0]),
                            tokens[2],
                            tokens.Length >= 4 ? RemoveLeadingColon(CondenseStrings(tokens, 3)) : "");
                    }
                    break;
                case QUIT:
                    if (OnQuit != null)
                    {
                        tokens[2] = RemoveLeadingColon(tokens[2]);
                        OnQuit(Rfc2812Util.UserInfoFromString(tokens[0]), CondenseStrings(tokens, 2));
                    }
                    break;
                case INVITE:
                    if (OnInvite != null)
                    {
                        OnInvite(
                            Rfc2812Util.UserInfoFromString(tokens[0]), RemoveLeadingColon(tokens[3]));
                    }
                    break;
                case KICK:
                    if (OnKick != null)
                    {
                        tokens[4] = RemoveLeadingColon(tokens[4]);
                        OnKick(Rfc2812Util.UserInfoFromString(tokens[0]), tokens[2], tokens[3], CondenseStrings(tokens, 4));
                    }
                    break;
                case MODE:
                    if (channelPattern.IsMatch(tokens[2]))
                    {
                        if (OnChannelModeChange != null)
                        {
                            UserInfo who = Rfc2812Util.UserInfoFromString(tokens[0]);
                            try
                            {
                                ChannelModeInfo[] modes = ChannelModeInfo.ParseModes(tokens, 3);
                                OnChannelModeChange(who, tokens[2], modes);
                            }
                            catch (Exception)
                            {
                                if (OnError != null) { OnError(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)); }
                                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Listener::ParseCommand() Bad IRC MODE string=" + tokens[0]);
                            }
                        }
                    }
                    else
                    {
                        if (OnUserModeChange != null)
                        {
                            tokens[3] = RemoveLeadingColon(tokens[3]);
                            OnUserModeChange(Rfc2812Util.CharToModeAction(tokens[3][0]),
                                Rfc2812Util.CharToUserMode(tokens[3][1]));
                        }
                    }
                    break;
                case KILL:
                    if (OnKill != null)
                    {
                        string reason = "";
                        if (tokens.Length >= 4)
                        {
                            tokens[3] = RemoveLeadingColon(tokens[3]);
                            reason = CondenseStrings(tokens, 3);
                        }
                        OnKill(Rfc2812Util.UserInfoFromString(tokens[0]), tokens[2], reason);
                    }
                    break;
                default:
                    if (OnError != null) { OnError(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)); }
                    Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Listener::ParseCommand() Unknown IRC command=" + tokens[1]);
                    break;
            }
        }
        private void ParseReply(string[] tokens)
        {
            ReplyCode code = (ReplyCode)int.Parse(tokens[1], CultureInfo.InvariantCulture);
            tokens[3] = RemoveLeadingColon(tokens[3]);
            switch (code)
            {
                //Messages sent upon successful registration 
                case ReplyCode.RPL_WELCOME:
                case ReplyCode.RPL_YOURESERVICE:
                    if (OnRegistered != null) { OnRegistered(); }
                    break;
                case ReplyCode.RPL_MOTDSTART:
                case ReplyCode.RPL_MOTD:
                    if (OnMotd != null) { OnMotd(CondenseStrings(tokens, 3), false); }
                    break;
                case ReplyCode.RPL_ENDOFMOTD:
                    if (OnMotd != null) { OnMotd(CondenseStrings(tokens, 3), true); }
                    break;
                case ReplyCode.RPL_ISON:
                    if (OnIson != null) { OnIson(tokens[3]); }
                    break;
                case ReplyCode.RPL_NAMREPLY:
                    if (OnNames != null)
                    {
                        tokens[5] = RemoveLeadingColon(tokens[5]);
                        int numberOfUsers = tokens.Length - 5;
                        string[] users = new string[numberOfUsers];
                        Array.Copy(tokens, 5, users, 0, numberOfUsers);
                        OnNames(tokens[4],
                            users,
                            false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFNAMES:
                    if (OnNames != null) { OnNames(tokens[3], new string[0], true); }
                    break;
                case ReplyCode.RPL_LIST:
                    if (OnList != null)
                    {
                        tokens[5] = RemoveLeadingColon(tokens[5]);
                        OnList(
                            tokens[3],
                            int.Parse(tokens[4], CultureInfo.InvariantCulture),
                            CondenseStrings(tokens, 5),
                            false);
                    }
                    break;
                case ReplyCode.RPL_LISTEND:
                    if (OnList != null) { OnList("", 0, "", true); }
                    break;
                case ReplyCode.ERR_NICKNAMEINUSE:
                case ReplyCode.ERR_NICKCOLLISION:
                    if (OnNickError != null)
                    {
                        tokens[4] = RemoveLeadingColon(tokens[4]);
                        OnNickError(tokens[3], CondenseStrings(tokens, 4));
                    }
                    break;
                case ReplyCode.RPL_NOTOPIC:
                    if (OnError != null) { OnError(code, CondenseStrings(tokens, 3)); }
                    break;
                case ReplyCode.RPL_TOPIC:
                    if (OnTopicRequest != null)
                    {
                        tokens[4] = RemoveLeadingColon(tokens[4]);
                        OnTopicRequest(tokens[3], CondenseStrings(tokens, 4));
                    }
                    break;
                case ReplyCode.RPL_INVITING:
                    if (OnInviteSent != null) { OnInviteSent(tokens[3], tokens[4]); }
                    break;
                case ReplyCode.RPL_AWAY:
                    if (OnAway != null) { OnAway(tokens[3], RemoveLeadingColon(CondenseStrings(tokens, 4))); }
                    break;
                case ReplyCode.RPL_WHOREPLY:
                    if (OnWho != null)
                    {
                        UserInfo user = new UserInfo(tokens[7], tokens[4], tokens[5]);
                        OnWho(
                            user,
                            tokens[3],
                            tokens[6],
                            tokens[8],
                            int.Parse(RemoveLeadingColon(tokens[9]), CultureInfo.InvariantCulture),
                            tokens[10],
                            false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFWHO:
                    if (OnWho != null) { OnWho(UserInfo.Empty, "", "", "", 0, "", true); }
                    break;
                case ReplyCode.RPL_WHOISUSER:
                    UserInfo whoUser = new UserInfo(tokens[3], tokens[4], tokens[5]);
                    WhoisInfo whoisInfo = LookupInfo(whoUser.Nick);
                    whoisInfo.userInfo = whoUser;
                    tokens[7] = RemoveLeadingColon(tokens[7]);
                    whoisInfo.realName = CondenseStrings(tokens, 7);
                    break;
                case ReplyCode.RPL_WHOISCHANNELS:
                    WhoisInfo whoisChannelInfo = LookupInfo(tokens[3]);
                    tokens[4] = RemoveLeadingColon(tokens[4]);
                    int numberOfChannels = tokens.Length - 4;
                    string[] channels = new String[numberOfChannels];
                    Array.Copy(tokens, 4, channels, 0, numberOfChannels);
                    whoisChannelInfo.SetChannels(channels);
                    break;
                case ReplyCode.RPL_WHOISSERVER:
                    WhoisInfo whoisServerInfo = LookupInfo(tokens[3]);
                    whoisServerInfo.ircServer = tokens[4];
                    tokens[5] = RemoveLeadingColon(tokens[5]);
                    whoisServerInfo.serverDescription = CondenseStrings(tokens, 5);
                    break;
                case ReplyCode.RPL_WHOISOPERATOR:
                    WhoisInfo whoisOpInfo = LookupInfo(tokens[3]);
                    whoisOpInfo.isOperator = true;
                    break;
                case ReplyCode.RPL_WHOISIDLE:
                    WhoisInfo whoisIdleInfo = LookupInfo(tokens[3]);
                    whoisIdleInfo.idleTime = long.Parse(tokens[5], CultureInfo.InvariantCulture);
                    break;
                case ReplyCode.RPL_ENDOFWHOIS:
                    string nick = tokens[3];
                    WhoisInfo whoisEndInfo = LookupInfo(nick);
                    if (OnWhois != null) { OnWhois(whoisEndInfo); }
                    whoisInfos.Remove(nick);
                    break;
                case ReplyCode.RPL_WHOWASUSER:
                    if (OnWhowas != null)
                    {
                        UserInfo whoWasUser = new UserInfo(tokens[3], tokens[4], tokens[5]);
                        tokens[7] = RemoveLeadingColon(tokens[7]);
                        OnWhowas(whoWasUser, CondenseStrings(tokens, 7), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFWHOWAS:
                    if (OnWhowas != null) { OnWhowas(UserInfo.Empty, "", true); }
                    break;
                case ReplyCode.RPL_UMODEIS:
                    if (OnUserModeRequest != null)
                    {
                        //First drop the '+'
                        string chars = tokens[3].Substring(1);
                        UserMode[] modes = Rfc2812Util.UserModesToArray(chars);
                        OnUserModeRequest(modes);
                    }
                    break;
                case ReplyCode.RPL_CHANNELMODEIS:
                    if (OnChannelModeRequest != null)
                    {
                        try
                        {
                            ChannelModeInfo[] modes = ChannelModeInfo.ParseModes(tokens, 4);
                            OnChannelModeRequest(tokens[3], modes);
                        }
                        catch (Exception)
                        {
                            if (OnError != null) { OnError(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)); }
                            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Listener::ParseReply() Bad IRC MODE string=" + tokens[0]);
                        }
                    }
                    break;
                case ReplyCode.RPL_BANLIST:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.Ban, tokens[4], Rfc2812Util.UserInfoFromString(tokens[5]), Convert.ToInt64(tokens[6], CultureInfo.InvariantCulture), false); }
                    break;
                case ReplyCode.RPL_ENDOFBANLIST:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.Ban, "", UserInfo.Empty, 0, true); }
                    break;
                case ReplyCode.RPL_INVITELIST:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.Invitation, tokens[4], Rfc2812Util.UserInfoFromString(tokens[5]), Convert.ToInt64(tokens[6]), false); }
                    break;
                case ReplyCode.RPL_ENDOFINVITELIST:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.Invitation, "", UserInfo.Empty, 0, true); }
                    break;
                case ReplyCode.RPL_EXCEPTLIST:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.Exception, tokens[4], Rfc2812Util.UserInfoFromString(tokens[5]), Convert.ToInt64(tokens[6]), false); }
                    break;
                case ReplyCode.RPL_ENDOFEXCEPTLIST:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.Exception, "", UserInfo.Empty, 0, true); }
                    break;
                case ReplyCode.RPL_UNIQOPIS:
                    if (OnChannelList != null) { OnChannelList(tokens[3], ChannelMode.ChannelCreator, tokens[4], UserInfo.Empty, 0, true); }
                    break;
                case ReplyCode.RPL_VERSION:
                    if (OnVersion != null) { OnVersion(CondenseStrings(tokens, 3)); }
                    break;
                case ReplyCode.RPL_TIME:
                    if (OnTime != null) { OnTime(CondenseStrings(tokens, 3)); }
                    break;
                case ReplyCode.RPL_INFO:
                    if (OnInfo != null) { OnInfo(CondenseStrings(tokens, 3), false); }
                    break;
                case ReplyCode.RPL_ENDOFINFO:
                    if (OnInfo != null) { OnInfo(CondenseStrings(tokens, 3), true); }
                    break;
                case ReplyCode.RPL_ADMINME:
                case ReplyCode.RPL_ADMINLOC1:
                case ReplyCode.RPL_ADMINLOC2:
                case ReplyCode.RPL_ADMINEMAIL:
                    if (OnAdmin != null) { OnAdmin(RemoveLeadingColon(CondenseStrings(tokens, 3))); }
                    break;
                case ReplyCode.RPL_LUSERCLIENT:
                case ReplyCode.RPL_LUSEROP:
                case ReplyCode.RPL_LUSERUNKNOWN:
                case ReplyCode.RPL_LUSERCHANNELS:
                case ReplyCode.RPL_LUSERME:
                    if (OnLusers != null) { OnLusers(RemoveLeadingColon(CondenseStrings(tokens, 3))); }
                    break;
                case ReplyCode.RPL_LINKS:
                    if (OnLinks != null)
                    {
                        OnLinks(tokens[3], //mask
                                    tokens[4], //hostname
                                    int.Parse(RemoveLeadingColon(tokens[5]), CultureInfo.InvariantCulture), //hopcount
                                    CondenseStrings(tokens, 6), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFLINKS:
                    if (OnLinks != null) { OnLinks(String.Empty, String.Empty, -1, String.Empty, true); }
                    break;
                case ReplyCode.RPL_STATSLINKINFO:
                case ReplyCode.RPL_STATSCOMMANDS:
                case ReplyCode.RPL_STATSUPTIME:
                case ReplyCode.RPL_STATSOLINE:
                    if (OnStats != null) { OnStats(GetQueryType(code), RemoveLeadingColon(CondenseStrings(tokens, 3)), false); }
                    break;
                case ReplyCode.RPL_ENDOFSTATS:
                    if (OnStats != null) { OnStats(Rfc2812Util.CharToStatsQuery(tokens[3][0]), RemoveLeadingColon(CondenseStrings(tokens, 4)), true); }
                    break;
                default:
                    HandleDefaultReply(code, tokens);
                    break;
            }
        }

        private void HandleDefaultReply(ReplyCode code, string[] tokens)
        {
            if (code >= ReplyCode.ERR_NOSUCHNICK && code <= ReplyCode.ERR_USERSDONTMATCH)
            {
                if (OnError != null) { OnError(code, CondenseStrings(tokens, 3)); }
            }
            else if (OnReply != null) { OnReply(code, CondenseStrings(tokens, 3)); }
        }

        private WhoisInfo LookupInfo(string nick)
        {
            if (whoisInfos == null)
            {
                whoisInfos = new Hashtable();
            }
            WhoisInfo info = (WhoisInfo)whoisInfos[nick];
            if (info == null)
            {
                info = new WhoisInfo();
                whoisInfos[nick] = info;
            }
            return info;
        }

        private string CondenseStrings(string[] strings, int start)
        {
            if (strings.Length == start + 1) { return strings[start]; }
            else { return String.Join(" ", strings, start, (strings.Length - start)); }
        }
        private string RemoveLeadingColon(string text)
        {
            if (text[0] == ':') { return text.Substring(1); }
            return text;
        }

        private string RemoveTrailingQuote(string text)
        {
            return text.Substring(0, text.Length - 1);
        }

        private StatsQuery GetQueryType(ReplyCode code)
        {
            switch (code)
            {
                case ReplyCode.RPL_STATSLINKINFO:
                    return StatsQuery.Connections;
                case ReplyCode.RPL_STATSCOMMANDS:
                    return StatsQuery.CommandUsage;
                case ReplyCode.RPL_STATSUPTIME:
                    return StatsQuery.Uptime;
                case ReplyCode.RPL_STATSOLINE:
                    return StatsQuery.Operators;
                //Should never get here
                default:
                    return StatsQuery.CommandUsage;
            }
        }

    }

    public class NameGenerator
    {
        private int[] numSyllables = new int[] { 1, 2, 3, 4, 5 };
        private int[] numSyllablesChance = new int[] { 150, 500, 80, 10, 1 };
        private int[] numConsonants = new int[] { 0, 1, 2, 3, 4 };
        private int[] numConsonantsChance = new int[] { 80, 350, 25, 5, 1 };
        private int[] numVowels = new int[] { 1, 2, 3 };
        private int[] numVowelsChance = new int[] { 180, 25, 1 };
        private char[] vowel = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        private int[] vowelChance = new int[] { 10, 12, 10, 10, 8, 2 };
        private char[] consonant = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
        private int[] consonantChance = new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 12, 12, 12, 10, 5, 12, 12, 12, 8, 8, 3, 4, 3 };
        private Random random;

        public NameGenerator() { random = new Random(); }

        private int IndexSelect(int[] intArray)
        {
            int totalPossible = 0;
            for (int i = 0; i < intArray.Length; i++) { totalPossible = totalPossible + intArray[i]; }
            int chosen = random.Next(totalPossible);
            int chancesSoFar = 0;
            for (int j = 0; j < intArray.Length; j++)
            {
                chancesSoFar = chancesSoFar + intArray[j];
                if (chancesSoFar > chosen) { return j; }
            }
            return 0;
        }
        private string MakeSyllable() { return MakeConsonantBlock() + MakeVowelBlock() + MakeConsonantBlock(); }
        private string MakeConsonantBlock()
        {
            string newName = "";
            int numberConsonants = numConsonants[IndexSelect(numConsonantsChance)];
            for (int i = 0; i < numberConsonants; i++) { newName += consonant[IndexSelect(consonantChance)]; }
            return newName;
        }
        private string MakeVowelBlock()
        {
            string newName = "";
            int numberVowels = numVowels[IndexSelect(numVowelsChance)];
            for (int i = 0; i < numberVowels; i++) { newName += vowel[IndexSelect(vowelChance)]; }
            return newName;
        }
        public string MakeName()
        {
            int numberSyllables = numSyllables[IndexSelect(numSyllablesChance)];
            string newName = "";
            for (int i = 0; i < numberSyllables; i++) { newName = newName + MakeSyllable(); }
            return char.ToUpper(newName[0]) + newName.Substring(1);
        }
    }

    public enum ReplyCode : int
    {
        RPL_WELCOME = 001,
        RPL_YOURHOST = 002,
        RPL_CREATED = 003,
        RPL_MYINFO = 004,
        RPL_BOUNCE = 005,
        RPL_USERHOST = 302,
        RPL_ISON = 303,
        RPL_AWAY = 301,
        RPL_UNAWAY = 305,
        RPL_NOWAWAY = 306,
        RPL_WHOISUSER = 311,
        RPL_WHOISSERVER = 312,
        RPL_WHOISOPERATOR = 313,
        RPL_WHOISIDLE = 317,
        RPL_ENDOFWHOIS = 318,
        RPL_WHOISCHANNELS = 319,
        RPL_WHOWASUSER = 314,
        RPL_ENDOFWHOWAS = 369,
        RPL_LISTSTART = 321,
        RPL_LIST = 322,
        RPL_LISTEND = 323,
        RPL_UNIQOPIS = 325,
        RPL_CHANNELMODEIS = 324,
        RPL_NOTOPIC = 331,
        RPL_TOPIC = 332,
        RPL_INVITING = 341,
        RPL_SUMMONING = 342,
        RPL_INVITELIST = 346,
        RPL_ENDOFINVITELIST = 347,
        RPL_EXCEPTLIST = 348,
        RPL_ENDOFEXCEPTLIST = 349,
        RPL_VERSION = 351,
        RPL_WHOREPLY = 352,
        RPL_ENDOFWHO = 315,
        RPL_NAMREPLY = 353,
        RPL_ENDOFNAMES = 366,
        RPL_LINKS = 364,
        RPL_ENDOFLINKS = 365,
        RPL_BANLIST = 367,
        RPL_ENDOFBANLIST = 368,
        RPL_INFO = 371,
        RPL_ENDOFINFO = 374,
        RPL_MOTDSTART = 375,
        RPL_MOTD = 372,
        RPL_ENDOFMOTD = 376,
        RPL_YOUREOPER = 381,
        RPL_REHASHING = 382,
        RPL_YOURESERVICE = 383,
        RPL_TIME = 391,
        RPL_USERSSTART = 392,
        RPL_USERS = 393,
        RPL_ENDOFUSERS = 394,
        RPL_NOUSERS = 395,
        RPL_TRACELINK = 200,
        RPL_TRACECONNECTING = 201,
        RPL_TRACEHANDSHAKE = 202,
        RPL_TRACEUNKNOWN = 203,
        RPL_TRACEOPERATOR = 204,
        RPL_TRACEUSER = 205,
        RPL_TRACESERVER = 206,
        RPL_TRACESERVICE = 207,
        RPL_TRACENEWTYPE = 208,
        RPL_TRACECLASS = 209,
        RPL_TRACERECONNECT = 210,
        RPL_TRACELOG = 261,
        RPL_TRACEEND = 262,
        RPL_STATSLINKINFO = 211,
        RPL_STATSCOMMANDS = 212,
        RPL_ENDOFSTATS = 219,
        RPL_STATSUPTIME = 242,
        RPL_STATSOLINE = 243,
        RPL_UMODEIS = 221,
        RPL_SERVLIST = 234,
        RPL_SERVLISTEND = 235,
        RPL_LUSERCLIENT = 251,
        RPL_LUSEROP = 252,
        RPL_LUSERUNKNOWN = 253,
        RPL_LUSERCHANNELS = 254,
        RPL_LUSERME = 255,
        RPL_ADMINME = 256,
        RPL_ADMINLOC1 = 257,
        RPL_ADMINLOC2 = 258,
        RPL_ADMINEMAIL = 259,
        RPL_TRYAGAIN = 263,
        ERR_NOSUCHNICK = 401,
        ERR_NOSUCHSERVER = 402,
        ERR_NOSUCHCHANNEL = 403,
        ERR_CANNOTSENDTOCHAN = 404,
        ERR_TOOMANYCHANNELS = 405,
        ERR_WASNOSUCHNICK = 406,
        ERR_TOOMANYTARGETS = 407,
        ERR_NOSUCHSERVICE = 408,
        ERR_NOORIGIN = 409,
        ERR_NORECIPIENT = 411,
        ERR_NOTEXTTOSEND = 412,
        ERR_NOTOPLEVEL = 413,
        ERR_WILDTOPLEVEL = 414,
        ERR_BADMASK = 415,
        ERR_TOOMANYLINES = 416,
        ERR_UNKNOWNCOMMAND = 421,
        ERR_NOMOTD = 422,
        ERR_NOADMININFO = 423,
        ERR_FILEERROR = 424,
        ERR_NONICKNAMEGIVEN = 431,
        ERR_ERRONEUSNICKNAME = 432,
        ERR_NICKNAMEINUSE = 433,
        ERR_NICKCOLLISION = 436,
        ERR_UNAVAILRESOURCE = 437,
        ERR_USERNOTINCHANNEL = 441,
        ERR_NOTONCHANNEL = 442,
        ERR_USERONCHANNEL = 443,
        ERR_NOLOGIN = 444,
        ERR_SUMMONDISABLED = 445,
        ERR_USERSDISABLED = 446,
        ERR_NOTREGISTERED = 451,
        ERR_NEEDMOREPARAMS = 461,
        ERR_ALREADYREGISTRED = 462,
        ERR_NOPERMFORHOST = 463,
        ERR_PASSWDMISMATCH = 464,
        ERR_YOUREBANNEDCREEP = 465,
        ERR_YOUWILLBEBANNED = 466,
        ERR_KEYSET = 467,
        ERR_CHANNELISFULL = 471,
        ERR_UNKNOWNMODE = 472,
        ERR_INVITEONLYCHAN = 473,
        ERR_BANNEDFROMCHAN = 474,
        ERR_BADCHANNELKEY = 475,
        ERR_BADCHANMASK = 476,
        ERR_NOCHANMODES = 477,
        ERR_BANLISTFULL = 478,
        ERR_NOPRIVILEGES = 481,
        ERR_CHANOPRIVSNEEDED = 482,
        ERR_CANTKILLSERVER = 483,
        ERR_RESTRICTED = 484,
        ERR_UNIQOPPRIVSNEEDED = 485,
        ERR_NOOPERHOST = 491,
        ERR_UMODEUNKNOWNFLAG = 501,
        ERR_USERSDONTMATCH = 502,
        ConnectionFailed = 1000,
        IrcServerError = 1001,
        BadDccEndpoint = 1002,
        UnparseableMessage = 1003,
        UnableToResume = 1004,
        UnknownEncryptionProtocol = 1005,
        BadDccAcceptValue = 1006,
        BadResumePosition = 1007,
        DccConnectionRefused = 1008
    }

    public class Rfc2812Util
    {
        private static readonly Regex nickRegex;
        private static readonly Regex nameSplitterRegex;
        private const string ChannelPrefix = "#!+&";
        private const string ActionModes = "+-";
        private const string UserModes = "awiorOs";
        private const string ChannelModes = "OohvaimnqpsrtklbeI";
        private const string Space = " ";

        internal static TraceSwitch IrcTrace = new TraceSwitch("IrcTraceSwitch", "Debug level for RFC2812 classes.");
        internal const string Special = "\\[\\]\\`_\\^\\{\\|\\}";
        internal const string Nick = "[" + Special + "a-zA-Z][\\w\\-" + Special + "]{0,8}";
        internal const string User = "(" + Nick + ")!([\\~\\w]+)@([\\w\\.\\-]+)";

        static Rfc2812Util()
        {
            nickRegex = new Regex(Nick);
            nameSplitterRegex = new Regex("[!@]", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        private Rfc2812Util() { }

        public static UserInfo UserInfoFromString(string fullUserName)
        {
            string[] parts = ParseUserInfoLine(fullUserName);
            if (parts == null) { return UserInfo.Empty; }
            else { return new UserInfo(parts[0], parts[1], parts[2]); }
        }
		
        public static string[] ParseUserInfoLine(string fullUserName)
        {
            if (fullUserName == null || fullUserName.Trim().Length == 0) { return null; }
            Match match = nameSplitterRegex.Match(fullUserName);
            if (match.Success)
            {
                string[] parts = nameSplitterRegex.Split(fullUserName);
                return parts;
            }
            else { return new string[] { fullUserName, "", "" }; }
        }

        public static bool IsValidChannelList(string[] channels)
        {
            if (channels == null || channels.Length == 0) { return false; }
            foreach (string channel in channels) { if (!IsValidChannelName(channel)) { return false; } }
            return true;
        }

        public static bool IsValidChannelName(string channel)
        {
            if (channel == null || channel.Trim().Length == 0) { return false; }
            if (Rfc2812Util.ContainsSpace(channel)) { return false; }
            if (ChannelPrefix.IndexOf(channel[0]) != -1 && channel.Length <= 50) { return true; }
            return false;
        }

        public static bool IsValidNick(string nick)
        {
            if (nick == null || nick.Trim().Length == 0) { return false; }
            if (Rfc2812Util.ContainsSpace(nick)) { return false; }
            if (nickRegex.IsMatch(nick)) { return true; }
            return false;
        }

        public static bool IsValidNicklList(string[] nicks)
        {
            if (nicks == null || nicks.Length == 0) { return false; }
            foreach (string nick in nicks)
            {
                if (!IsValidNick(nick)) { return false; }
            }
            return true;
        }

        public static char ModeActionToChar(ModeAction action) { return Convert.ToChar((byte)action, CultureInfo.InvariantCulture); }

        public static ModeAction CharToModeAction(char action)
        {
            ushort b = Convert.ToByte(action, CultureInfo.InvariantCulture);
            return (ModeAction)Enum.Parse(typeof(ModeAction), b.ToString(CultureInfo.InvariantCulture), false);
        }
		
        public static char UserModeToChar(UserMode mode) { return Convert.ToChar((byte)mode, CultureInfo.InvariantCulture); }

        public static UserMode[] UserModesToArray(string modes)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < modes.Length; i++)
                if (IsValidModeChar(modes[i], UserModes)) { list.Add(CharToUserMode(modes[i])); }
            return (UserMode[])list.ToArray(typeof(UserMode));
        }

        public static UserMode CharToUserMode(char mode)
        {
            ushort b = Convert.ToByte(mode, CultureInfo.InvariantCulture);
            return (UserMode)Enum.Parse(typeof(UserMode), b.ToString(CultureInfo.InvariantCulture), false);
        }

        public static ChannelMode[] ChannelModesToArray(string modes)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < modes.Length; i++)
                if (IsValidModeChar(modes[i], ChannelModes)) { list.Add(CharToChannelMode(modes[i])); }
            return (ChannelMode[])list.ToArray(typeof(ChannelMode));
        }

        public static char ChannelModeToChar(ChannelMode mode) { return Convert.ToChar((byte)mode, CultureInfo.InvariantCulture); }

        public static ChannelMode CharToChannelMode(char mode)
        {
            ushort b = Convert.ToByte(mode, CultureInfo.InvariantCulture);
            return (ChannelMode)Enum.Parse(typeof(ChannelMode), b.ToString(CultureInfo.InvariantCulture), false);
        }

        public static char StatsQueryToChar(StatsQuery query) { return Convert.ToChar((byte)query, CultureInfo.InvariantCulture); }

        public static StatsQuery CharToStatsQuery(char queryType)
        {
            ushort b = Convert.ToByte(queryType, CultureInfo.InvariantCulture);
            return (StatsQuery)Enum.Parse(typeof(StatsQuery), b.ToString(CultureInfo.InvariantCulture), false);
        }

        private static bool IsValidModeChar(char c, string validList) { return validList.IndexOf(c) != -1; }

        private static bool ContainsSpace(string text) { return text.IndexOf(Space, 0, text.Length) != -1; }
    }

    public class Sender : CommandBuilder
    {
        internal Sender(Connection connection) : base(connection) { }

        private bool IsEmpty(string aString) { return aString == null || aString.Trim().Length == 0; }

        private string Truncate(string parameter, int commandLength)
        {
            int max = MAX_COMMAND_SIZE - commandLength;
            if (parameter.Length > max) { return parameter.Substring(0, max); }
            else { return parameter; }
        }

        private bool TooLong(StringBuilder buffer) { return (buffer.Length + 2) > MAX_COMMAND_SIZE; }

        internal void User(ConnectionArgs args)
        {
            lock (this)
            {
                Buffer.Append("USER");
                Buffer.Append(SPACE);
                Buffer.Append(args.UserName);
                Buffer.Append(SPACE);
                Buffer.Append(args.ModeMask);
                Buffer.Append(SPACE);
                Buffer.Append('*');
                Buffer.Append(SPACE);
                Buffer.Append(args.RealName);
                Connection.SendCommand(Buffer);
            }
        }

        internal void Quit(string reason)
        {
            lock (this)
            {
                Buffer.Append("QUIT");
                if (IsEmpty(reason))
                {
                    ClearBuffer();
                    throw new ArgumentException("Quite reason cannot be null or empty.");
                }
                Buffer.Append(SPACE_COLON);
                if (reason.Length > 502) { reason = reason.Substring(0, 504); }
                Buffer.Append(reason);
                Connection.SendCommand(Buffer);
            }
        }

        internal void Pong(string message)
        {
            Buffer.Append("PONG");
            Buffer.Append(SPACE);
            Buffer.Append(message);
            Connection.SendAutomaticReply(Buffer);
        }

        internal void Pass(string password)
        {
            lock (this)
            {
                Buffer.Append("PASS");
                Buffer.Append(SPACE);
                Buffer.Append(password);
                Connection.SendCommand(Buffer);
            }
        }

        internal void RegisterConnection(ConnectionArgs args)
        {
            Pass(args.ServerPassword);
            Nick(args.Nick);
            User(args);
        }

        public void Join(string channel)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    Buffer.Append("JOIN");
                    Buffer.Append(SPACE);
                    Buffer.Append(channel);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void Join(string channel, string password)
        {
            lock (this)
            {
                if (IsEmpty(password))
                {
                    ClearBuffer();
                    throw new ArgumentException("Password cannot be empty or null.");
                }
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    Buffer.Append("JOIN");
                    Buffer.Append(SPACE);
                    Buffer.Append(channel);
                    Buffer.Append(SPACE);
                    //8 is the JOIN + 2 spaces + CR + LF
                    password = Truncate(password, 8);
                    Buffer.Append(password);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void Nick(string newNick)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidNick(newNick))
                {
                    Buffer.Append("NICK");
                    Buffer.Append(SPACE);
                    Buffer.Append(newNick);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(newNick + " is not a valid nickname.");
                }
            }
        }

        public void Names(params string[] channels)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidChannelList(channels))
                {
                    Buffer.Append("NAMES");
                    Buffer.Append(SPACE);
                    Buffer.Append(String.Join(",", channels));
                    if (TooLong(Buffer))
                    {
                        ClearBuffer();
                        throw new ArgumentException("Channels are too long.");
                    }
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException("One of the channel names is not valid.");
                }
            }
        }

        public void AllNames()
        {
            lock (this)
            {
                Buffer.Append("NAMES");
                Connection.SendCommand(Buffer);
            }
        }

        public void List(params string[] channels)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidChannelList(channels))
                {
                    Buffer.Append("LIST");
                    Buffer.Append(SPACE);
                    Buffer.Append(String.Join(",", channels));
                    if (TooLong(Buffer))
                    {
                        ClearBuffer();
                        throw new ArgumentException("Channels are too long.");
                    }
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException("One of the channel names is not valid.");
                }
            }
        }

        public void AllList()
        {
            lock (this)
            {
                Buffer.Append("LIST");
                Connection.SendCommand(Buffer);
            }
        }

        public void ChangeTopic(string channel, string newTopic)
        {
            lock (this)
            {
                if (IsEmpty(newTopic))
                {
                    ClearBuffer();
                    throw new ArgumentException("Topic cannot be empty or null.");
                }
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    Buffer.Append("TOPIC");
                    Buffer.Append(SPACE);
                    Buffer.Append(channel);
                    Buffer.Append(SPACE_COLON);
                    // 9 is TOPIC + 2 x Spaces + : + CR = LF
                    newTopic = Truncate(newTopic, 9 + channel.Length);
                    Buffer.Append(newTopic);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void ClearTopic(string channel)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    Buffer.Append("TOPIC");
                    Buffer.Append(SPACE);
                    Buffer.Append(channel);
                    Buffer.Append(SPACE_COLON);
                    Buffer.Append(SPACE);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void RequestTopic(string channel)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    Buffer.Append("TOPIC");
                    Buffer.Append(SPACE);
                    Buffer.Append(channel);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void Part(string reason, params string[] channels)
        {
            lock (this)
            {
                if (IsEmpty(reason))
                {
                    ClearBuffer();
                    throw new ArgumentException("Part reason cannot be empty or null.");
                }
                if (Rfc2812Util.IsValidChannelList(channels))
                {
                    Buffer.Append("PART");
                    Buffer.Append(SPACE);
                    string channelList = String.Join(",", channels);
                    Buffer.Append(channelList);
                    Buffer.Append(SPACE_COLON);
                    // 9 is PART + 2 x Spaces + : + CR + LF
                    reason = Truncate(reason, 9);
                    Buffer.Append(reason);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException("One of the channels names is not valid.");
                }
            }
        }

        public void Part(string channel)
        {
            lock (this)
            {
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    Buffer.Append("PART");
                    Buffer.Append(SPACE);
                    Buffer.Append(channel);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void PublicNotice(string channel, string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Notice message cannot be null or empty.");
                }
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    // 11 is NOTICE + 2 x Spaces + : + CR + LF
                    int max = MAX_COMMAND_SIZE - 11 - channel.Length;
                    if (message.Length > max)
                    {
                        string[] parts = BreakUpMessage(message, max);
                        foreach (string part in parts) { SendMessage("NOTICE", channel, part); }
                    }
                    else { SendMessage("NOTICE", channel, message); }
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void PrivateNotice(string nick, string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Notice message cannot be empty or null.");
                }
                if (Rfc2812Util.IsValidNick(nick))
                {
                    // 11 is NOTICE + 2 x Spaces + : + CR + LF
                    int max = MAX_COMMAND_SIZE - 11 - nick.Length;
                    if (message.Length > max)
                    {
                        string[] parts = BreakUpMessage(message, max);
                        foreach (string part in parts) { SendMessage("NOTICE", nick, part); }
                    }
                    else { SendMessage("NOTICE", nick, message); }
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nickname.");
                }
            }
        }

        public void PublicMessage(string channel, string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Public message cannot be null or empty.");
                }
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    // 11 is PRIVMSG + 2 x Spaces + : + CR + LF
                    int max = MAX_COMMAND_SIZE - 11 - channel.Length;
                    if (message.Length > max)
                    {
                        string[] parts = BreakUpMessage(message, max);
                        foreach (string part in parts) { SendMessage("PRIVMSG", channel, part); }
                    }
                    else { SendMessage("PRIVMSG", channel, message); }
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void PrivateMessage(string nick, string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Private message cannot be null or empty.");
                }
                if (Rfc2812Util.IsValidNick(nick))
                {
                    // 11 is PRIVMSG + 2 x Spaces + : + CR + LF
                    int max = MAX_COMMAND_SIZE - 11 - nick.Length;
                    if (message.Length > max)
                    {
                        string[] parts = BreakUpMessage(message, max);
                        foreach (string part in parts) { SendMessage("PRIVMSG", nick, part); }
                    }
                    else { SendMessage("PRIVMSG", nick, message); }
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nickname.");
                }
            }
        }

        public void Invite(string who, string channel)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(who))
                {
                    ClearBuffer();
                    throw new ArgumentException(who + " is not a valid nickname.");
                }
                if (!Rfc2812Util.IsValidChannelName(channel))
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel.");
                }
                Buffer.Append("INVITE");
                Buffer.Append(SPACE);
                Buffer.Append(who);
                Buffer.Append(SPACE);
                Buffer.Append(channel);
                Connection.SendCommand(Buffer);
            }
        }

        public void Kick(string channel, string reason, params string[] nicks)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNicklList(nicks))
                {
                    ClearBuffer();
                    throw new ArgumentException("One of the nicknames is invalid.");
                }
                if (!Rfc2812Util.IsValidChannelName(channel))
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel.");
                }
                if (IsEmpty(reason))
                {
                    ClearBuffer();
                    throw new ArgumentException("The reason for kicking cannot be null.");
                }
                string nickList = String.Join(",", nicks);
                // 10 is KICK + 3 x Spaces + : + CR + LF
                reason = Truncate(reason, 10 + channel.Length + nickList.Length);
                Buffer.Append("KICK");
                Buffer.Append(SPACE);
                Buffer.Append(channel);
                Buffer.Append(SPACE);
                Buffer.Append(nickList);
                Buffer.Append(SPACE_COLON);
                Buffer.Append(reason);
                Connection.SendCommand(Buffer);
            }
        }
 
        public void Ison(string nick)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nick.");
                }
                Buffer.Append("ISON");
                Buffer.Append(SPACE);
                Buffer.Append(nick);
                Connection.SendCommand(Buffer);
            }
        }

        public void Who(string mask, bool operatorsOnly)
        {
            lock (this)
            {
                //7 is WHO + Space +O + CR + LF
                int max = MAX_COMMAND_SIZE - 7;
                if (IsEmpty(mask) || mask.Length > max)
                {
                    ClearBuffer();
                    throw new ArgumentException("Who mask is invalid.");
                }
                Buffer.Append("WHO");
                Buffer.Append(SPACE);
                Buffer.Append(mask);
                if (operatorsOnly)
                {
                    Buffer.Append(SPACE);
                    Buffer.Append("o");
                }
                Connection.SendCommand(Buffer);
            }
        }
  
        public void AllWho()
        {
            lock (this)
            {
                Buffer.Append("WHO");
                Connection.SendCommand(Buffer);
            }
        }

        public void Whois(string nick)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nickname.");
                }
                Buffer.Append("WHOIS");
                Buffer.Append(SPACE);
                Buffer.Append(nick);
                Connection.SendCommand(Buffer);
            }
        }

        public void Away(string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Away message cannot be empty or null.");
                }
                Buffer.Append("AWAY");
                Buffer.Append(SPACE_COLON);
                // 8 is AWAY + Space + : + CR + LF
                message = Truncate(message, 8);
                Buffer.Append(message);
                Connection.SendCommand(Buffer);
            }
        }

        public void UnAway()
        {
            lock (this)
            {
                Buffer.Append("AWAY");
                Connection.SendCommand(Buffer);
            }
        }

        public void Whowas(string nick)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nickname.");
                }
                Buffer.Append("WHOWAS");
                Buffer.Append(SPACE);
                Buffer.Append(nick);
                Connection.SendCommand(Buffer);
            }
        }
 
        public void Whowas(string nick, int count)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nickname.");
                }
                if (count < 1)
                {
                    ClearBuffer();
                    throw new ArgumentException("Count must be more than zero.");
                }
                Buffer.Append("WHOWAS");
                Buffer.Append(SPACE);
                Buffer.Append(nick);
                Buffer.Append(SPACE);
                Buffer.Append(count);
                Connection.SendCommand(Buffer);
            }
        }

        public void RequestUserModes()
        {
            lock (this)
            {
                Buffer.Append("MODE");
                Buffer.Append(SPACE);
                Buffer.Append(Connection.ConnectionData.Nick);
                Connection.SendCommand(Buffer);
            }
        }

        public void ChangeUserMode(ModeAction action, UserMode mode)
        {
            lock (this)
            {
                if (mode == UserMode.Away)
                {
                    ClearBuffer();
                    throw new ArgumentException("Away mode can only be changed with the Away and Unaway commands.");
                }
                Buffer.Append("MODE");
                Buffer.Append(SPACE);
                Buffer.Append(Connection.ConnectionData.Nick);
                Buffer.Append(SPACE);
                Buffer.Append(Rfc2812Util.ModeActionToChar(action));
                Buffer.Append(Rfc2812Util.UserModeToChar(mode));
                Connection.SendCommand(Buffer);
            }
        }

        public void ChangeChannelMode(string channel, ModeAction action, ChannelMode mode, string param)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidChannelName(channel))
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel.");
                }
                Buffer.Append("MODE");
                Buffer.Append(SPACE);
                Buffer.Append(channel);
                Buffer.Append(SPACE);
                Buffer.Append(Rfc2812Util.ModeActionToChar(action));
                Buffer.Append(Rfc2812Util.ChannelModeToChar(mode));
                if (!IsEmpty(param))
                {
                    Buffer.Append(SPACE);
                    Buffer.Append(param);
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void RequestChannelList(string channel, ChannelMode mode)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidChannelName(channel))
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel.");
                }
                if (mode != ChannelMode.Ban &&
                    mode != ChannelMode.Exception &&
                    mode != ChannelMode.Invitation &&
                    mode != ChannelMode.ChannelCreator)
                {
                    ClearBuffer();
                    throw new ArgumentException(Enum.GetName(typeof(ChannelMode), mode) + " is not a valid channel mode for this request.");
                }
                Buffer.Append("MODE");
                Buffer.Append(SPACE);
                Buffer.Append(channel);
                Buffer.Append(SPACE);
                Buffer.Append(Rfc2812Util.ChannelModeToChar(mode));
                Connection.SendCommand(Buffer);
            }
        }

        public void RequestChannelModes(string channel)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidChannelName(channel))
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel.");
                }
                Buffer.Append("MODE");
                Buffer.Append(SPACE);
                Buffer.Append(channel);
                Connection.SendCommand(Buffer);
            }
        }

        public void Action(string channel, string description)
        {
            lock (this)
            {
                if (IsEmpty(description))
                {
                    ClearBuffer();
                    throw new ArgumentException("Action description cannot be null or empty.");
                }
                if (Rfc2812Util.IsValidChannelName(channel))
                {
                    description = Truncate(description, 19 + channel.Length);
                    SendMessage("PRIVMSG", channel, CtcpQuote + "ACTION " + description + CtcpQuote);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(channel + " is not a valid channel name.");
                }
            }
        }

        public void PrivateAction(string nick, string description)
        {
            lock (this)
            {
                if (IsEmpty(description))
                {
                    ClearBuffer();
                    throw new ArgumentException("Action description cannot be null or empty.");
                }
                if (Rfc2812Util.IsValidNick(nick))
                {
                    description = Truncate(description, 19 + nick.Length);
                    SendMessage("PRIVMSG", nick, CtcpQuote + "ACTION " + description + CtcpQuote);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nickname.");
                }
            }
        }

        public void Register(string newNick)
        {
            Connection.connectionArgs.Nick = newNick;
            Nick(Connection.connectionArgs.Nick);
            User(Connection.connectionArgs);
        }

        public void Raw(string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Message cannot be null or empty.");
                }
                if (message.Length > MAX_COMMAND_SIZE) { message = message.Substring(0, MAX_COMMAND_SIZE); }
                Buffer.Append(message);
                Connection.SendCommand(Buffer);
            }
        }

        public void Version() { Version(null); }

        public void Version(string targetServer)
        {
            lock (this)
            {
                Buffer.Append("VERSION");
                if (!IsEmpty(targetServer))
                {
                    targetServer = Truncate(targetServer, 10);
                    Buffer.Append(SPACE);
                    Buffer.Append(targetServer);
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Motd() { Motd(null); }

        public void Motd(string targetServer)
        {
            lock (this)
            {
                Buffer.Append("MOTD");
                if (!IsEmpty(targetServer))
                {
                    targetServer = Truncate(targetServer, 7);
                    Buffer.Append(SPACE);
                    Buffer.Append(targetServer);
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Time() { Time(null); }

        public void Time(string targetServer)
        {
            lock (this)
            {
                Buffer.Append("TIME");
                if (!IsEmpty(targetServer))
                {
                    targetServer = Truncate(targetServer, 8);
                    Buffer.Append(SPACE);
                    Buffer.Append(targetServer);
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Wallops(string message)
        {
            lock (this)
            {
                if (IsEmpty(message))
                {
                    ClearBuffer();
                    throw new ArgumentException("Wallops message cannot be null or empty.");
                }
                Buffer.Append("WALLOPS");
                message = Truncate(message, 10);
                Buffer.Append(SPACE);
                Buffer.Append(message);
                Connection.SendCommand(Buffer);
            }
        }

        public void Info() { Info(null); }

        public void Info(string target)
        {
            lock (this)
            {
                Buffer.Append("INFO");
                if (!IsEmpty(target))
                {
                    target = Truncate(target, 7);
                    Buffer.Append(SPACE);
                    Buffer.Append(target);
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Admin() { Admin(null); }

        public void Admin(string target)
        {
            lock (this)
            {
                Buffer.Append("ADMIN");
                if (!IsEmpty(target))
                {
                    target = Truncate(target, 8);
                    Buffer.Append(SPACE);
                    Buffer.Append(target);
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Lusers() { Lusers(null, null); }

        public void Lusers(string hostMask, string targetServer)
        {
            lock (this)
            {
                Buffer.Append("LUSERS");
                if (!IsEmpty(hostMask))
                {
                    Buffer.Append(SPACE);
                    Buffer.Append(hostMask);
                }
                if (!IsEmpty(targetServer))
                {
                    Buffer.Append(SPACE);
                    Buffer.Append(targetServer);
                }

                if (TooLong(Buffer))
                {
                    ClearBuffer();
                    throw new ArgumentException("Hostmask and TargetServer are too long.");
                }

                Connection.SendCommand(Buffer);
            }
        }

        public void Links() { Links(null); }

        public void Links(params string[] masks)
        {
            lock (this)
            {
                Buffer.Append("LINKS");
                if (masks != null)
                {
                    Buffer.Append(SPACE);
                    Buffer.Append(masks[0]);

                    if (masks.Length >= 2)
                    {
                        Buffer.Append(SPACE);
                        Buffer.Append(masks[1]);
                    }
                }

                if (TooLong(Buffer))
                {
                    ClearBuffer();
                    throw new ArgumentException("Masks are too long.");
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Stats(StatsQuery query) { Stats(query, null); }

        public void Stats(StatsQuery query, string targetServer)
        {
            lock (this)
            {
                Buffer.Append("STATS");
                Buffer.Append(SPACE);
                Buffer.Append(Rfc2812Util.StatsQueryToChar(query));
                if (targetServer != null)
                {
                    Buffer.Append(SPACE);
                    Buffer.Append(targetServer);
                }
                if (TooLong(Buffer))
                {
                    ClearBuffer();
                    throw new ArgumentException("Target server name is too long.");
                }
                Connection.SendCommand(Buffer);
            }
        }

        public void Kill(string nick, string reason)
        {
            lock (this)
            {
                if (IsEmpty(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException("Nick cannot be empty or null.");
                }
                if (IsEmpty(reason))
                {
                    ClearBuffer();
                    throw new ArgumentException("Reason cannot be empty or null.");
                }
                if (Rfc2812Util.IsValidNick(nick))
                {
                    Buffer.Append("KILL");
                    Buffer.Append(SPACE);
                    Buffer.Append(nick);
                    Buffer.Append(SPACE);
                    Buffer.Append(reason);
                    Connection.SendCommand(Buffer);
                }
                else
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nick name.");
                }
            }
        }

    }

    public class ServerProperties
    {
        private Hashtable properties;

        internal ServerProperties() { properties = new Hashtable(); }

        public string this[string key]
        {
            get
            {
                if (properties[key] != null) { return (string)properties[key]; }
                else { return String.Empty; }
            }
        }

        internal void SetProperty(string key, string propertyValue)
        {
            if (properties.ContainsKey(key))
            {
                properties[key] = propertyValue;
                return;
            }
            properties.Add(key, propertyValue);
        }

        public IDictionaryEnumerator GetEnumerator() { return properties.GetEnumerator(); }
        public bool ContainsKey(string key) { return properties[key] != null; }
    }

    public class TextColor
    {
        private const char ColorControl = '\x0003';
        private const char UnderlineControl = '\x001F';
        private const char BoldControl = '\x0002';
        private const char PlainControl = '\x000F';
        private const char ReverseControl = '\x0016';

        private const string TextColorFormat = "\x0003{0}{1}\x0003";
        private const string FullColorFormat = "\x0003{0},{1}{2}\x0003";

        private static readonly Regex colorPattern;

        static TextColor() { colorPattern = new Regex("\\u0003[\\d]{1,2}(,[\\d]{1,2})?([^\\u0003]+)\\u0003", RegexOptions.Compiled | RegexOptions.Singleline); }

        private TextColor() { }

        public static string StripControlChars(string text)
        {
            StringBuilder buffer = new StringBuilder();
            text = StripColor(text);

            foreach (char c in text) { if (!IsControlCode(c)) { buffer.Append(c); } }
            return buffer.ToString();
        }

        private static string StripColor(string text)
        {
            Match match = colorPattern.Match(text);
            if (match.Success)
            {
                return text.Substring(0, match.Index) +
                    match.Groups[2].ToString() +
                    text.Substring((match.Index + match.Length));
            }
            return text;
        }

        public static string MakeBold(string text) { return BoldControl + text + BoldControl; }
        public static string MakePlain(string text) { return PlainControl + text + PlainControl; }
        public static string MakeUnderline(string text) { return UnderlineControl + text + UnderlineControl; }
        public static string MakeReverseVideo(string text) { return ReverseControl + text + ReverseControl; }
        public static string MakeColor(string text, MircColor textColor) { return string.Format(TextColorFormat, (int)textColor, text); }
        public static string MakeColor(string text, MircColor textColor, MircColor backgroundColor) { return string.Format(FullColorFormat, (int)textColor, (int)backgroundColor, text); }

        private static bool IsControlCode(char c)
        {
            return
                c == '\x0003' ||
                c == '\x001F' ||
                c == '\x0002' ||
                c == '\x000F' ||
                c == '\x0016';
        }
    }

    public class UserInfo
    {
        private readonly string nickName;
        private readonly string userName;
        private readonly string hostName;
        private static readonly UserInfo EmptyInstance = new UserInfo();

        private UserInfo()
        {
            nickName = "";
            userName = "";
            hostName = "";
        }

        public UserInfo(string nick, string name, string host)
        {
            nickName = nick;
            userName = name;
            hostName = host;
        }

        public string Nick { get { return nickName; } }
        public string User { get { return userName; } }
        public string Hostname { get { return hostName; } }
        public static UserInfo Empty { get { return EmptyInstance; } }

        public override string ToString() { return string.Format("Nick={0} User={1} Host={2}", Nick, User, Hostname); }
    }

    public class WhoisInfo
    {
        internal UserInfo userInfo;
        internal string realName;
        internal string[] channels;
        internal string ircServer;
        internal string serverDescription;
        internal long idleTime;
        internal bool isOperator;

        internal WhoisInfo() { isOperator = false;}

        public UserInfo User { get { return userInfo; } }
        public string RealName { get { return realName; } }
        public string Server { get { return ircServer; } }
        public string ServerDescription { get { return serverDescription; } }
        public long IdleTime { get { return idleTime; } }
        public bool Operator { get { return isOperator; } }

        internal void SetChannels(string[] channels) { this.channels = channels; }
        public string[] GetChannels() { return channels; }
    }

    #region Ctcp

    public class CtcpListener
    {
        public event CtcpReplyEventHandler OnCtcpReply;
        public event CtcpRequestEventHandler OnCtcpRequest;
        public event CtcpPingReplyEventHandler OnCtcpPingReply;
        public event CtcpPingRequestEventHandler OnCtcpPingRequest;

        private static readonly Regex ctcpRegex;
        private static readonly string ctcpTypes;
        private Connection connection;
        private const int Name = 0;
        private const int Command = 1;
        private const int Text = 2;

        static CtcpListener()
        {
            ctcpTypes = "(FINGER|USERINFO|VERSION|SOURCE|CLIENTINFO|ERRMSG|PING|TIME)";
            ctcpRegex = new Regex(":([^ ]+) [A-Z]+ [^:]+:\u0001" + ctcpTypes + "([^\u0001]*)\u0001", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        internal CtcpListener(Connection connection) { this.connection = connection; }

        private bool IsReply(string[] tokens)
        {
            if (tokens[Text].Length == 0) { return false; }
            return true;
        }

        private static string[] TokenizeMessage(string message)
        {
            try
            {
                Match match = ctcpRegex.Match(message);
                return new string[] { match.Groups[1].ToString(), match.Groups[2].ToString(), match.Groups[3].ToString().Trim() };
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal void Parse(string line)
        {
            string[] ctcpTokens = TokenizeMessage(line);
            if (ctcpTokens != null)
            {
                if (ctcpTokens[Command].ToUpper(CultureInfo.CurrentCulture) == CtcpUtil.Ping)
                {
                    if (connection.CtcpSender.IsMyRequest(ctcpTokens[Text]))
                    {
                        connection.CtcpSender.ReplyReceived(ctcpTokens[Text]);
                        if (OnCtcpPingReply != null) { OnCtcpPingReply(Rfc2812Util.UserInfoFromString(ctcpTokens[Name]), ctcpTokens[Text]); }
                    }
                    else
                    {
                        //Ignore PING's with now parameters
                        if (ctcpTokens[Text] != null && ctcpTokens[Text].TrimEnd().Length != 0)
                        {
                            if (OnCtcpPingRequest != null) { OnCtcpPingRequest(Rfc2812Util.UserInfoFromString(ctcpTokens[Name]), ctcpTokens[Text]); }
                        }
                    }
                }
                else
                {
                    if (IsReply(ctcpTokens))
                    {
                        if (OnCtcpReply != null) { OnCtcpReply(ctcpTokens[Command].ToUpper(CultureInfo.CurrentCulture), Rfc2812Util.UserInfoFromString(ctcpTokens[Name]), ctcpTokens[Text]); }
                    }
                    else
                    {
                        if (OnCtcpRequest != null) { OnCtcpRequest(ctcpTokens[Command].ToUpper(CultureInfo.CurrentCulture), Rfc2812Util.UserInfoFromString(ctcpTokens[Name])); }
                    }
                }
            }
            else
            {
                connection.Listener.Error(ReplyCode.UnparseableMessage, line);
                Debug.WriteLineIf(CtcpUtil.CtcpTrace.TraceWarning, "Unknown CTCP command '" + line + "' recieved by CtcpListener");
            }
        }

        public static bool IsCtcpMessage(string message) { return ctcpRegex.IsMatch(message); }

    }

    public class CtcpResponder
    {
        private Connection connection;
        private long nextTime;
        private double floodDelay;
        private string fingerMessage;
        private string userInfoMessage;
        private string versionMessage;
        private string sourceMessage;
        private string clientInfoMessage;

        public CtcpResponder(Connection connection)
        {
            this.connection = connection;
            nextTime = DateTime.Now.ToFileTime();
            //Wait at least 2 second in between automatic CTCP responses
            floodDelay = 2000;
            //Send back user nick by default for finger requests.
            userInfoMessage = "Thresher CTCP Responder";
            fingerMessage = userInfoMessage;
            versionMessage = "Thresher IRC library 1.1";
            sourceMessage = "http://thresher.sourceforge.net";
            clientInfoMessage = "This client supports: UserInfo, Finger, Version, Source, Ping, Time and ClientInfo";
            if (connection.EnableCtcp)
            {
                connection.CtcpListener.OnCtcpRequest += new CtcpRequestEventHandler(OnCtcpRequest);
                connection.CtcpListener.OnCtcpPingRequest += new CtcpPingRequestEventHandler(OnCtcpPingRequest);
            }
        }

        public double ResponseDelay { get { return floodDelay; } set { floodDelay = value; } }
        public string FingerResponse { get { return fingerMessage; } set { fingerMessage = value; } }
        public string UserInfoResponse { get { return userInfoMessage; } set { userInfoMessage = value; } }
        public string VersionResponse { get { return versionMessage; } set { versionMessage = value; } }
        public string ClientInfoResponse { get { return clientInfoMessage; } set { clientInfoMessage = value; } }
        public string SourceResponse { get { return sourceMessage; } set { sourceMessage = value; } }

        private string FormatIdleTime()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(connection.IdleTime.Hours + " Hours, ");
            builder.Append(connection.IdleTime.Minutes + " Minutes, ");
            builder.Append(connection.IdleTime.Seconds + " Seconds");
            return builder.ToString();
        }

        private string FormatDateTime()
        {
            DateTime time = DateTime.Now;
            StringBuilder builder = new StringBuilder();
            builder.Append(time.ToLongDateString() + " ");
            builder.Append(time.ToLongTimeString() + " ");
            builder.Append("(" + TimeZone.CurrentTimeZone.StandardName + ")");
            return builder.ToString();
        }

        private void UpdateTime() { nextTime = DateTime.Now.ToFileTime() + (long)(floodDelay * TimeSpan.TicksPerMillisecond); }
        private void OnCtcpRequest(string command, UserInfo who)
        {
            if (DateTime.Now.ToFileTime() > nextTime)
            {
                switch (command)
                {
                    case CtcpUtil.Finger:
                        connection.CtcpSender.CtcpReply(command, who.Nick, fingerMessage + " Idle time: " + FormatIdleTime());
                        break;
                    case CtcpUtil.Time:
                        connection.CtcpSender.CtcpReply(command, who.Nick, FormatDateTime());
                        break;
                    case CtcpUtil.UserInfo:
                        connection.CtcpSender.CtcpReply(command, who.Nick, userInfoMessage);
                        break;
                    case CtcpUtil.Version:
                        connection.CtcpSender.CtcpReply(command, who.Nick, versionMessage);
                        break;
                    case CtcpUtil.Source:
                        connection.CtcpSender.CtcpReply(command, who.Nick, sourceMessage);
                        break;
                    case CtcpUtil.ClientInfo:
                        connection.CtcpSender.CtcpReply(command, who.Nick, clientInfoMessage);
                        break;
                    default:
                        string error = command + " is not a supported Ctcp query.";
                        connection.CtcpSender.CtcpReply(command, who.Nick, error);
                        break;
                }
                UpdateTime();
            }
        }
        private void OnCtcpPingRequest(UserInfo who, string timestamp) { connection.CtcpSender.CtcpPingReply(who.Nick, timestamp); }

        internal void Disable()
        {
            connection.CtcpListener.OnCtcpRequest -= new CtcpRequestEventHandler(OnCtcpRequest);
            connection.CtcpListener.OnCtcpPingRequest -= new CtcpPingRequestEventHandler(OnCtcpPingRequest);
        }

    }

    public class CtcpSender : CommandBuilder
    {
        private ArrayList pingList;

        internal CtcpSender(Connection connection)
            : base(connection)
        {
            pingList = new ArrayList();
        }

        internal bool IsMyRequest(string timestamp) { return pingList.Contains(timestamp); }
        internal void ReplyReceived(string timestamp) { pingList.Remove(timestamp); }

        public void CtcpReply(string command, string nick, string reply)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nick.");
                }
                if (reply == null || reply.Trim().Length == 0)
                {
                    ClearBuffer();
                    throw new ArgumentException("Reply cannot be null or empty.");
                }
                if (command == null || command.Trim().Length == 0)
                {
                    ClearBuffer();
                    throw new ArgumentException("The Ctcp command cannot be null or empty.");
                }
                // 14 is NOTICE + 3 x Spaces + : + CR + LF + 2xCtcpQuote
                int max = MAX_COMMAND_SIZE - 14 - nick.Length - command.Length;
                if (reply.Length > max) { reply = reply.Substring(0, max); }
                SendMessage("NOTICE", nick, CtcpQuote + command.ToUpper(CultureInfo.InvariantCulture) + " " + reply + CtcpQuote);
            }
        }
        public void CtcpRequest(string command, string nick)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nick.");
                }
                if (command == null || command.Trim().Length == 0)
                {
                    ClearBuffer();
                    throw new ArgumentException("The Ctcp command cannot be null or empty.");
                }
                SendMessage("PRIVMSG", nick, CtcpQuote + command.ToUpper(CultureInfo.InvariantCulture) + CtcpQuote);
            }
        }
        public void CtcpPingReply(string nick, string timestamp)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nick.");
                }
                if (timestamp == null || timestamp.Trim().Length == 0)
                {
                    ClearBuffer();
                    throw new ArgumentException("Timestamp cannot be null or empty.");
                }
                SendMessage("NOTICE", nick, CtcpQuote + CtcpUtil.Ping + " " + timestamp + CtcpQuote);
            }
        }
        public void CtcpPingRequest(string nick, string timestamp)
        {
            lock (this)
            {
                if (!Rfc2812Util.IsValidNick(nick))
                {
                    ClearBuffer();
                    throw new ArgumentException(nick + " is not a valid nick.");
                }
                pingList.Add(timestamp);
                SendMessage("PRIVMSG", nick, CtcpQuote + CtcpUtil.Ping + " " + timestamp + CtcpQuote);
            }
        }

    }

    public class CtcpUtil
    {
        public const string Finger = "FINGER";
        public const string UserInfo = "USERINFO";
        public const string Version = "VERSION";
        public const string Source = "SOURCE";
        public const string ClientInfo = "CLIENTINFO";
        public const string ErrorMessage = "ERRMSG";
        public const string Ping = "PING";
        public const string Time = "TIME";

        internal static TraceSwitch CtcpTrace = new TraceSwitch("CtcpTraceSwitch", "Debug level for CTCP classes.");

        private CtcpUtil() { }

        public static string CreateTimestamp() { return DateTime.Now.ToFileTime().ToString(CultureInfo.InvariantCulture); }
    }

    #endregion

    #region Dcc

    public class DccChatSession
    {
        public event ChatRequestTimeoutEventHandler OnChatRequestTimeout;
        public event ChatSessionOpenedEventHandler OnChatSessionOpened;
        public event ChatSessionClosedEventHandler OnChatSessionClosed;
        public event ChatMessageReceivedEventHandler OnChatMessageReceived;

        //Default timeout is 30 seconds
        private const int DefaultTimeout = 30000;
        private readonly DccUserInfo dccUserInfo;
        private TcpClient client;
        private TcpListener server;
        private Thread thread;
        private int listenPort;
        private bool listening;
        private bool receiving;

        internal DccChatSession(DccUserInfo dccUserInfo)
        {
            this.dccUserInfo = dccUserInfo;
            listening = false;
            receiving = false;
        }

        public bool Connected { get { return client != null; } }
        public DccUserInfo ClientInfo { get { return dccUserInfo; } }

        private void CloseClientConnection()
        {
            client.GetStream().Close();
            client.Close();
        }

        private void SendClosedEvent()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::SendClosedEvent()");
            if (OnChatSessionClosed != null) { OnChatSessionClosed(this); }
        }

        private void SendChatRequest(string listenIPAddress, int listenPort)
        {
            //512 is the max IRC message size
            StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
            builder.Append(dccUserInfo.Nick);
            builder.Append(" :\x0001DCC CHAT CHAT ");
            builder.Append(DccUtil.IPAddressToLong(IPAddress.Parse(listenIPAddress)));
            builder.Append(" ");
            builder.Append(listenPort);
            builder.Append("\x0001\n");
            dccUserInfo.Connection.Sender.Raw(builder.ToString());
        }

        private void TimerExpired(object state)
        {
            if (listening)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::TimerExpired() Chat session " + this.ToString() + " timed out.");
                if (OnChatRequestTimeout != null) { OnChatRequestTimeout(this); }
                Close();
            }
        }
        private void Listen()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Listen()");
            try
            {
                //Wait for remote client to connect
                IPEndPoint localEndPoint = new IPEndPoint(DccUtil.LocalHost(), listenPort);
                server = new TcpListener(localEndPoint);
                listening = true;
                server.Start();
                //Got one!
                client = server.AcceptTcpClient();
                server.Stop();
                listening = false;
                Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Listen() Remote user connected.");
                if (OnChatSessionOpened != null) { OnChatSessionOpened(this); }
                //Start listening for messages
                ReceiveMessages();
            }
            catch (Exception)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Listen() Connection broken");
            }
            finally
            {
                SendClosedEvent();
            }
        }

        private void Connect()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Connect()");
            try
            {
                client = new TcpClient();
                client.Connect(dccUserInfo.RemoteEndPoint);
                if (OnChatSessionOpened != null) { OnChatSessionOpened(this); }
                ReceiveMessages();
            }
            catch (Exception se)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::Connect() exception=" + se);
                if (se.Message.IndexOf("refused") > 0)
                {
                    dccUserInfo.Connection.Listener.Error(ReplyCode.DccConnectionRefused, "Connection refused by remote user.");
                }
                else
                {
                    dccUserInfo.Connection.Listener.Error(ReplyCode.ConnectionFailed, "Unknown socket error:" + se.Message);
                }
                CloseClientConnection();
            }
            finally
            {
                SendClosedEvent();
            }
        }
        private void ReceiveMessages()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages()");
            try
            {
                receiving = true;
                string message = "";
                StreamReader reader = new StreamReader(client.GetStream(), dccUserInfo.Connection.TextEncoding);
                while ((message = reader.ReadLine()) != null)
                {
                    if (OnChatMessageReceived != null)
                    {
                        Debug.Indent();
                        Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages() Session: " + ToString() + " Received: " + message);
                        Debug.Unindent();
                        OnChatMessageReceived(this, message);
                    }
                }
                receiving = false;
                //Read loop broken. Remote user must have closed the socket
                dccUserInfo.Connection.Listener.Error(ReplyCode.ConnectionFailed, "Chat connection closed by remote user.");
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages() Thread manually stopped. ");
                //Prevent the exception from being re-thrown in the Listen() method.
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages() exception= " + e);
            }
            finally
            {
                CloseClientConnection();
            }
        }

        public void SendMessage(string text)
        {
            if (Connected)
            {
                try
                {
                    byte[] messageBytes = dccUserInfo.Connection.TextEncoding.GetBytes(text.TrimEnd() + "\n");
                    client.GetStream().Write(messageBytes, 0, messageBytes.Length);
                    Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccChatSession::SendMessage() Sent : " + text + " Size: " + messageBytes.Length);
                }
                catch (Exception e)
                {
                    Debug.WriteLineIf(DccUtil.DccTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::SendMessage() " + e);
                }
            }
        }
        public void Close()
        {
            //Locked because it may be called by the Timer or client thread
            lock (this)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Close()");
                if (listening) { server.Stop(); }
                else if (receiving) { thread.Abort(); }
            }
        }
        public override string ToString() { return "DccChatSession::" + dccUserInfo.ToString(); }
        public static DccChatSession Accept(DccUserInfo dccUserInfo)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Accept()");
            DccChatSession session = new DccChatSession(dccUserInfo);
            //Start session Thread
            session.thread = new Thread(new ThreadStart(session.Connect));
            session.thread.Name = session.ToString();
            session.thread.Start();
            return session;
        }
        public static DccChatSession Request(DccUserInfo dccUserInfo, string listenIPAddress, int listenPort) { return Request(dccUserInfo, listenIPAddress, listenPort, DefaultTimeout); }
        public static DccChatSession Request(DccUserInfo dccUserInfo, string listenIPAddress, int listenPort, long timeout)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Request()");
            DccChatSession session = new DccChatSession(dccUserInfo);
            session.listenPort = listenPort;
            session.thread = new Thread(new ThreadStart(session.Listen));
            session.thread.Name = session.ToString();
            session.thread.Start();
            session.SendChatRequest(listenIPAddress, listenPort);
            if (timeout > 0)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Request timeout thread started");
            }
            return session;
        }

    }

    public class DccFileInfo
    {
        private FileInfo fileInfo;
        private FileStream fileStream;
        private long fileStartingPosition;
        private long bytesTransfered;
        private long completeFileSize;
        private long lastAckValue;

        public DccFileInfo(FileInfo fileInfo, long completeFileSize)
        {
            this.fileInfo = fileInfo;
            this.completeFileSize = completeFileSize;
            fileStartingPosition = 0;
            bytesTransfered = 0;
        }

        public DccFileInfo(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            if (!fileInfo.Exists)
            {
                throw new ArgumentException(fileInfo.Name + " does not exist.");
            }
            this.completeFileSize = fileInfo.Length;
            fileStartingPosition = 0;
            bytesTransfered = 0;
        }

        public DccFileInfo(string fileName)
        {
            this.fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException(fileName + " does not exist.");
            }
            this.completeFileSize = fileInfo.Length;
            fileStartingPosition = 0;
            bytesTransfered = 0;
        }

        public long FileStartingPosition { get { return fileStartingPosition; } }

        public long BytesTransfered
        {
            get
            {
                lock (this)
                {
                    return bytesTransfered;
                }
            }
        }

        public long CompleteFileSize { get { return completeFileSize; } }
        public string DccFileName { get { return DccUtil.SpacesToUnderscores(fileInfo.Name); } }
        internal FileStream TransferStream { get { return fileStream; } }

        internal void AddBytesTransfered(int additionalBytes)
        {
            lock (this)
            {
                bytesTransfered += additionalBytes;
            }
        }

        internal bool AcceptPositionMatches(long position)
        {
            return position == fileStartingPosition;
        }

        internal void GotoWritePosition()
        {
            fileStream.Seek(fileStartingPosition + 1, SeekOrigin.Begin);
        }

        internal void GotoReadPosition()
        {
            fileStream.Seek(fileStartingPosition, SeekOrigin.Begin);
        }

        internal bool ResumePositionValid(long position)
        {
            return position > 1 && position < fileInfo.Length;
        }

        internal bool CanResume()
        {
            return fileStream.CanSeek;
        }

        internal void SetResumeToFileSize()
        {
            fileStartingPosition = fileInfo.Length;
        }

        internal void SetResumePosition(long resumePosition)
        {
            fileStartingPosition = resumePosition;
            bytesTransfered = fileStartingPosition;
        }

        internal long CurrentFilePosition()
        {
            return BytesTransfered + fileStartingPosition;
        }

        internal Boolean AllBytesTransfered()
        {
            if (completeFileSize == 0)
            {
                return false;
            }
            else
            {
                return (fileStartingPosition + BytesTransfered) == completeFileSize;
            }
        }

        internal void CloseFile()
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }

        internal void OpenForRead()
        {
            fileStream = fileInfo.OpenRead();
        }

        internal void OpenForWrite()
        {
            fileStream = fileInfo.OpenWrite();
        }

        internal bool ShouldResume()
        {
            return fileInfo.Length > 0 && CanResume();
        }

        internal bool AcksFinished(long ack)
        {
            bool done = (ack == BytesTransfered || ack == lastAckValue);
            lastAckValue = ack;
            return done;
        }

    }

    public class DccFileSession
    {
        public event FileTransferTimeoutEventHandler OnFileTransferTimeout;
        public event FileTransferStartedEventHandler OnFileTransferStarted;
        public event FileTransferInterruptedEventHandler OnFileTransferInterrupted;
        public event FileTransferCompletedEventHandler OnFileTransferCompleted;
        public event FileTransferProgressEventHandler OnFileTransferProgress;

        private bool turboMode;
        private DateTime lastActivity;
        private bool waitingOnAccept;
        private DccUserInfo dccUserInfo;
        private byte[] buffer;
        private int listenPort;
        private string sessionID;
        private Socket socket;
        private Socket serverSocket;
        private Thread thread;

        internal DccFileInfo dccFileInfo;

        internal DccFileSession(DccUserInfo dccUserInfo, DccFileInfo dccFileInfo, int bufferSize, int listenPort, string sessionID)
        {
            this.dccUserInfo = dccUserInfo;
            this.dccFileInfo = dccFileInfo;
            buffer = new byte[bufferSize];
            this.listenPort = listenPort;
            this.sessionID = sessionID;
            lastActivity = DateTime.Now;
            waitingOnAccept = false;
        }

        internal DateTime LastActivity { get { return lastActivity; } }

        public string ID { get { return sessionID; } }

        public DccUserInfo User { get { return dccUserInfo; } }
        public DccFileInfo File { get { return dccFileInfo; } }
        public DccUserInfo ClientInfo { get { return dccUserInfo; } }

        private void SendAccept()
        {
            StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
            builder.Append(dccUserInfo.Nick);
            builder.Append(" :\x0001DCC ACCEPT ");
            builder.Append(dccFileInfo.DccFileName);
            builder.Append(" ");
            builder.Append(listenPort);
            builder.Append(" ");
            builder.Append(dccFileInfo.FileStartingPosition);
            builder.Append("\x0001\n");
            dccUserInfo.Connection.Sender.Raw(builder.ToString());
        }
        private void DccSend(IPAddress sendAddress)
        {
            StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
            builder.Append(dccUserInfo.Nick);
            builder.Append(" :\x0001DCC SEND ");
            builder.Append(dccFileInfo.DccFileName);
            builder.Append(" ");
            builder.Append(DccUtil.IPAddressToLong(sendAddress));
            builder.Append(" ");
            builder.Append(listenPort);
            builder.Append(" ");
            builder.Append(dccFileInfo.CompleteFileSize);
            builder.Append(turboMode ? " T" : "");
            builder.Append("\x0001\n");
            dccUserInfo.Connection.Sender.Raw(builder.ToString());
        }
        private void SendResume()
        {
            StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
            builder.Append(dccUserInfo.Nick);
            builder.Append(" :\x0001DCC RESUME ");
            builder.Append(dccFileInfo.DccFileName);
            builder.Append(" ");
            builder.Append(listenPort);
            builder.Append(" ");
            builder.Append(dccFileInfo.FileStartingPosition);
            builder.Append("\x0001\n");
            dccUserInfo.Connection.Sender.Raw(builder.ToString());
        }
        private void Cleanup()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Cleanup()");
            DccFileSessionManager.DefaultInstance.RemoveSession(this);
            if (serverSocket != null)
            {
                serverSocket.Close();
            }
            if (socket != null)
            {
                try
                {
                    socket.Close();
                }
                catch (Exception)
                {
                    //Ignore this exception
                }
            }
            dccFileInfo.CloseFile();
        }
        private void ResetActivityTimer()
        {
            lastActivity = DateTime.Now;
        }
        private void SignalTransferStart()
        {
            ResetActivityTimer();
            if (OnFileTransferStarted != null)
            {
                OnFileTransferStarted(this);
            }
        }
        private void Listen()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Listen()");
            try
            {
                //Wait for remote client to connect
                IPEndPoint localEndPoint = new IPEndPoint(DccUtil.LocalHost(), listenPort);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(1);
                //Got one!
                socket = serverSocket.Accept();
                serverSocket.Close();
                Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Listen() Remote user connected.");
                //Advance to the correct point in the file in case this is a resume 
                dccFileInfo.GotoReadPosition();
                SignalTransferStart();
                if (turboMode)
                {
                    Upload();
                }
                else
                {
                    UploadLegacy();
                }
            }
            catch (Exception)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::Listen() Connection broken");
                Interrupted();
            }
        }
        private void Upload()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Upload()" + (turboMode ? " Turbo" : " Legacy") + " mode");
            try
            {
                int bytesRead = 0;
                //	byte[] ack = new byte[4];
                while ((bytesRead = dccFileInfo.TransferStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    socket.Send(buffer, 0, bytesRead, SocketFlags.None);
                    ResetActivityTimer();
                    AddBytesProcessed(bytesRead);
                }
                //Now we are done
                Finished();
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::Upload() exception=" + e);
                Interrupted();
            }
        }
        private void UploadLegacy()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::UploadLegacy()");
            try
            {
                int bytesRead = 0;
                byte[] ack = new byte[4];
                while ((bytesRead = dccFileInfo.TransferStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    socket.Send(buffer, 0, bytesRead, SocketFlags.None);
                    ResetActivityTimer();
                    AddBytesProcessed(bytesRead);
                    //Wait for acks from client
                    socket.Receive(ack);
                }
                //Some IRC clients need a moment to catch up on their acks if our send buffer
                //is larger than their receive buffer. Test to make sure they ack all the bytes
                //before closing. This is only needed in legacy mode.
                while (!dccFileInfo.AcksFinished(DccUtil.DccBytesToLong(ack)))
                {
                    socket.Receive(ack);
                }
                //Now we are done
                Finished();
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::UploadLegacy() exception=" + e);
                Interrupted();
            }
        }
        private void Download()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Download()" + (turboMode ? " Turbo" : " Legacy") + " mode");
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(dccUserInfo.RemoteEndPoint);
                int bytesRead = 0;
                while (!dccFileInfo.AllBytesTransfered())
                {
                    bytesRead = socket.Receive(buffer);
                    //Remote server closed the connection before all bytes were sent
                    if (bytesRead == 0)
                    {
                        Interrupted();
                        return;
                    }
                    ResetActivityTimer();
                    AddBytesProcessed(bytesRead);
                    dccFileInfo.TransferStream.Write(buffer, 0, bytesRead);
                    //Send ack if in legacy mode
                    if (!turboMode)
                    {
                        socket.Send(DccUtil.DccBytesReceivedFormat(dccFileInfo.CurrentFilePosition()));
                    }
                }
                dccFileInfo.TransferStream.Flush();
                Finished();
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::Download() exception=" + e);
                if (e.Message.IndexOf("refused") > 0)
                {
                    dccUserInfo.Connection.Listener.Error(ReplyCode.DccConnectionRefused, "Connection refused by remote user.");
                }
                else
                {
                    dccUserInfo.Connection.Listener.Error(ReplyCode.ConnectionFailed, "Unknown socket error:" + e.Message);
                }
                Interrupted();
            }
        }

        internal void AddBytesProcessed(int bytesRead)
        {
            dccFileInfo.AddBytesTransfered(bytesRead);
            if (OnFileTransferProgress != null)
            {
                OnFileTransferProgress(this, bytesRead);
            }
        }

        internal void OnDccAcceptReceived(long position)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::OnDccAcceptReceived()");
            lock (this)
            {
                //Are we still waiting on the accept?
                if (!waitingOnAccept)
                {
                    //Assume that a normal receive has gone ahead
                    return;
                }
                //No longer waiting
                waitingOnAccept = false;
                if (!dccFileInfo.AcceptPositionMatches(position))
                {
                    dccUserInfo.Connection.Listener.Error(ReplyCode.BadDccAcceptValue, "Asked to start at " + dccFileInfo.FileStartingPosition + " but was sent " + position);
                    Interrupted();
                    return;
                }
                ResetActivityTimer();
                dccFileInfo.SetResumeToFileSize();
                dccFileInfo.GotoWritePosition();
                thread = new Thread(new ThreadStart(Download));
                thread.Name = ToString();
                thread.Start();
            }
        }

        internal void OnDccResumeRequest(long resumePosition)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::OnDccResumeRequest()");
            lock (this)
            {
                ResetActivityTimer();
                //Make sure we have not already started transfering data and that this file is
                //resumeable.
                if (dccFileInfo.BytesTransfered == 0 && dccFileInfo.CanResume())
                {
                    //Make sure the position is valid
                    if (dccFileInfo.ResumePositionValid(resumePosition))
                    {
                        dccFileInfo.SetResumePosition(resumePosition);
                        SendAccept();
                    }
                    else
                    {
                        dccUserInfo.Connection.Listener.Error(ReplyCode.BadResumePosition, ToString() + " sent an invalid resume position.");
                        //Close the socket and stop listening
                        Cleanup();
                    }
                }
            }
        }

        internal void TimedOut()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, ToString() + " timed out.");
            if (waitingOnAccept)
            {
                waitingOnAccept = false;
                //Start a new thread to download the whole file
                thread = new Thread(new ThreadStart(Download));
                thread.Name = ToString();
                thread.Start();
            }
            else
            {
                if (OnFileTransferTimeout != null)
                {
                    OnFileTransferTimeout(this);
                }
                Cleanup();
            }
        }

        internal void Interrupted()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Interrupted()");
            Cleanup();
            if (OnFileTransferInterrupted != null)
            {
                OnFileTransferInterrupted(this);
            }
        }

        internal void Finished()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Finished()");
            Cleanup();
            if (OnFileTransferCompleted != null)
            {
                OnFileTransferCompleted(this);
            }
        }

        public void Stop()
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Stop()");
            lock (this)
            {
                Cleanup();
                if (OnFileTransferInterrupted != null)
                {
                    OnFileTransferInterrupted(this);
                }
            }
        }

        public override string ToString()
        {
            return "DccFileSession:: ID=" + sessionID + " User=" + dccUserInfo.ToString() + " File=" + dccFileInfo.DccFileName;
        }

        public static void Get(Connection connection, string nick, string fileName, bool turbo)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Get()");
            StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
            builder.Append(nick);
            builder.Append(" :\x0001DCC GET ");
            builder.Append(fileName);
            builder.Append(turbo ? " T" : "");
            builder.Append("\x0001\n");
            connection.Sender.Raw(builder.ToString());
        }

        public static DccFileSession Send(
            DccUserInfo dccUserInfo,
            string listenIPAddress,
            int listenPort,
            DccFileInfo dccFileInfo,
            int bufferSize,
            bool turbo)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Send()");
            DccFileSession session = null;
            //Test if we are already using this port
            if (DccFileSessionManager.DefaultInstance.ContainsSession("S" + listenPort))
            {
                throw new ArgumentException("Already listening on port " + listenPort);
            }
            try
            {
                session = new DccFileSession(dccUserInfo, dccFileInfo, bufferSize, listenPort, "S" + listenPort);
                //set turbo mode
                session.turboMode = turbo;
                //Add session to active sessions hashtable
                DccFileSessionManager.DefaultInstance.AddSession(session);
                //Create stream to file
                dccFileInfo.OpenForRead();
                //Start session Thread
                session.thread = new Thread(new ThreadStart(session.Listen));
                session.thread.Name = session.ToString();
                session.thread.Start();
                //Send DCC Send request to remote user
                session.DccSend(IPAddress.Parse(listenIPAddress));
                return session;
            }
            catch (Exception e)
            {
                if (session != null)
                {
                    DccFileSessionManager.DefaultInstance.RemoveSession(session);
                }
                throw e;
            }
        }

        public static DccFileSession Receive(DccUserInfo dccUserInfo, DccFileInfo dccFileInfo, bool turbo)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Receive()");
            //Test if we are already using this port
            if (DccFileSessionManager.DefaultInstance.ContainsSession("C" + dccUserInfo.remoteEndPoint.Port))
            {
                throw new ArgumentException("Already listening on port " + dccUserInfo.remoteEndPoint.Port);
            }
            DccFileSession session = null;
            try
            {
                session = new DccFileSession(dccUserInfo, dccFileInfo, (64 * 1024),
                    dccUserInfo.remoteEndPoint.Port, "C" + dccUserInfo.remoteEndPoint.Port);
                //Has the initiator specified the turbo protocol? 
                session.turboMode = turbo;
                //Open file for writing
                dccFileInfo.OpenForWrite();
                DccFileSessionManager.DefaultInstance.AddSession(session);
                //Determine if we can resume a download
                if (session.dccFileInfo.ShouldResume())
                {
                    session.waitingOnAccept = true;
                    session.dccFileInfo.SetResumeToFileSize();
                    session.SendResume();
                }
                else
                {
                    session.thread = new Thread(new ThreadStart(session.Download));
                    session.thread.Name = session.ToString();
                    session.thread.Start();
                }
                return session;
            }
            catch (Exception e)
            {
                if (session != null)
                {
                    DccFileSessionManager.DefaultInstance.RemoveSession(session);
                }
                throw e;
            }
        }

    }

    public class DccFileSessionManager
    {
        //How long to wait
        private TimeSpan timeout;
        //A clone of the session hashtable to iterate over
        private Hashtable sessionClone;
        //A place to store the active sessions
        private Hashtable sessions;
        //Check for timeouts every 10 seconds
        private const int TimeoutCheckPeriod = 10000;
        //Default to tming out after 30 seconds of no activity.
        private const int DefaultTimeout = 30000;
        private static DccFileSessionManager defaultInstance;
        private static object lockObject = new object();
        private Timer timerThread;
        private bool timerStopped;

        private DccFileSessionManager()
        {
            timeout = new TimeSpan(DefaultTimeout * TimeSpan.TicksPerMillisecond);
            //Create Timer but don't start it yet
            timerThread = new Timer(new TimerCallback(CheckSessions), null, Timeout.Infinite, TimeoutCheckPeriod);
            timerStopped = true;
            sessions = Hashtable.Synchronized(new Hashtable());
        }

        private Boolean TimedOut(DccFileSession session)
        {
            if ((DateTime.Now - session.LastActivity) >= timeout)
            {
                return true;
            }
            return false;
        }

        internal void AddSession(DccFileSession session)
        {
            sessions.Add(session.ID, session);
            if (timerStopped)
            {
                timerStopped = false;
                timerThread.Change(TimeoutCheckPeriod, TimeoutCheckPeriod);
            }
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccSessionManager::AddSession() ID=" + session.ID);
        }
        internal void RemoveSession(DccFileSession session)
        {
            sessions.Remove(session.ID);
            if (sessions.Count == 0)
            {
                timerStopped = true;
                timerThread.Change(Timeout.Infinite, TimeoutCheckPeriod);
            }
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccSessionManager::RemoveSession() ID=" + session.ID);
        }
        internal void CheckSessions(object state)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccSessionManager::CheckSessions()");
            sessionClone = (Hashtable)sessions.Clone();
            foreach (object session in sessionClone.Values)
            {
                DccFileSession fileSession = (DccFileSession)session;
                lock (fileSession)
                {
                    if (TimedOut(fileSession))
                    {
                        fileSession.TimedOut();
                    }
                }
            }
        }
        internal bool ContainsSession(string sessionID) 
        {
            return sessions.Contains(sessionID);
        }
        internal DccFileSession LookupSession(string sessionID)
        {
            //Make sure this session is till active
            if (!ContainsSession(sessionID))
            {
                throw new ArgumentException(sessionID + " is not active.");
            }
            //Lookup corresponding session
            return (DccFileSession)sessions[sessionID];
        }

        public static DccFileSessionManager DefaultInstance
        {
            get
            {
                lock (lockObject)
                {
                    if (defaultInstance == null)
                    {
                        defaultInstance = new DccFileSessionManager();
                        Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccFileSessionManager::init");
                    }
                }
                return defaultInstance;
            }
        }

        public long TimeoutPeriod
        {
            get
            {
                lock (defaultInstance)
                {
                    return timeout.Ticks * TimeSpan.TicksPerMillisecond;
                }
            }
            set
            {
                lock (defaultInstance)
                {
                    timeout = new TimeSpan(value * TimeSpan.TicksPerMillisecond);
                }
            }
        }

    }

    public class DccListener
    {
        public event DccChatRequestEventHandler OnDccChatRequest;
        public event DccSendRequestEventHandler OnDccSendRequest;
        public event DccGetRequestEventHandler OnDccGetRequest;

        private static DccListener listener;
        private static readonly object lockObject = new object();
        //Checks if a line is likely a DCC request
        private static readonly Regex dccMatchRegex;
        //Split the DCC into space separated tokens
        private readonly Regex tokenizer;
        //Extract out the DCC specific info
        private readonly Regex parser;
        //The values of the DCC string tokens
        private const int Action = 0;
        private const int FileName = 1;
        private const int Address = 2;
        private const int Port = 3;
        private const int FileSize = 4;
        //DCC action types
        private const string CHAT = "CHAT";
        private const string SEND = "SEND";
        private const string GET = "GET";
        private const string RESUME = "RESUME";
        private const string ACCEPT = "ACCEPT";

        static DccListener()
        {
            dccMatchRegex = new Regex(":([^ ]+) PRIVMSG [^:]+:\u0001DCC (CHAT|SEND|GET|RESUME|ACCEPT)[^\u0001]*\u0001", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        private DccListener()
        {
            parser = new Regex(":([^ ]+) PRIVMSG [^:]+:\u0001DCC ([^\u0001]*)\u0001", RegexOptions.Compiled | RegexOptions.Singleline);
            tokenizer = new Regex("[\\s]+", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        private bool IsTurbo(int minimumTokens, string[] tokens)
        {
            if (tokens.Length <= minimumTokens)
            {
                return false;
            }
            return tokens[minimumTokens] == "T";
        }

        internal void Parse(Connection connection, string message)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccListener::Parse()");
            Match match = parser.Match(message);
            string requestor = match.Groups[1].ToString();
            string[] tokens = tokenizer.Split(match.Groups[2].ToString().Trim());
            switch (tokens[Action])
            {
                case CHAT:
                    if (OnDccChatRequest != null)
                    {
                        //Test for sufficient number of arguments
                        if (tokens.Length < 4)
                        {
                            connection.Listener.Error(ReplyCode.UnparseableMessage, "Incorrect CHAT arguments: " + message);
                            return;
                        }
                        //Send event
                        DccUserInfo dccUserInfo = null;
                        try
                        {
                            dccUserInfo = new DccUserInfo(
                                connection,
                                Rfc2812Util.ParseUserInfoLine(requestor),
                                new IPEndPoint(DccUtil.LongToIPAddress(tokens[Address]), int.Parse(tokens[Port], CultureInfo.InvariantCulture)));
                        }
                        catch (ArgumentException)
                        {
                            connection.Listener.Error(ReplyCode.BadDccEndpoint, "Invalid TCP/IP connection information sent.");
                            return;
                        }
                        try
                        {
                            OnDccChatRequest(dccUserInfo);
                        }
                        catch (ArgumentException ae)
                        {
                            connection.Listener.Error(ReplyCode.UnknownEncryptionProtocol, ae.ToString());
                        }
                    }
                    break;
                case SEND:
                    //Test for sufficient number of arguments
                    if (tokens.Length < 5)
                    {
                        connection.Listener.Error(ReplyCode.UnparseableMessage, "Incorrect SEND arguments: " + message);
                        return;
                    }
                    if (OnDccSendRequest != null)
                    {
                        DccUserInfo dccUserInfo = null;
                        try
                        {
                            dccUserInfo = new DccUserInfo(
                                connection,
                                Rfc2812Util.ParseUserInfoLine(requestor),
                                new IPEndPoint(DccUtil.LongToIPAddress(tokens[Address]), int.Parse(tokens[Port], CultureInfo.InvariantCulture)));
                        }
                        catch (ArgumentException ae)
                        {
                            connection.Listener.Error(ReplyCode.BadDccEndpoint, ae.ToString());
                            return;
                        }
                        try
                        {
                            OnDccSendRequest(
                                dccUserInfo,
                                tokens[FileName],
                                int.Parse(tokens[FileSize], CultureInfo.InvariantCulture),
                                IsTurbo(5, tokens));
                        }
                        catch (ArgumentException ae)
                        {
                            connection.Listener.Error(ReplyCode.UnknownEncryptionProtocol, ae.ToString());
                        }
                    }
                    break;
                case GET:
                    //Test for sufficient number of arguments
                    if (tokens.Length < 2)
                    {
                        connection.Listener.Error(ReplyCode.UnparseableMessage, "Incorrect GET arguments: " + message);
                        return;
                    }
                    if (OnDccGetRequest != null)
                    {
                        try
                        {
                            OnDccGetRequest(
                                new DccUserInfo(
                                connection,
                                Rfc2812Util.ParseUserInfoLine(requestor)),
                                tokens[FileName],
                                IsTurbo(2, tokens));
                        }
                        catch (ArgumentException ae)
                        {
                            connection.Listener.Error(ReplyCode.UnknownEncryptionProtocol, ae.ToString());
                        }
                    }
                    break;
                case ACCEPT:
                    //Test for sufficient number of arguments
                    if (tokens.Length < 4)
                    {
                        connection.Listener.Error(ReplyCode.UnparseableMessage, "Incorrect DCC ACCEPT arguments: " + message);
                        return;
                    }
                    //DccListener will try to handle Receive at correct file position
                    try
                    {
                        DccFileSession session = DccFileSessionManager.DefaultInstance.LookupSession("C" + tokens[2]);
                        session.OnDccAcceptReceived(long.Parse(tokens[3], CultureInfo.InvariantCulture));
                    }
                    catch (ArgumentException e)
                    {
                        connection.Listener.Error(ReplyCode.UnableToResume, e.ToString());
                    }
                    break;
                case RESUME:
                    //Test for sufficient number of arguments
                    if (tokens.Length < 4)
                    {
                        connection.Listener.Error(ReplyCode.UnparseableMessage, "Incorrect DCC RESUME arguments: " + message);
                        return;
                    }
                    //DccListener will automatically handle Resume/Accept interaction
                    try
                    {
                        DccFileSession session = DccFileSessionManager.DefaultInstance.LookupSession("S" + tokens[2]);
                        session.OnDccResumeRequest(long.Parse(tokens[3], CultureInfo.InvariantCulture));
                    }
                    catch (ArgumentException e)
                    {
                        connection.Listener.Error(ReplyCode.UnableToResume, e.ToString());
                    }
                    break;
                default:
                    connection.Listener.Error(ReplyCode.UnparseableMessage, message);
                    Debug.WriteLineIf(DccUtil.DccTrace.TraceError, "[" + Thread.CurrentThread.Name + "] DccListener::Parse() Unknown DCC command");
                    break;
            }
        }

        public static DccListener DefaultInstance
        {
            get
            {
                lock (lockObject)
                {
                    if (listener == null)
                    {
                        Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccListener::init");
                        listener = new DccListener();
                    }
                }
                return listener;
            }
        }

        public static bool IsDccRequest(string message) { return dccMatchRegex.IsMatch(message); }
    }

    public class DccUserInfo : UserInfo
    {
        private Connection connection;

        internal IPEndPoint remoteEndPoint;

        internal DccUserInfo(Connection connection, string[] userInfoParts, IPEndPoint remoteEndPoint) :
            base(userInfoParts[0], userInfoParts[1], userInfoParts[2])
        {
            this.connection = connection;
            this.remoteEndPoint = remoteEndPoint;
        }

        internal DccUserInfo(Connection connection, string[] userInfoParts) :
            base(userInfoParts[0], userInfoParts[1], userInfoParts[2])
        {
            this.connection = connection;
        }

        public DccUserInfo(Connection connection, string nick) :
            base(nick, "", "")
        {
            this.connection = connection;
        }

        public IPAddress RemoteAddress
        {
            get
            {
                if (remoteEndPoint == null)
                {
                    return null;
                }
                return remoteEndPoint.Address;
            }
        }

        public int Port
        {
            get
            {
                if (remoteEndPoint == null)
                {
                    return -1;
                }
                return remoteEndPoint.Port;
            }
        }

        public IPEndPoint RemoteEndPoint { get { return remoteEndPoint; } }
        public Connection Connection { get { return connection; } }

        public override string ToString()
        {
            if (RemoteAddress == null)
            {
                return Nick;
            }
            else
            {
                return Nick + "@" + RemoteAddress.ToString();
            }
        }
    }

    public class DccUtil
    {
        internal static TraceSwitch DccTrace = new TraceSwitch("DccTraceSwitch", "Debug level for DCC classes.");

        private DccUtil() { }

        public static IPAddress LocalHost()
        {
            IPHostEntry localhost = Dns.GetHostEntry(Dns.GetHostName());
            return localhost.AddressList[0];
        }

        public static byte[] DccBytesReceivedFormat(long bytesReceived)
        {
            byte[] size = new byte[4];
            byte[] longBytes = BitConverter.GetBytes(NetworkUnsignedLong(bytesReceived));
            Array.Copy(longBytes, 0, size, 0, 4);
            return size;
        }

        public static long DccBytesToLong(byte[] received) { return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(received, 0)); }

        public static string IPAddressToLong(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentException("Address cannot be null");
            }
            return NetworkUnsignedLong(ipAddress.Address).ToString(CultureInfo.InvariantCulture);
        }

        public static IPAddress LongToIPAddress(string networkOrder)
        {
            if (networkOrder == null || networkOrder.Trim() == "")
            {
                throw new ArgumentException("Network order address cannot be null or empty.");
            }
            try
            {
                byte[] quads = BitConverter.GetBytes(long.Parse(networkOrder, CultureInfo.InvariantCulture));
                return IPAddress.Parse(quads[3] + "." + quads[2] + "." + quads[1] + "." + quads[0]);
            }
            catch (FormatException)
            {
                throw new ArgumentException(networkOrder + " is not a valid network address.");
            }

        }

        public static string SpacesToUnderscores(string fileName) { return fileName.Replace(' ', '_'); }

        private static long NetworkUnsignedLong(long hostOrderLong)
        {
            long networkLong = IPAddress.HostToNetworkOrder(hostOrderLong);
            return (networkLong >> 32) & 0x00000000ffffffff;
        }

    }

    #endregion
}
