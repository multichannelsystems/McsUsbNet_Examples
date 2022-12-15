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

        public Form1()
        {
            InitializeComponent();

            list.DeviceArrival += List_DeviceArrivalRemoval;
            list.DeviceRemoval += List_DeviceArrivalRemoval;

            dacq.ErrorEvent += Dacq_ErrorEvent;
            dacq.ChannelDataEvent += Dacq_ChannelDataEvent;

            RefreshDeviceList();
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
                BeginInvoke(new OnError(Dacq_ErrorEvent), msg, action);
            }
            else
            {
                listBox1.Items.Add(msg);
            }
        }

        private int Electrode1 = 0;
        private int Electrode2 = 1;
        private int HeadStage = 3;

        private int channelsInBlock = 0;
        private int threshold = 0;
        private void Dacq_ChannelDataEvent(CMcsUsbDacqNet dacq, int CbHandle, int numFrames)
        {
            List<int[]> data = new List<int[]>();
            for (int i = 0; i < channelsInBlock / 2; i++)
            {
                data.Add(dacq.ChannelBlock.ReadFramesI32(i, 0, threshold, out int frames_ret));
            }

            if (!inHandleData) // skip if last invoke has not finished yet (weak graphic performance)
            {
                // 0 - 59: Elektrode Channels
                // 60 - 67 IFB Analog IFB channels
                // 68 Digital In/Out
                // 69 - 74 Sideband channels
                // 75 - 76 Checksum channels
                BeginInvoke(new HandleDataDelegate(HandleData), data[Electrode1], data[69 + HeadStage]);
            }
        }

        delegate void HandleDataDelegate(int[] data1, int[] data2);

        // Used for weak graphic performance
        private bool inHandleData = false;

        private void HandleData(int[] data1, int[] data2)
        {
            inHandleData = true;

            bool scaled = false;
            if (scaled)
            {
                double[] scaledData = ScaleData(data1, resolutionHS);
                FillSeries(0, scaledData);
            }
            else
            {
                FillSeries(0, data1);
            }
            FillSeries(1, data2);

            inHandleData = false;
        }

        private void FillSeries(int serie, int[] data)
        {
            chart1.Series[serie].Points.Clear();
            int length = data.Length;
            int joined = length / chart1.Width;
            for (int i = 0; i < length; i++)
            {
#if true // workaround for performance 
                double x = i;
                double max = Double.MinValue;
                double min = Double.MaxValue;
                for (int j = 0; j < joined && i < length; j++, i++)
                {
                    if (data[i] > max)
                    {
                        max = data[i];
                    }

                    if (data[i] < min)
                    {
                        min = data[i];
                    }
                }

                if (max != Double.MinValue)
                {
                    chart1.Series[serie].Points.AddXY(x, max);
                    chart1.Series[serie].Points.AddXY(x, min);
                }
#else
                chart1.Series[serie].Points.AddXY(i, data[i]);
#endif
            }
        }

        private void FillSeries(int serie, double[] data)
        {
            chart1.Series[serie].Points.Clear();
            int length = data.Length;
            int joined = length / chart1.Width;
            for (int i = 0; i < length; i++)
            {
#if true // workaround for performance 
                double x = i;
                double max = Double.MinValue;
                double min = Double.MaxValue;
                for (int j = 0; j < joined && i < length; j++, i++)
                {
                    if (data[i] > max)
                    {
                        max = data[i];
                    }

                    if (data[i] < min)
                    {
                        min = data[i];
                    }
                }

                if (max != Double.MinValue)
                {
                    chart1.Series[serie].Points.AddXY(x, max);
                    chart1.Series[serie].Points.AddXY(x, min);
                }
#else
                chart1.Series[serie].Points.AddXY(i, data[i]);
#endif
            }
        }

        private double resolutionHS = 1;
        private double resolutionIFBADC = 1;

        double[] ScaleData(int[] data, double resolution)
        {
            double[] scaled = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                scaled[i] = resolution * (double) data[i];
            }

            return scaled;
        }

        private void btRecordingStart_Click(object sender, EventArgs e)
        {
            CMcsUsbListEntryNet deviceEntry = (CMcsUsbListEntryNet) cbDeviceList.SelectedItem;
            uint status = dacq.Connect(deviceEntry);
            if (status == 0)
            {
                dacq.StopDacq(0); // if software hat not stopped sampling correctly before

                CSCUFunctionNet scu = new CSCUFunctionNet(dacq);
                scu.SetDacqLegacyMode(false);

                int samplerate = 50000;
                dacq.SetSamplerate(samplerate, 0, 0);

                dacq.SetDataMode(DataModeEnumNet.Signed_32bit, 0);

                dacq.GetResolutionPerDigit(0, DacqGroupChannelEnumNet.HeadstageElectrodeGroup, out int resHS, out int resHSUnit);
                resolutionHS = resHS * Math.Pow(10, -resHSUnit);

                dacq.GetResolutionPerDigit(0, DacqGroupChannelEnumNet.InterfaceADCGroup, out int resIFBADC, out int resIFBADCUnit);
                resolutionIFBADC = resIFBADC * Math.Pow(10, -resIFBADCUnit);


                // for MEA2100-Mini it is assumed that only one HS is connected
                dacq.SetNumberOfAnalogChannels(60, 0, 0, 8, 0);

                dacq.EnableDigitalIn(DigitalDatastreamEnableEnumNet.DigitalIn | DigitalDatastreamEnableEnumNet.DigitalOut |
                                     DigitalDatastreamEnableEnumNet.Hs1SidebandLow | DigitalDatastreamEnableEnumNet.Hs1SidebandHigh, 0);
                dacq.EnableChecksum(true, 0);

                // numbers are in 16bit
                dacq.GetChannelLayout(out int analogChannels, out int digitalChannels, out int checksumChannels, out int timestampChannels, out channelsInBlock, 0);

                int queuesize = samplerate;
                threshold = samplerate / 10;
                // channelsInBlock / 2 gives the number of channels in 32bit
                dacq.ChannelBlock.SetSelectedChannels(channelsInBlock / 2, queuesize, threshold, SampleSizeNet.SampleSize32Signed, SampleDstSizeNet.SampleDstSize32, channelsInBlock);

                dacq.ChannelBlock.SetCommonThreshold(threshold);

                dacq.ChannelBlock.SetCheckChecksum((uint) checksumChannels, (uint) timestampChannels);

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
        }

    }
}
