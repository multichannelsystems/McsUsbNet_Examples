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

        // If true, it is possible to retrieve channel data as a single array and use the useChannelData flag.
        // If false, the ReadFramesDict call can be used to retrieve the data as a dictionary with channel index as the key
        private bool useChannelMethod;

        // If true, data for multiple channels will be retrieved with a single Read_Frames call
        // If false, each Read_Frames call will retrieve only a single channel
        private bool useChannelData;

        // If true, data is retrieved by (timer-controlled) polling.
        // If false, callbacks are used to retrieve the data
        private bool usePollForData; 
        
        // This flag should be active for W2100 devices
        private bool useWireless; 

        // The overall number of channels (including data, digital, checksum, timestamp) in one sample. 
        // Checksum and timestamp are not available for MC_Card
        // With the MC_Card you lose one analog channel, when using the digital channel 
        private int channelblocksize;

        // "Poll For Data" only
        private Timer timer = new Timer();
        private bool LastData = false;

        // "Channel Method" only
        private int mChannelHandles;

        // for drawing
        private ushort[] mData;

        public Form1()
        {
            InitializeComponent();

            timer.Tick += Timer_Tick;
            timer.Interval = 20;

            checkBoxChannelData.Checked = useChannelData;
            checkBoxChannelData.Enabled = useChannelMethod;
            checkBoxPollForData.Checked = usePollForData;
            checkBoxChannelMethod.Checked = useChannelMethod;
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
            device.HWInfo().GetNumberOfHWADCChannels(out int hwchannels);
            tbDeviceInfo.Text += @"Number of Hardware channels: " + hwchannels.ToString("D") + Environment.NewLine;

            if (hwchannels == 0)
            {
                hwchannels = 64;
            }

            // configure MeaDevice: MC_Card or Usb
            device.SetNumberOfChannels(hwchannels);

            const int Samplingrate = 20000; // MC_Card does not support all settings, please see MC_Rack for valid settings
            device.SetSamplerate(Samplingrate, 1, 0);
            
            int miliGain = device.GetGain();

            var voltageRanges = device.HWInfo().GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(miliGain);
            for (int i = 0; i < voltageRanges.Count; i++)
            {
                tbDeviceInfo.Text += @"(" + i.ToString("D") + @") " + voltageRanges[i].VoltageRangeDisplayStringMilliVolt + Environment.NewLine;
            }

            // Set the range according to the index (only valid for MC_Card)
            if (isMCCard)
            {
                device.SetVoltageRangeByIndex(0, 0);
            }

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

            // set the channel combo box with the channels
            SetChannelCombo(block);

            channelblocksize = Samplingrate / 10; // good choice for MC_Card

            bool[] selChannels = new bool[block];

            for (int i = 0; i < block; i++)
            {
                selChannels[i] = true; // With true channel i is selected
                // selChannels[i] = false; // With false the channel i is deselected
            }
            mChannelHandles = block; // for this case, if all channels are selected
            // queue size and threshold should be selected carefully
            if (useChannelMethod && useChannelData)
            {
                device.SetSelectedData(selChannels, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned, block);
                //device.AddSelectedChannelsQueue(10, 2, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned);
                //device.ChannelBlock_SetCommonThreshold(channelblocksize);
                // Alternative call if you want to select all channels
                //device.SetSelectedData(block, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned, block);
            }
            else if (useChannelMethod && !useChannelData)
            {
                device.SetSelectedChannels(selChannels, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned, block);
            }
            else
            {
                device.SetSelectedChannelsQueue(selChannels, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned, block);
            }

            device.ChannelBlock_SetCheckChecksum((uint)che, (uint)tim);
        }

        /* Here follow the callback function for receiving data and receiving error messages
         * Please note, it is an error to use both data receiving callbacks at a time unless you know want you are doing
         */

        delegate void OnChannelDataWithChannelMethodDelegate(ushort[] data, int offset);
        delegate void OnChannelDataWithoutChannelMethodDelegate(Dictionary<int, ushort[]> data);

        #region Poll For Data

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (useChannelMethod)
            {
                if (useChannelData)
                {
                    TimerTickForChannelMethodAndChannelData();
                }
                else
                {
                    TimerTickForChannelMethodWithoutChannelData();
                }
            }
            else
            {
                TimerTickWithoutChannelMethod();
            }

            while (device.GetErrorMessage(out string errorString, out int info) == 0) // Poll for error messages in the dacq threads
            {
                OnError(errorString, info);
            }
        }

        private void TimerTickForChannelMethodAndChannelData()
        {
            uint frames = device.ChannelBlock_AvailFrames(0);
            if ((LastData || frames > channelblocksize) && frames > 0)
            {
                device.ChannelBlock_GetChannel(0, 0, out int totalChannels, out int byte_offset, out int channel_offset, out int channels);
                ushort[] data = device.ChannelBlock_ReadFramesUI16(0, channelblocksize, out int sizeRet);
                for (int i = 0; i < totalChannels; i++)
                {
                    ushort[] channelData = new ushort[sizeRet];
                    for (int j = 0; j < sizeRet; j++)
                    {
                        channelData[j] = data[j * totalChannels + i];
                    }
                    DrawChannelDataWithChannelMethod(channelData, i);
                }
            }
        }

        private void TimerTickForChannelMethodWithoutChannelData()
        {
            for (int i = 0; i < mChannelHandles; i++)
            {
                uint frames = device.ChannelBlock_AvailFrames(i);
                if ((LastData || frames > channelblocksize) && frames > 0)
                {
                    device.ChannelBlock_GetChannel(i, 0, out int totalchannels, out int byte_offset, out int channel_offset, out int channels);
                    Debug.Assert(totalchannels == 1);
                    Debug.Assert(channels == 1);
                    ushort[] data = device.ChannelBlock_ReadFramesUI16(i, channelblocksize, out int sizeRet);
                    DrawChannelDataWithChannelMethod(data, channel_offset);
                }
            }
        }

        private void TimerTickWithoutChannelMethod()
        {
            uint frames = device.ChannelBlock_AvailFrames(0);
            if ((LastData || frames > channelblocksize) && frames > 0)
            {
                Dictionary<int, ushort[]> data = device.ChannelBlock_ReadFramesDictUI16(0, channelblocksize, out int sizeRet);
                DrawChannelDataWithoutChannelMethod(data);
            }
        }

        #endregion

        #region Callbacks For Data Retrieval

        private void OnChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples)
        {
            if (useChannelMethod)
            {
                if (useChannelData)
                {
                    ChannelDataForChannelMethodAndChannelData();
                }
                else
                {
                    ChannelDataForChannelMethodWithoutChannelData(cbHandle, numSamples);
                }
            }
            else
            {
                ChannelDataWithoutChannelMethod(cbHandle, numSamples);
            }
        }

        private void ChannelDataForChannelMethodAndChannelData()
        {
            device.ChannelBlock_GetChannel(0, 0, out int totalchannels, out int vyte_offset, out int channel_offset, out int channels);

            // Get a data frame. This frame contains data from all channels and needs to be "unmixed" manually
            ushort[] data = device.ChannelBlock_ReadFramesUI16(0, channelblocksize, out int sizeRet);

            for (int i = 0; i < totalchannels; i++)
            {
                ushort[] channelData = new ushort[sizeRet];
                for (int j = 0; j < sizeRet; j++)
                {
                    channelData[j] = data[j * mChannelHandles + i];
                }

                DrawChannelDataWithChannelMethod(channelData, i);
            }
        }

        private void ChannelDataForChannelMethodWithoutChannelData(int cbHandle, int numSamples)
        {
            device.ChannelBlock_GetChannel(cbHandle, 0, out int totalchannels, out int byte_offset, out int channel_offset, out int channels);
            Debug.Assert(totalchannels == 1);
            Debug.Assert(channels == 1);

            // Get a data frame. This contains the data for the channel with index cbHandle
            ushort[] data = device.ChannelBlock_ReadFramesUI16(cbHandle, numSamples, out int sizeRet);
            DrawChannelDataWithChannelMethod(data, cbHandle);
        }

        private void ChannelDataWithoutChannelMethod(int cbHandle, int numSamples)
        {
            // Get the data frame as a dictionary with channel index as the key
            Dictionary<int, ushort[]> data = device.ChannelBlock_ReadFramesDictUI16(cbHandle, numSamples, out int sizeRet);
            
            DrawChannelDataWithoutChannelMethod(data);
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

        private void DrawChannelDataWithChannelMethod(ushort[] data, int offset)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new OnChannelDataWithChannelMethodDelegate(DrawChannelDataWithChannelMethod), data, offset);
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

        private void DrawChannelDataWithoutChannelMethod(Dictionary<int, ushort[]> data)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new OnChannelDataWithoutChannelMethodDelegate(DrawChannelDataWithoutChannelMethod), data);
            }
            else
            {
                int channel = cbChannel.SelectedIndex;
                if (channel >= 0)
                {
                    DrawChannel(data[channel]);
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
            mData = data;
            panel1.Invalidate();
        }

        private void Panel1Paint(object sender, PaintEventArgs e)
        {
            int width = panel1.Width;
            int height = panel1.Height;
            int max = 0;
            int min = 65536;
            if (mData != null && mData.Length > 1)
            {
                foreach (ushort t in mData)
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
                    points[i] = new Point(i * width / mData.Length, (mData[i]-min + 1) * height / (max-min + 2));
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

        private void OnCheckBoxChannelDataCheckedChanged(object sender, EventArgs e)
        {
            useChannelData = checkBoxChannelData.Checked;
        }

        private void OnCheckBoxChannelMethodCheckedChanged(object sender, EventArgs e)
        {
            useChannelMethod = checkBoxChannelMethod.Checked;
            checkBoxChannelData.Enabled = useChannelMethod;
            if (!useChannelMethod)
            {
                checkBoxChannelData.Checked = false;
            }
        }

        #endregion
    }

}