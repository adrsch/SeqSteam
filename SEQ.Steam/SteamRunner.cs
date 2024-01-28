using Steamworks;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using Stride.UI.Controls;

// TODO quitting steam may break things
namespace SEQ.Steam
{
    public class SteamRunner : ITouchModule
    {
        public static SteamRunner Inst;
        int RetryTime = 1000;
        int RetryTimeMax = 30000;
        bool isRunning;
        public bool Initialized => InSteam;
        public static bool InSteam => Inst != null && Inst.isRunning;
        public bool TouchEnabled => InSteam && SteamUtils.IsSteamRunningOnSteamDeck();

        public void Init()
        {
            G.S.Script.AddTask(Execute);
        }

        public int Priority => -10;

        public async Task Execute()
        {
            Inst = this;
            while (!TryInit())
            {
                await Task.Delay(RetryTime);
                RetryTime *= 2;
                RetryTime = Math.Min(RetryTime, RetryTimeMax);
            }
            isRunning = true;
            //OnLoadAction?.Invoke();
            //OnLoadAction = null;
            while (isRunning)
            {
                await Systems.Script.NextFrame();
                SteamAPI.RunCallbacks();
            }
        }

        public void Exit()
        {
            if (isRunning)
            {
                try
                {
                    SteamAPI.Shutdown();
                }
                catch (System.Exception e)
                {
                    // Something went wrong - it's one of these:
                    //
                    //     Steam is closed?
                    //     Can't find steam_api dll?
                    //     Don't have permission to play app?
                    //
                    Logger.Log(Channel.Modules, LogPriority.Error, e.ToString());
                }

                isRunning = false;
            }

        }

        bool TryInit()
        {
            if (SteamAPI.Init())
            {
                return true;
            }
            else
            {
                Logger.Log(Channel.Modules, LogPriority.Info, "Cannot find steam client. Either run through steam or put steam_appid.txt in the exe directory.");
                return false;
            }
        }

        public void OpenKeyboard(string name, bool multiline)
        {
            SteamUtils.ShowGamepadTextInput(
                EGamepadTextInputMode.k_EGamepadTextInputModeNormal,
                multiline 
                    ? EGamepadTextInputLineMode.k_EGamepadTextInputLineModeMultipleLines
                    : EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, 
                name, 512, "");
        }
    }
}
