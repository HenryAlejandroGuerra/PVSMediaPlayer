namespace PVSPlayerExample
{
    partial class AspectRatioDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        ///// <summary>
        ///// Clean up any resources being used.
        ///// </summary>
        ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.headLabel = new System.Windows.Forms.Label();
			this.infoLabel = new System.Windows.Forms.Label();
			this.widthBox = new System.Windows.Forms.TextBox();
			this.heightBox = new System.Windows.Forms.TextBox();
			this.colonLabel = new System.Windows.Forms.Label();
			this.presetButton = new PVSPlayerExample.DropDownButton();
			this.presetRatesMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.reversedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.restoreCheckBox = new System.Windows.Forms.CheckBox();
			this.dvdCheckBox = new System.Windows.Forms.CheckBox();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.widthErrorPanel = new System.Windows.Forms.Panel();
			this.heightErrorPanel = new System.Windows.Forms.Panel();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.updateButton = new PVSPlayerExample.CustomButton();
			this.pvsPanel = new System.Windows.Forms.Panel();
			this.cancelButton = new PVSPlayerExample.CustomButton();
			this.okButton = new PVSPlayerExample.CustomButton();
			this.useRatioCheckBox = new System.Windows.Forms.CheckBox();
			this.displayPanel = new System.Windows.Forms.Panel();
			this.videoPanel = new PVSPlayerExample.VideoPanel();
			this.nameLabel = new System.Windows.Forms.Label();
			this.sizeTitleLabel = new System.Windows.Forms.Label();
			this.sizeLabel = new System.Windows.Forms.Label();
			this.presetRatesMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.displayPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// headLabel
			// 
			this.headLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.headLabel.AutoSize = true;
			this.headLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.headLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.headLabel.Location = new System.Drawing.Point(350, 12);
			this.headLabel.Name = "headLabel";
			this.headLabel.Size = new System.Drawing.Size(146, 20);
			this.headLabel.TabIndex = 1;
			this.headLabel.Text = "Video Aspect Ratio";
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.infoLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.infoLabel.Location = new System.Drawing.Point(351, 50);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(347, 32);
			this.infoLabel.TabIndex = 2;
			this.infoLabel.Text = "If a video image is not displayed in the correct aspect ratio, you can set the co" +
    "rrect aspect ratio here, for example 16:9 for HD widescreen.";
			// 
			// widthBox
			// 
			this.widthBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.widthBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
			this.widthBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.widthBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.widthBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.errorProvider1.SetIconAlignment(this.widthBox, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.widthBox.Location = new System.Drawing.Point(445, 166);
			this.widthBox.MaxLength = 5;
			this.widthBox.Name = "widthBox";
			this.widthBox.Size = new System.Drawing.Size(56, 22);
			this.widthBox.TabIndex = 9;
			this.widthBox.Text = "16";
			this.widthBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.widthBox.Enter += new System.EventHandler(this.TextBox_Enter);
			this.widthBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
			this.widthBox.Validating += new System.ComponentModel.CancelEventHandler(this.WidthBox_Validating);
			// 
			// heightBox
			// 
			this.heightBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.heightBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
			this.heightBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.heightBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.heightBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.heightBox.Location = new System.Drawing.Point(518, 166);
			this.heightBox.MaxLength = 5;
			this.heightBox.Name = "heightBox";
			this.heightBox.Size = new System.Drawing.Size(56, 22);
			this.heightBox.TabIndex = 11;
			this.heightBox.Text = "9";
			this.heightBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.heightBox.Enter += new System.EventHandler(this.TextBox_Enter);
			this.heightBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
			this.heightBox.Validating += new System.ComponentModel.CancelEventHandler(this.HeightBox_Validating);
			// 
			// colonLabel
			// 
			this.colonLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.colonLabel.AutoSize = true;
			this.colonLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.colonLabel.Location = new System.Drawing.Point(505, 170);
			this.colonLabel.Name = "colonLabel";
			this.colonLabel.Size = new System.Drawing.Size(10, 13);
			this.colonLabel.TabIndex = 10;
			this.colonLabel.Text = ":";
			// 
			// presetButton
			// 
			this.presetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.presetButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.presetButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.presetButton.DropDown = this.presetRatesMenu;
			this.presetButton.Location = new System.Drawing.Point(589, 166);
			this.presetButton.Name = "presetButton";
			this.presetButton.Size = new System.Drawing.Size(93, 22);
			this.presetButton.TabIndex = 12;
			this.presetButton.Text = "Presets  ";
			this.presetButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.presetButton.UseVisualStyleBackColor = true;
			// 
			// presetRatesMenu
			// 
			this.presetRatesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7,
            this.toolStripMenuItem10,
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripSeparator1,
            this.reversedMenuItem});
			this.presetRatesMenu.Name = "presetRatesMenu";
			this.presetRatesMenu.Size = new System.Drawing.Size(115, 230);
			this.presetRatesMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.PresetRatesMenu_ItemClicked);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem2.Text = "1:1";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem3.Text = "5:4";
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem4.Text = "4:3";
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem5.Text = "16:10";
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem6.Text = "16:9";
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem7.Text = "1.85:1";
			// 
			// toolStripMenuItem10
			// 
			this.toolStripMenuItem10.Name = "toolStripMenuItem10";
			this.toolStripMenuItem10.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem10.Text = "2:1";
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			this.toolStripMenuItem8.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem8.Text = "2.35:1";
			// 
			// toolStripMenuItem9
			// 
			this.toolStripMenuItem9.Name = "toolStripMenuItem9";
			this.toolStripMenuItem9.Size = new System.Drawing.Size(114, 22);
			this.toolStripMenuItem9.Text = "2.39:1";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(111, 6);
			// 
			// reversedMenuItem
			// 
			this.reversedMenuItem.Name = "reversedMenuItem";
			this.reversedMenuItem.Size = new System.Drawing.Size(114, 22);
			this.reversedMenuItem.Text = "Reverse";
			// 
			// restoreCheckBox
			// 
			this.restoreCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.restoreCheckBox.AutoSize = true;
			this.restoreCheckBox.Location = new System.Drawing.Point(352, 215);
			this.restoreCheckBox.Name = "restoreCheckBox";
			this.restoreCheckBox.Size = new System.Drawing.Size(332, 17);
			this.restoreCheckBox.TabIndex = 13;
			this.restoreCheckBox.Text = "Restore to original aspect ratios when media has finished playing.";
			this.restoreCheckBox.UseVisualStyleBackColor = true;
			// 
			// dvdCheckBox
			// 
			this.dvdCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dvdCheckBox.AutoSize = true;
			this.dvdCheckBox.Location = new System.Drawing.Point(352, 238);
			this.dvdCheckBox.Name = "dvdCheckBox";
			this.dvdCheckBox.Size = new System.Drawing.Size(344, 17);
			this.dvdCheckBox.TabIndex = 14;
			this.dvdCheckBox.Text = "Always set 704x567 and 720x576 resolutions to a 16:9 aspect ratio.";
			this.dvdCheckBox.UseVisualStyleBackColor = true;
			this.dvdCheckBox.CheckedChanged += new System.EventHandler(this.DvdCheckBox_CheckedChanged);
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// widthErrorPanel
			// 
			this.widthErrorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.errorProvider1.SetIconAlignment(this.widthErrorPanel, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.widthErrorPanel.Location = new System.Drawing.Point(481, 150);
			this.widthErrorPanel.Name = "widthErrorPanel";
			this.widthErrorPanel.Size = new System.Drawing.Size(17, 10);
			this.widthErrorPanel.TabIndex = 6;
			// 
			// heightErrorPanel
			// 
			this.heightErrorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.errorProvider1.SetIconAlignment(this.heightErrorPanel, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.heightErrorPanel.Location = new System.Drawing.Point(554, 150);
			this.heightErrorPanel.Name = "heightErrorPanel";
			this.heightErrorPanel.Size = new System.Drawing.Size(17, 10);
			this.heightErrorPanel.TabIndex = 7;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bottomPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
			this.bottomPanel.Controls.Add(this.updateButton);
			this.bottomPanel.Controls.Add(this.pvsPanel);
			this.bottomPanel.Controls.Add(this.cancelButton);
			this.bottomPanel.Controls.Add(this.okButton);
			this.bottomPanel.Location = new System.Drawing.Point(0, 269);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(712, 50);
			this.bottomPanel.TabIndex = 15;
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.updateButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.updateButton.Enabled = false;
			this.updateButton.FocusBorder = true;
			this.updateButton.Location = new System.Drawing.Point(250, 14);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(84, 23);
			this.updateButton.TabIndex = 1;
			this.updateButton.Text = "Update";
			this.updateButton.UseVisualStyleBackColor = true;
			this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// pvsPanel
			// 
			this.pvsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pvsPanel.Location = new System.Drawing.Point(9, 9);
			this.pvsPanel.Name = "pvsPanel";
			this.pvsPanel.Size = new System.Drawing.Size(42, 33);
			this.pvsPanel.TabIndex = 0;
			this.pvsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.PvsPanel_Paint);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FocusBorder = true;
			this.cancelButton.Location = new System.Drawing.Point(615, 14);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(84, 23);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.FocusBorder = true;
			this.okButton.Location = new System.Drawing.Point(522, 14);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(84, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// useRatioCheckBox
			// 
			this.useRatioCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.useRatioCheckBox.AutoSize = true;
			this.useRatioCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.useRatioCheckBox.Location = new System.Drawing.Point(352, 167);
			this.useRatioCheckBox.Name = "useRatioCheckBox";
			this.useRatioCheckBox.Size = new System.Drawing.Size(84, 20);
			this.useRatioCheckBox.TabIndex = 8;
			this.useRatioCheckBox.Text = "Use ratio:";
			this.useRatioCheckBox.UseVisualStyleBackColor = true;
			this.useRatioCheckBox.CheckedChanged += new System.EventHandler(this.UseRatioCheckBox_CheckedChanged);
			// 
			// displayPanel
			// 
			this.displayPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.displayPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.displayPanel.Controls.Add(this.videoPanel);
			this.displayPanel.Location = new System.Drawing.Point(14, 14);
			this.displayPanel.Name = "displayPanel";
			this.displayPanel.Size = new System.Drawing.Size(320, 240);
			this.displayPanel.TabIndex = 0;
			// 
			// videoPanel
			// 
			this.videoPanel.ContextMenuStrip = this.presetRatesMenu;
			this.videoPanel.Location = new System.Drawing.Point(0, 0);
			this.videoPanel.Name = "videoPanel";
			this.videoPanel.Size = new System.Drawing.Size(320, 240);
			this.videoPanel.TabIndex = 0;
			// 
			// nameLabel
			// 
			this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.nameLabel.Location = new System.Drawing.Point(351, 107);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(348, 14);
			this.nameLabel.TabIndex = 3;
			this.nameLabel.Text = "Name";
			this.nameLabel.UseMnemonic = false;
			// 
			// sizeTitleLabel
			// 
			this.sizeTitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sizeTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sizeTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.sizeTitleLabel.Location = new System.Drawing.Point(351, 124);
			this.sizeTitleLabel.Name = "sizeTitleLabel";
			this.sizeTitleLabel.Size = new System.Drawing.Size(69, 14);
			this.sizeTitleLabel.TabIndex = 4;
			this.sizeTitleLabel.Text = "Original size:";
			// 
			// sizeLabel
			// 
			this.sizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sizeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.sizeLabel.Location = new System.Drawing.Point(415, 124);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(275, 14);
			this.sizeLabel.TabIndex = 5;
			this.sizeLabel.Text = "Size";
			// 
			// AspectRatioDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(712, 319);
			this.Controls.Add(this.sizeLabel);
			this.Controls.Add(this.sizeTitleLabel);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.heightErrorPanel);
			this.Controls.Add(this.widthErrorPanel);
			this.Controls.Add(this.displayPanel);
			this.Controls.Add(this.useRatioCheckBox);
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.dvdCheckBox);
			this.Controls.Add(this.restoreCheckBox);
			this.Controls.Add(this.presetButton);
			this.Controls.Add(this.colonLabel);
			this.Controls.Add(this.heightBox);
			this.Controls.Add(this.widthBox);
			this.Controls.Add(this.infoLabel);
			this.Controls.Add(this.headLabel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(173)))), ((int)(((byte)(146)))));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(728, 358);
			this.Name = "AspectRatioDialog";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Video Aspect Ratio - Preview";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AspectRatioDialog_FormClosing);
			this.Shown += new System.EventHandler(this.AspectRatioDialog_Shown);
			this.presetRatesMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.displayPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label headLabel;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.TextBox widthBox;
        private System.Windows.Forms.TextBox heightBox;
        private System.Windows.Forms.Label colonLabel;
        private DropDownButton presetButton;
        private System.Windows.Forms.CheckBox restoreCheckBox;
        private System.Windows.Forms.CheckBox dvdCheckBox;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Panel bottomPanel;
        private CustomButton cancelButton;
        private CustomButton okButton;
        private System.Windows.Forms.Panel pvsPanel;
        private System.Windows.Forms.CheckBox useRatioCheckBox;
        private System.Windows.Forms.Panel displayPanel;
        private CustomButton updateButton;
        private System.Windows.Forms.ContextMenuStrip presetRatesMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem reversedMenuItem;
        private VideoPanel videoPanel;
        private System.Windows.Forms.Panel heightErrorPanel;
        private System.Windows.Forms.Panel widthErrorPanel;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.Label sizeTitleLabel;
        private System.Windows.Forms.Label nameLabel;
    }
}