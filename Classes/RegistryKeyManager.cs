using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsertAe.Classes
{
    public static class RegistryKeyManager
    {
        public static void ChangeKeyValue(string keyPath, string keyName, string keyValue, RegistryValueKind valueType = RegistryValueKind.DWord)
        {
            try
            {
                RegistryKey Key;
                bool bLocalMachine = keyPath.Contains("HKEY_LOCAL_MACHINE");

                if (bLocalMachine)
                {
                    keyPath = keyPath.Replace(@"HKEY_LOCAL_MACHINE\", "");
                    Key = Registry.LocalMachine.CreateSubKey(keyPath);
                }
                else
                {
                    keyPath = keyPath.Replace(@"HKEY_CURRENT_USER\", "");
                    Key = Registry.CurrentUser.CreateSubKey(keyPath);
                }

                if (Key == null || Key.GetValue(keyName) == null || Key.GetValue(keyName).ToString() != keyValue)
                    Key.SetValue(keyName, keyValue, valueType);

                Key.Close();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        public static void DeleteKeyValue(string keyPath, string keyName)
        {
            try
            {
                RegistryKey Key;
                bool bLocalMachine = keyPath.Contains("HKEY_LOCAL_MACHINE");

                if (bLocalMachine)
                {
                    keyPath = keyPath.Replace(@"HKEY_LOCAL_MACHINE\", "");
                    Key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                }
                else
                {
                    keyPath = keyPath.Replace(@"HKEY_CURRENT_USER\", "");
                    Key = Registry.CurrentUser.OpenSubKey(keyPath, true);
                }

                Key.DeleteValue(keyName, false);
                Key.Close();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
    }
}
