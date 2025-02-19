using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioHub
{
    class OutputButton : RadioButton
    {
        public IBMDSwitcherAudioRoutingOutput AudioRoutingOutput { get; set; }

        public OutputButton(IBMDSwitcherAudioRoutingOutput output)
        {
            AudioRoutingOutput = output;
            output.GetName(out string name);
            Text = name;
            Appearance = Appearance.Button;
            AudioRoutingOutput.GetExternalPortType(out _BMDSwitcherExternalPortType externalPortType);
            AudioRoutingOutput.GetInternalPortType(out _BMDSwitcherAudioInternalPortType internalPortType);
            AudioRoutingOutput.GetChannelPair(out _BMDSwitcherAudioChannelPair channelPair);
            AudioRoutingOutput.GetIsNameDefault(out int isDefault);
            string defaultName;
            if (isDefault == 1)
                defaultName = "Yes";
            else
                defaultName = "No";
            string toolTipText = "External Port Type: " + externalPortType.ToString() + "\n" +
                                 "Internal Port Type: " + internalPortType.ToString() + "\n" +
                                 "Channel Pair: " + channelPair.ToString() + "\n" +
                                 "Default Name: " + defaultName;

            ToolTip toolTip = new ToolTip();
            toolTip.AutomaticDelay = 1500;
            toolTip.SetToolTip(this, toolTipText);
            ContextMenuStrip = new RenameMenuStrip(RenameMenuStrip.RenameOutput_Click);
        }
    }

    class SourceButton : RadioButton
    {
        public IBMDSwitcherAudioRoutingSource AudioRoutingSource { get; set; }
        public SourceButton(IBMDSwitcherAudioRoutingSource source)
        {
            AudioRoutingSource = source;
            source.GetName(out string name);
            Text = name;
            Appearance = Appearance.Button;
            AudioRoutingSource.GetExternalPortType(out _BMDSwitcherExternalPortType externalPortType);
            AudioRoutingSource.GetInternalPortType(out _BMDSwitcherAudioInternalPortType internalPortType);
            AudioRoutingSource.GetChannelPair(out _BMDSwitcherAudioChannelPair channelPair);
            AudioRoutingSource.GetIsNameDefault(out int isDefault);
            string defaultName;
            if (isDefault == 1)
                defaultName = "Yes";
            else
                defaultName = "No";
            string toolTipText = "External Port Type: " + externalPortType.ToString() + "\n" +
                                 "Internal Port Type: " + internalPortType.ToString() + "\n" +
                                 "Channel Pair: " + channelPair.ToString() + "\n" +
                                 "Default Name: " + defaultName;

            ToolTip toolTip = new ToolTip();
            toolTip.AutomaticDelay = 1500;
            toolTip.SetToolTip(this, toolTipText);
            ContextMenuStrip = new RenameMenuStrip(RenameMenuStrip.RenameSource_Click);
        }
    }

    public class RenameMenuStrip : ContextMenuStrip
    {
        public RenameMenuStrip(EventHandler eventHandler)
        {
            ToolStripMenuItem renameItem = new ToolStripMenuItem("Rename");
            renameItem.Click += eventHandler;
            Items.Add(renameItem);
        }

        public static void RenameSource_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            ContextMenuStrip menu = (ContextMenuStrip)item.Owner;
            SourceButton sourceButton = (SourceButton)menu.SourceControl;
            string newName = InputDialog.Show("Rename Source", "Enter new name for '" + sourceButton.Text + "':");
            if (!string.IsNullOrEmpty(newName))
            {
                sourceButton.AudioRoutingSource.SetName(newName);
            }
        }

        public static void RenameOutput_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            ContextMenuStrip menu = (ContextMenuStrip)item.Owner;
            OutputButton outputButton = (OutputButton)menu.SourceControl;
            string newName = InputDialog.Show("Rename Source", "Enter new name '" + outputButton.Text + "':");
            if (!string.IsNullOrEmpty(newName))
            {
                outputButton.AudioRoutingOutput.SetName(newName);
            }
        }
    }

    public class InputDialog : Form
    {
        private TextBox inputTextBox;
        private Button okButton;
        private Button cancelButton;

        public string InputText { get; private set; }

        public InputDialog(string title, string prompt)
        {
            Text = title;
            Width = 300;
            Height = 150;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;

            Label promptLabel = new Label() { Left = 10, Top = 10, Text = prompt, AutoSize = true };
            inputTextBox = new TextBox() { Left = 10, Top = 30, Width = 260 };
            okButton = new Button() { Text = "OK", Left = 100, Width = 80, Top = 60, DialogResult = DialogResult.OK };
            cancelButton = new Button() { Text = "Cancel", Left = 190, Width = 80, Top = 60, DialogResult = DialogResult.Cancel };

            okButton.Click += OkButton_Click;
            cancelButton.Click += (sender, e) => { Close(); };

            Controls.Add(promptLabel);
            Controls.Add(inputTextBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            InputText = inputTextBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        public static string Show(string title, string prompt)
        {
            using (InputDialog dialog = new InputDialog(title, prompt))
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.InputText : null;
            }
        }
    }
}
