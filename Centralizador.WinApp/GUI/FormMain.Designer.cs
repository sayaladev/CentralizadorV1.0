namespace Centralizador.WinApp.GUI
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
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.TssLblFechaHora = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblProgBar = new System.Windows.Forms.ToolStripProgressBar();
            this.TssLblMensaje = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblUserEmail = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.IGridMain = new TenTec.Windows.iGridLib.iGrid();
            this.iGrid1DefaultCellStyle = new TenTec.Windows.iGridLib.iGCellStyle(true);
            this.iGrid1DefaultColHdrStyle = new TenTec.Windows.iGridLib.iGColHdrStyle(true);
            this.BtnPdfDebtor = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TxtDateTimeEmail = new System.Windows.Forms.TextBox();
            this.BtnOutlook = new System.Windows.Forms.Button();
            this.BtnFacturar = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.CboParticipants = new System.Windows.Forms.ComboBox();
            this.BtnDebitor = new System.Windows.Forms.Button();
            this.BtnCreditor = new System.Windows.Forms.Button();
            this.CboYears = new System.Windows.Forms.ComboBox();
            this.CboMonths = new System.Windows.Forms.ComboBox();
            this.BackgroundW = new System.ComponentModel.BackgroundWorker();
            this.BtnPdfCreditor = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.StatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TssLblFechaHora,
            this.TssLblProgBar,
            this.TssLblMensaje,
            this.TssLblUserEmail});
            this.StatusStrip.Location = new System.Drawing.Point(0, 707);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1350, 22);
            this.StatusStrip.TabIndex = 0;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // TssLblFechaHora
            // 
            this.TssLblFechaHora.AutoSize = false;
            this.TssLblFechaHora.Name = "TssLblFechaHora";
            this.TssLblFechaHora.Size = new System.Drawing.Size(250, 17);
            // 
            // TssLblProgBar
            // 
            this.TssLblProgBar.Name = "TssLblProgBar";
            this.TssLblProgBar.Size = new System.Drawing.Size(300, 16);
            // 
            // TssLblMensaje
            // 
            this.TssLblMensaje.AutoSize = false;
            this.TssLblMensaje.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.TssLblMensaje.Name = "TssLblMensaje";
            this.TssLblMensaje.Size = new System.Drawing.Size(400, 17);
            this.TssLblMensaje.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TssLblUserEmail
            // 
            this.TssLblUserEmail.AutoSize = false;
            this.TssLblUserEmail.Name = "TssLblUserEmail";
            this.TssLblUserEmail.Size = new System.Drawing.Size(200, 17);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.IGridMain);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.groupBox3);
            this.SplitContainer.Panel2.Controls.Add(this.groupBox2);
            this.SplitContainer.Panel2.Controls.Add(this.GroupBox1);
            this.SplitContainer.Size = new System.Drawing.Size(1350, 707);
            this.SplitContainer.SplitterDistance = 1087;
            this.SplitContainer.TabIndex = 1;
            // 
            // IGridMain
            // 
            this.IGridMain.DefaultCol.CellStyle = this.iGrid1DefaultCellStyle;
            this.IGridMain.DefaultCol.ColHdrStyle = this.iGrid1DefaultColHdrStyle;
            this.IGridMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IGridMain.Location = new System.Drawing.Point(0, 0);
            this.IGridMain.Name = "IGridMain";
            this.IGridMain.Size = new System.Drawing.Size(1087, 707);
            this.IGridMain.TabIndex = 0;
            this.IGridMain.CustomDrawCellForeground += new TenTec.Windows.iGridLib.iGCustomDrawCellEventHandler(this.IGridMain_CustomDrawCellForeground);
            this.IGridMain.ColHdrMouseDown += new TenTec.Windows.iGridLib.iGColHdrMouseDownEventHandler(this.IGridMain_ColHdrMouseDown);
            // 
            // BtnPdfDebtor
            // 
            this.BtnPdfDebtor.Location = new System.Drawing.Point(115, 80);
            this.BtnPdfDebtor.Name = "BtnPdfDebtor";
            this.BtnPdfDebtor.Size = new System.Drawing.Size(75, 39);
            this.BtnPdfDebtor.TabIndex = 4;
            this.BtnPdfDebtor.Text = "Pdf";
            this.BtnPdfDebtor.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.BtnPdfDebtor);
            this.groupBox3.Controls.Add(this.TxtDateTimeEmail);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.BtnOutlook);
            this.groupBox3.Controls.Add(this.BtnDebitor);
            this.groupBox3.Location = new System.Drawing.Point(13, 351);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(234, 269);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Debtor";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 29);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(222, 20);
            this.textBox1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.BtnPdfCreditor);
            this.groupBox2.Controls.Add(this.BtnCreditor);
            this.groupBox2.Controls.Add(this.BtnFacturar);
            this.groupBox2.Location = new System.Drawing.Point(13, 120);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 204);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Creditor";
            // 
            // TxtDateTimeEmail
            // 
            this.TxtDateTimeEmail.Location = new System.Drawing.Point(18, 139);
            this.TxtDateTimeEmail.Name = "TxtDateTimeEmail";
            this.TxtDateTimeEmail.Size = new System.Drawing.Size(100, 20);
            this.TxtDateTimeEmail.TabIndex = 2;
            // 
            // BtnOutlook
            // 
            this.BtnOutlook.Location = new System.Drawing.Point(140, 137);
            this.BtnOutlook.Name = "BtnOutlook";
            this.BtnOutlook.Size = new System.Drawing.Size(75, 23);
            this.BtnOutlook.TabIndex = 1;
            this.BtnOutlook.Text = "Outlook";
            this.BtnOutlook.UseVisualStyleBackColor = true;
            this.BtnOutlook.Click += new System.EventHandler(this.BtnOutlook_Click);
            // 
            // BtnFacturar
            // 
            this.BtnFacturar.Location = new System.Drawing.Point(18, 150);
            this.BtnFacturar.Name = "BtnFacturar";
            this.BtnFacturar.Size = new System.Drawing.Size(75, 23);
            this.BtnFacturar.TabIndex = 0;
            this.BtnFacturar.Text = "Facturar";
            this.BtnFacturar.UseVisualStyleBackColor = true;
            this.BtnFacturar.Click += new System.EventHandler(this.BtnFacturar_Click);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox1.Controls.Add(this.CboParticipants);
            this.GroupBox1.Controls.Add(this.CboYears);
            this.GroupBox1.Controls.Add(this.CboMonths);
            this.GroupBox1.Location = new System.Drawing.Point(13, 12);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(234, 93);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Inicio";
            // 
            // CboParticipants
            // 
            this.CboParticipants.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboParticipants.FormattingEnabled = true;
            this.CboParticipants.Location = new System.Drawing.Point(8, 46);
            this.CboParticipants.Name = "CboParticipants";
            this.CboParticipants.Size = new System.Drawing.Size(220, 21);
            this.CboParticipants.TabIndex = 0;
            this.CboParticipants.SelectionChangeCommitted += new System.EventHandler(this.CboParticipants_SelectionChangeCommitted);
            // 
            // BtnDebitor
            // 
            this.BtnDebitor.Location = new System.Drawing.Point(18, 73);
            this.BtnDebitor.Name = "BtnDebitor";
            this.BtnDebitor.Size = new System.Drawing.Size(75, 46);
            this.BtnDebitor.TabIndex = 5;
            this.BtnDebitor.Text = "Debtor";
            this.BtnDebitor.UseVisualStyleBackColor = true;
            this.BtnDebitor.Click += new System.EventHandler(this.BtnDebitor_Click);
            // 
            // BtnCreditor
            // 
            this.BtnCreditor.Location = new System.Drawing.Point(18, 42);
            this.BtnCreditor.Name = "BtnCreditor";
            this.BtnCreditor.Size = new System.Drawing.Size(75, 46);
            this.BtnCreditor.TabIndex = 4;
            this.BtnCreditor.Text = "Creditor";
            this.BtnCreditor.UseVisualStyleBackColor = true;
            this.BtnCreditor.Click += new System.EventHandler(this.BtnCreditor_Click);
            // 
            // CboYears
            // 
            this.CboYears.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboYears.FormattingEnabled = true;
            this.CboYears.Location = new System.Drawing.Point(150, 19);
            this.CboYears.Name = "CboYears";
            this.CboYears.Size = new System.Drawing.Size(78, 21);
            this.CboYears.TabIndex = 3;
            // 
            // CboMonths
            // 
            this.CboMonths.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboMonths.FormattingEnabled = true;
            this.CboMonths.Location = new System.Drawing.Point(8, 19);
            this.CboMonths.Name = "CboMonths";
            this.CboMonths.Size = new System.Drawing.Size(136, 21);
            this.CboMonths.TabIndex = 2;
            // 
            // BackgroundW
            // 
            this.BackgroundW.WorkerReportsProgress = true;
            this.BackgroundW.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundW_DoWork);
            this.BackgroundW.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundW_ProgressChanged);
            this.BackgroundW.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundW_RunWorkerCompleted);
            // 
            // BtnPdfCreditor
            // 
            this.BtnPdfCreditor.Location = new System.Drawing.Point(129, 49);
            this.BtnPdfCreditor.Name = "BtnPdfCreditor";
            this.BtnPdfCreditor.Size = new System.Drawing.Size(75, 39);
            this.BtnPdfCreditor.TabIndex = 5;
            this.BtnPdfCreditor.Text = "Pdf";
            this.BtnPdfCreditor.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(27, 190);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this.SplitContainer);
            this.Controls.Add(this.StatusStrip);
            this.Name = "FormMain";
            this.Text = "FormMain";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.SplitContainer SplitContainer;
        private TenTec.Windows.iGridLib.iGrid IGridMain;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1DefaultCellStyle;
        private TenTec.Windows.iGridLib.iGColHdrStyle iGrid1DefaultColHdrStyle;
        private System.Windows.Forms.GroupBox GroupBox1;
        private System.Windows.Forms.ComboBox CboParticipants;
        private System.Windows.Forms.ComboBox CboYears;
        private System.Windows.Forms.ComboBox CboMonths;
        private System.Windows.Forms.Button BtnDebitor;
        private System.Windows.Forms.Button BtnCreditor;
        private System.Windows.Forms.ToolStripProgressBar TssLblProgBar;
        private System.ComponentModel.BackgroundWorker BackgroundW;
        private System.Windows.Forms.ToolStripStatusLabel TssLblMensaje;
        private System.Windows.Forms.ToolStripStatusLabel TssLblFechaHora;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStripStatusLabel TssLblUserEmail;
        private System.Windows.Forms.Button BtnPdfDebtor;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button BtnFacturar;
        private System.Windows.Forms.Button BtnOutlook;
        private System.Windows.Forms.TextBox TxtDateTimeEmail;
        private System.Windows.Forms.Button BtnPdfCreditor;
        private System.Windows.Forms.Button button1;
    }
}