﻿using System;
using LabNation.DeviceInterface.Devices;
using LabNation.Common;
using LabNation.DeviceInterface.DataSources;
using System.Threading;
using System.IO;
using System.Linq;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace HackerSpecialTest
{
    class MainClass
    {
        /// <summary>
        /// The DeviceManager detects device connections
        /// </summary>
        static HackerSpecialManager hsManager;
        static HackerSpecial device;
        static bool running = true;

        [STAThread]
        static void Main(string[] args)
        {
            //Open logger on console window
            FileLogger consoleLog = new FileLogger(new StreamWriter(Console.OpenStandardOutput()), LogLevel.INFO);

            Logger.Info("LabNation Hacker Special Demo");
            Logger.Info("---------------------------------");

            //Set up device manager with a device connection handler (see below)
            hsManager = new HackerSpecialManager(connectHandler);
            hsManager.Start();

            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            while (running)
            {
#if WINDOWS
                Application.DoEvents();
#endif
                Thread.Sleep(100);

                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey(true);
                    HandleKey(cki);

                    if (cki.Key == ConsoleKey.R)
                    {
                        if (device != null)
                        {
                            Logger.Info("Reading ROM");
                            for (uint i = 0; i < device.FpgaRom.Registers.Count; i++)
                            {
                                byte val = device.FpgaRom[i].Read().GetByte();
                                Logger.Info("Reg {0} = 0x{1:X}", i, val);
                            }
                        }
                    }
                }
            }
            Logger.Info("Stopping device manager");
            hsManager.Stop();
            Logger.Info("Stopping Logger");
            consoleLog.Stop();
        }

        static void connectHandler(IDevice dev, bool connected)
        {
            //Only accept devices of the IScope type (i.e. not IWaveGenerator)
            //and block out the fallback device (dummy scope)
            if (connected && dev is HackerSpecial && !(dev is DummyScope))
            {
                Logger.Info("Device connected of type " + dev.GetType().Name + " with serial " + dev.Serial);
                device = (HackerSpecial)dev;
            }
            else
            {
                device = null;
            }
        }

        static void HandleKey(ConsoleKeyInfo k)
        {
            switch (k.Key)
            {
                case ConsoleKey.Q:
                case ConsoleKey.X:
                case ConsoleKey.Escape:
                    running = false;
                    break;

            }
        }

    }
}