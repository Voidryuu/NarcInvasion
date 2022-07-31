using System;
using System.Collections.Generic;
using System.Text;

namespace NarcInvasion
{
    class Sound
    {

        public static void Play(WMPLib.WindowsMediaPlayer soundPlayer, string soundPath)
        {
            soundPlayer.URL = soundPath;
        }

        public static void Stop(WMPLib.WindowsMediaPlayer soundPlayer)
        {
            soundPlayer.controls.stop();
        }
    }
}
