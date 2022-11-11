using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mcs.Usb;

namespace MEA2100_Recording_and_Stimulation
{
    public partial class Form1 : Form
    {
        private CMcsUsbListNet list = new CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB);

        private CMeaUSBDeviceNet dacq = new CMeaUSBDeviceNet();
        private CStg200xDownloadNet stg = new CStg200xDownloadNet();

        public Form1()
        {
            InitializeComponent();

            list.DeviceArrival += List_DeviceArrivalRemoval;
            list.DeviceRemoval += List_DeviceArrivalRemoval;

            dacq.ErrorEvent += Dacq_ErrorEvent;
            dacq.ChannelDataEvent += Dacq_ChannelDataEvent;

            stg.Stg200xPollStatusEvent += Stg_Stg200xPollStatusEvent;

            RefreshDeviceList();
        }

        private void Stg_Stg200xPollStatusEvent(uint status, StgStatusNet stgStatusNet, int[] index_list)
        {
            if (InvokeRequired)
            {
                Invoke(new OnStgPollStatus(Stg_Stg200xPollStatusEvent), status, stgStatusNet, index_list);
            }
            else
            {
                listBox1.Items.Add("STG: " + status.ToString("X8"));
            }
        }

        private void RefreshDeviceList()
        {
            cbDeviceList.Items.Clear();
            cbDeviceList.Items.AddRange(list.GetUsbListEntries());
            if (cbDeviceList.Items.Count > 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private void List_DeviceArrivalRemoval(CMcsUsbListEntryNet entry)
        {
            RefreshDeviceList();
        }

        private void Dacq_ErrorEvent(string msg, int action)
        {
            if (InvokeRequired)
            {
                Invoke(new OnError(Dacq_ErrorEvent), msg, action);
            }
            else
            {
                listBox1.Items.Add(msg);
            }
        }

        private int channelsInBlock = 0;
        private int threshold = 0;
        private void Dacq_ChannelDataEvent(CMcsUsbDacqNet dacq, int CbHandle, int numFrames)
        {
            List<int[]> data = new List<int[]>();
            for (int i = 0; i < channelsInBlock / 2; i++)
            {
                data.Add(dacq.ChannelBlock_ReadFramesI32(i, threshold, out int frames_ret));
            }

            BeginInvoke(new HandleDataDelegate(HandleData), data[60], data[70]);
        }

        delegate void HandleDataDelegate(int[] data1, int[] data2);

        private void HandleData(int[] data1, int[] data2)
        {
            FillSeried(0, data1);
            FillSeried(1, data2);
        }

        private void FillSeried(int serie, int[] data)
        {
            chart1.Series[serie].Points.Clear();
            for (int i = 0; i < data.Length; i++)
            {
                chart1.Series[serie].Points.AddXY(i, data[i]);
            }
        }

        private void btRecordingStart_Click(object sender, EventArgs e)
        {
            CMcsUsbListEntryNet deviceEntry = (CMcsUsbListEntryNet) cbDeviceList.SelectedItem;
            uint status = dacq.Connect(deviceEntry);
            if (status == 0)
            {
                stg.Connect(deviceEntry);

                dacq.StopDacq(0); // if software hat not stopped sampling correctly before

                int samplerate = 50000;
                dacq.SetSamplerate(samplerate, 0, 0);

                dacq.SetDataMode(DataModeEnumNet.Signed_32bit, 0);
                dacq.SetNumberOfAnalogChannels(60, 0, 0, 8, 0);

                dacq.EnableDigitalIn(DigitalDatastreamEnableEnumNet.DigitalIn | DigitalDatastreamEnableEnumNet.DigitalOut |
                                     DigitalDatastreamEnableEnumNet.Hs1SidebandLow | DigitalDatastreamEnableEnumNet.Hs1SidebandHigh, 0);
                dacq.EnableChecksum(true, 0);

                dacq.GetChannelLayout(out int analogChannels, out int digitalChannels, out int checksumChannels, out int timestampChannels, out channelsInBlock, 0);

                int queuesize = samplerate;
                threshold = samplerate / 10;
                dacq.SetSelectedChannels(channelsInBlock / 2, queuesize, threshold, SampleSizeNet.SampleSize32Signed, channelsInBlock);

                dacq.ChannelBlock_SetCommonThreshold(threshold);

                dacq.ChannelBlock_SetCheckChecksum((uint) checksumChannels, (uint) timestampChannels);

                dacq.StartDacq();
            }
            else
            {
                MessageBox.Show("Connection failed: " + CMcsUsbNet.GetErrorText(status));
            }
        }

        private void btRecordingStop_Click(object sender, EventArgs e)
        {
            dacq.StopDacq(0);
            dacq.Disconnect();
            stg.Disconnect();
        }

        private void btStimulationStart_Click(object sender, EventArgs e)
        {
            int[] amplitude = new int[]{10000, -10000, 0, 20000, -20000, 0};
            int [] sideband = new int[] { 1, 3, 0, 1, 3, 0 };
            ulong[] duration = new ulong[] {1000, 1000, 1000, 1000, 1000, 1000};
            
            stg.SetVoltageMode(0);

            stg.PrepareAndSendData(2, amplitude, duration, STG_DestinationEnumNet.channeldata_voltage);
            stg.PrepareAndSendData(2, sideband, duration, STG_DestinationEnumNet.syncoutdata);

            stg.SetupTrigger(0, new uint[]{255}, new uint[]{255}, new uint[]{1});

            stg.SendStart(1);
        }

        private void btStimulatinStop_Click(object sender, EventArgs e)
        {
            stg.SendStop(1);
        }
    }
}
