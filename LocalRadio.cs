using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Colossal.Logging;
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


    }

    private static void RadioStationChanged(string newStation)
    {
        log.Info($"Station changed to {newStation}");
        _stationActive = newStation == "Radio Media Control";
    }

    private static void RadioPlayPause()
    {
        log.Info("Hello there");
        _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PLAY_PAUSE);
    }

    private static void RadioNext()
    {
        _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_NEXT_TRACK);
    }

    private static void RadioPrevious()
    {
        _simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PREV_TRACK);
    }

    private static void RadioVolumeChanged(float newVolume)
    {
        log.Info($"Volume changed to {newVolume}");
        if (_stationActive)
        {
            log.Info("Station active");
            SetVolumeTo((int)(newVolume * 100));
        }
    }


    private const int APPCOMMAND_VOLUME_UP = 0xA0000;
    private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
    private const int WM_APPCOMMAND = 0x319;

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private static void SetVolumeTo(int newVolume)
    {
        if (newVolume > currentVolume)
        {
            log.Info("New volume is higher than current volume");
            for (int i = currentVolume; i < newVolume; i++)
            {
                VolumeUp();
            }
        }
        else
        {
            log.Info("New volume is lower than current volume");
            for (int i = currentVolume; i > newVolume; i--)
            {
                VolumeDown();
            }
        }
        currentVolume = newVolume;
    }

    private static void VolumeUp()
    {
        log.Info("Volume up");
        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
    }

    private static void VolumeDown()
    {
        log.Info("Volume down");
        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
    }
}