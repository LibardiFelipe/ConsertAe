using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.ServiceProcess;
using System.Windows.Forms;
using ConsertAe.Classes;

namespace ConsertAe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Marca as checkBox conforme as chaves do regedit
            UpdateCheckBoxes();
        }

        private void UpdateCheckBoxes()
        {

        }

        private void GrantAccess(string fullPath)
        {
            // Garante permissão total para todos os usuários em uma determinada pasta
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        private void ExecuteCommandOnCMD(string Command, bool bHideConsole = true)
        {
            if (string.IsNullOrEmpty(Command))
                return;

            try
            {
                string CMDPath = Environment.GetEnvironmentVariable("ComSpec");
                if (CMDPath == "")
                    CMDPath = "cmd.exe";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = CMDPath
                };

                if (bHideConsole)
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Arguments = "/C " + Command;
                }
                else
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    startInfo.Arguments = "/K " + Command;
                }

                // TODO: Retornar o conteúdo do CMD como string
                Process.Start(startInfo);
            }
            catch (Exception x) {
                TextDialog.Show(x.Message, false, EMessageCode.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (fbd.ShowDialog() == DialogResult.OK)
                        GrantAccess(fbd.SelectedPath);

                    TextDialog.Show($"Permissão total garantida!\n\"{fbd.SelectedPath}\"");
                }
            }
            catch (Exception x)
            {
                TextDialog.Show(x.Message, false, EMessageCode.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string LocalTemp = Environment.GetEnvironmentVariable("TEMP");
            string MachineTemp = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);

            DirectoryInfo di = new DirectoryInfo(@LocalTemp);
            foreach (FileInfo file in di.EnumerateFiles())
                try { file.Delete(); } catch { }

            foreach (DirectoryInfo subDirectory in di.EnumerateDirectories())
                try { subDirectory.Delete(true); } catch { }

            di = new DirectoryInfo(@MachineTemp);
            foreach (FileInfo file in di.EnumerateFiles())
                try { file.Delete(); } catch { }

            foreach (DirectoryInfo subDirectory in di.EnumerateDirectories())
                try { subDirectory.Delete(true); } catch { }

            TextDialog.Show("Arquivos temporários limpos!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExecuteCommandOnCMD("Dism /Online /Cleanup-Image /RestoreHealth", false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ExecuteCommandOnCMD("chkdsk /r", false);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ExecuteCommandOnCMD("sfc /scannow", false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Tem certeza que deseja desligar o computador?", "ConsertAe", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                ExecuteCommandOnCMD("shutdown /s /f /t 0");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ExecuteCommandOnCMD("net stop \"Spooler\" && net start \"Spooler\"");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Para o spooler
            ExecuteCommandOnCMD("net stop \"Spooler\"");

            // Espera um tempo até o serviço desligar
            Thread.Sleep(2000);

            // C:\Windows\System32\Spool\Printers -> E apaga todo o conteúdo da pasta
            DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\Spool\Printers\");
            foreach (FileInfo file in di.GetFiles())
                try { file.Delete(); } catch (Exception) { }

            // Espera um tempo até os arquivos serem deletados
            Thread.Sleep(1000);

            // Inicia o spooler
            ExecuteCommandOnCMD("net start \"Spooler\"");

            TextDialog.Show("Fila de impressão limpa!");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ExecuteCommandOnCMD("net users", false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Checkbox 1:
            DisableWindowsAds(checkBox1.Checked);
            DisableSearchOnStore(checkBox1.Checked);
            DisableWUDriversUpdates(checkBox1.Checked);
            DisableTimeline(checkBox1.Checked);
            DisableLiveTiles(checkBox1.Checked);
            DisableWebSearch(checkBox1.Checked);
            DisableAutoStoreUpdates(checkBox1.Checked);
            DisableCortana(checkBox1.Checked);
            DisableTelemetry(checkBox1.Checked);

            // Checkbox 2:
            DisableWindowsUpdate(checkBox2.Checked);

            // Checkbox 3:

            // Checkbox 4:
            ManageWindowsClock(checkBox4.Checked);

            var result = TextDialog.Show("Mudanças aplicadas!\nDeseja reiniciar o computador agora?", true, EMessageCode.Question);
            if (result == DialogResult.Yes)
            {
                ExecuteCommandOnCMD("shutdown /r /t 0");
                Close();
            }
        }

        private void FixBadNetworkSharing()
        {
            // Ativa os serviços necessários para o compartilhamento
            // em rede.
            string[] AC = { "fdPHost", "FDResPub", "SSDPSRV", "upnphost", "lmhosts" };
            foreach (string s in AC)
            {
                try
                {
                    ServiceController sc = new ServiceController(s);
                    if (sc != null && sc.Status != ServiceControllerStatus.Running)
                        ExecuteCommandOnCMD($"sc config \"{s}\" start= auto && sc start \"{s}\"");
                }
                catch (Exception) { }
            }

            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LanmanWorkstation
            // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters
            // AllowInsecureGuestAuth = 1
            // RequireSecuritySignature = 0
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LanmanWorkstation";
            string keyName = "AllowInsecureGuestAuth";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            keyName = "RequireSecuritySignature";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");

            keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            keyName = "AllowInsecureGuestAuth";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");

            // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa
            // everyoneincludeanonymous = 1
            // restrictnullsessaccess = 0
            // LimitBlankPasswordUse = 0
            keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa";
            keyName = "everyoneincludeanonymous";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            keyName = "restrictnullsessaccess";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            keyName = "LimitBlankPasswordUse";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");

            // Fix pras impressoras compartilhadas dando problema
            // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides
            // 713073804 = 0
            // 1921033356 = 0
            // 3598754956 = 0
            keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides";
            keyName = "713073804";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            keyName = "1921033356";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            keyName = "3598754956";
            RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");

            // TODO: Testar (seta o usuário atual da máquina como o usuário ativo)
            ExecuteCommandOnCMD($"NET USER \"{Environment.MachineName}\" /ACTIVE:YES");
            TextDialog.Show("O procotolo SMB1 será instalado!\nPor favor, não feche a janela nem desligue o computador.");
            ExecuteCommandOnCMD("DISM /Online /Enable-Feature /All /FeatureName:SMB1Protocol", false); // Janela precisa ser exibida pra informar status da instalação
        }

        private void ManageWindowsClock(bool bUpdate)
        {
            if (!bUpdate)
                return;

            // TODO: Atualizar a data e hora do windows usando a internet
        }

        private void DisableTelemetry(bool bDisable)
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection
            // AllowTelemetry = 0
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection";
            string keyName = "AllowTelemetry";

            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // Desativa os serviços que pesam o Windows
            string[] OS = { "DiagTrack", "SysMain", "dmwappushsvc" /* Antigo DiagTrack */};
            var serviceStatus = bDisable ? ServiceControllerStatus.Stopped : ServiceControllerStatus.Running;

            var startStatus = bDisable ? "disabled" : "enabled";
            var runningStatus = bDisable ? "stop" : "start";

            foreach (string s in OS)
            {
                try
                {
                    ServiceController sc = new ServiceController(s);
                    if (sc != null && sc.Status != serviceStatus)
                        ExecuteCommandOnCMD($"sc config \"{s}\" start= {startStatus} && sc {runningStatus} \"{s}\"");
                }
                catch (Exception) { }
            }

            // TODO: Executar arquivo .bat pra bloquear as portas
            // no firewall contra a telemetria.
        }

        private void DisableCortana(bool bDisable)
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search
            // AllowCortana = 0
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search";
            string keyName = "AllowCortana";

            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void DisableAutoStoreUpdates(bool bDisable)
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore
            // AutoDownload = 2
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore";
            string keyName = "AutoDownload";

            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "2");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void DisableWebSearch(bool bDisable)
        {
            // HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Explorer
            // DisableSearchBoxSuggestions = 1
            string keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Explorer";
            string keyName = "DisableSearchBoxSuggestions";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search
            // BingSearchEnabled = 0
            // AllowSearchToUseLocation = 0
            // CortanaConsent = 0
            keyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search";
            keyName = "BingSearchEnabled";
            if (bDisable)
            {
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
                keyName = "AllowSearchToUseLocation";
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
                keyName = "CortanaConsent";
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            }
            else
            {
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
                keyName = "AllowSearchToUseLocation";
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
                keyName = "CortanaConsent";
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
            }

            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search
            // AllowCortana = 0
            // DisableWebSearch = 1
            keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search";
            keyName = "AllowCortana";
            if (bDisable)
            {
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
                keyName = "DisableWebSearch";
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            }
            else
            {
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
                keyName = "DisableWebSearch";
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
            }
        }

        private void DisableLiveTiles(bool bDisable)
        {
            // HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications
            // NoTileApplicationNotification = 1
            string keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications";
            string keyName = "NoTileApplicationNotification";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer
            // ClearTilesOnExit = 1
            keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer";
            keyName = "ClearTilesOnExit";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void DisableWindowsUpdate(bool bDisable)
        {
            try
            {
                // Checar se o serviço está ativado antes de tentar desativa-lo
                ServiceController sc = new ServiceController("wuauserv");
                if (sc != null && sc.Status != ServiceControllerStatus.Stopped)
                    ExecuteCommandOnCMD($"sc config \"wuauserv\" start= disabled && sc stop \"wuauserv\"");
            }
            catch (Exception) { }

            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU
            // AUOptions = 2
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
            string keyName = "AUOptions";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "2");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // IMPORTANTE:
            TextDialog.Show("Para desativar/ativar, abra o WU e clique em 'verificar se há atualizações' antes de reiniciar o PC!");
        }

        private void DisableTimeline(bool bDisable)
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System
            // EnableActivityFeed = 0
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System";
            string keyName = "EnableActivityFeed";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void DisableWUDriversUpdates(bool bDisable)
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate
            // ExcludeWUDriversInQualityUpdate = 1
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
            string keyName = "ExcludeWUDriversInQualityUpdate";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void DisableSearchOnStore(bool bDisable)
        {
            // HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\Explorer
            // NoUseStoreOpenWith = 1
            string keyPath = @"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\Explorer";
            string keyName = "NoUseStoreOpenWith";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "1");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void DisableWindowsAds(bool bDisable)
        {
            // HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager
            // SilentInstalledAppsEnabled = 0
            string keyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
            string keyName = "SilentInstalledAppsEnabled";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager
            // SystemPaneSuggestionsEnabled = 0
            keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
            keyName = "SystemPaneSuggestionsEnabled";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced
            // ShowSyncProviderNotifications = 0
            keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            keyName = "ShowSyncProviderNotifications";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager
            // SoftLandingEnabled = 0
            keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
            keyName = "SoftLandingEnabled";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager
            // RotatingLockScreenEnabled = 0
            keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
            keyName = "RotatingLockScreenEnabled";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager
            // RotatingLockScreenOverlayEnabled = 0
            keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
            keyName = "RotatingLockScreenOverlayEnabled";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);

            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager
            // SubscribedContent-310093Enabled = 0
            keyPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
            keyName = "SubscribedContent-310093Enabled";
            if (bDisable)
                RegistryKeyManager.ChangeKeyValue(keyPath, keyName, "0");
            else
                RegistryKeyManager.DeleteKeyValue(keyPath, keyName);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ExecuteCommandOnCMD("wmic baseboard get product, manufacturer", false);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            FixBadNetworkSharing();
            TextDialog.Show("Não esqueça de ativar o compartilhamento de arquivos e impressoras nas configurações de rede.");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // TODO: Receber o nome do endereço ipv4 pelo cmd e setar o ip como estático
            // usando os comandos abaixo:
            // FIXO: netsh interface ipv4 set address name="Wi-Fi" static 192.168.15.8 255.255.255.0 192.168.15.1
            // AUTO: netsh interface ipv4 set address name=”Wi-Fi” source=dhcp
            // netsh interface ipv4 set dns name="Wi-Fi" static 8.8.8.8
            // netsh interface ipv4 set dns name="Wi-Fi" static 8.8.4.4 index=2
        }
    }
}
