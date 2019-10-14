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

Filename: AppSettings.cs

 */

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
