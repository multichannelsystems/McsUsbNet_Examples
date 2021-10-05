using Mcs.Usb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;


namespace Feedback
{
    public partial class Form1 : Form
    {
        const int Channels = 32;
        const int AnalogChannels = 8;
        const int Checksum = 2;

        private int TotalChannels = 0;

        int Samplerate = 50000;
        private int Threshold = 10;

        Timer timer = new Timer();

        CMcsUsbListNet UsbDeviceList = new CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB);
        CMeaUSBDeviceNet mea = new CMeaUSBDeviceNet();

        private int dig_count = 0;
        private bool dig_data = true;

        private int digitallength = 0;

        public Form1()
        {
            InitializeComponent();

            timer.Elapsed += Timer_Elapsed;

            mea.ErrorEvent += Mea_ErrorEvent;
            mea.ChannelDataEvent += Mea_ChannelDataEvent;

            UsbDeviceList.DeviceArrival += UsbDeviceList_DeviceArrivalRemoval;
            UsbDeviceList.DeviceRemoval += UsbDeviceList_DeviceArrivalRemoval;

            btStop.Enabled = false;

            UpdateDeviceList();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CMeaDigitalDataFunctionNet dig = new CMeaDigitalDataFunctionNet(mea);

            if (dig_count > 0)
            {
                dig_count--;
                if (dig_count == 0)
                {
                    dig.SetDigitalData(0, true);
                }
            }
        }

        private void UsbDeviceList_DeviceArrivalRemoval(CMcsUsbListEntryNet entry)
        {
            UpdateDeviceList();
        }

        private void UpdateDeviceList()
        {
            cbDeviceList.Items.Clear();
            cbDeviceList.Items.AddRange(UsbDeviceList.GetUsbListEntries());
            if (cbDeviceList.Items.Count > 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private void Mea_ErrorEvent(string msg, int action)
        {
            if (InvokeRequired)
            {
                Invoke(new OnError(Mea_ErrorEvent), msg, action);
            }
            else
            {
                listBox2.Items.Add(msg);
            }
        }

        private void Mea_ChannelDataEvent(CMcsUsbDacqNet dacq, int CbHandle, int numFrames)
        {
            CMeaDigitalDataFunctionNet dig = new CMeaDigitalDataFunctionNet(dacq);

            int[] data = mea.ChannelBlock_ReadFramesI32(0, Threshold, out int frames_read);
            for (int i = Channels + AnalogChannels; i < data.Length; i += TotalChannels)
            {
                int z = data[i];
                if (z == 1)
                {
                    if (dig_data)
                    {
                        dig.SetDigitalData(0, false);
                        dig_count = 10;
                        dig_data = false;
                        digitallength = 0;
                    }

                    digitallength++;
                }
                else
                {
                    if (!dig_data)
                    {
                        dig_data = true;
                        BeginInvoke(new InfoDataAction(InfoData), digitallength);
                    }
                }
            }
        }

        delegate void InfoDataAction(int length);

        private double infolength = 0;
        private double ninfolength = 0;
        private double infolengthmax = 1;
        private double infolengthmin = 10000;

        private const int HISTO_LENGTH = 100;
        private double[] infoHistogram = new double[HISTO_LENGTH];

        void ClearInfo()
        {
            infolength = 0;
            ninfolength = 0;
            infolengthmax = 1;
            infolengthmin = 10000;

            infoHistogram = new double[HISTO_LENGTH];
        }

        void InfoData(int length)
        {
            double index = length / ((double)Samplerate / 1000.0);
            infolength += index;
            ninfolength += 1;
            if (index > infolengthmax) infolengthmax = index;
            if (index < infolengthmin) infolengthmin = index;
            listBox1.Items.Clear();
            listBox1.Items.Add(index.ToString());
            listBox1.Items.Add((infolength / ninfolength).ToString());
            listBox1.Items.Add(infolengthmax.ToString());
            listBox1.Items.Add(infolengthmin.ToString());

            int index_i = (int)Math.Round(index);
            infoHistogram[index_i < HISTO_LENGTH ? index_i : HISTO_LENGTH - 1] += 1;

            chart1.Series[0].Points.Clear();
            int max;
            for (max = HISTO_LENGTH - 1; max >= 0; max--)
            {
                if (infoHistogram[max] > 0)
                {
                    break;
                }
            }

            max = (max * 10 + 10)/ 10;
            for (int i = 0; i < max; i++)
            {
                chart1.Series[0].Points.Add(infoHistogram[i]);
            }
        }
        private void btStart_Click(object sender, EventArgs e)
        {
            if (mea.Connect((CMcsUsbListEntryNet) cbDeviceList.SelectedItem) == 0)
            {
                mea.SetDataMode(DataModeEnumNet.Signed_32bit, 0);
                if (mea.GetDeviceId().IdProduct == ProductIdEnumNet.W2100)
                {
                    Samplerate = 20000;
                }

                mea.SetNumberOfAnalogChannels(Channels, 0, 0, AnalogChannels, 0); // Read raw data

                try
                {
                    mea.SetSamplerate(Samplerate, 1, 0);
                }
                catch (CUsbExceptionNet)
                {
                    Samplerate = mea.GetSamplerate(0);
                }

                mea.EnableDigitalIn(DigitalDatastreamEnableEnumNet.RegisterLow | DigitalDatastreamEnableEnumNet.RegisterLow, 0);

                mea.SetDigitalSource(DigitalTargetEnumNet.Digout, 0, SCUDigitalSourceEnumNet.DigitalData, 0);

                mea.EnableChecksum(true, 0);
                int ChannelsInBlock = mea.GetChannelsInBlock(0);

                mea.GetChannelLayout(out int analogChannels, out int digitalChannels, out int checksumChannels, out int timestampChannels, out int channelsInBlock, 0);

                TotalChannels = channelsInBlock / 2;
                mea.SetSelectedData(TotalChannels, Samplerate * 10, Threshold, SampleSizeNet.SampleSize32Signed, ChannelsInBlock);

                mea.ChannelBlock_SetCheckChecksum((uint)checksumChannels, (uint)timestampChannels);

                CMeaDigitalDataFunctionNet dig = new CMeaDigitalDataFunctionNet(mea);
                dig.SetDigitalData(0, false);
                dig_count = 10;
                dig_data = true;

                timer.Interval = 9;
                timer.Enabled = true;

                ClearInfo();

                //mea.StartDacq(-1, 32, 64, 128, 0);
                mea.StartDacq(-1, 128, 256, 16, 0);

                btStop.Enabled = true;
                btStart.Enabled = false;
            }
            else
            {
                MessageBox.Show("Could not connect");
            }
        }

        private void btStop_Click(object sender, EventArgs e)
        {

            mea.StopDacq();

            mea.Disconnect();
            btStart.Enabled = true;
            btStop.Enabled = false;

        }
    }
}
