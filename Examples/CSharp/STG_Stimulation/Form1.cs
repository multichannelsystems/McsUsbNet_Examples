using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Mcs.Usb;

namespace STG_Stimulation
{
    public partial class Form1 : Form
    {
        private readonly CMcsUsbListNet usblist = new CMcsUsbListNet(DeviceEnumNet.MCS_STG_DEVICE);
        
        private CStg200xDownloadNet device = null;

        public Form1()
        {
            InitializeComponent();
            UpdateDeviceList();

            usblist.DeviceArrival += Usblist_DeviceArrival;
            usblist.DeviceRemoval += Usblist_DeviceArrival;
        }

        private void Usblist_DeviceArrival(CMcsUsbListEntryNet entry)
        {
            UpdateDeviceList();
        }

        private void bt_StgDevice_present_Click(object sender, EventArgs e)
        {
            UpdateDeviceList();
        }

        private void UpdateDeviceList()
        {
            cbDevices.Items.Clear();

            for (uint i = 0; i < usblist.Count; i++)
            {
                var listEntry = usblist.GetUsbListEntry(i);
                cbDevices.Items.Add(listEntry.DeviceName + " / " + listEntry.SerialNumber);
            }
            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
                btConnect.Enabled = true;
            }
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (device != null)
            {
                device.SendStop(1);
                device.Disconnect();
                device.Dispose();
                device = null;
            }

            device = new CStg200xDownloadNet(PollHandler);
            CMcsUsbListEntryNet listEntry = usblist.GetUsbListEntry((uint) cbDevices.SelectedIndex);
            device.Connect(listEntry);

            cbDevices.Enabled = false;
            btConnect.Enabled = false;
            btStart.Enabled = true;
            btStop.Enabled = false;
            btDisconnect.Enabled = true;
        }

        private void btDisconnect_Click(object sender, EventArgs e)
        {
            device.SendStop(1);
            device.Disconnect();
            device.Dispose();
            device = null;

            cbDevices.Enabled = true;
            btConnect.Enabled = true;
            btStart.Enabled = false;
            btStop.Enabled = false;
            btDisconnect.Enabled = false;
        }
       
        private void btStart_Click(object sender, EventArgs e)
        {
            int DACResolution = device.GetDACResolution();

            device.SetCurrentMode();
            device.GetCurrentRangeInNanoAmp(0);
            device.GetCurrentResolutionInNanoAmp(0);

            device.SendSegmentSelect(0, Stg200xSegmentFlagsEnumNet.None);

            // Setup Trigger
            uint triggerInputs = device.GetNumberOfTriggerInputs();
            uint[] channelmap = new uint[triggerInputs];
            uint[] syncoutmap = new uint[triggerInputs];
            uint[] repeat = new uint[triggerInputs];
            for (int i = 0; i < triggerInputs; i++)
            {
                channelmap[i] = 0;
                syncoutmap[i] = 0;
                repeat[i] = 0;
            }
            // Trigger 0
            channelmap[0] = 1; // Channel 1
            syncoutmap[0] = 1; // Syncout 1
            repeat[0] = 0; // forever

            // Trigger 1
            channelmap[1] = 4; // Channel 3 

            device.SetupTrigger(0, channelmap, syncoutmap, repeat);

            // Data for Channel 0
            {
                device.ClearChannelData(0);

                double factor = 1;

                const int l = 1000;
                // without compression
                ushort[] pData = new ushort[l];
                ulong[] tData = new ulong[l];
                for (int i = 0; i < l; i++)
                {
                    // calculate Sin-Wave
                    double sin = factor * (Math.Pow(2, DACResolution - 1) - 1.0) * Math.Sin(2.0 * i * Math.PI / l);

                    // calculate sign
                    pData[i] = sin >= 0 ? (ushort)sin : (ushort)((int)Math.Abs(sin) + (int)Math.Pow(2, DACResolution - 1));

                    tData[i] = 20; // duration in µs
                }
                device.SendChannelData(0, pData, tData);
                /*
                // with compression
                List<ushort> pData = new List<ushort>();
                List<UInt64> tData = new List<UInt64>();
                int j = 0;
                for (int i = 0; i < l; i++)
                {
                    // calculate Sin-Wave
                    double sin = factor * (Math.Pow(2, DACResolution - 1) - 1.0) * 
                        Math.Sin(2.0 * (double)i * Math.PI / (double)l);

                    // calculate sign
                    ushort newval = sin >= 0 ? (ushort)sin : (ushort)((int)Math.Abs(sin) + 
                        (int)Math.Pow(2, DACResolution - 1));

                    // do compression, duration in µs
                    if (j > 0 && pData[j - 1] == newval)
                    {
                        tData[j - 1] += 20;
                    }
                    else
                    {
                        pData.Add(newval);
                        tData.Add(20);
                        j++;
                    }
                }
                device.SendChannelData(0, pData.ToArray(), tData.ToArray());
                */
            }

            // Data for Channel 3
            {
                device.ClearChannelData(2);

                double factor = 0.1;

                const int l = 700;
                // without compression
                ushort[] pData = new ushort[l];
                ulong[] tData = new ulong[l];
                for (int i = 0; i < l; i++)
                {
                    // calculate Sin-Wave
                    double sin = factor * (Math.Pow(2, DACResolution - 1) - 1.0) * Math.Sin(2.0 * i * Math.PI / l);

                    // calculate sign
                    pData[i] = sin >= 0 ? (ushort)sin : (ushort)((int)Math.Abs(sin) + (int)Math.Pow(2, DACResolution - 1));

                    tData[i] = 20; // duration in µs
                }
                device.SendChannelData(2, pData, tData);
            }
            // Data for Sync 0
            {
                device.ClearSyncData(0);
                
                ushort[] pData = new ushort[1000];
                ulong[] tData = new ulong[1000];
                for (int i = 0; i < 1000; i++)
                {
                    pData[i] = (ushort)(i & 1);
                    tData[i] = 20; // duration in µs
                }
                device.SendSyncData(0, pData, tData);
            }

            // Only meaningful for STG400x
            device.SetVoltageMode();

            // Start Trigger 1 and 2
            device.SendStart(1 + 2); // Trigger 1 and 2

            btStart.Enabled = false;
            btStop.Enabled = true;
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            device.SendStop(1 + 2); // Trigger 1 and 2

            btStart.Enabled = true;
            btStop.Enabled = false;
        }

        private void PollHandler(uint status, StgStatusNet stgStatusNet, int[] index_list)
        {
            if (InvokeRequired)// in the context of the poll thread 
            {
                // Change Context
                Invoke(new OnStgPollStatus(PollHandler), status, stgStatusNet, index_list);
            }
            else // in the context of the GUI
            {
                tbPollValue.Text = status.ToString("X8");
            }
        }
    }
}
