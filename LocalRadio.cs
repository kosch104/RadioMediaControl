using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Colossal.Logging;
using Game.Audio;
using Game.SceneFlow;
using WindowsInput;

namespace RadioMediaControl;

public class LocalRadio
{
    private static bool _stationActive = false;
    private static InputSimulator _simulator;
    private static int currentVolume = 0;
    private static readonly ILog log = LogManager.GetLogger($"{nameof(RadioMediaControl)}").SetShowsErrorsInUI(false);

    internal static void OnLoad()
    {
        _simulator = new InputSimulator();
        ExtendedRadio.ExtendedRadio.OnRadioPaused += RadioPlayPause;
        ExtendedRadio.ExtendedRadio.OnRadioUnPaused += RadioPlayPause;
        ExtendedRadio.ExtendedRadio.OnRadioPreviousSong += RadioPrevious;
        ExtendedRadio.ExtendedRadio.OnRadioNextSong += RadioNext;
        ExtendedRadio.ExtendedRadio.OnRadioVolumeChanged += RadioVolumeChanged;
        ExtendedRadio.ExtendedRadio.OnRadioStationChanged += RadioStationChanged;

        currentVolume = 30;
    }

    private static void RadioStationChanged(string newStation)
    {
        log.Info($"Station changed to {newStation}");
        _stationActive = newStation == "Radio Media Control";
        if (_stationActive)
        {
            for (int i = 0; i < 60; i++)
            {
                VolumeDown();
            }

            float newVolume = currentVolume / 100f;
            currentVolume = 0;
            log.Info("Setting radio volume to " + newVolume);
            AudioManager.instance.radioVolume = newVolume;
        }
    }

    private static void RadioPlayPause()
    {
        if (_stationActive)
            _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PLAY_PAUSE);
    }

    private static void RadioNext()
    {
        if (_stationActive)
            _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_NEXT_TRACK);
    }

    private static void RadioPrevious()
    {
        if (_stationActive)
            _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PREV_TRACK);
    }

    private static object lockObject = new ();

    private static void RadioVolumeChanged(float newValue)
    {
        lock (lockObject)
        {
            int newVolume = (int) (newValue * 100);
            log.Info($"Volume changed to {newVolume}");
            if (_stationActive)
            {
                if (newVolume > currentVolume)
                {
                    int steps = (newVolume - currentVolume) / 2;
                    for (int i = 0; i < steps; i++)
                    {
                        VolumeUp();
                        currentVolume += 2;
                    }
                }
                else if (newVolume < currentVolume)
                {
                    int steps = (currentVolume - newVolume) / 2;
                    for (int i = 0; i < steps; i++)
                    {
                        VolumeDown();
                        currentVolume -= 2;
                    }
                }
            }
        }
    }


    /*private const int APPCOMMAND_VOLUME_UP = 0xA0000;
    private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
    private const int WM_APPCOMMAND = 0x319;

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);*/

    private static void VolumeUp()
    {
        _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VOLUME_UP);
        //SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
    }

    private static void VolumeDown()
    {
        _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VOLUME_DOWN);
        //SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
    }
}