using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AudioHub
{
    public partial class AudioHubPanel: Form
    {
        private string appName = "ATEM Audio Hub";

        private IBMDSwitcherDiscovery switcherDiscovery;
        private IBMDSwitcher switcher;

        private SwitcherMonitor switcherMonitor;
        private List<AudioRoutingOutputMonitor> audioRoutingOutputMonitors = new List<AudioRoutingOutputMonitor>();
        private List<AudioRoutingSourceMonitor> audioRoutingSourceMonitors = new List<AudioRoutingSourceMonitor>();

        private OutputButton selectedOutputButton;
        private SourceButton selectedSourceButton;
        private SourceButton preSelectedSourceButton;
        private bool toggleTake = false;

        private Dictionary<uint, OutputButton> outputDict = new Dictionary<uint, OutputButton>();
        private Dictionary<uint, SourceButton> sourceDict = new Dictionary<uint, SourceButton>();

        private ToolStripLabel toolStripLabelIP = new ToolStripLabel("IP address:");
        private ToolStripTextBox toolStripTextBoxIP = new ToolStripTextBox();
        private ToolStripButton toolStripButtonConnect = new ToolStripButton("Connect");
        private ToolStripLabel toolStripLabelTake = new ToolStripLabel("Toggle take:");
        private ToolStripButton toolStripButtonTakeToggle = new ToolStripButton("OFF");
        private ToolStripButton toolStripButtonTake = new ToolStripButton("Take");

        private void OnButtonToggleTake_Click(object sender, EventArgs e)
        {
            if (toggleTake)
            {
                preSelectedSourceButton.Checked = true;
                toggleTake = false;
                toolStripButtonTakeToggle.Text = "OFF";
                toolStripButtonTake.Enabled = false;
            }
            else
            {
                toggleTake = true;
                toolStripButtonTakeToggle.Text = "ON";
                toolStripButtonTake.Enabled = true;
            }
        }

        private void OnButtonTake_Click(object sender, EventArgs e)
        {
            selectedSourceButton.AudioRoutingSource.GetId(out uint sourceID);
            selectedOutputButton.AudioRoutingOutput.SetSource(sourceID);
            SourceButton oldButton = preSelectedSourceButton;
            preSelectedSourceButton = selectedSourceButton;
            OnSourceButtonCheckedChangedColor(oldButton, null);
        }

        private void AudioHubPanel_Load(object sender, EventArgs e)
        {
            ActiveControl = menuStrip;
        }

        public AudioHubPanel()
        {
            InitializeComponent();
            Load += new EventHandler(AudioHubPanel_Load);
            Text = appName;
            toolStripTextBoxIP.Text = Properties.Settings.Default.IPAddress;
            toolStripTextBoxIP.KeyPress += new KeyPressEventHandler(OnEnterKeyPress);
            toolStripButtonConnect.Click += new EventHandler(OnButtonConnect_Click);
            toolStripButtonTakeToggle.Click += new EventHandler(OnButtonToggleTake_Click);
            toolStripButtonTake.Click += new EventHandler(OnButtonTake_Click);
            toolStripButtonConnect.Font = new Font(toolStripButtonConnect.Font.Name, toolStripButtonConnect.Font.Size, FontStyle.Bold);
            toolStripButtonTakeToggle.Font = toolStripButtonConnect.Font;
            toolStripButtonTake.Font = toolStripButtonConnect.Font;
            toolStripButtonTake.Enabled = false;
            menuStrip.Items.Add(toolStripLabelIP);
            menuStrip.Items.Add(toolStripTextBoxIP);
            menuStrip.Items.Add(toolStripButtonConnect);
            menuStrip.Items.Add(toolStripLabelTake);
            menuStrip.Items.Add(toolStripButtonTakeToggle);
            menuStrip.Items.Add(toolStripButtonTake);


            switcherMonitor = new SwitcherMonitor();
            // note: this invoke pattern ensures our callback is called in the main thread. We are making double
            // use of lambda expressions here to achieve this.
            // Essentially, the events will arrive at the callback class (implemented by our monitor classes)
            // on a separate thread. We must marshal these to the main thread, and we're doing this by calling
            // invoke on the Windows Forms object. The lambda expression is just a simplification.
            switcherMonitor.SwitcherDisconnected += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => SwitcherDisconnected())));


            switcherDiscovery = new CBMDSwitcherDiscovery();
            if (switcherDiscovery == null)
            {
                MessageBox.Show("Could not create Switcher Discovery Instance.\nATEM Switcher Software may not be installed.", "Error");
                Environment.Exit(1);
            }

            SwitcherDisconnected();		// start with switcher disconnected
        }

        private void AudioHubPanel_Load1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SwitcherDisconnected()
        {
            // Remove all monitors, remove callbacks
            foreach (AudioRoutingOutputMonitor outputMonitor in audioRoutingOutputMonitors)
            {
                outputMonitor.Button.AudioRoutingOutput.RemoveCallback(outputMonitor);
                outputMonitor.Button.AudioRoutingOutput = null;
                outputMonitor.Button.CheckedChanged -= new EventHandler(OnOutputButtonCheckedChanged);
                outputMonitor.Button.CheckedChanged -= new EventHandler(OnOutputButtonCheckedChangedColor);
                outputMonitor.Button = null;
                outputMonitor.NameChanged -= new SwitcherEventHandler(OnAudioRoutingOutputNameChanged);
                outputMonitor.NameDefaultChanged -= new SwitcherEventHandler(OnAudioRoutingOutputNameDefaultChanged);
                outputMonitor.SourceChanged -= new SwitcherEventHandler(OnAudioRoutingOutputSourceChanged);
            }
            audioRoutingOutputMonitors.Clear();

            foreach (AudioRoutingSourceMonitor sourceMonitor in audioRoutingSourceMonitors)
            {
                sourceMonitor.Button.AudioRoutingSource.RemoveCallback(sourceMonitor);
                sourceMonitor.Button.AudioRoutingSource = null;
                sourceMonitor.Button.CheckedChanged -= new EventHandler(OnSourceButtonCheckedChanged);
                sourceMonitor.Button.CheckedChanged -= new EventHandler(OnSourceButtonCheckedChangedColor);
                sourceMonitor.Button = null;
                sourceMonitor.NameChanged -= new SwitcherEventHandler(OnAudioRoutingSourceNameChanged);
                sourceMonitor.NameDefaultChanged -= new SwitcherEventHandler(OnAudioRoutingSourceNameDefaultChanged);
            }
            audioRoutingSourceMonitors.Clear();

            outputDict.Clear();
            sourceDict.Clear();

            flowLayoutPanelOutput.Controls.Clear();
            flowLayoutPanelSource.Controls.Clear();

            if (switcher != null)
            {
                // Remove callback:
                switcher.RemoveCallback(switcherMonitor);
                // release reference:
                switcher = null;
            }
        }

        private void OnButtonConnect_Click(object sender, EventArgs e)
        {
            if (toolStripButtonConnect.Text == "Connect")
            {
                toolStripButtonConnect.Text = "Connecting";
                toolStripButtonConnect.Enabled = false;
                _BMDSwitcherConnectToFailure failReason = 0;
                string address = toolStripTextBoxIP.Text;

                try
                {
                    // Note that ConnectTo() can take several seconds to return, both for success or failure,
                    // depending upon hostname resolution and network response times, so it may be best to
                    // do this in a separate thread to prevent the main GUI thread blocking.
                    switcherDiscovery.ConnectTo(address, out switcher, out failReason);
                }
                catch (COMException)
                {
                    toolStripButtonConnect.Enabled = true;
                    toolStripButtonConnect.Text = "Connect";
                    // An exception will be thrown if ConnectTo fails. For more information, see failReason.
                    switch (failReason)
                    {
                        case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                            MessageBox.Show("No response from Switcher", "Error");
                            break;
                        case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                            MessageBox.Show("Switcher has incompatible firmware", "Error");
                            break;
                        default:
                            MessageBox.Show("Connection failed for unknown reason", "Error");
                            break;
                    }
                    return;
                }

                switcher.GetProductName(out string switcherName);
                Text = appName + " - " + switcherName;
                Properties.Settings.Default.IPAddress = address;
                Properties.Settings.Default.Save();
                SwitcherConnected();
                toolStripButtonConnect.Enabled = true;
                toolStripButtonConnect.Text = "Disconnect";
                toolStripTextBoxIP.ReadOnly = true;
                flowLayoutPanelOutput.Enabled = true;
                flowLayoutPanelSource.Enabled = true;

            }
            else if (toolStripButtonConnect.Text == "Disconnect")
            {
                flowLayoutPanelOutput.Enabled = false;
                flowLayoutPanelSource.Enabled = false;
                toolStripButtonConnect.Text = "Disconnecting";
                toolStripButtonConnect.Enabled = false;
                Text = appName;
                SwitcherDisconnected();
                toolStripButtonConnect.Enabled = true;
                toolStripButtonConnect.Text = "Connect";
                toolStripTextBoxIP.ReadOnly = false;
            }
        }

        private void SwitcherConnected()
        {
            // Install SwitcherMonitor callbacks:
            switcher.AddCallback(switcherMonitor);

            Guid audioRoutingOutputIteratorIID = typeof(IBMDSwitcherAudioRoutingOutputIterator).GUID;
            switcher.CreateIterator(ref audioRoutingOutputIteratorIID, out IntPtr audioRoutingOutputIteratorPtr);
            IBMDSwitcherAudioRoutingOutputIterator audioRoutingOutputIterator = null;
            if (audioRoutingOutputIteratorPtr != null)
            {
                audioRoutingOutputIterator = (IBMDSwitcherAudioRoutingOutputIterator)Marshal.GetObjectForIUnknown(audioRoutingOutputIteratorPtr);
            }

            if (audioRoutingOutputIterator != null)
            {
                audioRoutingOutputIterator.Next(out IBMDSwitcherAudioRoutingOutput output);
                while (output != null)
                {
                    OutputButton newButton = new OutputButton(output);
                    newButton.CheckedChanged += new EventHandler(OnOutputButtonCheckedChanged);
                    newButton.CheckedChanged += new EventHandler(OnOutputButtonCheckedChangedColor);
                    AudioRoutingOutputMonitor newOutputMonitor = new AudioRoutingOutputMonitor(newButton);
                    output.AddCallback(newOutputMonitor);
                    newOutputMonitor.NameChanged += new SwitcherEventHandler(OnAudioRoutingOutputNameChanged);
                    newOutputMonitor.NameDefaultChanged += new SwitcherEventHandler(OnAudioRoutingOutputNameDefaultChanged);
                    newOutputMonitor.SourceChanged += new SwitcherEventHandler(OnAudioRoutingOutputSourceChanged);

                    audioRoutingOutputMonitors.Add(newOutputMonitor);
                    flowLayoutPanelOutput.Controls.Add(newButton);
                    output.GetId(out uint outputID);
                    outputDict.Add(outputID, newButton);

                    audioRoutingOutputIterator.Next(out output);
                }
            }

            Guid audioRoutingSourceIteratorIID = typeof(IBMDSwitcherAudioRoutingSourceIterator).GUID;
            switcher.CreateIterator(ref audioRoutingSourceIteratorIID, out IntPtr audioRoutingSourceIteratorPtr);
            IBMDSwitcherAudioRoutingSourceIterator audioRoutingSourceIterator = null;
            if (audioRoutingSourceIteratorPtr != null)
            {
                audioRoutingSourceIterator = (IBMDSwitcherAudioRoutingSourceIterator)Marshal.GetObjectForIUnknown(audioRoutingSourceIteratorPtr);
            }

            if (audioRoutingSourceIterator != null)
            {
                audioRoutingSourceIterator.Next(out IBMDSwitcherAudioRoutingSource source);
                while (source != null)
                {
                    SourceButton newButton = new SourceButton(source);
                    newButton.CheckedChanged += new EventHandler(OnSourceButtonCheckedChanged);
                    newButton.CheckedChanged += new EventHandler(OnSourceButtonCheckedChangedColor);
                    AudioRoutingSourceMonitor newSourceMonitor = new AudioRoutingSourceMonitor(newButton);
                    source.AddCallback(newSourceMonitor);
                    newSourceMonitor.NameChanged += new SwitcherEventHandler(OnAudioRoutingSourceNameChanged);
                    newSourceMonitor.NameDefaultChanged += new SwitcherEventHandler(OnAudioRoutingSourceNameDefaultChanged);

                    audioRoutingSourceMonitors.Add(newSourceMonitor);
                    flowLayoutPanelSource.Controls.Add(newButton);
                    source.GetId(out uint sourceID);
                    sourceDict.Add(sourceID, newButton);

                    audioRoutingSourceIterator.Next(out source);
                }
            }

            outputDict.Values.First().Checked = true;
        }

        private void OnAudioRoutingOutputNameChanged(object sender, object args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, object>(OnAudioRoutingOutputNameChanged), sender, args);
                return;
            }

            AudioRoutingOutputMonitor monitor = sender as AudioRoutingOutputMonitor;
            if (monitor != null)
            {
                string name;
                monitor.Button.AudioRoutingOutput.GetName(out name);
                monitor.Button.Text = name;
            }
        }

        private void OnAudioRoutingOutputNameDefaultChanged(object sender, object args)
        {

        }

        private void OnAudioRoutingOutputSourceChanged(object sender, object args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, object>(OnAudioRoutingOutputSourceChanged), sender, args);
                return;
            }

            AudioRoutingOutputMonitor monitor = sender as AudioRoutingOutputMonitor;
            if (monitor != null)
            {
                if (selectedOutputButton == monitor.Button)
                {
                    if (toggleTake)
                    {
                        monitor.Button.AudioRoutingOutput.GetSource(out uint sourceID);
                        sourceDict.TryGetValue(sourceID, out SourceButton newSelectedSourceButton);
                        SourceButton oldButton = preSelectedSourceButton;
                        preSelectedSourceButton = newSelectedSourceButton;
                        OnSourceButtonCheckedChangedColor(oldButton, null);
                        OnSourceButtonCheckedChangedColor(preSelectedSourceButton, null);
                    }
                    else
                    {
                        monitor.Button.AudioRoutingOutput.GetSource(out uint sourceID);
                        sourceDict.TryGetValue(sourceID, out SourceButton newSelectedSourceButton);
                        newSelectedSourceButton.Checked = true;
                        selectedSourceButton = newSelectedSourceButton;
                        preSelectedSourceButton = selectedSourceButton;
                    }
                }
            }
        }

        private void OnAudioRoutingSourceNameChanged(object sender, object args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, object>(OnAudioRoutingSourceNameChanged), sender, args);
                return;
            }

            AudioRoutingSourceMonitor monitor = sender as AudioRoutingSourceMonitor;
            if (monitor != null)
            {
                string name;
                monitor.Button.AudioRoutingSource.GetName(out name);
                monitor.Button.Text = name;
            }
        }

        private void OnAudioRoutingSourceNameDefaultChanged(object sender, object args)
        {

        }

        private void OnOutputButtonCheckedChanged(object sender, EventArgs e)
        {
            OutputButton button = sender as OutputButton;
            if (button != null)
            {
                if (button.Checked)
                {
                    selectedOutputButton = button;
                    button.AudioRoutingOutput.GetSource(out uint sourceID);
                    sourceDict.TryGetValue(sourceID, out SourceButton newSelectedSourceButton);
                    newSelectedSourceButton.Checked = true;
                    SourceButton oldButton = selectedSourceButton;
                    selectedSourceButton = newSelectedSourceButton;
                    OnSourceButtonCheckedChangedColor(oldButton, null);
                    oldButton = preSelectedSourceButton;
                    preSelectedSourceButton = selectedSourceButton;
                    OnSourceButtonCheckedChangedColor(oldButton, null);
                }
            }
        }

        private void OnOutputButtonCheckedChangedColor(object sender, EventArgs e)
        {
            OutputButton button = sender as OutputButton;
            if (button != null)
            {
                if (button.Checked)
                {
                    button.BackColor = System.Drawing.Color.Red;
                }
                else
                {
                    button.BackColor = SystemColors.ControlLight;
                }
            }
        }

        private void OnSourceButtonCheckedChangedColor(object sender, EventArgs e)
        {
            SourceButton button = sender as SourceButton;
            if (button != null)
            {
                if (toggleTake)
                {
                    if (button.Checked)
                    {
                        button.BackColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        if (button == preSelectedSourceButton)
                        {
                            button.BackColor = System.Drawing.Color.LightGreen;
                        }
                        else
                        {
                            button.BackColor = SystemColors.ControlLight;
                        }
                    }
                }
                else
                {
                    if (button.Checked)
                    {
                        button.BackColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        button.BackColor = SystemColors.ControlLight;
                    }
                }
            }
        }

        private void OnSourceButtonCheckedChanged(object sender, EventArgs e)
        {
            SourceButton button = sender as SourceButton;
            if (button != null)
            {
                if (toggleTake)
                {
                    if (button.Checked)
                    {
                        selectedSourceButton = button;
                    }
                }
                else
                {
                    if (button.Checked)
                    {
                        selectedSourceButton = button;
                        button.AudioRoutingSource.GetId(out uint sourceID);
                        selectedOutputButton.AudioRoutingOutput.SetSource(sourceID);
                    }
                }
            }
        }

        private void OnEnterKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                OnButtonConnect_Click(sender, e);
            }
        }
    }
}
