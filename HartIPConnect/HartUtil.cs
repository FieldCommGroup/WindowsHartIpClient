using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;

namespace FieldCommGroup.HartIPConnect
{
  /// <summary>
  /// Contain utility methods
  /// </summary>
  public class HartUtil
  {
    /// <summary>
    /// Calculates the checksum for a HART message.
    /// </summary>
    /// <param name="Command">byte[] Command byte array  
    /// <para>Array should have frame, device type, device
    /// id, command, byte count, and data bytes.</para>
    /// <remarks>See HART specification 081r8.2.pdf section 5.1, 5.2, and 5.3 for frame, 
    /// address, expansion, and data bytes information.
    /// </remarks>
    /// </param>
    /// <param name="cByteCount">byte number of bytes in specified Command array</param>
    /// <returns>byte the calcuated checksum value</returns>
    public static byte GetCheckSum(byte[] Command, byte cByteCount)
    {
      int iIdx;
      byte cSum;

      for (cSum = 0, iIdx = 0; iIdx < cByteCount; iIdx++)
      {
          cSum ^= Command[iIdx];        
      }
      return cSum;
    }

    /// <summary>
    /// Create a HARTDevice object from the HART response command 0.
    /// </summary>
    /// <param name="Rsp"><see cref="HartIPResponse">HartIPResponse</see></param>
    /// <param name="HParent"><see cref="HartDevice">HartDevice device parent</see></param>
    /// <returns>HartDevice object if success. Otherwise return NULL.</returns>
    public static HartDevice CreateHartDevice(HartIPResponse Rsp, HartDevice HParent)
    {
      HartDevice HDevice = null;
      ushort usDeviceType;
      uint   uiDeviceId = 0;
      byte   cUnivCmdRev;
      byte   cDataCount;

      if (Rsp == null)
      {
        throw new ArgumentException("CreateHartDevice Error: Invalid arguments.");
      }

      if (Rsp.ResponseCode != HARTIPMessage.RSP_SUCCESS)
      {
        throw new ArgumentException("CreateHartDevice Error: Response code=" +
          Rsp.ResponseCode);
      }

      cDataCount = Rsp.DataCount;
      // check if data btye count less than 12 or cmd is equal to 0
      if ((cDataCount < 12) || (Rsp.Command != HARTIPMessage.CMD_READ_UID))
      {
        throw new ArgumentException("CreateHartDevice Error: Invalid Command Response.");
      }

      // Copy the response data
      byte[] Data = new byte[cDataCount];
      Data = Rsp.Data;      

      // check if it is HART5 or above device
      if (Data[0] != 254)
      {
        throw new ArgumentException("CreateHartDevice Error: Invalid Target Hart Device.");
      }

      //  convert network to host byte order
      usDeviceType = (ushort)((Data[1] << 8) + (Data[2] & 0x0ff));
      uiDeviceId = (uint)((Data[9] << 16) | (Data[10] << 8) | (Data[11] & 0x0ff));
      cUnivCmdRev = Data[4];

      HDevice = new HartDevice(usDeviceType, uiDeviceId, cUnivCmdRev, HParent);
      HDevice.FlagAssignment = (DeviceFlagAssignment)Data[8];

	  if ((cUnivCmdRev >= 7) && (cDataCount >= 22))
        HDevice.Profile = (DeviceProfile)Data[21]; 

      return HDevice;
    }

    /// <summary>
    /// Get the HART Device Tag from HART Response command 13 or 20
    /// </summary>
    /// <param name="Rsp"><see cref="HartIPResponse">HartIPResponse</see></param>
    /// <returns>String device tag</returns>
    public static String GetHARTDeviceTag(HartIPResponse Rsp)
    {
      String strTag = String.Empty;
      byte   cDataCount, cCmd;

      if (Rsp == null)
      {
        throw new ArgumentException("GetHARTDeviceTag Error: Invalid arguments.");
      }

      cCmd = Rsp.Command;
      cDataCount = Rsp.DataCount;
      
      // check if we receive the collect cmd and data byte count
      if (((cCmd != (byte)HARTIPMessage.CMD_READ_LONG_TAG) && (cCmd != (byte)HARTIPMessage.CMD_READ_TAG_DESC_DATE)) ||
          ((cCmd == (byte)HARTIPMessage.CMD_READ_LONG_TAG) && (cDataCount < HARTIPMessage.MAX_LONG_TAG_LENGTH)) ||
          ((cCmd == (byte)HARTIPMessage.CMD_READ_TAG_DESC_DATE) && (cDataCount < HARTIPMessage.MAX_SHORT_TAG_LENGTH)))
      {
        throw new ArgumentException("GetHARTDeviceTag Error: Invalid Command Response.");
      }

      byte[] Data = new byte[cDataCount];
      Data = Rsp.Data;

      // if it is cmd 20
      if (cCmd == HARTIPMessage.CMD_READ_LONG_TAG)
      {
        for (int i = 0; i < HARTIPMessage.MAX_LONG_TAG_LENGTH; i++)
        {
          char ch = (char)(Data[i] & 0x00ff);
          if (ch == 0)
            break;

          strTag += ch;
        }        
      }
      else
        // it is cmd 13
        strTag = UnpackAscii(Data, (byte)HARTIPMessage.MAX_SHORT_TAG_LENGTH);

      return strTag;
    }

    /// <summary>
    /// Unpack the HART packed ASCII string from the specified array
    /// into a null terminated string. 
    /// <note>Only translates the closes multiple of 3 of the packed length.</note>
    /// </summary>
    /// <param name="acResponse">byte[] array containing the packed ASCII string</param>
    /// <param name="cPackedLength">byte number of bytes in the packed string</param>
    /// <returns>String unpacked ASCII string</returns>
    public static String UnpackAscii(byte[] acResponse, byte cPackedLength)
    {
      ushort      usIdx;
      ushort      usGroupCnt;
      ushort      usMaxGroups;    // Number of 4 byte groups to pack.
      ushort      usMask;
      ushort[]    usBuf = new ushort[4];
      String ascii = String.Empty;
      int         iIndex = 0;

      usMaxGroups = (ushort)(cPackedLength / 3);

      for (usGroupCnt = 0; usGroupCnt < usMaxGroups; usGroupCnt++)
      {
        // First unpack 3 bytes into a group of 4 bytes, clearing bits 6 & 7.
        usBuf[0] = (ushort)(acResponse[iIndex] >> 2);
        usBuf[1] = (ushort)(((acResponse[iIndex] << 4) & 0x30) | (acResponse[iIndex + 1] >> 4));
        usBuf[2] = (ushort)(((acResponse[iIndex + 1] << 2) & 0x3C) | (acResponse[iIndex + 2] >> 6));
        usBuf[3] = (ushort)(acResponse[iIndex + 2] & 0x3F);
        iIndex += 3;

        // Now transfer to unpacked area, setting bit 6 to complement of bit 5.
        for (usIdx = 0; usIdx < 4; usIdx++)
        {
          usMask = (ushort)(((usBuf[usIdx] & 0x20) << 1) ^ 0x40);
          ascii += (char)(usBuf[usIdx] | usMask);
        }
      }
      return ascii;
    }

    /// <summary>
    /// Get HIPDevice's number of sub-devices from the response
    /// Hart-5 command 128 or Hart-7 command 74.
    /// </summary>
    /// <param name="Rsp"><see cref="HartIPResponse">HartIPResponse object to parse</see></param>
    /// <returns>ushort number of sub-devices in HIPDevice.</returns>
    public static ushort GetHIPDeviceNumOfDevices(HartIPResponse Rsp)
    {
      String strTag = String.Empty;
      
      byte cDataCount, cCmd;
      ushort usNumOfDevices;

      if (Rsp == null)
      {
        throw new ArgumentException("GetHARTDeviceTag Error: Invalid arguments.");
      }
     
      cDataCount = Rsp.DataCount;
      cCmd = Rsp.Command;

      // check if we receive the collect cmd and data byte count
            if ((Rsp.ResponseCode != HARTIPMessage.RSP_SUCCESS) &&
                (((cCmd == (byte)HARTIPMessage.CMD_READ_HARTPORT_PARAMETERS) && (cDataCount < 18)) ||
                ((cCmd == (byte)HARTIPMessage.CMD_READ_IO_SYSTEM_CAPABILITIES) && (cDataCount < 8))))
            {
                throw new ArgumentException("GetHIPDeviceNumOfDevices Error: Invalid Command Response.");
            }

      byte[] Data = new byte[cDataCount];
      Data = Rsp.Data;

      if (cCmd == HARTIPMessage.CMD_READ_HARTPORT_PARAMETERS)
        usNumOfDevices = (ushort)((Data[8] << 8) + (Data[9] & 0x0ff));
      else
        usNumOfDevices = (ushort)((Data[3] << 8) + (Data[4] & 0x0ff));

      return usNumOfDevices; 
    }

    /// <summary>
    /// Convert Hex char to nibble value
    /// </summary>
    /// <param name="c">char Character to convert</param>
    /// <param name="b">out byte Converted byte value</param>
    /// <returns>bool</returns>
    public static bool HexCharToNibble(char c, out byte b)
    {
      bool bSuccess = true;
      b = 0;

      if (('0' <= c) && (c <= '9'))
      {
        b = Convert.ToByte(c - '0');
      }
      else if (('a' <= c) && (c <= 'f'))
      {
        b = (byte)(Convert.ToByte(c - 'a') + 10);
      }
      else if (('A' <= c) && (c <= 'F'))
      {
        b = (byte)(Convert.ToByte(c - 'A') + 10);
      }
      else
      {
        bSuccess = false;
      }

      return bSuccess;
    }

    /// <summary>
    /// Determines the classification of the response code from the HART response.
    /// </summary>
    /// <param name="cCode">byte first byte of the response code from the HART response.</param>
    /// <returns>byte Response code</returns>
    public static byte GetResponseCode(byte cCode)
    {
      byte cType = 0;
      if (cCode == 0)
        cType = HARTIPMessage.RSP_SUCCESS;
      else if (cCode == 0xFF)
        cType = HARTIPMessage.RSP_NO_RESPONSE;
      else if ((cCode & 0x80) == 0x80)
        cType = HARTIPMessage.RSP_COMM_ERROR;
      else if (cCode == HARTIPMessage.RSP_DEVICE_BUSY)
        cType = HARTIPMessage.RSP_DEVICE_BUSY;
      else if (cCode == HARTIPMessage.RSP_CMD_NOT_IMPLEMENTED)
        cType = HARTIPMessage.RSP_CMD_NOT_IMPLEMENTED;
      else if (cCode == HARTIPMessage.RSP_DR_INITIATE)
        cType = HARTIPMessage.RSP_DR_INITIATE;
      else if (cCode == HARTIPMessage.RSP_DR_RUNNING)
        cType = HARTIPMessage.RSP_DR_RUNNING;
      else if (cCode == HARTIPMessage.RSP_DR_DEAD)
        cType = HARTIPMessage.RSP_DR_DEAD;
      else if (cCode == HARTIPMessage.RSP_TOO_FEW_DATA_BYTES)
        cType = HARTIPMessage.RSP_CMD_ERROR;
      else if (cCode == HARTIPMessage.RSP_ALL_SESSIONS_IN_USE)
        cType = HARTIPMessage.RSP_DEVICE_BUSY;
      else if (cCode == HARTIPMessage.RSP_ADDRESSED_DEVICE_DISCONNECTED)
        cType = HARTIPMessage.RSP_NO_RESPONSE;
      else if ((cCode >= 96) && (cCode <= 127))
        cType = HARTIPMessage.RSP_CMD_WARNING;
      else
      {
        switch (cCode)
        {
          case 8:
          case 14:
          case 24:
          case 25:
          case 26:
          case 27:
          case 30:
          case 31:
            cType = HARTIPMessage.RSP_CMD_WARNING;
            break;

          default:
            cType = HARTIPMessage.RSP_CMD_ERROR;
            break;
        }
      }
      return cType;
    }

    /// <summary>
    /// Get current timestamp
    /// </summary>
    /// <returns>String</returns>
    public static String GetTimeStamp()
    {
      DateTime dt = DateTime.Now;
      return dt.ToString("MM/dd/yyyy HH:mm:ss:fff", DateTimeFormatInfo.InvariantInfo);
    }
  }


    /// <summary>
    /// HARTMsgResult class contains 
    /// request, response message,
    /// and errors strings.
    /// </summary>
    public class HARTMsgResult
    {
        string m_Request = string.Empty;
        string m_Response = string.Empty;
        byte m_Status = HARTIPMessage.RSP_COMM_ERROR;

        /// <summary>
        /// Request message strings
        /// </summary>
        public string RequestMsg
        {
            get { return m_Request; }
        }
        /// <summary>
        /// Response message strings
        /// </summary>
        public string ResponseMsg
        {
            get { return m_Response; }
        }
        /// <summary>
        /// Command result message status
        /// </summary>
        public byte Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        /// <summary>
        /// Add a request or response message string
        /// </summary>
        /// <param name="Msg">string a message to add</param>
        /// <param name="bRequest">bool true is added to request message</param>
        /// <param name="bAddTimeStamp">bool true is added a timestamp in front of the specified string</param>
        public void AddMessage(string Msg, bool bRequest, bool bAddTimeStamp)
        {
            string Output = string.Empty;
            if (bAddTimeStamp)
                Output = string.Format("{0} {1}", HartUtil.GetTimeStamp(), Msg);
            else
                Output = Msg;

            if (bRequest)
            {
                if (m_Request.Length > 0)
                    m_Request += "\r\n";

                m_Request += Output;
            }
            else
            {
                if (m_Response.Length > 0)
                    m_Response += "\r\n";

                m_Response += Output;
            }
        }
    }

    /// <summary>
    /// DeviceData class contains device name, universal
    /// revision, device type, and device id information.
    /// </summary>
    public class DeviceData
  {
    String m_Name;
    byte   m_cUnivRev;
    DeviceID m_DeviceID = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name">stirng device name</param>
    /// <param name="usDeviceType">ushort device type</param>
    /// <param name="nDeviceId">uint device ID</param>
    /// <param name="cUnivRev">byte device universal revision</param>
    public DeviceData(String Name, ushort usDeviceType, uint nDeviceId, byte cUnivRev)
    {
      this.m_Name = Name;     
      this.m_cUnivRev = cUnivRev;
      this.m_DeviceID = new DeviceID(usDeviceType, nDeviceId);
    }

    /// <summary>
    /// Device name
    /// </summary>
    public String Name
    {
      get { return m_Name; }
      set { m_Name = value; }
    }
    /// <summary>
    /// Device type
    /// </summary>
    public ushort DeviceType
    {
      get { return m_DeviceID.DeviceType; }
    }
    /// <summary>
    /// Device ID
    /// </summary>
    public uint DeviceId
    {
      get { return m_DeviceID.DeviceId; }
    }
    /// <summary>
    /// Universal revision
    /// </summary>
    public byte UnivRev
    {
      get { return m_cUnivRev; }
    }
    /// <summary>
    /// Device ID object that held the
    /// the device type and device id
    /// </summary>
    public DeviceID LongAddress
    {
      get { return m_DeviceID;}
    }

    /// <summary>
    /// Override the ToString
    /// </summary>
    /// <returns>String Device name</returns>
    public override String ToString()
    {
      return this.m_Name;
    }
  }

  /// <summary>
  /// DeviceID class contains device type and
  /// device id a HART device's long address.
  /// </summary>
  public class DeviceID
  {
    ushort m_usDeviceType;
    uint m_nDeviceId;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="usDeviceType">ushort device type</param>
    /// <param name="nDeviceId">uint Device ID</param>
    public DeviceID(ushort usDeviceType, uint nDeviceId)
    {
      this.m_usDeviceType = usDeviceType;
      this.m_nDeviceId = nDeviceId;
    }

    /// <summary>
    /// Device type
    /// </summary>
    public ushort DeviceType
    {
      get { return m_usDeviceType; }
    }
    /// <summary>
    /// Device ID
    /// </summary>
    public uint DeviceId
    {
      get { return m_nDeviceId; }
    }
    /// <summary>
    /// Device type and device ID in a Hex string
    /// </summary>
    /// <param name="bExtended">bool true is to use the extended device bits
    /// in returning hex String</param>
    /// <returns>String a hex string</returns>
    public String ToHexString(bool bExtended)
    {
      String Expand;
      if (bExtended)
        Expand = String.Format("{0:X2}", (byte)(((m_usDeviceType >> 8) & 0x3F) | 0x80));
      else
        Expand = String.Format("{0:X2}", (byte)(m_usDeviceType >> 8));
        
      return String.Format("{0}{1:X2} {2:X2}{3:X2}{4:X2}",
        Expand,
        (byte)(m_usDeviceType & 0x0ff),
        (byte)((m_nDeviceId >> 16) & 0x0ff),
        (byte)((m_nDeviceId >> 8) & 0x0ff),
        (byte)(m_nDeviceId & 0x0ff));
    }
  }

  /// <summary>
  /// This class is used to block/wait until the response message is received
  /// and the result is set, or wait blocking timeout.
  /// </summary>
  public class MsgResponse
  {
      private bool m_bResponse;
      private bool m_bResponseSet;
      private HartIPResponse m_RspMsg;
      private int m_WaitTimeout;    

      /// <summary>
      /// Default constructor and use 60000 millisecond blocking timeout
      /// </summary>
      public MsgResponse()
          : this(HARTIPConnect.SOCKET_TIMEOUT_DEFAULT)
      {
      }

      /// <summary>
      /// Contrutor
      /// </summary>
      /// <param name="WaitTimeout">int blocking timeout in milliseconds</param>
      public MsgResponse(int WaitTimeout)
      {
            if (WaitTimeout == HARTIPConnect.USE_SOCKET_TIMEOUT_DEFAULT)
                m_WaitTimeout = HARTIPConnect.SOCKET_TIMEOUT_DEFAULT;
            else
                m_WaitTimeout = WaitTimeout;
      }

      /// <summary>
      /// Response message property
      /// </summary>
      public HartIPResponse ResponseMsg
      {
          get { return m_RspMsg; }
      }

      /// <summary>
      ///  Blocking/waiting until this signal release by SetResponse()
      ///  or blocking timeout.
      /// </summary>
      public bool GetResponse()
      {
          lock (this)
          {
              if (m_bResponseSet)
              {
                  return m_bResponse;
              }

              try
              {
                  // wait for the response
                  Monitor.Wait(this, m_WaitTimeout);
                  return m_bResponse;
              }
              catch (ThreadInterruptedException)
              {
                  throw;
              }
          }
      }

      /// <summary>
      /// Release the lock signal.
      /// </summary>     
      public void SetResponse()
      {
          lock (this)
          {
              m_bResponseSet = true;
              try
              {
                  // release the waiting lock
                  Monitor.Pulse(this);
              }
              catch (ThreadStateException)
              {
                  throw;
              }
          }
      }

      /// <summary>
      /// Release the lock signal.
      /// </summary>     
      /// <param name="RspMsg"><see cref="HartIPResponse"/>returned 
      /// response message</param>
      public void SetResponse(HartIPResponse RspMsg)
      {
          lock (this)
          {
              this.m_RspMsg = RspMsg;
              m_bResponse = true;
              m_bResponseSet = true;
              try
              {
                  // release the waiting lock
                  Monitor.Pulse(this);
              }
              catch (ThreadStateException)
              {
                  throw;
              }
          }
      }
  } 

}
