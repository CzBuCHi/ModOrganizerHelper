namespace ModOrganizerHelper
{
    partial class FormMain
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
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelIniPath = new System.Windows.Forms.Label();
            this.buttonFindIni = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonSelectSavePath = new System.Windows.Forms.Button();
            this.labelSavesDirectoryPath = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelProfile = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonDelete
            // 
            this.buttonDelete.Enabled = false;
            this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDelete.Location = new System.Drawing.Point(290, 130);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(100, 30);
            this.buttonDelete.TabIndex = 0;
            this.buttonDelete.Text = "Delete all links";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonUpdate.Location = new System.Drawing.Point(400, 130);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(100, 30);
            this.buttonUpdate.TabIndex = 1;
            this.buttonUpdate.Text = "Update links";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "MO data directory";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelIniPath
            // 
            this.labelIniPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelIniPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ModOrganizerHelper.Properties.Settings.Default, "IniPath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.labelIniPath.Location = new System.Drawing.Point(110, 10);
            this.labelIniPath.Name = "labelIniPath";
            this.labelIniPath.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelIniPath.Size = new System.Drawing.Size(350, 30);
            this.labelIniPath.TabIndex = 3;
            this.labelIniPath.Text = global::ModOrganizerHelper.Properties.Settings.Default.IniPath;
            this.labelIniPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelIniPath.DoubleClick += new System.EventHandler(this.labelIniPath_DoubleClick);
            // 
            // buttonFindIni
            // 
            this.buttonFindIni.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFindIni.Location = new System.Drawing.Point(470, 10);
            this.buttonFindIni.Name = "buttonFindIni";
            this.buttonFindIni.Size = new System.Drawing.Size(30, 30);
            this.buttonFindIni.TabIndex = 4;
            this.buttonFindIni.Text = "...";
            this.buttonFindIni.UseVisualStyleBackColor = true;
            this.buttonFindIni.Click += new System.EventHandler(this.buttonFindIni_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 26);
            this.label2.TabIndex = 6;
            this.label2.Text = "Profile";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonSelectSavePath
            // 
            this.buttonSelectSavePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSelectSavePath.Location = new System.Drawing.Point(470, 50);
            this.buttonSelectSavePath.Name = "buttonSelectSavePath";
            this.buttonSelectSavePath.Size = new System.Drawing.Size(30, 30);
            this.buttonSelectSavePath.TabIndex = 10;
            this.buttonSelectSavePath.Text = "...";
            this.buttonSelectSavePath.UseVisualStyleBackColor = true;
            this.buttonSelectSavePath.Click += new System.EventHandler(this.buttonSelectSavePath_Click);
            // 
            // labelSavesDirectoryPath
            // 
            this.labelSavesDirectoryPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelSavesDirectoryPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ModOrganizerHelper.Properties.Settings.Default, "SavesPath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.labelSavesDirectoryPath.Location = new System.Drawing.Point(110, 50);
            this.labelSavesDirectoryPath.Name = "labelSavesDirectoryPath";
            this.labelSavesDirectoryPath.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelSavesDirectoryPath.Size = new System.Drawing.Size(350, 30);
            this.labelSavesDirectoryPath.TabIndex = 9;
            this.labelSavesDirectoryPath.Text = global::ModOrganizerHelper.Properties.Settings.Default.SavesPath;
            this.labelSavesDirectoryPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSavesDirectoryPath.DoubleClick += new System.EventHandler(this.labelSavesDirectoryPath_DoubleClick);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(10, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 30);
            this.label4.TabIndex = 8;
            this.label4.Text = "Saves directory";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Location = new System.Drawing.Point(10, 170);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(490, 200);
            this.textBoxLog.TabIndex = 11;
            // 
            // labelProfile
            // 
            this.labelProfile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelProfile.Location = new System.Drawing.Point(110, 90);
            this.labelProfile.Name = "labelProfile";
            this.labelProfile.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelProfile.Size = new System.Drawing.Size(390, 30);
            this.labelProfile.TabIndex = 12;
            this.labelProfile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 376);
            this.Controls.Add(this.labelProfile);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.buttonSelectSavePath);
            this.Controls.Add(this.labelSavesDirectoryPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonFindIni);
            this.Controls.Add(this.labelIniPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonDelete);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ModOrganizerHelper.Properties.Settings.Default, "IniPath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormMain";
            this.Text = global::ModOrganizerHelper.Properties.Settings.Default.IniPath;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelIniPath;
        private System.Windows.Forms.Button buttonFindIni;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonSelectSavePath;
        private System.Windows.Forms.Label labelSavesDirectoryPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelProfile;
    }
}

