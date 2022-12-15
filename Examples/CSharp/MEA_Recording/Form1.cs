using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Mcs.Usb;

namespace MEA_Recording
{
    public partial class Form1 : Form
    {

        private readonly CMcsUsbListNet usblist = new CMcsUsbListNet(DeviceEnumNet.MCS_MEA_DEVICE);
        private CMeaDeviceNet device;

        enum DataSelectionMethod
        {
            SetSelectedChannels,
            SetSelectedData,
        }
        // If setSelectedChannels or SetSelectedData is used
        private DataSelectionMethod dataSelectionMethod = DataSelectionMethod.SetSelectedChannels;

        // If true, data is retrieved by (timer-controlled) polling.
        // If false, callbacks are used to retrieve the data
        private bool usePollForData;

        // Only for DataSelectionMethod dataSelectionMethod == DataSelectionMethod.selectedChannels;
        // If true, all handles are checked for the threshold
        // If false, each handle has an individual threshold
        private bool useCommonThreshold;

        // This flag should be active for W2100 devices
        private bool useWireless;

        //private DataModeEnumNet dataMode = DataModeEnumNet.Unsigned_16bit;
        private DataModeEnumNet dataMode = DataModeEnumNet.Signed_32bit;

        // The overall number of channels (including data, digital, checksum, timestamp) in one sample. 
        // Checksum and timestamp are not available for MC_Card
        // With the MC_Card you lose one analog channel, when using the digital channel 
        private int callbackThreshold;

        // "Poll For Data" only
        private Timer timer = new Timer();
        private bool LastData = false;

        // useChannelData == false only
        private int mChannels;

        // for drawing
        private double[] mData;

        Dictionary<DataModeEnumNet, SampleSizeNet> DataModeToSampleSizeDict = new Dictionary<DataModeEnumNet, SampleSizeNet>();
        Dictionary<DataModeEnumNet, SampleDstSizeNet> DataModeToSampleDstSizeDict = new Dictionary<DataModeEnumNet, SampleDstSizeNet>();

        public Form1()
        {
            DataModeToSampleSizeDict.Add(DataModeEnumNet.Unsigned_16bit, SampleSizeNet.SampleSize16Unsigned);
            DataModeToSampleSizeDict.Add(DataModeEnumNet.Signed_32bit, SampleSizeNet.SampleSize32Signed);

            DataModeToSampleDstSizeDict.Add(DataModeEnumNet.Unsigned_16bit, SampleDstSizeNet.SampleDstSize16);
            DataModeToSampleDstSizeDict.Add(DataModeEnumNet.Signed_32bit, SampleDstSizeNet.SampleDstSize32);

            InitializeComponent();

            comboBoxDataSelectionMethod.Items.Add(DataSelectionMethod.SetSelectedChannels);
            comboBoxDataSelectionMethod.Items.Add(DataSelectionMethod.SetSelectedData);

            comboBoxDataMode.Items.Add(DataModeEnumNet.Unsigned_16bit);
            comboBoxDataMode.Items.Add(DataModeEnumNet.Signed_32bit);

            timer.Tick += Timer_Tick;
            timer.Interval = 20;

            comboBoxDataSelectionMethod.SelectedItem = dataSelectionMethod;
            checkBoxPollForData.Checked = usePollForData;
            checkBoxCommonThreshold.Checked = useCommonThreshold;
            comboBoxDataMode.SelectedItem = dataMode;
        }

        private void BtMeaDevicePresentClick(object sender, EventArgs e)
        {
            cbDevices.Items.Clear();
            for (uint i = 0; i < usblist.Count; i++)
            {
                cbDevices.Items.Add(usblist.GetUsbListEntry(i).DeviceName + " / " + usblist.GetUsbListEntry(i).SerialNumber);
            }
            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
            }

        }

        private void CbDevicesSelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxSettings.Enabled = false;

            if (device != null)
            {
                device.StopDacq();

                device.Disconnect();
                device.Dispose();

                device = null;
            }

            btStart.Enabled = cbDevices.SelectedIndex >= 0;
            btStop.Enabled = false;

            var entry = usblist.GetUsbListEntry((uint)cbDevices.SelectedIndex);
            /* choose one of the following constructors:
             * The first one uses the OnNewData callback and gives you a reference to the raw multiplexed data.
             * This could be used without further initialization
             * The second one uses the more advanced callback which gives you the data for each channel in a callback but needs initialization
             * for buffers and the selected channels
             */
            if (usePollForData)
            {
                device = new CMeaDeviceNet(entry.DeviceId.BusType);
            }
            else
            {
                device = new CMeaDeviceNet(entry.DeviceId.BusType, OnChannelData, OnError);
            }

            device.Connect(entry);
            useWireless = entry.DeviceId.IdProduct == ProductIdEnumNet.W2100;
            bool isMCCard = entry.DeviceId.IdProduct == ProductIdEnumNet.MC_Card;

            if (useWireless)
            {
                CW2100_FunctionNet func = new CW2100_FunctionNet(device);
                HeadStageIDTypeState headstagestate = func.GetSelectedHeadstageState(0); // information about currently selected headstage
                HeadStageIDType[] headstages = func.GetAvailableHeadstages(30); // list of available headstage in range

                if (headstagestate.IdType.ID == 0xFFFF) // if no headstage is currently selected
                {
                    if (headstages.Length > 0) // if at least one headstage is available
                    {
                        func.SelectHeadstage(headstages[0].ID, 0); // then selected the first one
                    }
                }
                // func.DeselectHeadstage(0); // use this to deselect a selected headstage
            }

            device.SendStopDacq(); // only to be sure

            tbDeviceInfo.Text = "";
            tbDeviceInfo.Text += "Serial number: " + device.SerialNumber + Environment.NewLine;

            device.SetDataMode(dataMode, 0);

            device.HWInfo().GetNumberOfHWADCChannels(out int hwchannels);
            tbDeviceInfo.Text += @"Number of Hardware channels: " + hwchannels.ToString("D") + Environment.NewLine;

            if (hwchannels == 0)
            {
                hwchannels = 64;
            }

            // configure MeaDevice: MC_Card or Usb
            device.SetNumberOfChannels(hwchannels);

            const int Samplingrate = 50000; // MC_Card does not support all settings, please see MC_Rack for valid settings
            device.SetSamplerate(Samplingrate, 1, 0);
            
            int miliGain = device.GetGain();

            if (miliGain > 0)
            {
                var voltageRanges = device.HWInfo().GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(miliGain);
                for (int i = 0; i < voltageRanges.Count; i++)
                {
                    tbDeviceInfo.Text += @"(" + i.ToString("D") + @") " + voltageRanges[i].VoltageRangeDisplayStringMilliVolt + Environment.NewLine;
                }
            }

            // Set the range according to the index (only valid for MC_Card)
            device.SetVoltageRangeByIndex(0, 0); // for dataMode == DataModeEnumNet.Signed_32bit only one index exists

            device.EnableDigitalIn(true, 0);
 
            // Checksum not supported by MC_Card
            if (!isMCCard)
            {
                device.EnableChecksum(true, 0);
            }

            // Get the layout to know how the data look like that you receive
            device.GetChannelLayout(out int ana, out int digi, out int che, out int tim, out int block, 0);

            // or
            block = device.GetChannelsInBlock(0);

            if (dataMode == DataModeEnumNet.Unsigned_16bit)
            {
                mChannels = block; // for this case, if all channels are selected
            }
            else // (dataMode == DataModeEnumNet.Signed_32bit)
            {
                mChannels = block / 2; // for this case, if all channels are selected
            }

            // set the channel combo box with the channels
            SetChannelCombo(mChannels);

            callbackThreshold = Samplingrate / 10; // good choice for MC_Card and others

            // queue size and threshold should be selected carefully
            if (dataSelectionMethod == DataSelectionMethod.SetSelectedData)
            {
                device.ChannelBlock.SetSelectedData(mChannels, 10 * callbackThreshold, callbackThreshold, DataModeToSampleSizeDict[dataMode], DataModeToSampleDstSizeDict[dataMode], block);
            }
            else // (dataSelectionMethod == DataSelectionMethod.SetSelectedChannels)
            {
                device.ChannelBlock.SetSelectedChannels(mChannels, 10 * callbackThreshold, callbackThreshold, DataModeToSampleSizeDict[dataMode], DataModeToSampleDstSizeDict[dataMode], block);

                if (useCommonThreshold)
                {
                    device.ChannelBlock.SetCommonThreshold(callbackThreshold);
                }
            }

            device.ChannelBlock.SetCheckChecksum((uint)che, (uint)tim);
        }

        /* Here follow the callback function for receiving data and receiving error messages
         * Please note, it is an error to use both data receiving callbacks at a time unless you know want you are doing
         */

        delegate void OnChannelDataDelegateUI16(ushort[] data, int offset);
        delegate void OnChannelDataDelegateI32(int[] data, int offset);

        #region Poll For Data

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (dataSelectionMethod == DataSelectionMethod.SetSelectedData)
            {
                TimerTickForChannelData();
            }
            else
            {
                TimerTickWithoutChannelData();
            }

            while (device.GetErrorMessage(out string errorString, out int info) == 0) // Poll for error messages in the dacq threads
            {
                OnError(errorString, info);
            }
        }

        private void TimerTickForChannelData()
        {
            uint frames = device.ChannelBlock.AvailFrames(0, 0);
            if ((LastData || frames > callbackThreshold) && frames > 0)
            {
                device.ChannelBlock.GetChannel(0, 0, 0, out int totalChannels, out int byte_offset, out int channel_offset, out int channels);
                if (dataMode == DataModeEnumNet.Unsigned_16bit)
                {
                    ushort[] data = device.ChannelBlock.ReadFramesUI16(0, 0, callbackThreshold, out int sizeRet);
                    for (int i = 0; i < totalChannels; i++)
                    {
                        ushort[] channelData = new ushort[sizeRet];
                        for (int j = 0; j < sizeRet; j++)
                        {
                            channelData[j] = data[j * totalChannels + i];
                        }

                        DrawChannelData(channelData, i);
                    }
                }
                else // (dataMode == DataModeEnumNet.Signed_32bit)
                {
                    int[] data = device.ChannelBlock.ReadFramesI32(0, 0, callbackThreshold, out int sizeRet);
                    for (int i = 0; i < totalChannels; i++)
                    {
                        int[] channelData = new int[sizeRet];
                        for (int j = 0; j < sizeRet; j++)
                        {
                            channelData[j] = data[j * totalChannels + i];
                        }

                        DrawChannelData(channelData, i);
                    }
                }
            }
        }

        private void TimerTickWithoutChannelData()
        {
            uint frames = 0;
            if (useCommonThreshold)
            {
                frames = device.ChannelBlock.AvailFrames(-1, -1);
            }
            for (int i = 0; i < mChannels; i++)
            {
                if (!useCommonThreshold)
                {
                    frames = device.ChannelBlock.AvailFrames(i, 0);
                }
                if ((LastData || frames > callbackThreshold) && frames > 0)
                {
                    device.ChannelBlock.GetChannel(i, 0, 0, out int totalchannels, out int byte_offset, out int channel_offset, out int channels);
                    Debug.Assert(totalchannels == 1);
                    Debug.Assert(channels == 1);
                    if (dataMode == DataModeEnumNet.Unsigned_16bit)
                    {
                        ushort[] data = device.ChannelBlock.ReadFramesUI16(i, 0, callbackThreshold, out int sizeRet);
                        DrawChannelData(data, channel_offset);
                    }
                    else // (dataMode == DataModeEnumNet.Signed_32bit)
                    {
                        int[] data = device.ChannelBlock.ReadFramesI32(i, 0, callbackThreshold, out int sizeRet);
                        DrawChannelData(data, channel_offset);
                    }
                }
            }
        }

        #endregion

        #region Callbacks For Data Retrieval

        private void OnChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples)
        {
            if (dataSelectionMethod == DataSelectionMethod.SetSelectedData)
            {
                ChannelDataForChannelData(numSamples);
            }
            else
            {
                ChannelDataWithoutChannelData(cbHandle, numSamples);
            }
        }

        private void ChannelDataForChannelData(int numSamples)
        {
            device.ChannelBlock.GetChannel(0, 0, 0, out int totalchannels, out int byte_offset, out int channel_offset, out int channels);

            Debug.Assert(totalchannels == mChannels);

            // Get a data frame. This frame contains data from all channels and needs to be "unmixed" manually
            if (dataMode == DataModeEnumNet.Unsigned_16bit)
            {
                ushort[] data = device.ChannelBlock.ReadFramesUI16(0, 0, callbackThreshold, out int sizeRet);

                for (int i = 0; i < totalchannels; i++)
                {
                    ushort[] channelData = new ushort[sizeRet];
                    for (int j = 0; j < sizeRet; j++)
                    {
                        channelData[j] = data[j * totalchannels + i];
                    }

                    DrawChannelData(channelData, i);
                }
            }
            else // (dataMode == DataModeEnumNet.Signed_32bit)
            {
                int[] data = device.ChannelBlock.ReadFramesI32(0, 0, callbackThreshold, out int sizeRet);

                for (int i = 0; i < totalchannels; i++)
                {
                    int[] channelData = new int[sizeRet];
                    for (int j = 0; j < sizeRet; j++)
                    {
                        channelData[j] = data[j * totalchannels + i];
                    }

                    DrawChannelData(channelData, i);
                }

            }
        }

        private void ChannelDataWithoutChannelData(int cbHandle, int numSamples)
        {
            if (cbHandle == -1)
            {
                Debug.Assert(useCommonThreshold);
                for (int handle = 0; handle < mChannels; handle++)
                {
                    ChannelDataWithoutChannelData(handle, numSamples);
                }
            }
            else
            {
                device.ChannelBlock.GetChannel(cbHandle, 0, 0, out int totalchannels, out int byte_offset, out int channel_offset, out int channels);
                Debug.Assert(totalchannels == 1);
                Debug.Assert(channels == 1);

                // Get a data frame. This contains the data for the channel with index cbHandle
                if (dataMode == DataModeEnumNet.Unsigned_16bit)
                {
                    ushort[] data = device.ChannelBlock.ReadFramesUI16(cbHandle, 0, callbackThreshold, out int sizeRet);
                    DrawChannelData(data, cbHandle);
                }
                else // (dataMode == DataModeEnumNet.Signed_32bit)
                {
                    int[] data = device.ChannelBlock.ReadFramesI32(cbHandle, 0, callbackThreshold, out int sizeRet);
                    DrawChannelData(data, cbHandle);
                }
            }
        }

        private void OnError(string msg, int info)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new OnError(OnError), msg, info);
            }
            else
            {
                listBoxErrors.Items.Add(msg);
            }
        }

        #endregion

        private void DrawChannelData(ushort[] data, int offset)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new OnChannelDataDelegateUI16(DrawChannelData), data, offset);
            }
            else
            {
                int channel = cbChannel.SelectedIndex;
                if (channel >= 0 && channel == offset)
                {
                    DrawChannel(data);
                }
            }
        }

        private void DrawChannelData(int[] data, int offset)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new OnChannelDataDelegateI32(DrawChannelData), data, offset);
            }
            else
            {
                int channel = cbChannel.SelectedIndex;
                if (channel >= 0 && channel == offset)
                {
                    DrawChannel(data);
                }
            }
        }

        private void BtStartClick(object sender, EventArgs e)
        {
            listBoxErrors.Items.Clear();

            device.StartDacq();

            if (useWireless)
            {
                CW2100_FunctionNet func = new CW2100_FunctionNet(device);
                func.SetHeadstageSamplingActive(true, 0);
            }

            btMeaDevice_present.Enabled = false;
            cbDevices.Enabled = false;
            btStart.Enabled = false;
            btStop.Enabled = true;

            if (usePollForData)
            {
                LastData = false;
                timer.Enabled = true;
            }
        }

        private void BtStopClick(object sender, EventArgs e)
        {
            if (usePollForData)
            {
                LastData = true;
                device.SendStopDacq(); // StopDacq needs to be split into separate calls: SendStopDacq 
                System.Threading.Thread.Sleep(200);
                Timer_Tick(null, null);
                device.StopLoop();
            }
            else
            {
                device.StopDacq();
            }
            if (useWireless)
            {
                CW2100_FunctionNet func = new CW2100_FunctionNet(device);
                func.SetHeadstageSamplingActive(false, 0);
            }

            btMeaDevice_present.Enabled = true;
            cbDevices.Enabled = true;
            btStart.Enabled = true;
            btStop.Enabled = false;
            if (usePollForData)
            {
                timer.Enabled = false;
            }
        }

        private void SetChannelCombo(int channels)
        {
            cbChannel.Items.Clear();
            for (int i = 0; i < channels; i++)
            {
                cbChannel.Items.Add((i + 1).ToString("D"));
            }
            if (channels > 0)
            {
                cbChannel.SelectedIndex = 0;
            }
        }

        private void Form1FormClosed(object sender, FormClosedEventArgs e)
        {
            if (device != null)
            {
                device.SendStopDacq();

                device.Disconnect();
                device.Dispose();

                device = null;
            }

        }

        void DrawChannel(ushort[] data)
        {
            mData = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                mData[i] = data[i];
            }
            panel1.Invalidate();
        }

        void DrawChannel(int[] data)
        {
            mData = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                mData[i] = data[i];
            }
            panel1.Invalidate();
        }

            private void Panel1Paint(object sender, PaintEventArgs e)
        {
            double width = panel1.Width;
            double height = panel1.Height;
            double max = Double.MinValue;
            double min = Double.MaxValue;
            if (mData != null && mData.Length > 1)
            {
                foreach (double t in mData)
                {
                    if (t > max)
                    {
                        max = t;
                    }
                    if (t < min)
                    {
                        min = t;
                    }
                }
                Point[] points = new Point[mData.Length];
                for (int i = 0; i < mData.Length; i++)
                {
                    points[i] = new Point((int)(i * width / mData.Length), (int)((mData[i]-min + 1) * height / (max-min + 2)));
                }
                Pen pen = new Pen(Color.Black, 1);
                e.Graphics.DrawLines(pen, points);
            }
 
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device != null)
            {
                if (btMeaDevice_present.Enabled == false) // this means we are sampling
                {
                    e.Cancel = true;
                }
            }
        }

        #region Settings

        private void OnCheckBoxPollForDataCheckedChanged(object sender, EventArgs e)
        {
            usePollForData = checkBoxPollForData.Checked;
        }

        #endregion

        private void checkBoxCommonThreshold_CheckedChanged(object sender, EventArgs e)
        {
            useCommonThreshold = checkBoxCommonThreshold.Checked;
        }

        private void comboBoxDataSelectionMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataSelectionMethod = (DataSelectionMethod)comboBoxDataSelectionMethod.SelectedItem;
            checkBoxCommonThreshold.Enabled = dataSelectionMethod == DataSelectionMethod.SetSelectedChannels;
        }

        private void comboBoxDataMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataMode = (DataModeEnumNet) comboBoxDataMode.SelectedItem;
        }
    }

}