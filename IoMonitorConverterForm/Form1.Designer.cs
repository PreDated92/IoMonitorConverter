namespace IoMonitorConverterForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btTextConvert = new Button();
            rtbPreview = new RichTextBox();
            btCancel = new Button();
            progressBar = new ProgressBar();
            progressBarPreview = new ProgressBar();
            lblStatus = new Label();
            lblXmlFile = new Label();
            txtXmlFilePath = new TextBox();
            btBrowseXml = new Button();
            lblCsvFile = new Label();
            txtCsvFilePath = new TextBox();
            btBrowseCsv = new Button();
            lblAvailableXml = new Label();
            lblCurrentFolder = new Label();
            lvXmlFiles = new ListView();
            colFileName = new ColumnHeader();
            colFileSize = new ColumnHeader();
            colModified = new ColumnHeader();
            lblPreview = new Label();
            SuspendLayout();
            // 
            // lblAvailableXml
            // 
            lblAvailableXml.AutoSize = true;
            lblAvailableXml.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAvailableXml.Location = new Point(12, 9);
            lblAvailableXml.Name = "lblAvailableXml";
            lblAvailableXml.Size = new Size(117, 15);
            lblAvailableXml.TabIndex = 12;
            lblAvailableXml.Text = "Available XML Files:";
            // 
            // lblCurrentFolder
            // 
            lblCurrentFolder.AutoSize = true;
            lblCurrentFolder.ForeColor = SystemColors.GrayText;
            lblCurrentFolder.Location = new Point(12, 24);
            lblCurrentFolder.Name = "lblCurrentFolder";
            lblCurrentFolder.Size = new Size(101, 15);
            lblCurrentFolder.TabIndex = 13;
            lblCurrentFolder.Text = "No folder selected";
            // 
            // lvXmlFiles
            // 
            lvXmlFiles.Columns.AddRange(new ColumnHeader[] { colFileName, colFileSize, colModified });
            lvXmlFiles.FullRowSelect = true;
            lvXmlFiles.GridLines = true;
            lvXmlFiles.Location = new Point(12, 42);
            lvXmlFiles.Name = "lvXmlFiles";
            lvXmlFiles.Size = new Size(824, 120);
            lvXmlFiles.TabIndex = 14;
            lvXmlFiles.UseCompatibleStateImageBehavior = false;
            lvXmlFiles.View = View.Details;
            lvXmlFiles.DoubleClick += lvXmlFiles_DoubleClick;
            // 
            // colFileName
            // 
            colFileName.Text = "File Name";
            colFileName.Width = 550;
            // 
            // colFileSize
            // 
            colFileSize.Text = "Size";
            colFileSize.Width = 100;
            // 
            // colModified
            // 
            colModified.Text = "Modified";
            colModified.Width = 150;
            // 
            // lblXmlFile
            // 
            lblXmlFile.AutoSize = true;
            lblXmlFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblXmlFile.Location = new Point(12, 165);
            lblXmlFile.Name = "lblXmlFile";
            lblXmlFile.Size = new Size(88, 15);
            lblXmlFile.TabIndex = 6;
            lblXmlFile.Text = "XML Input File:";
            // 
            // txtXmlFilePath
            // 
            txtXmlFilePath.Location = new Point(12, 183);
            txtXmlFilePath.Name = "txtXmlFilePath";
            txtXmlFilePath.ReadOnly = true;
            txtXmlFilePath.Size = new Size(730, 23);
            txtXmlFilePath.TabIndex = 7;
            // 
            // btBrowseXml
            // 
            btBrowseXml.Location = new Point(748, 182);
            btBrowseXml.Name = "btBrowseXml";
            btBrowseXml.Size = new Size(88, 23);
            btBrowseXml.TabIndex = 8;
            btBrowseXml.Text = "Browse...";
            btBrowseXml.UseVisualStyleBackColor = true;
            btBrowseXml.Click += btBrowseXml_Click;
            // 
            // lblCsvFile
            // 
            lblCsvFile.AutoSize = true;
            lblCsvFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCsvFile.Location = new Point(12, 209);
            lblCsvFile.Name = "lblCsvFile";
            lblCsvFile.Size = new Size(98, 15);
            lblCsvFile.TabIndex = 9;
            lblCsvFile.Text = "CSV Output File:";
            // 
            // txtCsvFilePath
            // 
            txtCsvFilePath.Location = new Point(12, 227);
            txtCsvFilePath.Name = "txtCsvFilePath";
            txtCsvFilePath.ReadOnly = true;
            txtCsvFilePath.Size = new Size(730, 23);
            txtCsvFilePath.TabIndex = 10;
            // 
            // btBrowseCsv
            // 
            btBrowseCsv.Location = new Point(748, 226);
            btBrowseCsv.Name = "btBrowseCsv";
            btBrowseCsv.Size = new Size(88, 23);
            btBrowseCsv.TabIndex = 11;
            btBrowseCsv.Text = "Browse...";
            btBrowseCsv.UseVisualStyleBackColor = true;
            btBrowseCsv.Click += btBrowseCsv_Click;
            // 
            // lblPreview
            // 
            lblPreview.AutoSize = true;
            lblPreview.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPreview.Location = new Point(12, 253);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(163, 15);
            lblPreview.TabIndex = 15;
            lblPreview.Text = "CSV Preview (first 10 records):";
            // 
            // btTextConvert
            // 
            btTextConvert.Location = new Point(12, 271);
            btTextConvert.Name = "btTextConvert";
            btTextConvert.Size = new Size(170, 30);
            btTextConvert.TabIndex = 1;
            btTextConvert.Text = "Convert to CSV";
            btTextConvert.UseVisualStyleBackColor = true;
            btTextConvert.Click += btTextConvert_Click;
            // 
            // rtbPreview
            // 
            rtbPreview.Font = new Font("Consolas", 9F);
            rtbPreview.Location = new Point(188, 271);
            rtbPreview.Name = "rtbPreview";
            rtbPreview.ReadOnly = true;
            rtbPreview.Size = new Size(648, 280);
            rtbPreview.TabIndex = 2;
            rtbPreview.Text = "Select an XML file to preview the CSV output...";
            // 
            // btCancel
            // 
            btCancel.Enabled = false;
            btCancel.Location = new Point(12, 307);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(170, 30);
            btCancel.TabIndex = 3;
            btCancel.Text = "Cancel";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 343);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(170, 23);
            progressBar.TabIndex = 4;
            progressBar.Visible = false;
            // 
            // progressBarPreview
            // 
            progressBarPreview.Location = new Point(12, 372);
            progressBarPreview.Name = "progressBarPreview";
            progressBarPreview.Size = new Size(170, 23);
            progressBarPreview.TabIndex = 16;
            progressBarPreview.Visible = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 398);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Ready";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(848, 563);
            Controls.Add(progressBarPreview);
            Controls.Add(lblPreview);
            Controls.Add(lvXmlFiles);
            Controls.Add(lblCurrentFolder);
            Controls.Add(lblAvailableXml);
            Controls.Add(btBrowseCsv);
            Controls.Add(txtCsvFilePath);
            Controls.Add(lblCsvFile);
            Controls.Add(btBrowseXml);
            Controls.Add(txtXmlFilePath);
            Controls.Add(lblXmlFile);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btCancel);
            Controls.Add(rtbPreview);
            Controls.Add(btTextConvert);
            Name = "Form1";
            Text = "IO Monitor Converter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btTextConvert;
        private RichTextBox rtbPreview;
        private Button btCancel;
        private ProgressBar progressBar;
        private ProgressBar progressBarPreview;
        private Label lblStatus;
        private Label lblXmlFile;
        private TextBox txtXmlFilePath;
        private Button btBrowseXml;
        private Label lblCsvFile;
        private TextBox txtCsvFilePath;
        private Button btBrowseCsv;
        private Label lblAvailableXml;
        private Label lblCurrentFolder;
        private ListView lvXmlFiles;
        private ColumnHeader colFileName;
        private ColumnHeader colFileSize;
        private ColumnHeader colModified;
        private Label lblPreview;
    }
}
