using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace FieldCommGroup.HartIPConnect
{
  /// <summary>
  /// HART IP Message class
  /// </summary>
  public static class HARTIPMessage
  {
    /// <summary>
    /// Maximum HART message length
    /// </summary>
    public const int MAX_HARTMSG_LENGTH = 512;

    /// <summary>
    /// Maximum HART request length
    /// </summary>
    public const int MAX_REQUEST_MSG_LEN = 255;

    /// <summary>
    ///  Maximum HART signle byte command
    /// </summary>
    public const int MAX_SINGLE_BYTE_CMD = 253;

    /// <summary>
    /// Minimum HART response length
    /// </summary>
    public const int MIN_RESPONSE_LENGTH = 10;

    /// <summary>
    /// Maximum short Tag length
    /// </summary>
    public const int MAX_SHORT_TAG_LENGTH = 8;

    /// <summary>
    /// Maximum long Tag length
    /// </summary>
    public const int MAX_LONG_TAG_LENGTH = 32;

        /// <summary>
        /// Session Inactivity Close Time, milliseconds
        /// </summary>
        public const uint INACTIVITY_CLOSE_TIME = 600000;

    /// <summary>
    /// delay Retry
    /// </summary>
    public const uint DR_DELAYRETRY = 1000;

    /// <summary>
    /// DrDelayBase Max
    /// </summary>
    public const uint DrDelayBaseMax = 30000;

    /// <summary>
    /// DrDelayBase Min
    /// </summary>
    public const uint DrDelayBaseMin = 500;
    /// <summary>
    /// Number of Delay retries
    /// </summary>
    public const uint DR_RETRIES = 10;

    /// <summary>
    /// HART command Response Code
    /// </summary>
    public const byte RSP_SUCCESS = 0;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_CMD_ERROR = 1;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_INVALID_SELECTION = 2;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    /// 
    public const byte RSP_PARAMETER_TOO_LARGE = 3;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_PARAMETER_TOO_SMALL = 4;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_TOO_FEW_DATA_BYTES = 5;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_XMSPECIFIC = 6;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_IN_WRITE_PROTECT_MODE = 7;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_SET_TO_NEAREST_POSSIBLE_VALUE = 8;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_ADDRESSED_DEVICE_DISCONNECTED = 9;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_INCORRECT_FORMAT = 12;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_ALL_SESSIONS_IN_USE = 15;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_ACCESS_RESTRICTED = 16;
    /// <summary>
    ///  HART command Response Code
    /// </summary>
    public const byte RSP_CMD_WARNING = 24;
    
    /// <summary>
    /// HART response device busy
    /// </summary>
    public const byte RSP_DEVICE_BUSY = 32;
    
    /// <summary>
    /// HART response delayed response initiated
    /// </summary>
    public const byte RSP_DR_INITIATE = 33;

    /// <summary>
    /// HART response delayed response initiated or running 
    /// </summary>
    public const byte RSP_DR_RUNNING = 34;

    /// <summary>
    /// HART response delayed response failed 
    /// </summary>
    public const byte RSP_DR_DEAD = 35;

        /// <summary>
        /// Command not implemented
        /// </summary>
        public const byte RSP_CMD_NOT_IMPLEMENTED = 64;

        /// <summary>
        /// communication error - 0x80
        /// </summary>
        public const byte RSP_COMM_ERROR = 128;
        /// <summary>
        /// no response
        /// </summary>
        public const byte RSP_NO_RESPONSE = 255;

    /// <summary>
    /// HART Command 0
    /// </summary>
    public const int CMD_READ_UID = 0;
    /// <summary>
    /// HART Command 13
    /// </summary>
    public const int CMD_READ_TAG_DESC_DATE = 13;
    /// <summary>
    /// HART Command 20
    /// </summary>
    public const int CMD_READ_LONG_TAG = 20;
    /// <summary>
    /// HART Command 31
    /// </summary>
    public const int CMD_EXTENDED_CMD = 31;
    /// <summary>
    /// HART Command 74
    /// </summary>
    public const int CMD_READ_IO_SYSTEM_CAPABILITIES = 74;
    /// <summary>
    /// HART Command 77
    /// </summary>
    public const int CMD_SEND_CMD_TO_SUB_DEVICE = 77;
    /// <summary>
    /// HART Command 84
    /// </summary>
    public const int CMD_READ_SUB_DEVICE_IDENTITY = 84;
    /// <summary>
    /// HART Command 106
    /// </summary>
    public const int CMD_FLUSH_DELAYED_RESPONSES = 106;
    
    /// <summary>
    /// HART-5 command 128
    /// </summary>
    public const int CMD_READ_HARTPORT_PARAMETERS = 128;
    /// <summary>
    /// HART-5 command 170
    /// </summary>
    public const int CMD_READ_DEVICE_LIST = 170;

    /// <summary>
    /// HART UDP/TCP message idenfifier Ids
    /// </summary>
    internal const byte HART_STX = 0;
    internal const byte HART_ACK = 1;
    internal const byte HART_PUBLISH_NOTIFY = 2;
    internal const byte HART_NAK = 15;

    /// <summary>
    /// HART UDP/TCP message header size
    /// </summary>
    internal const int HART_MSG_HEADER_SIZE = 8;

    /// <summary>
    ///  HART UDP/TCP version
    /// </summary>
    internal const byte HART_UDP_TCP_MSG_VERSION = 1;

    /// <summary>
    ///  HART UDP/TCP session master type
    /// </summary>
    internal const byte HART_SESSION_MASTER_TYPE = 1;    
  }      

  /// <summary>
  /// TCP/IP HART MessageId enum
  /// </summary>
  internal enum MessageId
  {
      SESSION_INITIATE = 0,
      SESSION_CLOSE = SESSION_INITIATE + 1,
      KEEP_ALIVE = SESSION_CLOSE + 1,
      HART_WIRED_PDU = KEEP_ALIVE + 1,
      HART_WIRELESS_DIRECT_PDU = HART_WIRED_PDU + 1,
      DISCOVERY = 128
  }

  /// <summary>
  /// HartIPMessageHeader class
  /// </summary>
  internal class HartIPMessageHeader
  {
    byte m_MessageType;
    MessageId m_MessageId;
    byte m_Status;

    /// <summary>
    /// HART UDP/TCP protocol Message header.
    /// </summary>
    /// <param name="cMsgType">byte message type</param>
    /// <param name="cMsgId">MessageId message id</param>
    /// <param name="cStatus">byte status</param>
    internal HartIPMessageHeader(byte cMsgType, MessageId cMsgId, byte cStatus)
    {
      this.m_MessageId = cMsgId;
      this.m_MessageType = cMsgType;
      this.m_Status = cStatus;
    }

    internal byte MessageType
    {
      get { return m_MessageType; }
    }

    internal byte Status
    {
      get { return m_Status; }
    }

    internal MessageId MsgId
    {
      get { return m_MessageId; }
    }

    internal ushort ByteCount
    {
        get { return HARTIPMessage.HART_MSG_HEADER_SIZE; }
    }

    internal bool IsStatusSuccess
    {
        get { return (m_Status == HARTIPMessage.RSP_SUCCESS); }
    }

    /// <summary>
    /// override the ToString method
    /// </summary>
    /// <returns>String that includes version, msg type, msg id, and status.</returns>
    public override String ToString()
    {
      return String.Format("Message Header: Ver: {0}, MsgType: {1}, MsgId: {2}, MsgStatus: 0x{3:X2}",
            new object[] { HARTIPMessage.HART_UDP_TCP_MSG_VERSION, m_MessageType, 
                (int)m_MessageId, m_Status });
    }
  }
  
  /// <summary>
  /// HartCommon class contains HART request and
  /// response common properties and methods
  /// </summary>
  public class HartIPCommon
  {
    /// <summary>
    /// Hart command size
    /// </summary>
    protected ushort m_HartCmdSize = 0;
    /// <summary>
    /// Transaction ID
    /// </summary>
    protected ushort m_TransactionId; 
    /// <summary>
    /// Timestamp
    /// </summary>
    protected String m_TimeStamp; 
    /// <summary>
    /// Timeout in millisecs, use this value to override the default
    /// </summary>
    public int m_Timeout = HARTIPConnect.USE_SOCKET_TIMEOUT_DEFAULT;  
    
    String m_Perfix;
    internal byte[] m_HartCmd = new byte[HARTIPMessage.MAX_HARTMSG_LENGTH];
    HartIPMessageHeader m_MsgHeader = null;

    internal HartIPCommon(String Perfix)
        : this(0, Perfix)
    {
    }

    /// <summary>
    /// Common values for a HART request and response message
    /// </summary>
    /// <param name="TransId">ushort Transaction ID</param>
    ///  <param name="Perfix">String prefix string</param>
    internal HartIPCommon(ushort TransId, String Perfix)
    {      
      this.m_TransactionId = TransId;
      this.m_Perfix = Perfix;
    }

    internal ushort HARTCmdSize
    {
        get { return m_HartCmdSize; }
        set { m_HartCmdSize = value; }
    }

    internal String PerfixStr
    {
        get { return m_Perfix; }
    }

    internal byte MessageType
    {
        get { return ((m_MsgHeader != null) ? m_MsgHeader.MessageType : HARTIPMessage.HART_ACK); }
    }

    /// <summary>
    /// HART UDP/TCP message header object,
    /// </summary>
    internal HartIPMessageHeader MsgHeader
    {
        get { return m_MsgHeader; }
        set { m_MsgHeader = value; }
    }

    /// <summary>
    /// Transaction Id
    /// </summary>
    public ushort TransactionId
    {
        get { return m_TransactionId; }
        protected set { m_TransactionId = value; }
    }   

    /// <summary>
    /// Byte count is included the HART UDP/TCP message header 
    /// and HART command body byte count.
    /// </summary>
    public ushort ByteCount
    {
        get { return (ushort)((m_MsgHeader != null) ? m_MsgHeader.ByteCount
            + m_HartCmdSize : m_HartCmdSize); }
    }   
   
    /// <summary>
    /// HART request or response command bytes array
    /// </summary>
    public byte[] HARTCommand
    {
      get { return m_HartCmd; }
      set { value.CopyTo(m_HartCmd, 0); }
    }
   
    /// <summary>
    /// HART request or response TimeStamp
    /// </summary>
    public String TimeStamp
    {
        get { return m_TimeStamp; }
        set { m_TimeStamp = value; }
    }

    /// <summary>
    /// Request or response message bytes. Array bytes include the HART UDP/TCP 
    /// protocol message header and HART Command body.
    /// <Note>Transaction Id and Byte Count are in host to network order.</Note>
    /// </summary>
    public byte[] Message
    {
      get
      {
        ushort uByteCount = this.ByteCount;
        byte[] data = new byte[uByteCount];
        int Index = 0;

        if (m_MsgHeader != null)
        {
          // get the message header values first
          data[Index++] = (byte)HARTIPMessage.HART_UDP_TCP_MSG_VERSION;
          data[Index++] = m_MsgHeader.MessageType;
          data[Index++] = (byte)m_MsgHeader.MsgId;
          data[Index++] = m_MsgHeader.Status;
          // swap the m_TransactionId bytes to 'host to network order'
          data[Index++] = (byte)((m_TransactionId >> 8) & 0x0ff);
          data[Index++] = (byte)(m_TransactionId & 0x0ff);
          // swap the uByteCount btyes to 'host to network order'
          data[Index++] = (byte)((uByteCount >> 8) & 0x0ff);
          data[Index++] = (byte)(uByteCount & 0x0ff);
        }

        if (m_HartCmdSize > 0)
        {
          int j = 0;
          int i;

          // get the parameters bytes
          for (i = Index; i < ByteCount; i++)
          {
            data[i] = m_HartCmd[j++];
          }          
        }
        return data;
      }
    }   

    /// <summary>
    /// override the ToString method
    /// </summary>
    /// <returns>String that includes timestamp, message header, and message body in hex string.</returns>
    public override String ToString()
    {
      String strHeader;
      if ((m_TimeStamp != null) && (m_TimeStamp.Length > 0))
      {
        String[] msgs = { m_TimeStamp, m_Perfix, ((MsgHeader != null) ? MsgHeader.ToString() : "")};
          strHeader = String.Format("{0}, {1}: {2}", msgs);
      }
      else
        strHeader = String.Format("{0}: {1}", m_Perfix, ((MsgHeader != null) ? MsgHeader.ToString() : ""));

      String strBody;

      if (m_HartCmdSize > 0)
        strBody = String.Format(", TranId: {0}, ByteCount: {1}, Data: ", m_TransactionId, m_HartCmdSize);
      else
        strBody = String.Format(", TranId: {0}, ByteCount: {1}", m_TransactionId, m_HartCmdSize);

      for (int i = 0; i < m_HartCmdSize; i++)
      {
        strBody += (String.Format("{0:X2} ", m_HartCmd[i]));        
      }
      return (strHeader + strBody);
    }
  }

  /// <summary>
  /// HartIPRequest class contains Hart request properties
  /// and methods
  /// </summary>
  public class HartIPRequest : HartIPCommon
  {
    /// <summary>
    /// HART Request
    /// </summary>
    /// <param name="MsgId">MessageId message id</param>
    /// <param name="TransactionId">ushort transaction id</param>
      internal HartIPRequest(MessageId MsgId, ushort TransactionId)
      : base(TransactionId, "Tx")
    {
      MsgHeader = new HartIPMessageHeader(HARTIPMessage.HART_STX, MsgId, 0);           
    } 

    /// <summary>
    /// Initiate a HART Session request
    /// </summary>
    /// <param name="usTranId">ushort Transaction Id</param>
    /// <param name="InactivityCloseTime">uint Session inactivity close time</param>   
    /// <returns><see cref="HartIPRequest"> initiate session request</see></returns>
      public static HartIPRequest InitiateSession(ushort usTranId, uint InactivityCloseTime)
    {
      // create message header for this request
      HartIPRequest request = new HartIPRequest(MessageId.SESSION_INITIATE, usTranId);

      byte[] Params = new byte[5];
      Params[0] = HARTIPMessage.HART_SESSION_MASTER_TYPE;
      // swap the InactivityCloseTime bytes to 'host to network order'
      Params[1] = (byte)((InactivityCloseTime >> 24) & 0x0ff);
      Params[2] = (byte)((InactivityCloseTime >> 16) & 0x0ff);
      Params[3] = (byte)((InactivityCloseTime >> 8) & 0x0ff);
      Params[4] = (byte)(InactivityCloseTime & 0x0ff);

      // set it into request member variable
      request.HARTCommand = Params;
      // set the request's byte count
      request.m_HartCmdSize = 5;

      return request;
    }

    /// <summary>
    /// Close Session Request
    /// </summary>
    /// <param name="usTranId">ushort Transaction Id</param>
    /// <returns><see cref="HartIPRequest">HartIPRequest close session request</see></returns>
    public static HartIPRequest CloseSession(ushort usTranId)
    {
      HartIPRequest HRequest = new HartIPRequest(MessageId.SESSION_CLOSE, usTranId);     
      return HRequest;
    }

    /// <summary>
    /// Keep Alive Request
    /// </summary>
    /// <param name="usTranId">ushort Transaction Id</param>
    /// <returns><see cref="HartIPRequest"> keep alive request</see></returns>
    public static HartIPRequest KeepAlive(ushort usTranId)
    {
      HartIPRequest Request = new HartIPRequest(MessageId.KEEP_ALIVE, usTranId);     
      return Request;
    }

    /// <summary>
    /// Create a HART Command Request
    /// </summary>
    /// <param name="usTranId">ushort Transaction Id</param>
    /// <param name="Command">byte[] Command byte array    
    /// <para>Array should have frame, device address, command, byte count, 
    /// data, and checksum bytes.</para>
    /// <para>device address is the device type and device id 5 bytes with expanded type mask</para>
    /// <remarks>See HART specification 081r8.2.pdf section 5.1, 5.2, and 5.3 for frame, 
    /// address, expansion, data, and checksum bytes information.
    /// </remarks>
    /// </param>
    /// <param name="usByteCount">ushort the specified Command array byte count</param>   
    /// <returns><see cref="HartIPRequest"> request</see></returns>
    public static HartIPRequest HartCommandRequest(ushort usTranId, byte[] Command, 
                                                 ushort usByteCount, int Timeout = HARTIPConnect.USE_SOCKET_TIMEOUT_DEFAULT)
    {
      if(usByteCount > HARTIPMessage.MAX_REQUEST_MSG_LEN)
        throw new ArgumentException(String.Format("HartCommandRequest Error: Invalid cmd length: {0}.",
            usByteCount));

      HartIPRequest Request = new HartIPRequest(MessageId.HART_WIRED_PDU, usTranId);

      Request.m_Timeout = Timeout;

      // if command has data
      if (usByteCount > 0)
      {
        Request.m_HartCmdSize = usByteCount;
        Request.HARTCommand = Command;               
      }

      return Request;
    }
  }

  /// <summary>
  /// HartIPResponse class contains Hart response properties
  /// and methods
  /// </summary>
  public class HartIPResponse : HartIPCommon
  {
    byte m_cCmd;
    byte m_cRspcode = HARTIPMessage.RSP_SUCCESS;
    byte m_cDataCount = 0;
    byte[] m_Data = null;
	byte m_cDeviceStatus = 0;

    /// <summary>
    /// HartIPResponse
    /// </summary>
    /// <param name="inBuffer">BinaryReader</param>    
    /// <param name="TimeStamp">String response timestamp</param>
    internal HartIPResponse(BinaryReader inBuffer, String TimeStamp)
        : base("Rx")
    {      
        byte Version, MsgType, MsgId, Status;
        ushort usMsgByteCount;
        
        Version = inBuffer.ReadByte();
        if (Version != HARTIPMessage.HART_UDP_TCP_MSG_VERSION)
            throw new ArgumentException(String.Format("HartIPResponse Error: Unsupported Response Message Version: {0}.",
                Version));

        // If the input timestamp is null or empty, get the current time as timestamp
        if ((TimeStamp != null) && (TimeStamp.Length > 0))
            m_TimeStamp = TimeStamp;
        else
            m_TimeStamp = HartUtil.GetTimeStamp();
        
        MsgType = inBuffer.ReadByte();
        MsgId = inBuffer.ReadByte();
        Status = inBuffer.ReadByte();     

        // Create message header
        MsgHeader = new HartIPMessageHeader(MsgType, (MessageId)MsgId, Status);
        // transaction id 
        m_TransactionId = (ushort)IPAddress.NetworkToHostOrder((short)inBuffer.ReadUInt16());
        // byte count
        usMsgByteCount = (ushort)IPAddress.NetworkToHostOrder((short)inBuffer.ReadUInt16());
        // message body byte count
        m_HartCmdSize = (ushort)(usMsgByteCount - HARTIPMessage.HART_MSG_HEADER_SIZE);

        if (m_HartCmdSize > HARTIPMessage.MAX_HARTMSG_LENGTH)
            m_HartCmdSize = HARTIPMessage.MAX_HARTMSG_LENGTH;

        // get data bytes if message has data
        if (m_HartCmdSize > 0)
        {
            ushort usExtra;
            int nIndex;
            int j, i;

            // get the message bytes
            for (i = MsgHeader.ByteCount, j= 0; i < usMsgByteCount; i++, j++)
            {
                m_HartCmd[j] = inBuffer.ReadByte(); 
            }
            
            // if the message is the ACK or BACK (notify) type
            if (MsgType == HARTIPMessage.HART_ACK || MsgType == HARTIPMessage.HART_PUBLISH_NOTIFY)
            {
                // Check if response is initiate session
                if (MsgHeader.MsgId == MessageId.SESSION_INITIATE)
                {
                    nIndex = 0;
                    m_cDataCount = (byte)m_HartCmdSize;
                    m_cRspcode = MsgHeader.Status;
                }
                // Check if it is a long frame
                else if ((m_HartCmd[0] & 0x80) == 0x80)
                {
                    // get the extra         
                    usExtra = (ushort)((m_HartCmd[0] & 0x60) >> 5);
                    nIndex = 6 + usExtra;
                    m_cCmd = m_HartCmd[nIndex++];
                    m_cDataCount = m_HartCmd[nIndex++];
                    // subtract response code and device status bytes
                    m_cDataCount -= 2;
                    // get respsone code and device status bytes    
                    m_cRspcode = m_HartCmd[nIndex++];
                    m_cDeviceStatus = m_HartCmd[nIndex++];
                }
                else
                {
                    // it is a short frame
                    nIndex = 2;
                    m_cCmd = m_HartCmd[nIndex++];
                    m_cDataCount = m_HartCmd[nIndex++];
                    m_cRspcode = m_HartCmd[nIndex++];
                    m_cDeviceStatus = m_HartCmd[nIndex++];
                    // subtract response code and device status bytes
                    m_cDataCount -= 2;
                }

                // Get the rest of response data bytes and store them in the data array
                m_Data = new byte[m_cDataCount];
                for (i = nIndex, j= 0; j < m_cDataCount; i++, j++)
                {
                    m_Data[j] = m_HartCmd[i];
                }
            }
        }
        else
        {
            m_cRspcode = MsgHeader.Status;
        }
    }

    /// <summary>
    /// Convert the command 77 to a standard HART PDU *in place*
    /// </summary>
    public void Unwrap77()
    {
        ushort usExtra;
        int nIndex;
        int j, i;

        // the response data of the cmd77 becomes the HART PDU of the embedded command
        Array.Copy(m_Data, 2, m_HartCmd, 0, m_cDataCount - 2);

        if ((m_HartCmd[0] & 0x80) == 0x80)
        {
            // get the extra         
            usExtra = (ushort)((m_HartCmd[0] & 0x60) >> 5);
            nIndex = 6 + usExtra;
            m_cCmd = m_HartCmd[nIndex++];
            m_cDataCount = m_HartCmd[nIndex++];
            // subtract response code and device status bytes
            m_cDataCount -= 2;
            // get response code and device status bytes    
            m_cRspcode = m_HartCmd[nIndex++];
            m_cDeviceStatus = m_HartCmd[nIndex++];
        }
        else
        {
            // it is a short frame
            nIndex = 2;
            m_cCmd = m_HartCmd[nIndex++];
            m_cDataCount = m_HartCmd[nIndex++];
            m_cRspcode = m_HartCmd[nIndex++];
            m_cDeviceStatus = m_HartCmd[nIndex++];
            // subtract response code and device status bytes
            m_cDataCount -= 2;
        }

        // Get the rest of response data bytes and store them in the data array
        m_Data = new byte[m_cDataCount];
        for (i = nIndex, j = 0; j < m_cDataCount; i++, j++)
        {
            m_Data[j] = m_HartCmd[i];
        }

    }


        /// <summary>
        /// Check if the Response code is is an error code
        /// </summary>    
        public bool IsErrorResponse()
        {
            int c = this.m_cRspcode;

            bool iserr = (
                (1 <= c && c<= 7) ||
                (16 <= c && c <= 23) ||
                (32 <= c && c <= 64) || 
                (9 <= c && c <= 13) || 
                (15 == c) ||
                (28 == c) || 
                (29 == c) ||
                (65 <= c && c <= 95)
                );

            return iserr;
    }

    /// <summary>
    /// The Message Status
    /// </summary>
    public byte Status
    {
      get { return ((MsgHeader != null) ? MsgHeader.Status : (byte)0); }
    }
    /// <summary>
    /// Check if the Response Message is a valid response.
    /// </summary>    
    public bool IsValidResponse
    {
        get
        {
          byte MsgType = (MsgHeader != null) ? MsgHeader.MessageType : HARTIPMessage.HART_ACK;
            return (MsgType == HARTIPMessage.HART_ACK);
        }    
    }

    /// <summary>
    /// Check if the Response status is success
    /// </summary>    
    public bool IsStatusSuccess
    {
        get { return ((MsgHeader != null) ? MsgHeader.IsStatusSuccess : true); }
    }

    /// <summary>
    /// Response Code
    /// </summary>
    public byte ResponseCode
    {
      get { return m_cRspcode; }
    }

   /// <summary>
   /// Device status
   /// </summary>
	public byte DeviceStatus
	{
	  get { return m_cDeviceStatus; }
	}
    
    /// <summary>
    /// Data count is the subtracted response code,
    /// device status, and checksum byte count value.
    /// </summary>
    public byte DataCount
    {
      get { return m_cDataCount; } 
    }
    
    /// <summary>
    /// HART Response Comand byte
    /// </summary>
    public byte Command
    {
      get { return m_cCmd; }
    }

    /// <summary>
    /// Response Data bytes without the response code, device status,
    /// and checksum bytes.
    /// </summary>
    public byte[] Data
    {
      get { return m_Data; }
    }    

  }
}
