using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skt_05;
using System.Net.Sockets;

namespace Virtual.Engine.Sockets
{
    public class Listener
    {

        private AsyncSkt _socketListener;
        private AsyncSkt _socketTalker;

        private string _identifier = "";
        /*
                string TalkingIP;
        */
        string TalkingPort = "1001";
        string ListeningPort = "1000";

        #region "Events"

        public delegate void DataArrivalEventHandler(SocketCommand Command, string ID);
        public event DataArrivalEventHandler DataArrival;
        protected void OnDataArrival(SocketCommand Command, string ID)
        {
            //DataArrival(CommandtoRaise);
            if (DataArrival != null)
            {
                DataArrival(Command, ID);
            }
        }

        public delegate void ConnectionHandler();
        public event ConnectionHandler Connected;
        protected void OnConnected()
        {
            if (Connected != null)
            {
                Connected();
            }
        }

        public delegate void DisconnectionHandler();

        public event DisconnectionHandler Disconnected;
        protected void OnDisconnected()
        {
            if (Disconnected != null)
            {
                Disconnected();
            }
        }

        #endregion

        #region "Public"

        // on instantiation...
        public Listener()
        {
            try
            {
                //LogAccess.LogOutput = LogAccess.LogOutputType.Local;
            }
            catch (Exception)
            {
            }

            InitializeListener();
        }

        public Listener(string PortToListenOn, string ID = "")
        {
            try
            {
                //LogAccess.LogOutput = LogAccess.LogOutputType.Local;
            }
            catch (Exception)
            {
            }

            _identifier = ID;

            InitializeListener(PortToListenOn);
        }

        private void InitializeListener(string PortToListenOn = "")
        {
            if (PortToListenOn == "")
            {
                // if not Port passed in, use default
                PortToListenOn = ListeningPort;
            }

            try
            {
                //LogAccess.WriteLog("Initializing Listener...", "Listener");
            }
            catch (Exception ex)
            {
            }

            _socketListener = new AsyncSkt();

            // create callback delegates:
            _socketListener.ConnectionAccepted += new AsyncSkt.ConnectionAcceptedEventHandler(ProcessOnTalkerConnect);
            _socketListener.MsgReceived += new AsyncSkt.MsgReceivedEventHandler(ProcessOnDataArrival);
            _socketListener.SktError += new AsyncSkt.SktErrorEventHandler(ProcessError);
            //SocketConnection.SktError += new AsyncSkt.SktErrorEventHandler(SocketErrorHandler);
            _socketListener.Closed += ProcessOnDisconnect;

            Listen(PortToListenOn);

        }

        ~Listener()
        {
            if (_socketTalker != null)
            {
                _socketTalker.Close();
            }

            if (_socketListener != null)
            {
                _socketListener.Close();
            }
        }

        // public routines
        public void Connect(string DestinationIP, string DesitnationPort)
        {
            System.Net.IPAddress remoteConnection;
            remoteConnection = System.Net.IPAddress.Parse(DestinationIP);

            if (_socketTalker == null) { _socketTalker = new AsyncSkt(); }

            _socketTalker.RemoteIP = remoteConnection;
            _socketTalker.Port = Convert.ToInt32(TalkingPort);
            _socketTalker.PacketSize = 8192;

            //LogAccess.WriteLog("Connecting (port=" + TalkingPort + ")", "Listener");

            _socketTalker.Connect();

        }

        //added by wknight to allow tcplistener to send back command success or failure from
        //the listener
        public bool SendData(SocketCommand CommandObjectToSend)
        {
            return SendData(CommandObjectToSend, _socketTalker);
        }

        public bool SendData(string dataString)
        {
            return SendData(dataString, _socketTalker);
        }

        #endregion

        #region "Private"


        private void SendAcknowledgement(string receivedCommandID)
        {
            SocketCommand acknowledgementCommand = new SocketCommand
            {
                Command = CommandType.ReceiptAcknowledgement,
                CommandID = receivedCommandID
            };

            //LogAccess.WriteLog("Sending Acknowledgement.", "Listener");
            SendData(acknowledgementCommand, _socketTalker);
        }

        private void Listen(string PortToListenOn)
        {
            //LogAccess.WriteLog("Listening...", "Listener");
            // begin listening
            _socketListener.Port = Convert.ToInt32(PortToListenOn); // 1000;
            //_socketListener.NoDelay = false;
            _socketListener.PacketSize = 8192;

            //_socketListener.
            //_socketListener.Close();
            _socketListener.StartListening();

        }

        // private routines
        private void SendHeartBeat(AsyncSkt SocketToSendOn)
        {
            SocketCommand myHeartBeat = new SocketCommand()
            {
                ID = TcpIpCommon.GetMyID(),
                Timestamp = DateTime.Now,
                Command = CommandType.Heartbeat
            };
            //LogAccess.WriteLog("...heartbeat...", "Listener");
            SendData(myHeartBeat, SocketToSendOn);
        }

        private bool SendData(SocketCommand CommandObjectToSend, AsyncSkt ConnectionToUse)
        {
            // do we have a valid connection?
            if (ConnectionToUse == null)
            { return false; }

            // do we have an object to send?
            if (CommandObjectToSend == null)
            { return false; }

            string SerializedData = "";
            SerializedData = TcpIpCommon.SerializeCommandObject(CommandObjectToSend);

            // (bsk) add divider for splitting...
            SerializedData += "<msg>";

            try
            {
                ConnectionToUse.SendData(SerializedData);
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog("Error sending data: " + ex.ToString(), "Listener");
                //throw;
            }

            return true;
        }

        private bool SendData(string dataString, AsyncSkt ConnectionToUse)
        {
            if (ConnectionToUse == null)
            { return false; }

            // do we have an object to send?
            if (dataString == null)
            { return false; }

            try
            {
                ConnectionToUse.SendData(dataString);
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog("Error sending data: " + ex.ToString(), "Listener");
                //throw;
            }

            return true;
        }

        #endregion

        #region "CallBack Events"

        private void ProcessError(SocketException ex)
        {
            if (ex.NativeErrorCode == 10054)
            {
                // socket has been closed
                ProcessOnDisconnect();
            }
            else
            {
                throw ex;
            }
            //Debug.Print(ex.Message.ToString());
        }

        // routines to process events from socket delegates: 
        private void ProcessOnTalkerConnect()
        {
            // get talker's ip...
            System.Net.IPAddress talkerIP;
            talkerIP = _socketListener.RemoteIP;

            int TalkingPortInt = _socketListener.Port + 1;
            TalkingPort = TalkingPortInt.ToString();


            //LogAccess.WriteLog("Talker connected!", "Listener");
            OnConnected();

            Connect(talkerIP.ToString(), TalkingPort);

        }

        private bool tracing = true;
        private void ProcessOnDataArrival(string incomingCommand)
        {

            SocketCommand incomingCommandObject = new SocketCommand();

            // split..?
            string[] stringSeperators = new string[] { "<msg>" };
            string[] myCommands = incomingCommand.Split(stringSeperators, StringSplitOptions.RemoveEmptyEntries);

            //LogAccess.WriteLog("Data arrival event", "Listener");
            foreach (string command in myCommands)
            {

                if (tracing)
                {
                    // we need to log the command with a timestamp
                    //TcpIpCommon.WriteToTraceLogSimple(command.ToString());
                }

                try
                {
                    incomingCommandObject = TcpIpCommon.DeserializeCommandObject(command);
                }
                catch (Exception ex)
                {
                    // log here?
                    //Debug.Print(ex.ToString());
                    //LogAccess.WriteLog(ex.ToString());
                    //LogAccess.WriteLog("Bad Command received: " + command.ToString(), "Listener");
                    incomingCommandObject = null;
                    //throw;
                }

                if (incomingCommandObject != null)
                {
                    SendAcknowledgement(incomingCommandObject.CommandID);
                    OnDataArrival(incomingCommandObject, _identifier);
                }

            }
        }

        private void ProcessOnDisconnect()
        {

            //LogAccess.WriteLog("Disconnected!", "Listener");
            //Debug.Print("disconnected");
            if (_socketTalker != null)
            {
                _socketTalker.Close();
            }
            if (_socketListener != null)
            {
                string myPort = _socketListener.Port.ToString();
                _socketListener.Close();
                //_socketListener = new AsyncSkt();

                //Listen(ListeningPort);

                InitializeListener(myPort);

            }
        }

        #endregion


    }
}
