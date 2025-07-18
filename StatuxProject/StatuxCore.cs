using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

class StatuxCore
{
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);
    private static EventHandler _handler;

    enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    static readonly string dataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "statux-data");
    
    static readonly string logPath = Path.Combine(dataFolder, "statuxCoreLog.dat");
    static int sessionTime = 0;
    const int activeThreshold = 60;
    static bool hasSaved = false; 

    static void Main()
    {
        Directory.CreateDirectory(dataFolder);
        
        bool isFirstRun = !File.Exists(logPath);
        
        if (isFirstRun)
        {
            File.WriteAllText(logPath,
                $"STATUX CORE HANDLE - INITIALISED {DateTime.Now}\n" +
                $"Written by Ashar - 18 July 2025\n\n" +
                "xox\n");
            sessionTime = 1; 
            SaveSessionData();
        }

        _handler = new EventHandler(HandleConsoleClose);
        SetConsoleCtrlHandler(_handler, true);
        AppDomain.CurrentDomain.ProcessExit += (s, e) => SaveSessionData();

        Console.WriteLine($"Logging to: {logPath}");

        // Main loop
        while (true)
        {
            if (IsUserActive(activeThreshold))
            {
                sessionTime++;
            }
            Thread.Sleep(1000);
        }
    }

    static bool HandleConsoleClose(CtrlType sig)
    {
        if (!hasSaved) // Only save if not already saved
        {
            Console.WriteLine($"Shutting down due to: {sig}");
            SaveSessionData();
        }
        Environment.Exit(0);
        return true;
    }

    static void SaveSessionData()
    {
        if (hasSaved) return; // Prevent duplicate saves
        hasSaved = true;

        try
        {
            string[] lines = Array.Empty<string>();
            if (File.Exists(logPath))
            {
                lines = File.ReadAllLines(logPath);
            }

            int lastIndex = 0;
            bool foundData = false;
            
            // Search backwards for last data line
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Contains(","))
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out lastIndex))
                    {
                        foundData = true;
                        break;
                    }
                }
            }

            // Only append if we have meaningful activity
            if (sessionTime > 0 || !foundData)
            {
                File.AppendAllText(logPath, $"{lastIndex + 1},{sessionTime}\n");
                Console.WriteLine($"SAVED: Session #{lastIndex + 1}, {sessionTime}s");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BACKUP SAVE: {ex.Message}");
            File.AppendAllText(Path.Combine(dataFolder, "statux_crashlog.txt"), 
                $"[{DateTime.Now}] {sessionTime}s\n");
        }
    }

    static bool IsUserActive(int thresholdSeconds)
    {
        LASTINPUTINFO lastInput = new LASTINPUTINFO();
        lastInput.cbSize = (uint)Marshal.SizeOf(lastInput);
        GetLastInputInfo(ref lastInput);

        uint idleTime = (uint)Environment.TickCount - lastInput.dwTime;
        return (idleTime / 1000) < thresholdSeconds;
    }
}