using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Colossal.Logging;
using Game.Audio;
using WindowsInput;

namespace RadioMediaControl;

public class LocalRadio
{
    private static bool _stationActive = false;
    private static InputSimulator _simulator;
    private static int currentVolume = 30;
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


    }

    private static void RadioStationChanged(string newStation)
    {
        log.Info($"Station changed to {newStation}");
        _stationActive = newStation == "Radio Media Control";
        if (_stationActive)
        {
            log.Info("Setting radio volume to 0.3");
            AudioManager.instance.radioVolume = 0.3f;
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

    private static void RadioVolumeChanged(float newValue)
    {
        int newVolume = (int) (newValue * 100);
        log.Info($"Volume changed to {newVolume}");
        if (_stationActive)
        {
            if (newVolume == 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    VolumeDown();
                }
            }
            if (newVolume == 100)
            {
                for (int i = 0; i < 50; i++)
                {
                    VolumeUp();
                }
            }

            int steps = Math.Abs(newVolume - currentVolume) / 2;
            log.Info("Changing from " + currentVolume + " to " + newVolume + " in " + steps + " steps");
            if (newVolume > currentVolume)
            {
                for (int i = 0; i < steps; i++)
                {
                    VolumeUp();
                }
            }
            else if (newVolume < currentVolume)
            {
                for (int i = 0; i < steps; i++)
                {
                    VolumeDown();
                }
            }
            currentVolume = newVolume;
        }
    }


    private const int APPCOMMAND_VOLUME_UP = 0xA0000;
    private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
    private const int WM_APPCOMMAND = 0x319;

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private static void VolumeUp()
    {
        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
    }

    private static void VolumeDown()
    {
        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
    }
}