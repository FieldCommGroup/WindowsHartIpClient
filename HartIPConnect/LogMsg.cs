using System;
using System.IO;
using System.Security;
using System.Globalization;

namespace FieldCommGroup.HartIPConnect
{
  /// <summary>
  /// LogMsg class write messages to a file
  /// with append current timestamp to log filename,
  /// create a new file with timestamp when log entries over the limit. 
  /// <para>Entries Limit is set 100000 entries in a file before create 
  /// another new log file.</para>
  /// </summary>
  public sealed class LogMsg
  {
    private const uint m_Limit = 100000;

    private static volatile LogMsg instance;
    private static Object SyncRoot = new Object();

    private String m_strFileName = String.Empty;
    private String m_strTimeStampFileName = String.Empty;
    private bool m_bLogToFile = false;    
    private uint m_Count = 0;
    private bool m_bOutputToScreen = false;

    /// <summary>
    /// Private contructor
    /// </summary>
    private LogMsg()
    {
    }

    /// <summary>
    /// Get the LogMsg instance.
    /// </summary>
    public static LogMsg Instance
    {
      get
      {
        if (instance == null)
        {
          lock (SyncRoot)
          {
            instance = new LogMsg();            
          }
        }
        return instance;
      }
    }     

    /// <summary>
    /// Check if it is logging to file
    /// </summary>
    public bool IsLoggingToFile
    {
        get 
        {
          bool bLogFile;
          lock (SyncRoot)
          {
              bLogFile = m_bLogToFile;
          }
          return bLogFile;
        }
    }    

    /// <summary>
    /// Get the current filename
    /// </summary>
    public String LogFileName
    {
        get 
        {
          String FileName;
          lock (SyncRoot)
          {
            FileName = m_strFileName;
          }
          return FileName;
        }
    }

    /// <summary>
    /// Get/Set the output to screen
    /// </summary>
    public bool OutputToScreen
    {
      get 
      {
        bool bOutputtoScreen;
        lock (SyncRoot)
        {
          bOutputtoScreen = m_bOutputToScreen;
        }           
        return m_bOutputToScreen;
      }
      set
      {
        lock (SyncRoot)
        {
          m_bOutputToScreen = value;
        }
      }
    }
   
    /// <summary>
    /// Call to start log messages to the specified filename
    /// </summary>
    /// <param name="strFileName">String log filename</param>   
    public void StartLogging(String strFileName)
    {
      strFileName = strFileName.Trim();
      if (strFileName.Length == 0)
        throw new ArgumentException("StartLogging Error: FileName is empty.");      

      lock (SyncRoot)
      {
        m_strFileName = strFileName;
        AddTimeStampInFileName();
        m_bLogToFile = true;
      }     
    }

    public string GetCurrentLogFile()
    {
            return m_strTimeStampFileName;
    }

    /// <summary>
    /// Stop logging messages
    /// </summary>
    public void StopLogging()
    {
        lock (SyncRoot)
        {
          if (m_bLogToFile)
          {
            Log("Stop logging and closing the Log File.", true);
            m_bLogToFile = false;
          }
        }
    }

    /// <summary>
    /// Write message in debug console, console screen, and/or write into a file.
    /// </summary>
    /// <param name="msg">String message to write</param>
    public void Log(String msg)
    {
      Log(msg, false, false);
    }

    /// <summary>
    /// Write message in debug console and/or write into a file.
    /// </summary>
    /// <param name="msg">String message to write</param>
    /// <param name="bAddTimeStamp">bool true to add timestamp in front of message</param>
    /// <remark>It will write a message to debug console console screen,
    /// and/or write to log file.</remark>
    public void Log(String msg, bool bAddTimeStamp)
    {
      Log(msg, bAddTimeStamp, false);
    }

    /// <summary>
    /// Write message in debug console and/or write into a file.
    /// </summary>
    /// <param name="msg">String message to write</param>
    /// <param name="bAddTimeStamp">bool true to add timestamp in front of message</param>
    /// <param name="bAddEmptyLine">bool true to add empty line after the message</param>
    /// <remark>It will write a message to debug console console screen,
    /// and/or write to log file.</remark>
    public void Log(String msg, bool bAddTimeStamp, bool bAddEmptyLine)
    {
      do
      {
        lock (SyncRoot)
        {
          String strOutput;

          if (bAddTimeStamp)
          {
            String TimeStamp = HartUtil.GetTimeStamp();
            strOutput = String.Format("{0}, {1}", TimeStamp, msg);
          }
          else
            strOutput = msg;

          // Check if display output string to screen
          if (m_bOutputToScreen)
          {
            Console.WriteLine(strOutput);
            Console.WriteLine();
          }

          if (m_bLogToFile)
          {
            if (m_Count >= m_Limit)
            {
              AddTimeStampInFileName();
              m_Count = 0;
            }
            WriteMsgToFile(m_strTimeStampFileName, strOutput, bAddEmptyLine);
            m_Count++;
          }
          else
          {
            System.Diagnostics.Debug.WriteLine(strOutput);
          }
        }
      } while (false); /* ONCE */
    }

    /// <summary>
    /// Write message to a file
    /// </summary>
    /// <param name="FileName">String log filename</param>
    /// <param name="strOutput">String message to write</param>
    /// <param name="bAddEmptyLine">bool true to add an empty line at the end</param>
    /// <returns>bool true if it is success</returns>
    public static bool WriteMsgToFile(String FileName, String strOutput , bool bAddEmptyLine)
    {
      bool bSuccess = false;
      TextWriter textWriter = null;

      FileName = FileName.Trim();
      if (FileName.Length == 0)
        throw new ArgumentException("WriteMsgToFile Error: Invalid Filename.");        

      // If log msg to file, write msg to log file
      try
      {       
        // if file is not exist, create it first
        if (!File.Exists(FileName))
        {
            // create the directory if it is not existed.
            String folder = Path.GetDirectoryName(FileName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

          // creae the file
          textWriter = new StreamWriter(FileName);
        }
        else
        {
          // append the read value to log file
          textWriter = File.AppendText(FileName);
        }
        textWriter.WriteLine(strOutput);
        if (bAddEmptyLine)
        {
          textWriter.WriteLine();
        }
        textWriter.Flush();
        bSuccess = true;
      }
      catch (PathTooLongException pex)
      {
        System.Diagnostics.Debug.WriteLine("WriteMsgToFile has PathTooLongException:" + pex.Message);
      }
      catch (IOException io)
      {
        System.Diagnostics.Debug.WriteLine("WriteMsgToFile has IOException:" + io.Message);
      }
      catch (UnauthorizedAccessException uaex)
      {
        System.Diagnostics.Debug.WriteLine("WriteMsgToFile has UnauthorizedAccessException:" + uaex.Message);
      }
      catch (ArgumentOutOfRangeException argOutOfRange)
      {
        System.Diagnostics.Debug.WriteLine("WriteMsgToFile has ArgumentOutOfRangeException:" + argOutOfRange.Message);
      }
      catch (SecurityException sex)
      {
        System.Diagnostics.Debug.WriteLine("WriteMsgToFile has SecurityException:" + sex.Message);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine("WriteMsgToFile has SecurityException:" + ex.Message);
      }
      finally
      {
        if (textWriter != null)
          textWriter.Close();
      }      
      
      return bSuccess;
    }

    /// <summary>
    /// Add current timestamp in the filename
    /// </summary>
    private void AddTimeStampInFileName()
    {
        DateTime dt = DateTime.Now;
        String TimeStamp = dt.ToString("MM-dd-yyyy-HH-mm-ss", DateTimeFormatInfo.InvariantInfo);
        int Index = m_strFileName.LastIndexOf('.');

        if (Index != -1)
        {
            int Len = m_strFileName.Length - Index;
            m_strTimeStampFileName = m_strFileName.Substring(0, Index) + "_" + TimeStamp +
                                     m_strFileName.Substring(Index, Len);
        }
        else
            m_strTimeStampFileName = m_strFileName + TimeStamp;
    }
    
  }
}
