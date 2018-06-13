using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace FieldCommGroup.HartIPClient
{
    public class AppSettings
    {
        public string LogFilePath;

        private void Init()
        {
            LogFilePath = GetFolder() + "\\MessageLog.txt";
        }

        public AppSettings()
        {
            Init();
        }

        // get settings from file
        public AppSettings(string path)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(path);
                LogFilePath = reader.ReadLine();
            }
            catch (IOException ex)
            {
                Init(); // file not found, use defaults
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:\r\n\r\n" + ex.Message);
            }

            if (reader != null)
                reader.Close();
        }

        public static string GetFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "\\FieldComm_Group\\HARTIPClient";
        }

        public void Save(string path)
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(path);
                writer.WriteLine(LogFilePath);
            }
            catch (IOException ex)
            {
                MessageBox.Show("IOException:\r\n\r\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:\r\n\r\n" + ex.Message);
            }

            if (writer != null)
                writer.Close();

            writer.Close();
        }
    }
}
