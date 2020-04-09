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
            this.TssLblTokenSii = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.IGridMain = new TenTec.Windows.iGridLib.iGrid();
            this.iGrid1DefaultCellStyle = new TenTec.Windows.iGridLib.iGCellStyle(true);
            this.iGrid1DefaultColHdrStyle = new TenTec.Windows.iGridLib.iGColHdrStyle(true);
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.TxtDteEmail = new System.Windows.Forms.TextBox();
            this.TxtRznRef = new System.Windows.Forms.TextBox();
            this.TxtFolioRef = new System.Windows.Forms.TextBox();
            this.TxtRznSocial = new System.Windows.Forms.TextBox();
            this.BtnPdfConvert = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.BtnPagar = new System.Windows.Forms.Button();
            this.TxtDateTimeEmail = new System.Windows.Forms.TextBox();
            this.BtnOutlook = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BtnFacturar = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnCreditor = new System.Windows.Forms.Button();
            this.CboParticipants = new System.Windows.Forms.ComboBox();
            this.CboYears = new System.Windows.Forms.ComboBox();
            this.CboMonths = new System.Windows.Forms.ComboBox();
            this.BtnDebtor = new System.Windows.Forms.Button();
            this.BackgroundW = new System.ComponentModel.BackgroundWorker();
            this.TxtRutParticipant = new System.Windows.Forms.TextBox();
            this.TxtCtaCteParticipant = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.StatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
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
            this.TssLblUserEmail,
            this.TssLblTokenSii});
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
            this.TssLblFechaHora.Size = new System.Drawing.Size(200, 17);
            this.TssLblFechaHora.Text = "FechaHora";
            // 
            // TssLblProgBar
            // 
            this.TssLblProgBar.Name = "TssLblProgBar";
            this.TssLblProgBar.Size = new System.Drawing.Size(430, 16);
            // 
            // TssLblMensaje
            // 
            this.TssLblMensaje.AutoSize = false;
            this.TssLblMensaje.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.TssLblMensaje.Name = "TssLblMensaje";
            this.TssLblMensaje.Size = new System.Drawing.Size(400, 17);
            this.TssLblMensaje.Text = "Message";
            this.TssLblMensaje.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TssLblUserEmail
            // 
            this.TssLblUserEmail.AutoSize = false;
            this.TssLblUserEmail.Name = "TssLblUserEmail";
            this.TssLblUserEmail.Size = new System.Drawing.Size(200, 17);
            this.TssLblUserEmail.Text = "UserMail";
            // 
            // TssLblTokenSii
            // 
            this.TssLblTokenSii.AutoSize = false;
            this.TssLblTokenSii.Name = "TssLblTokenSii";
            this.TssLblTokenSii.Size = new System.Drawing.Size(100, 17);
            this.TssLblTokenSii.Text = "TokenSii";
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
            this.SplitContainer.Panel2.Controls.Add(this.groupBox4);
            this.SplitContainer.Panel2.Controls.Add(this.BtnPdfConvert);
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
            this.IGridMain.CurCellChanged += new System.EventHandler(this.IGridMain_CurCellChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.TxtDteEmail);
            this.groupBox4.Controls.Add(this.TxtRznSocial);
            this.groupBox4.Location = new System.Drawing.Point(13, 237);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(234, 103);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Document info:";
            // 
            // TxtDteEmail
            // 
            this.TxtDteEmail.Location = new System.Drawing.Point(59, 45);
            this.TxtDteEmail.Name = "TxtDteEmail";
            this.TxtDteEmail.ReadOnly = true;
            this.TxtDteEmail.Size = new System.Drawing.Size(167, 20);
            this.TxtDteEmail.TabIndex = 9;
            // 
            // TxtRznRef
            // 
            this.TxtRznRef.Location = new System.Drawing.Point(63, 193);
            this.TxtRznRef.Name = "TxtRznRef";
            this.TxtRznRef.ReadOnly = true;
            this.TxtRznRef.Size = new System.Drawing.Size(165, 20);
            this.TxtRznRef.TabIndex = 8;
            // 
            // TxtFolioRef
            // 
            this.TxtFolioRef.Location = new System.Drawing.Point(63, 166);
            this.TxtFolioRef.Name = "TxtFolioRef";
            this.TxtFolioRef.ReadOnly = true;
            this.TxtFolioRef.Size = new System.Drawing.Size(165, 20);
            this.TxtFolioRef.TabIndex = 7;
            // 
            // TxtRznSocial
            // 
            this.TxtRznSocial.Location = new System.Drawing.Point(8, 19);
            this.TxtRznSocial.Name = "TxtRznSocial";
            this.TxtRznSocial.ReadOnly = true;
            this.TxtRznSocial.Size = new System.Drawing.Size(218, 20);
            this.TxtRznSocial.TabIndex = 0;
            // 
            // BtnPdfConvert
            // 
            this.BtnPdfConvert.Location = new System.Drawing.Point(92, 647);
            this.BtnPdfConvert.Name = "BtnPdfConvert";
            this.BtnPdfConvert.Size = new System.Drawing.Size(75, 46);
            this.BtnPdfConvert.TabIndex = 4;
            this.BtnPdfConvert.Text = "Pdf";
            this.BtnPdfConvert.UseVisualStyleBackColor = true;
            this.BtnPdfConvert.Click += new System.EventHandler(this.BtnPdfConvert_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox5);
            this.groupBox3.Controls.Add(this.BtnPagar);
            this.groupBox3.Controls.Add(this.TxtDateTimeEmail);
            this.groupBox3.Controls.Add(this.BtnOutlook);
            this.groupBox3.Location = new System.Drawing.Point(13, 426);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(234, 215);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Debtor";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button2);
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Location = new System.Drawing.Point(8, 58);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(171, 77);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "CEN";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(19, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "Aceptar";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(19, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Rechazar";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // BtnPagar
            // 
            this.BtnPagar.Location = new System.Drawing.Point(8, 29);
            this.BtnPagar.Name = "BtnPagar";
            this.BtnPagar.Size = new System.Drawing.Size(220, 23);
            this.BtnPagar.TabIndex = 6;
            this.BtnPagar.Text = "Pagar";
            this.BtnPagar.UseVisualStyleBackColor = true;
            // 
            // TxtDateTimeEmail
            // 
            this.TxtDateTimeEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtDateTimeEmail.Location = new System.Drawing.Point(99, 169);
            this.TxtDateTimeEmail.Name = "TxtDateTimeEmail";
            this.TxtDateTimeEmail.ReadOnly = true;
            this.TxtDateTimeEmail.Size = new System.Drawing.Size(129, 20);
            this.TxtDateTimeEmail.TabIndex = 2;
            // 
            // BtnOutlook
            // 
            this.BtnOutlook.Location = new System.Drawing.Point(8, 155);
            this.BtnOutlook.Name = "BtnOutlook";
            this.BtnOutlook.Size = new System.Drawing.Size(75, 46);
            this.BtnOutlook.TabIndex = 1;
            this.BtnOutlook.Text = "Outlook";
            this.BtnOutlook.UseVisualStyleBackColor = true;
            this.BtnOutlook.Click += new System.EventHandler(this.BtnOutlook_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BtnFacturar);
            this.groupBox2.Location = new System.Drawing.Point(13, 346);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 74);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Creditor";
            // 
            // BtnFacturar
            // 
            this.BtnFacturar.Enabled = false;
            this.BtnFacturar.Location = new System.Drawing.Point(27, 19);
            this.BtnFacturar.Name = "BtnFacturar";
            this.BtnFacturar.Size = new System.Drawing.Size(75, 46);
            this.BtnFacturar.TabIndex = 0;
            this.BtnFacturar.Text = "Facturar";
            this.BtnFacturar.UseVisualStyleBackColor = true;
            this.BtnFacturar.Click += new System.EventHandler(this.BtnFacturar_Click);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.label2);
            this.GroupBox1.Controls.Add(this.label1);
            this.GroupBox1.Controls.Add(this.TxtCtaCteParticipant);
            this.GroupBox1.Controls.Add(this.TxtRutParticipant);
            this.GroupBox1.Controls.Add(this.BtnCreditor);
            this.GroupBox1.Controls.Add(this.TxtRznRef);
            this.GroupBox1.Controls.Add(this.CboParticipants);
            this.GroupBox1.Controls.Add(this.TxtFolioRef);
            this.GroupBox1.Controls.Add(this.CboYears);
            this.GroupBox1.Controls.Add(this.CboMonths);
            this.GroupBox1.Controls.Add(this.BtnDebtor);
            this.GroupBox1.Location = new System.Drawing.Point(13, 12);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(234, 219);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Start";
            // 
            // BtnCreditor
            // 
            this.BtnCreditor.Location = new System.Drawing.Point(9, 99);
            this.BtnCreditor.Name = "BtnCreditor";
            this.BtnCreditor.Size = new System.Drawing.Size(97, 51);
            this.BtnCreditor.TabIndex = 4;
            this.BtnCreditor.Text = "Creditor";
            this.BtnCreditor.UseVisualStyleBackColor = true;
            this.BtnCreditor.Click += new System.EventHandler(this.BtnCreditor_Click);
            // 
            // CboParticipants
            // 
            this.CboParticipants.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboParticipants.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboParticipants.FormattingEnabled = true;
            this.CboParticipants.Location = new System.Drawing.Point(7, 46);
            this.CboParticipants.Name = "CboParticipants";
            this.CboParticipants.Size = new System.Drawing.Size(220, 21);
            this.CboParticipants.TabIndex = 0;
            this.CboParticipants.SelectionChangeCommitted += new System.EventHandler(this.CboParticipants_SelectionChangeCommitted);
            // 
            // CboYears
            // 
            this.CboYears.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboYears.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboYears.FormattingEnabled = true;
            this.CboYears.Location = new System.Drawing.Point(150, 19);
            this.CboYears.Name = "CboYears";
            this.CboYears.Size = new System.Drawing.Size(78, 21);
            this.CboYears.TabIndex = 3;
            // 
            // CboMonths
            // 
            this.CboMonths.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboMonths.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboMonths.FormattingEnabled = true;
            this.CboMonths.Location = new System.Drawing.Point(8, 19);
            this.CboMonths.Name = "CboMonths";
            this.CboMonths.Size = new System.Drawing.Size(136, 21);
            this.CboMonths.TabIndex = 2;
            // 
            // BtnDebtor
            // 
            this.BtnDebtor.Location = new System.Drawing.Point(128, 99);
            this.BtnDebtor.Name = "BtnDebtor";
            this.BtnDebtor.Size = new System.Drawing.Size(97, 51);
            this.BtnDebtor.TabIndex = 5;
            this.BtnDebtor.Text = "Debtor";
            this.BtnDebtor.UseVisualStyleBackColor = true;
            this.BtnDebtor.Click += new System.EventHandler(this.BtnDebtor_Click);
            // 
            // BackgroundW
            // 
            this.BackgroundW.WorkerReportsProgress = true;
            this.BackgroundW.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundW_DoWork);
            this.BackgroundW.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundW_ProgressChanged);
            this.BackgroundW.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundW_RunWorkerCompleted);
            // 
            // TxtRutParticipant
            // 
            this.TxtRutParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtRutParticipant.Location = new System.Drawing.Point(7, 73);
            this.TxtRutParticipant.Name = "TxtRutParticipant";
            this.TxtRutParticipant.ReadOnly = true;
            this.TxtRutParticipant.Size = new System.Drawing.Size(104, 20);
            this.TxtRutParticipant.TabIndex = 9;
            this.TxtRutParticipant.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtCtaCteParticipant
            // 
            this.TxtCtaCteParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtCtaCteParticipant.Location = new System.Drawing.Point(121, 73);
            this.TxtCtaCteParticipant.Name = "TxtCtaCteParticipant";
            this.TxtCtaCteParticipant.ReadOnly = true;
            this.TxtCtaCteParticipant.Size = new System.Drawing.Size(106, 20);
            this.TxtCtaCteParticipant.TabIndex = 10;
            this.TxtCtaCteParticipant.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 173);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "FolioRef:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 199);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "RznRef:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
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
        private System.Windows.Forms.Button BtnDebtor;
        private System.Windows.Forms.Button BtnCreditor;
        private System.Windows.Forms.ToolStripProgressBar TssLblProgBar;
        private System.ComponentModel.BackgroundWorker BackgroundW;
        private System.Windows.Forms.ToolStripStatusLabel TssLblMensaje;
        private System.Windows.Forms.ToolStripStatusLabel TssLblFechaHora;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStripStatusLabel TssLblUserEmail;
        private System.Windows.Forms.Button BtnPdfConvert;
        private System.Windows.Forms.TextBox TxtRznSocial;
        private System.Windows.Forms.Button BtnFacturar;
        private System.Windows.Forms.Button BtnOutlook;
        private System.Windows.Forms.TextBox TxtDateTimeEmail;
        private System.Windows.Forms.ToolStripStatusLabel TssLblTokenSii;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox TxtDteEmail;
        private System.Windows.Forms.TextBox TxtRznRef;
        private System.Windows.Forms.TextBox TxtFolioRef;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button BtnPagar;
        private System.Windows.Forms.TextBox TxtCtaCteParticipant;
        private System.Windows.Forms.TextBox TxtRutParticipant;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}