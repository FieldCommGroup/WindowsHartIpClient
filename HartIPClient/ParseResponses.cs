using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.IO;
using FieldCommGroup.HartIPConnect;

namespace FieldCommGroup.HartIPClient
{
  /// <summary>
  /// ParseResponses class reads section, key, values format file to
  /// parse responses.
  /// </summary>
  public class ParseResponses
  {
    [DllImport("KERNEL32.DLL")]
    private static extern int GetPrivateProfileSectionNames(
      byte[] ReturnedString,
      int nSize, String FileName);
   
    [DllImport("KERNEL32.DLL")]
    private static extern int GetPrivateProfileSection(
      String AppName,
      byte[] ReturnedString, int nSize,
      String FileName);

    private const int BufferSize = 32768;
    private const int OutBufferSize = 2048;
   
    private static Object SyncRoot = new Object(); 
    private IDictionary<String, IDictionary<String, String>> m_Commands;
    private String m_Filenam = String.Empty;

    /// <summary>
    /// contructor
    /// </summary>
    public ParseResponses()
    {
        m_Commands = new Dictionary<String, IDictionary<String, String>>();
    }   

    /// <summary>
    /// Load the file and put into m_Commands list
    /// </summary>
    /// <param name="Filename">String</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if Filename is invalid value.      
    /// </exception> 
    public void StartParsing(String Filename)
    {
      lock (SyncRoot)
      {
        if (String.IsNullOrEmpty(Filename))
          throw new System.ArgumentNullException("ParseResponses Error: Invalid argument.");

        if (!File.Exists(Filename))
          throw new ArgumentException("ParseResponses Error: TalkFile does not exist.");

        m_Filenam = Filename;
        
        if (m_Commands.Count > 0)
          m_Commands.Clear();

        GetCommandsNames();
      }
    }

    /// <summary>
    /// Stop parsing Responses.
    /// </summary>
    public void StopParsing()
    {
      lock (SyncRoot)
      {        
        // clear the m_Commands
        if (m_Commands.Count > 0)
          m_Commands.Clear();
      }
    }

    /// <summary>
    /// Parse HART Response to readable string.
    /// </summary>
    /// <param name="Rsp">HartIPResponse</param>
    /// <returns>String</returns>
    public String ParseResponse(HartIPResponse Rsp)
    {
      if (Rsp == null)
        throw new System.ArgumentNullException("ParseResponse Error: Invalid argument.");

      bool bSuccess = false;
      String CmdID;
      ushort usExtendedCmd = 0;
      IDictionary<String, String> Outputs = new Dictionary<String, String>();
      byte cCmd = Rsp.Command;
      String Data = String.Empty;
      bool bSkipExtendedCmdBtyes = false;

      do
      {             
          lock (SyncRoot)
          {
            try
            {              
              if (m_Commands.Count > 0)
              {   // parse success and error responses                               
                  if ((cCmd == HARTIPMessage.CMD_EXTENDED_CMD))
                  {
                      usExtendedCmd = (ushort)((Rsp.Data[0] << 8) + (Rsp.Data[1] & 0x0ff));
                      CmdID = String.Format("{0:000}", usExtendedCmd);
                      bSuccess = m_Commands.TryGetValue(CmdID, out Outputs);
                      if (!bSuccess)
                      {
                          CmdID = String.Format("031_{0}", usExtendedCmd);
                          bSuccess = m_Commands.TryGetValue(CmdID, out Outputs);
                      }
                      else
                          bSkipExtendedCmdBtyes = true;
                  }
                  else
                  {
                      CmdID = String.Format("{0:000}", cCmd);
                      bSuccess = m_Commands.TryGetValue(CmdID, out Outputs);
                  }                                                            
              }

              if (bSuccess)
              {
                byte Len;
                int iIndex = 0;
                Data = String.Format("Rx Cmd={0}", cCmd);
                if (bSkipExtendedCmdBtyes)
                {
                    Len = (byte)(Rsp.DataCount);
                    iIndex = 2;
                    Data = String.Format("Rx Cmd={0}", usExtendedCmd);
                }
                else
                    Len = (byte)(Rsp.DataCount + 2);

                byte[] tmp = new byte[Len];                

                // put the response code and device status values in the data array first
                tmp[0] = Rsp.ResponseCode;
                tmp[1] = Rsp.DeviceStatus;
               
                for (int i = 2; i < Len; i++)
                {
                  tmp[i] = Rsp.Data[iIndex++];
                }
                Data += ParseData(tmp, Len, Outputs);               
              }
              else
              {
                Data += "\r\nData: ";
                byte[] Temp = Rsp.Data;
                for (int i = 0; i < Rsp.DataCount; i++)
                {
                  Data += (String.Format("{0:X2} ", Temp[i]));
                }
              }
            }
            catch (Exception ex)
            {
              Data = ex.Message;
            }
          }
        
      } while (false); /* ONCE */

      return Data;
    }    

    /// <summary>
    /// Get commands names
    /// </summary>
    private void GetCommandsNames()
    {
      do
      {
        byte[] SectionNames = new byte[BufferSize];
        int nLen = 0;
        nLen = ParseResponses.GetPrivateProfileSectionNames(SectionNames,
               SectionNames.GetUpperBound(0), m_Filenam);

        if (nLen == 0)        
          break;       
     
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < nLen; i++)
        {
          if (SectionNames[i] != 0)
          {
            sb.Append((char)SectionNames[i]);
          }
          else
          {
            if (sb.Length > 0)
            {
              String Cmd = sb.ToString();
              IDictionary<String, String> Outputs = new Dictionary<String, String>();
                          
              // Get the command output keys and values
              GetCmdOutputs(Cmd, Outputs);              
              m_Commands.Add(Cmd, Outputs);
                           
              sb = new StringBuilder();
            }
          }
        }      
      } while (false); /* ONCE */
    }

    /// <summary>
    /// Get command keys and values
    /// </summary>
    /// <param name="Command">String</param>
    /// <param name="Outputs">Dictionary</param>
    /// <returns>bool</returns>
    private void GetCmdOutputs(String Command, IDictionary<String, String> Outputs)
    {
      if (String.IsNullOrEmpty(Command))
      {
        throw new System.ArgumentNullException("Command cannot be null or empty string.");
      }

      do
      {
        byte[] Items = new byte[OutBufferSize];
        int nLen = 0;
        nLen = ParseResponses.GetPrivateProfileSection(Command, Items,
               Items.GetUpperBound(0), m_Filenam);

        if (nLen == 0)
        {          
          break;
        }
        
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < nLen; i++)
        {
          if (Items[i] != 0)
          {
            sb.Append((char)Items[i]);
          }
          else
          {
            if (sb.Length > 0)
            {
              String[] Values = sb.ToString().Split('=');
              String Key = Values[0];
              // only want the output keys and values
              if (Key.Contains("Output") || Key.Contains("output"))
              {                
                Outputs[Values[0].Trim()] = Values[1].Trim();
              }              
              sb = new StringBuilder();
            }
          }
        }
      } while (false); /* ONCE */
    }

    /// <summary>
    /// Parse data to readable string
    /// </summary>
    /// <param name="Data">byte[]</param>
    /// <param name="cDataCount">byte</param>
    /// <param name="Outputs">IDictionary</param>
    /// <returns></returns>
    private String ParseData(byte[] Data, byte cDataCount, 
                             IDictionary<String, String> Outputs)
    {
      String Outs = String.Empty;
      String Name;
      String DataType;
      String Value;
      int nIndex = 0;
      bool bSuccess = true;

      do
      {
        foreach (KeyValuePair<String, String> pair in Outputs)
        {
          // Check if  failed to parse or more outputs than the data
          if (!bSuccess || (nIndex >= (int)cDataCount))
            break;

          String[] Values = pair.Value.Split(',') ;
          if (Values.Length < 2)
            break;

          Name = Values[0];
          Value = String.Empty;
          DataType = Values[1].Trim();
          
          if (DataType.IndexOf("HEX:", StringComparison.OrdinalIgnoreCase) == 0)
          {
            String[] Val = Values[1].Split(':');
            int nSize = Convert.ToInt32(Val[1]);
            for (int i = 0; i < nSize; i++)
            {
              Value += String.Format("{0:X2}", Data[nIndex + i]);
            }
            nIndex += nSize;
          }
          else if ((DataType.IndexOf("AS:", StringComparison.OrdinalIgnoreCase) == 0) ||
                   (DataType.IndexOf("ASCII:", StringComparison.OrdinalIgnoreCase) == 0))
          {                     
            String[] Ascii = Values[1].Split(':');
            int nSize = Convert.ToInt32(Ascii[1]);
            for (int i = 0; i < nSize; i++)
            {
              char ch = (char)(Data[nIndex + i] & 0x00ff);
              if (ch == 0)
                break;

                Value += ch;
            }
            Value = Value.Trim();
            nIndex += nSize;                              
          }
          else if ((DataType.IndexOf("PA:", StringComparison.OrdinalIgnoreCase) == 0) ||
                   (DataType.IndexOf("PACKED ASCII:", StringComparison.OrdinalIgnoreCase) == 0))
          {
            String[] Pa = Values[1].Split(':');
            int nSize = Convert.ToInt32(Pa[1]);
            byte[] Temp = new byte[nSize];
            for (int i = 0; i < nSize; i++)
            {
              Temp[i] = Data[nIndex + i];
            }
            Value = (HartUtil.UnpackAscii(Temp, (byte)nSize)).Trim();
            nIndex += nSize;
          }
          else
          {
             switch (DataType)
             {
                case "BITS8":
                {
                   byte[] c = new byte[1];
                   c[0] = Data[nIndex++];
                   BitArray bits = new BitArray(c);
                   for (int i = 0; i < bits.Length; i++)
                   {
                     Value += ((bits[i]) ? 1 : 0);
                   }
                   break;
                 }                   

                 case "BL":
                 case "BOOLEAN":
                 case "UNSIGNED8":
                 case "U8":
                 {
                   byte c = Data[nIndex++];
                   Value = c.ToString();
                   break;
                 }

                 case "SIGNED8":
                 case "S8":
                 {
                   sbyte c = (sbyte)(Data[nIndex++]);
                   Value = c.ToString();
                   break;
                 }

                 case "UNSIGNED16":
                 case "U16":
                 {
                   ushort sh = (ushort)((Data[nIndex] << 8) + (Data[nIndex + 1] & 0x0ff));
                   nIndex += 2;
                   Value = sh.ToString();
                   break;
                 }

                case "SIGNED16":
                case "S16":
                {
                  short sh = (short)((Data[nIndex] << 8) + (Data[nIndex + 1] & 0x0ff));
                  nIndex += 2;
                  Value = sh.ToString();
                  break;
                }

               case "UNSIGNED24":
               case "U24":
               {
                 uint ui = (uint)((Data[nIndex] << 16) + (Data[nIndex + 1] << 8) +
                           (Data[nIndex + 2] & 0x0ff));
                 nIndex += 3;
                 Value = ui.ToString();
                 break;
               }

               case "SIGNED24":
               case "S24":
               case "RAW24":
               case "R24":
               {
                 int i = (int)((Data[nIndex] << 16) + (Data[nIndex + 1] << 8) +
                         (Data[nIndex + 2] & 0x0ff));
                 nIndex += 3;
                 Value = i.ToString();
                 break;
               }

               case "UNSIGNED32":
               case "U32":
               {
                 uint i = (uint)((Data[nIndex] << 24) + (Data[nIndex + 1] << 16) +
                             (Data[nIndex + 2] << 8) + (Data[nIndex + 3] & 0x0ff));
                 nIndex += 4;
                 Value = String.Format("{0}", i);
                 break;
              }

               case "UNSIGNED40":
               case "U40":
               {
                 UInt64 i = (uint)((Data[nIndex] << 32) + (Data[nIndex + 1] << 24) +
                             (Data[nIndex + 2] << 16) + (Data[nIndex + 3] << 8) + (Data[nIndex + 4] & 0x0ff));
                 nIndex += 5;
                 Value = String.Format("{0}", i);
                 break;
              }

              case "SIGNED32":
              case "S32":
              case "RAW32":
              case "R32":
              {
                int i = (int)((Data[nIndex] << 24) + (Data[nIndex + 1] << 16) +
                          (Data[nIndex + 2] << 8) + (Data[nIndex + 3] & 0x0ff));
                nIndex += 4;
                Value = String.Format("{0}", i);
                break;
              }

             case "FLOAT":
             case "FL":
             case "F32":
             {
               byte[] arr = new byte[4];
               for (int i = 0, j = 3; i < 4; i++, j--)
               {
                 arr[i] = Data[nIndex + j];
               }
               float fl = BitConverter.ToSingle(arr, 0);
               nIndex += 4;
               Value = String.Format("{0}", fl);
               break;
             }

             case "LONG":
             case "LONG:TIME":
             {
               byte[] arr = new byte[8];
               for (int i = 0, j = 7; i < 8; i++, j--)
               {
                 arr[i] = Data[nIndex + j];
               }
               long l = BitConverter.ToInt64(arr, 0);
               nIndex += 8;

               if (DataType.Contains(":TIME"))
               {
                 // value is in milliseconds since 1/1/1970
                 DateTime started = new DateTime(1970, 1, 1, 0, 0, 0);
                 DateTime timeOfday = started.AddMilliseconds((double)l);
                 Value = timeOfday.ToString("MM/dd/yy HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
               }
               else
                 Value = String.Format("{0}", l);

               break;
             }                

             case "T12":
             case "T24":
             case "TIME":
             case "TIME_12":
             case "TIME-12":
             case "TIME_12H":
             case "TIME-12H":
             case "TIME_24":
             case "TIME-24":
             case "TIME_24H":
             case "TIME-24H":
             {
               uint t = (uint)((Data[nIndex] << 24) + (Data[nIndex + 1] << 16) +
                        (Data[nIndex + 2] << 8) + (Data[nIndex + 3] & 0x0ff));
               // calcuate ms in 1/32s of a millisecond
               t /= 32;
               uint ms = t % 1000;
               t /= 1000;
               uint secs = t % 60;
               t /= 60;
               uint mins = t % 60;
               uint hrs = (uint)(t / 60);
               bool b24h = false;

               // Check if value is in 24 hour display
               if (DataType.Contains("24"))
                 b24h = true;

               if (b24h)
                 Value = String.Format("{0:d2}:{1:d2}:{2:d2}.{3:d2}", hrs, mins, secs, ms);
               else
               {
                 if (hrs >= 12)
                   Value = String.Format("{0:d2}:{1:d2}:{2:d2}.{3:d2} PM", (hrs - 12), mins, secs, ms);
                 else
                   Value = String.Format("{0:d2}:{1:d2}:{2:d2}.{3:d2} AM", (hrs), mins, secs, ms);
               }
               nIndex += 4;
               break;
             }

             default:
             // unknown data type, stop parsing data
             Value = "???";
             bSuccess = false;
             break;
            }
          }

          Outs += (String.Format("\r\n{0}={1}", Name, Value));
        }

        // unknown data bytes or have more bytes than output parameters
        if (nIndex < (int)cDataCount)
        {
          Outs += "\r\nUndefined data bytes=";           
          for (int i = nIndex; i < cDataCount; i++)
          {
            Outs += String.Format("{0:X2}", Data[i]);               
          }
        }
        
      } while (false); /* ONCE */
      return Outs;
    }

  }
}
