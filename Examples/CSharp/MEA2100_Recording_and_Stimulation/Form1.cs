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
                BeginInvoke(new OnStgPollStatus(Stg_Stg200xPollStatusEvent), status, stgStatusNet, index_list);
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
                stg.Connect(deviceEntry);

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
                bool[] sel = Enumerable.Repeat(true, channelsInBlock / 2).ToArray();
                dacq.ChannelBlock.SetSelectedChannels(Enumerable.Repeat(true, channelsInBlock / 2).ToArray(), queuesize, threshold, SampleSizeNet.SampleSize32Signed, SampleDstSizeNet.SampleDstSize32, channelsInBlock);

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
            stg.Disconnect();
        }

        private void btStimulationStart_Click(object sender, EventArgs e)
        {
            int[] amplitude1 = new int[] { 10000, -10000, 0, 20000, -20000, 0 };
            int[] amplitude2 = new int[] { -10000, 10000, 0, -20000, 20000, 0 };
            int[] sideband1 = new int[] { 1 << 8, 3 << 8, 0, 1 << 8, 3 << 8, 0 }; // user defined sideband (use bits > 8)
            int[] StimulusActive1 = new int[] {1, 1, 0, 1, 1, 0};
            int[] sideband2 = new int[] { 4 << 8, 12 << 8, 0, 4 << 8, 12 << 8, 0 }; // user defined sideband (use bits > 8)
            int[] StimulusActive2 = new int[] { 1, 1, 0, 1, 1, 0 };
            ulong[] duration = new ulong[] {1000, 1000, 10000, 1000, 1000, 100000}; // could be different in length and numbers for each amplitude and sideband, it only needs to be equal in length for the individual amplitude and sideband 

            // Defining the Elektrodes. Choose if you want use automatic or manual mode for the stimulation electrode here:
            ElectrodeModeEnumNet electrodeMode = ElectrodeModeEnumNet.emAutomatic;
            //ElectrodeModeEnumNet electrodeMode = ElectrodeModeEnumNet.emManual;
            for (int i = 0; i < 60; i++)
            {
                stg.SetElectrodeMode((uint)HeadStage, (uint)i, i == Electrode1 || i == Electrode2 ? electrodeMode : ElectrodeModeEnumNet.emAutomatic);
                stg.SetElectrodeEnable((uint)HeadStage, (uint)i, 0, i == Electrode1 || i == Electrode2 ? true : false);
                stg.SetElectrodeDacMux((uint)HeadStage, (uint)i, 0, 
                    i == Electrode1 ? ElectrodeDacMuxEnumNet.Stg1 : 
                    ( i == Electrode2 ? ElectrodeDacMuxEnumNet.Stg2 : ElectrodeDacMuxEnumNet.Ground));
                stg.SetEnableAmplifierProtectionSwitch((uint)HeadStage, (uint)i, false); // Enable the switch if you want to protect the amplifier from an overload
                stg.SetBlankingEnable((uint)HeadStage, (uint)i, false); // Choose if you want Filter blanking during stimulation for an electrode
            }

            stg.SetVoltageMode(0);

            // bit0 (blanking switch) activation duration prolongation in µs
            uint Bit0Time = 40;

            // bit3 (stimulation switch) activation duration prolongation in µs
            uint Bit3Time = 800;

            // bit4 (stimulus selection switch) activation duration prolongation in µs
            uint Bit4Time = 40;

            // STG Channel 1:
            stg.PrepareAndSendData(2 * (uint)HeadStage + 0, amplitude1, duration, STG_DestinationEnumNet.channeldata_voltage);

            if (electrodeMode == ElectrodeModeEnumNet.emManual)
            {
                //pure user defined sideband:
                stg.PrepareAndSendData(2 * (uint)HeadStage + 0, sideband1, duration, STG_DestinationEnumNet.syncoutdata);
            }
            else
            {
                //alternative: adding sideband data for automatic stimulation mode:
                CStimulusFunctionNet.SidebandData SidebandData1 = stg.Stimulus.CreateSideband(StimulusActive1, sideband1, duration, Bit0Time, Bit3Time, Bit4Time);
                stg.PrepareAndSendData(2 * (uint) HeadStage + 0, SidebandData1.Sideband, SidebandData1.Duration, STG_DestinationEnumNet.syncoutdata);
            }

            // STG Channel 2:
            stg.PrepareAndSendData(2 * (uint)HeadStage + 1, amplitude2, duration, STG_DestinationEnumNet.channeldata_voltage);

            if (electrodeMode == ElectrodeModeEnumNet.emManual)
            {
                //pure user defined sideband:
                stg.PrepareAndSendData(2 * (uint)HeadStage + 1, sideband2, duration, STG_DestinationEnumNet.syncoutdata);
            }
            else
            {
                //alternative: adding sideband data for automatic stimulation mode:
                CStimulusFunctionNet.SidebandData SidebandData2 = stg.Stimulus.CreateSideband(StimulusActive2, sideband2, duration, Bit0Time, Bit3Time, Bit4Time);
                stg.PrepareAndSendData(2 * (uint) HeadStage + 1, SidebandData2.Sideband, SidebandData2.Duration, STG_DestinationEnumNet.syncoutdata);
            }

            stg.SetupTrigger(0, new uint[]{255}, new uint[]{255}, new uint[]{10});

            stg.SendStart(1);
        }

        private void btStimulatinStop_Click(object sender, EventArgs e)
        {
            stg.SendStop(1);
        }
    }
}
