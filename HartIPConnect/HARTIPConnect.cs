/*
Copyright 2019 FieldComm Group, Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Filename: HARTIPConnect.cs

Description: Use UDP or TCP HART-IP Protocol to send HART-IP messages
to the Server.

 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace FieldCommGroup.HartIPConnect
{
    /// <summary>
    /// MsgRequest class store the request's transaction id and
    /// the MsgResponse blocking object.
    /// </summary>
    internal class MsgRequest
    {
        public ushort ReqMsgId;
        public MsgResponse Response;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="ReqMsgId">ushort request's Transaction Id</param>
        /// <param name="Response"><see cref="MsgResponse"/></param>
        public MsgRequest(ushort ReqMsgId, MsgResponse Response)
        {
            this.ReqMsgId = ReqMsgId;
            this.Response = Response;
        }
    }

    /// <summary>
    /// Published command notify Callback event argument
    /// </summary>
    public class HartIPResponseArg : EventArgs
    {
        private HartIPResponse m_Rsp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Rsp"><see cref="HartIPResponse"/></param>
        internal HartIPResponseArg(HartIPResponse Rsp)
        {
            this.m_Rsp = Rsp;
        }

        /// <summary>
        /// Hart IP response
        /// </summary>
        public HartIPResponse Response
        {
            get { return m_Rsp; }
        }
    }
    
    /// <summary>
    /// Network connection using the UDP/TCP HART-IP Protocol
    /// </summary>
    public abstract class HARTIPConnect
    {
      /// <summary>
      /// UDP connection type
      /// </summary>
      public const uint UDP = 0;

      /// <summary>
      /// TCP connection type
      /// </summary>
      public const uint TCP = 1;

      /// <summary>
      /// Secured TCP connection type
      /// </summary>
      public const uint SECUREDTCP = 2;
    
      /// <summary>
      /// Default HART-IP port
      /// </summary>
      public const int HARTPORT_DEFAULT = 5094;

        /// <summary>
        /// Use Default Socket timeout
        /// </summary>
        public const int USE_SOCKET_TIMEOUT_DEFAULT = -1;

        /// <summary>
        /// Default Socket timeout
        /// </summary>
        public const int SOCKET_TIMEOUT_DEFAULT = 60000;    // ms

      /// <summary>
      /// Minimum Socket timeout
      /// </summary>
      public const int MIN_SOCKET_TIMEOUT = 30000;  // ms

      /// <summary>
      /// IO Socket timeout used for the IO device that is connected via IP
      /// </summary>
      public const int IO_SOCKET_TIMEOUT = 32;  // ms

      /// <summary>
      /// Receive published data command event notify
      /// </summary>
      /// <remarks>Add delegate method to consume the received published data commands.
      /// <para>
      /// Notify event will be sent after the connected client application 
      /// sent the HART 219 'Write Burst Through Mode' command with the 
      /// 'Burst Through Mode'=1 request to the HIPDevice and
      /// received the HIPDevice's broadcast burst messages.
      /// </para>
      /// </remarks>
      public event EventHandler<HartIPResponseArg> PublishedCmdNotify;

      /// <summary>
      /// Maximun transaction Id
      /// </summary>
      private const ushort MAX_TRANSACTION_ID = 10000;

      /// <summary>
      /// Listen thread closeing wait interval
      /// </summary>
      private const int WAIT_INTERVAL = 100;

      private String m_strHost = String.Empty;
      private long m_lLastActivityTime = 0;
      private uint m_nNextTranIndex = 0;
      private uint m_InactivityCloseTime = 0;
      private bool m_bStopped = true;
      private Thread m_RspMsgReader;
      private List<MsgRequest> m_Requests = new List<MsgRequest>();
      private Dictionary<int, string> m_SessionInitStatusCodes = new Dictionary<int, string>();

      /// <summary>
      /// SyncRoot
      /// </summary>
      protected Object SyncRoot = new Object();
      /// <summary>
      /// m_bConnected
      /// </summary>
      protected bool m_bConnected = false;
      /// <summary>
      /// m_HostIp
      /// </summary>
      protected IPAddress m_HostIp;
      /// <summary>
      /// m_nPort
      /// </summary>
      protected int m_nPort = 0;
      /// <summary>
      /// m_nSendPort
      /// </summary>
      protected int m_nSendPort = 0; 
      /// <summary>
      /// m_nConnection
      /// </summary>
      protected uint m_nConnection = 0;      
      /// <summary>
      /// m_nSocketTimeout
      /// </summary>
      protected int m_nSocketTimeout = SOCKET_TIMEOUT_DEFAULT;
      /// <summary>
      /// m_nSocketTimeout
      /// </summary>
      protected String m_Error = String.Empty;
      /// <summary>
      /// m_bTraceMsgs
      /// </summary>
      protected bool m_bTraceMsgs;

      /// <summary>
      /// Send a Hart-IP request to network HART-IP device.     
      /// </summary>
      /// <param name="Request">HartIPRequest</param>      
      /// <returns>bool True if it is success to send the HART-IP request.</returns>
      protected abstract bool SendRequest(HartIPRequest Request);

      /// <summary>
      /// Receive HART-IP Response.
      /// </summary>
      /// <returns>HartIPResponse</returns>
      protected abstract HartIPResponse GetResponse();

      /// <summary>
      /// Connect the network
      /// </summary>
      /// <returns>bool</returns>
      protected abstract bool Connect();

      /// <summary>
      /// Close the network connection
      /// </summary>
      public abstract void Close();    

      /// <summary>
      /// Transaction ID
      /// </summary>
      public ushort TransactionId
      {
          get
          {
              m_nNextTranIndex = (m_nNextTranIndex + 1) % MAX_TRANSACTION_ID;
              return (ushort)m_nNextTranIndex;
          }
      }

      /// <summary>
      /// Check if it is connected to the network 
      /// HART-IP device.
      /// </summary>
      public bool IsConnected
      {
          get { return m_bConnected; }
      }

      /// <summary>
      /// Last activity time in 100-nanosecond units that sends
      /// or receive a message
      /// </summary>
      public long LastActivityTime
      {
          get { return m_lLastActivityTime; }
      }

      /// <summary>
      /// Connected host hostname or IP Address
      /// </summary>
      public String HostIPAddr
      {
          get { return m_strHost; }
      }

      /// <summary>
      /// Connected HART Port
      /// </summary>
      public int Port
      {
          get { return m_nPort; }
      }

      /// <summary>
      /// Established network connection type
      /// </summary>
      public uint ConnectionType
      {
          get { return m_nConnection; }
      }

      /// <summary>
      /// Socket timeout
      /// </summary>
      public int SocketTimeout
      {
          get { return m_nSocketTimeout; }       
      }

      /// <summary>
      /// Last Error message
      /// </summary>
      public String LastError
      {
          get { return m_Error; }
      }     

      /// <summary>
      /// init the member variables
      /// </summary>
      /// <param name="IPAddr">String</param>
      /// <param name="nPort">int</param>
      /// <param name="nSocketTimeout">int</param>
      /// <param name="InactivityCloseTime">uint</param>
      /// <returns>bool true if it is success</returns>
      private bool Init(String IPAddr, int nPort, int nSocketTimeout,
          uint InactivityCloseTime)
      {
          bool bSuccess = false;
          IPAddr = IPAddr.Trim();
          if ((IPAddr.Length == 0) || nPort <= 0)
              throw new ArgumentException("Error, Invalid argument in initial the network connection.");

          do
          {
            lock (SyncRoot)
            {
                m_Error = String.Empty;
                if (m_bConnected)
                {
                    m_Error = "Init Error: A HART-IP Port is already opened.";
                    LogMsg.Instance.Log("Error, Init: A HART-IP Port is already opened.", true);
                    break;
                }
          
                try
                {
                    String ResolvedIP;
                    // check whether IP address is in dotted-quad format
                    String Pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +
                        @"(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";

                    Regex Check = new Regex(Pattern);
                    if (Check.IsMatch(IPAddr, 0))
                        ResolvedIP = IPAddr;
                    else
                    {
                        // resolve the hostname to dotted-quad IP address format
                        IPHostEntry host = Dns.GetHostEntry(IPAddr);
                        ResolvedIP = host.AddressList[0].ToString();
                    }

                    // don't set the timeout too low
                    if (nSocketTimeout < MIN_SOCKET_TIMEOUT)
                        m_nSocketTimeout = MIN_SOCKET_TIMEOUT;
                    else
                        m_nSocketTimeout = nSocketTimeout;
                  
                    m_strHost = IPAddr;
                    m_nPort = nPort;
                    m_nSendPort = nPort;
                    m_HostIp = IPAddress.Parse(ResolvedIP);                   
                    m_InactivityCloseTime = InactivityCloseTime;
                    bSuccess = true;
                }
                catch (Exception ex)
                {
                    m_Error = String.Format("Init Exception: {0}", ex.Message);
                    LogMsg.Instance.Log("Error, " + m_Error, true);
                }
            }
        } while (false); /* ONCE */

        return bSuccess;              
    }    

    /// <summary>
    /// Stop the receive messages thread
    /// </summary>
    protected void StopReading()
    {
        lock (SyncRoot)
        {
            m_bStopped = true;
            if (m_RspMsgReader != null)
            {
                m_RspMsgReader.Join(WAIT_INTERVAL);
                m_RspMsgReader.Abort();
                m_RspMsgReader = null;
            }
        }
    }

    /// <summary>
    /// Initiate session with the network HART-IP device.
    /// </summary> 
    /// <returns>bool</returns>
    private bool InitSession()
    {
        bool bSuccess = false;
        HartIPResponse Response = null;

        do
        {              
            // create init session request
            HartIPRequest Request = HartIPRequest.InitiateSession(TransactionId, m_InactivityCloseTime);
            if (!SendHartIPRequest(Request))
            {               
                LogMsg.Instance.Log("Error, sending InitSession request failure.", true);
                Close();
                break;
            }

            try
            {
                Response = GetResponse();
            }
            catch (SocketException se)
            {
                m_Error = String.Format("InitSession SocketException: ErrorCode:{0}. {1}.",
                    se.ErrorCode, se.Message);               
                LogMsg.Instance.Log("Error, " + m_Error, true);
                Close();
                break;
            }
            catch (Exception ex)
            {
                m_Error = String.Format("InitSession Exception: {0}", ex.Message);
                LogMsg.Instance.Log("Error, " + m_Error, true);
                Close();
                break;
            }
           
            if (Response == null)
            {
                m_Error = "Initiate Session failed getting response.";
                LogMsg.Instance.Log("Error, " + m_Error, true);
                Close();
                break;
            }
            else if (0 != Response.ResponseCode)
            {
                    m_Error = "InitSession received an error response code: " +
                        Response.ResponseCode.ToString() + "  " +
                        GetSessionInitStatusCode(Response.ResponseCode);

                    LogMsg.Instance.Log("Error, " + m_Error, true);
                    LogMsg.Instance.Log(Response.ToString());
                    Close();
                    break;
                }
             else if (!Response.IsValidResponse)
            {
                m_Error = "InitSession received an invalid response Msg Type.";
                LogMsg.Instance.Log("Error, " + m_Error, true);
                LogMsg.Instance.Log(Response.ToString());
                Close();
                break;
            }                           
          
            LogMsg.Instance.Log(Response.ToString());
            bSuccess = (Response.Status == HARTIPMessage.RSP_SUCCESS || 
                Response.Status == HARTIPMessage.RSP_SET_TO_NEAREST_POSSIBLE_VALUE) ? true : false;

        } while (false); /* ONCE */

        return bSuccess;
    }

    /// <summary>
    /// Send a Hart-IP Request
    /// </summary>
    /// <param name="Request"><see cref="HartIPRequest"></see></param>
    /// <para>see the HartIPRequest.Command for HART specification references.</para>
    /// <returns>bool true if the request is sent success.</returns>
    private bool SendHartIPRequest(HartIPRequest Request)
    {
        bool bSuccess = false;
        lock (SyncRoot)
        {
            m_Error = String.Empty;
            try
            {
                bSuccess = SendRequest(Request);
                LogMsg.Instance.Log(Request.ToString());
            }
            catch (SocketException se)
            {
                m_Error = String.Format("SendHartIPRequest SocketException: ErrorCode:{0}. {1}",
                    se.ErrorCode, se.Message);
                LogMsg.Instance.Log("Error, " + m_Error, true);                
            }
            catch (Exception e)
            {
                m_Error = String.Format("SendHartIPRequest Exception: {0}", e.Message);
                LogMsg.Instance.Log("Error, " + m_Error, true);
            }
        }

        return bSuccess;
    }   

    /// <summary>
    /// Continue get Hart-IP Response from the connected stream.
    /// Remove the received request from the list and release the
    /// MsgResponse lock signal.
    /// </summary>    
    private void ReceiveMsg()
    {
        bool stoploop;
        lock(SyncRoot)
        {
            stoploop = m_bStopped;
        }

        while (!stoploop)
        {
            try
            {                
                HartIPResponse Rsp = GetResponse();
                if (Rsp == null)
                    continue;

                m_lLastActivityTime = DateTime.Now.Ticks;
                if (Rsp.MessageType == HARTIPMessage.HART_ACK)
                {
                    MsgRequest Req = DequeueRequest(Rsp.TransactionId);
                    if (Req != null)
                        Req.Response.SetResponse(Rsp);
					else
						// just in case can't find the request in the list
                        LogMsg.Instance.Log(Rsp.ToString());
                }
                else
                {
                    if ((Rsp.MessageType == HARTIPMessage.HART_PUBLISH_NOTIFY) && 
                        (PublishedCmdNotify != null))
                    {
                        HartIPResponseArg Arg = new HartIPResponseArg(Rsp);
                        PublishedCmdNotify(this, Arg);
                    }
                    else
                        LogMsg.Instance.Log(Rsp.ToString());
                }
            }
            catch (SocketException se)
            {
                int nErrorCode = se.ErrorCode;
                // socket receive timeout
                if ((nErrorCode == 10060) || (nErrorCode == 10035))
                {
                    if (m_nConnection != UDP)                    
                        break;
                }
                // socket closed, server close the connection or interrupted
                else if ((nErrorCode == 995) || (nErrorCode == 10004) || (nErrorCode == 10054))
                    break;                    
                else
                {
                    LogMsg.Instance.Log("Error, ReceiveMsg SocketException: ErrorCode:" +
                        nErrorCode + ". " + se.Message, true);                   
                    break;
                }
            }
            catch (EndOfStreamException e)
            {
                LogMsg.Instance.Log("Error, ReceiveMsg EndOfStreamException: " + e.Message, true);
                break;
            }
            catch (IOException io)
            {
                if (m_nConnection != TCP)
                {
                    LogMsg.Instance.Log("Error, ReceiveMsg IOException: " + io.Message, true);
                    break;
                }
            }
            catch (ObjectDisposedException obex)
            {
                LogMsg.Instance.Log("Error, ReceiveMsg Expection :" + obex.Message, true);
                break;
            }
            catch (ThreadStateException tsEx)
            {
                // caller failed to release the lock signal and it will timeout 
                LogMsg.Instance.Log("Error, ReceiveMsg ThreadStateException: " + tsEx.Message, true);                               
            }
        }

        ClearRequests();
    }

    /// <summary>
    /// Dequeue a request msg from the requests list  
    /// </summary>
    /// <param name="id">uint</param>
    /// <returns><see cref="MsgRequest"/></returns>
    private MsgRequest DequeueRequest(ushort id)
    {
        lock (SyncRoot)
        {
            MsgRequest[] reqs = GetRequests();
            if (reqs != null)
            {
                for (int i = 0; i < reqs.Length; i++)
                {
                    if (reqs[i].ReqMsgId == id)
                    {
                        m_Requests.Remove(reqs[i]);
                        return reqs[i];
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Get the queued requests.  
    /// </summary>
    /// <returns><see cref="MsgRequest"/> object array</returns>
    private MsgRequest[] GetRequests()
    {
        MsgRequest[] reqs = null;
        if (m_Requests.Count > 0)
        {
            // make a copy of all the request objects first
            reqs = new MsgRequest[m_Requests.Count];
            m_Requests.CopyTo(reqs, 0);
        }
        return reqs;
    }

    /// <summary>
    /// Clear the queued requests and release the requests' 
    /// MsgResponse lock signal.     
    /// </summary>
    private void ClearRequests()
    {
        lock (SyncRoot)
        {
            MsgRequest[] reqs = GetRequests();
            if (reqs != null)
            {
                for (int i = 0; i < reqs.Length; i++)
                {
                    MsgRequest req = DequeueRequest(reqs[i].ReqMsgId);
                    if (req != null)
                    {
                        try
                        {
                            req.Response.SetResponse();
                        }
                        catch (ThreadStateException)
                        {
                            ; // do nothing
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Create an UDP or TCP connection, and connect to specified network 
    /// HART-IP device's Hostname or IP Address.
    /// </summary>
    /// <param name="IPAddr">string network HART-IP device hostname or IP Address</param>
    /// <param name="nPort">int HART port to connect</param>     
    /// <returns>bool true if connects to the specified hostname or IP Address.</returns>
    /// <remarks>
    /// <para>Socket timeout 60000 milliseconds will be used.</para>
    /// <para>Inactivity Close Time 600000 milliseconds will be used for 
    /// connected session inactivity timeout.
    /// </para>
    /// <para>It will initiate session with the network HART-IP device if it is
    /// successful connected to the device.</para>
    /// </remarks>
    public bool Connect(string IPAddr, int nPort)
    {
        return Connect(IPAddr, nPort, SOCKET_TIMEOUT_DEFAULT,
            HARTIPMessage.INACTIVITY_CLOSE_TIME);       
    }  
    
    /// <summary>
    /// Create an UDP or TCP connection, and connect to specified network
    /// HART-IP device's Hostname or IP Address.
    /// </summary>
    /// <param name="IPAddr">String network HART-IP device's hostname or IP Address</param>
    /// <param name="nPort">int HART port to connect.    
    /// <see cref="TCP"></see>, or <see cref="SECUREDTCP"></see> network connection type</param>
    /// <param name="nSocketTimeout">int Socket timeout in milliseconds. 
    /// If value is less than 30000 milliseconds, 30000 milliseconds will be used.</param>
    /// <returns>bool true if connects to the specified hostname or IP Address.</returns>
    /// <remarks>
    /// <para>Inactivity Close Time 600000 milliseconds will be used for 
    /// connected session inactivity timeout.
    /// </para>
    /// <para>It will initiate session with the network HART-IP device if it is
    /// successful connected to the device.</para>
    /// </remarks>
    public bool Connect(String IPAddr, int nPort, int nSocketTimeout)
    {
        return Connect(IPAddr, nPort, nSocketTimeout,
            HARTIPMessage.INACTIVITY_CLOSE_TIME);       
    }

    /// <summary>
    /// Create an UDP or TCP socket and connect to specified network HART-IP device's 
    /// Hostname or IP Address.
    /// </summary>
    /// <param name="IPAddr">String network HART-IP device's hostname or IP Address</param>
    /// <param name="nPort">int HART port to connect</param>     
    /// <param name="nSocketTimeout">int Socket timeout in milliseconds. 
    /// If value is less than 30000 milliseconds, 30000 milliseconds will be used.</param>
    /// <param name="InactivityCloseTime">uint session inactivity close time</param>   
    /// <returns>bool true if connects to the specified hostname or IP Address.</returns>
    /// <remarks>It will initiate session with the network HART-IP device if it is
    /// successful connected to the device.</remarks>
    public bool Connect(String IPAddr, int nPort, int nSocketTimeout,
                        uint InactivityCloseTime)
    {
        lock (SyncRoot)
        {
            if (Init(IPAddr, nPort, nSocketTimeout, InactivityCloseTime))
            {
                if (Connect())
                {
                    // init session
                    m_bConnected = InitSession();
                }
            }

            return m_bConnected;
        }
    }

    /// <summary>
    /// Send a Hart-IP Request  
    /// </summary>
    /// <param name="Command">byte[] request Command byte array
    /// <para>Array should have frame, device address, command, byte count, 
    /// data, and checksum bytes.</para>
    /// <para>device address is the device type and device id 5 bytes with expanded type mask</para>
    /// <para>Example: Send command 20 to a device that has device address 2658 3B86A3, 
    /// Command byte[] should have: 82 A6 58 3B 86 A3 14 00 76</para>   
    /// <remarks>See HART specification 081r8.2.pdf section 5.1, 5.2, and 5.3 for frame, 
    /// address, expansion, data, and checksum bytes information.
    /// </remarks>  
    /// </param>
    /// <param name="usByteCount">ushort the specified Command array byte count</param> 
    /// <param name="MsgRsp"><see cref="MsgResponse"></see></param>
    /// <returns>bool if it is success</returns>    
    public bool SendHartRequest(byte[] Command, ushort usByteCount, MsgResponse MsgRsp)
    {
        HartIPRequest Req = HartIPRequest.HartCommandRequest(TransactionId, Command, usByteCount);
        return SendHartRequest(Req, MsgRsp);
    }        
    
    /// <summary>
    /// Send a Hart-IP Request
    /// </summary>
    /// <param name="Request"><see cref="HartIPRequest"></see></param>
    /// <param name="MsgRsp"><see cref="MsgResponse"></see></param>
    /// <para>see the HartIPRequest.Command for HART specification references.</para>
    /// <returns>bool if it is success</returns>
    public bool SendHartRequest(HartIPRequest Request, MsgResponse MsgRsp)
    {
        bool bSuccess = false;

        if (Request == null)
            throw new ArgumentException("Invalid argument in SendHartRequest.");        

        lock (SyncRoot)
        {
            m_Error = String.Empty;
            try
            {
                if (m_RspMsgReader == null)
                {
                    // Create a thread that is constantly reading on the input stream
                    m_bStopped = false;
                    m_RspMsgReader = new Thread(new ThreadStart(this.ReceiveMsg));
                    m_RspMsgReader.Name = "HartIPConnection";
                    m_RspMsgReader.Start();
                }                         
           
                // add the request object into the m_Requests list                
                MsgRequest MsgReq = new MsgRequest(Request.TransactionId, MsgRsp);
                m_Requests.Add(MsgReq);

                // send the request
                bSuccess = SendRequest(Request);
                LogMsg.Instance.Log(Request.ToString());
            }
            catch (SocketException se)
            {               
                m_Error = String.Format("SendHartRequest SocketException: ErrorCode:{0}. {1}",
                    se.ErrorCode, se.Message);               
                LogMsg.Instance.Log("Error, " + m_Error, true);               
            }
            catch (Exception e)
            {               
                m_Error = String.Format("SendHartRequest Exception: {0}", e.Message);                
                LogMsg.Instance.Log("Error, " + m_Error, true);
            }

            if (!bSuccess)
            {              
                LogMsg.Instance.Log("Error, Failed sending Request: " + Request.ToString(), true);
                DequeueRequest(Request.TransactionId);
            }
        }

        return bSuccess;
    }   
    
    /// <summary>
    /// Build a Hart-IP request
    /// </summary>
    /// <param name="Command">byte[] request Command byte array
    /// <para>Array should include the frame, device address, command, byte count, 
    /// data, and checksum bytes.</para>para</param>
    /// <param name="usByteCount">ushort the specified Command array byte count</param>
    /// <returns><see cref="HartIPRequest"/></returns>   
    public HartIPRequest BuildHartRequest(byte[] Command, ushort usByteCount)
    {
        return HartIPRequest.HartCommandRequest(TransactionId, Command, usByteCount);
    }

    /// <summary>
    /// Reconncet the network HART-IP device and initial session.
    /// </summary>   
    /// <returns>bool true if it is success</returns>
    public bool Reconnect()
    {
        if (m_strHost.Length == 0)
            throw new ArgumentException("Reconnect Error: Host IP is empty.");

        if (m_nPort == 0)
            throw new ArgumentException("Reconnect Error: Uninitialize Port value.");

        if (m_bConnected)
            throw new ArgumentException("Reconnect Error: Connection is opened.");

        return Connect(m_strHost, m_nPort, m_nSocketTimeout,
            m_InactivityCloseTime);
    }


        /// <summary>
        /// Create a HART Close Session Request, send it to the Gateway,
        /// and close the socket.
        /// </summary>
        /// <param name="Result"><see cref="HARTMsgResult">HARTMsgResult</see></param>
        public void CloseSession(HARTMsgResult Result)
        {
            HartIPRequest Req = null;

            do
            {
                Req = HartIPRequest.CloseSession(TransactionId);
                MsgResponse MsgRsp = new MsgResponse(HARTIPConnect.USE_SOCKET_TIMEOUT_DEFAULT);
                if (SendHartRequest(Req, MsgRsp))
                {
                    try
                    {
                        if (!MsgRsp.ResponseMsg.IsValidResponse)
                        {
                            Result.AddMessage("Close Session failed. Receive an invalid response msg type.", false, true);
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    Result.AddMessage("Failed sending Session Close Command to Gateway.", false, true);
                    break;
                }
            } while (false); /* ONCE */

            Close();
            Result.AddMessage("Closed the HPort socket.", false, true);
        }

        /// <summary>
        /// Send KeepAlive message to the Gateway
        /// </summary>
        /// <param name="Result">HARTMsgResult</param>
        public void SendKeepAlive(HARTMsgResult Result)
        {
            HartIPRequest Req = null;

            do
            {
                Req = HartIPRequest.KeepAlive(TransactionId);
                MsgResponse MsgRsp = new MsgResponse(HARTIPConnect.USE_SOCKET_TIMEOUT_DEFAULT);
                if (SendHartRequest(Req, MsgRsp))
                {
                    if (!MsgRsp.ResponseMsg.IsValidResponse)
                    {
                        Result.AddMessage("Send KeepAlive failed. Receive an invalid response msg type.", false, true);
                        break;
                    }
                }
                else
                {
                    Result.AddMessage("Failed sending KeepAlive request to Gateway.", false, true);
                    break;
                }
             } while (false); /* ONCE */
        }

        private string GetSessionInitStatusCode(int ResponseCode)
        {
            string s = "Undefined";
            if (m_SessionInitStatusCodes.Count == 0)
            {
                m_SessionInitStatusCodes.Add(0,   "Success No error occurred");
                m_SessionInitStatusCodes.Add(2,   "Invalid Selection(Invalid Master Type)");
                m_SessionInitStatusCodes.Add(5,   "Too Few Data Bytes Received");
                m_SessionInitStatusCodes.Add(6,   "Device Specific Command Error");
                m_SessionInitStatusCodes.Add(8,   "Set to Nearest Possible Value(Inactivity timer value)");
                m_SessionInitStatusCodes.Add(14,  "Version not supported");
                m_SessionInitStatusCodes.Add(15,  "All available sessions in use");
                m_SessionInitStatusCodes.Add(16,  "Session already established");
            }

            if (m_SessionInitStatusCodes.ContainsKey(ResponseCode))
            {
                s = m_SessionInitStatusCodes[ResponseCode];
            }
            return s;
         }

    }
}
