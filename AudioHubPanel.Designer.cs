namespace AudioHub
{
    partial class AudioHubPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioHubPanel));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanelOutput = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelSource = new System.Windows.Forms.FlowLayoutPanel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer.Location = new System.Drawing.Point(12, 35);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.AutoScroll = true;
            this.splitContainer.Panel1.Controls.Add(this.flowLayoutPanelOutput);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.AutoScroll = true;
            this.splitContainer.Panel2.Controls.Add(this.flowLayoutPanelSource);
            this.splitContainer.Size = new System.Drawing.Size(964, 654);
            this.splitContainer.SplitterDistance = 303;
            this.splitContainer.SplitterWidth = 10;
            this.splitContainer.TabIndex = 10;
            // 
            // flowLayoutPanelOutput
            // 
            this.flowLayoutPanelOutput.AutoScroll = true;
            this.flowLayoutPanelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelOutput.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelOutput.Name = "flowLayoutPanelOutput";
            this.flowLayoutPanelOutput.Size = new System.Drawing.Size(960, 299);
            this.flowLayoutPanelOutput.TabIndex = 7;
            // 
            // flowLayoutPanelSource
            // 
            this.flowLayoutPanelSource.AutoScroll = true;
            this.flowLayoutPanelSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelSource.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelSource.Name = "flowLayoutPanelSource";
            this.flowLayoutPanelSource.Size = new System.Drawing.Size(960, 337);
            this.flowLayoutPanelSource.TabIndex = 8;
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(8, 2, 0, 6);
            this.menuStrip.Size = new System.Drawing.Size(988, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // AudioHubPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 701);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "AudioHubPanel";
            this.Text = "ATEM Audio Hub";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelOutput;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSource;
        private System.Windows.Forms.MenuStrip menuStrip;
    }
}

