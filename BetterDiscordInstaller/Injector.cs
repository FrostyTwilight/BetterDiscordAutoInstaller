using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

#nullable enable

namespace BetterDiscordInstaller
{
    public class Injector : AppDomainManager
    {
        private const string BETTER_DISCORD_DOWNLOAD = "https://github.com/BetterDiscord/BetterDiscord/releases/latest/download/betterdiscord.asar";

        [HarmonyPatch(typeof(Process), nameof(Process.Start), typeof(ProcessStartInfo))]
        internal static class ProcessStartPatch
        {

            [HarmonyPrefix]
            private static bool Prefix(ProcessStartInfo startInfo, ref Process __result)
            {
                if (startInfo?.FileName is null)
                    return true;

                string fileName = Path.GetFileName(startInfo.FileName);
                if (string.Equals(fileName, "Discord.exe", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        CheckBetterDiscord(startInfo.FileName);
                    }catch(Exception ex)
                    {
                        MessageBox.Show("Failed to install better discord: " + ex.ToString(), "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                return true;
            }
        }

        private Harmony? _harmony;

        private static void CheckBetterDiscord(string discordPath)
        {
            var root = Path.GetDirectoryName(discordPath);
            var bd_asar = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                "AppData/Roaming/BetterDiscord/data/betterdiscord.asar"));
            
            var js_inject = Path.GetFullPath(Path.Combine(root, "modules/discord_desktop_core-1/discord_desktop_core/index.js"));

            if(File.Exists(bd_asar) && File.Exists(js_inject))
            {
                var text = File.ReadAllText(js_inject);

                if(text.Contains("betterdiscord.asar"))
                {
                    // Better Discord Installed
                    return;
                }
            }

            Debug.WriteLine("Installing Better Discord");

            if(!File.Exists(bd_asar))
            {
                Debug.WriteLine("Downloading Better Disocrd");

                using (var wc = new WebClient())
                {
                    wc.DownloadFile(BETTER_DISCORD_DOWNLOAD, bd_asar);
                }
            }

            Debug.WriteLine("Installing app asar");

            var js = File.ReadAllText(js_inject);
            js = $"require(\"{bd_asar.Replace("\\", "\\\\")}\");\n" + js;

            File.WriteAllText(js_inject, js);
        }

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ApplyProcessStartPatch();
        }

        private void ApplyProcessStartPatch()
        {
            _harmony = new Harmony("com.betterdiscord.process-intercept");

            _harmony.PatchAll(typeof(ProcessStartPatch).Assembly);
        }
    }
}
