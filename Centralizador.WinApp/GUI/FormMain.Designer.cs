﻿namespace Centralizador.WinApp.GUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.TssLblFechaHora = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblUserEmail = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblProgBar = new System.Windows.Forms.ToolStripProgressBar();
            this.TssLblMensaje = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.FpicBoxSearch = new System.Windows.Forms.PictureBox();
            this.IGridMain = new TenTec.Windows.iGridLib.iGrid();
            this.iGrid1DefaultCellStyle = new TenTec.Windows.iGridLib.iGCellStyle(true);
            this.iGrid1DefaultColHdrStyle = new TenTec.Windows.iGridLib.iGColHdrStyle(true);
            this.TxtDateTimeEmail = new System.Windows.Forms.TextBox();
            this.BtnOutlook = new System.Windows.Forms.Button();
            this.BtnExcelConvert = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.TxtTpoDocRef = new System.Windows.Forms.TextBox();
            this.TxtFmaPago = new System.Windows.Forms.TextBox();
            this.TxtDscItem = new System.Windows.Forms.TextBox();
            this.TxtRznRef = new System.Windows.Forms.TextBox();
            this.TxtFolioRef = new System.Windows.Forms.TextBox();
            this.TxtNmbItem = new System.Windows.Forms.TextBox();
            this.BtnPdfConvert = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ChkIncludeCEN = new System.Windows.Forms.CheckBox();
            this.BtnHiperLink = new System.Windows.Forms.Button();
            this.BtnInsertNv = new System.Windows.Forms.Button();
            this.ChkIncludeReclaimed = new System.Windows.Forms.CheckBox();
            this.BtnPagar = new System.Windows.Forms.Button();
            this.BtnInsertRef = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.TxtCtaCteParticipant = new System.Windows.Forms.TextBox();
            this.TxtRutParticipant = new System.Windows.Forms.TextBox();
            this.BtnCreditor = new System.Windows.Forms.Button();
            this.CboParticipants = new System.Windows.Forms.ComboBox();
            this.CboYears = new System.Windows.Forms.ComboBox();
            this.CboMonths = new System.Windows.Forms.ComboBox();
            this.BtnDebtor = new System.Windows.Forms.Button();
            this.FListPics = new System.Windows.Forms.ImageList(this.components);
            this.ChkIncludeAll = new System.Windows.Forms.CheckBox();
            this.StatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FpicBoxSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TssLblFechaHora,
            this.TssLblUserEmail,
            this.TssLblProgBar,
            this.TssLblMensaje});
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
            this.TssLblFechaHora.Size = new System.Drawing.Size(180, 17);
            // 
            // TssLblUserEmail
            // 
            this.TssLblUserEmail.AutoSize = false;
            this.TssLblUserEmail.Name = "TssLblUserEmail";
            this.TssLblUserEmail.Size = new System.Drawing.Size(200, 17);
            // 
            // TssLblProgBar
            // 
            this.TssLblProgBar.Name = "TssLblProgBar";
            this.TssLblProgBar.Size = new System.Drawing.Size(350, 16);
            // 
            // TssLblMensaje
            // 
            this.TssLblMensaje.AutoSize = false;
            this.TssLblMensaje.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.TssLblMensaje.Name = "TssLblMensaje";
            this.TssLblMensaje.Size = new System.Drawing.Size(600, 17);
            this.TssLblMensaje.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.FpicBoxSearch);
            this.SplitContainer.Panel1.Controls.Add(this.IGridMain);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.TxtDateTimeEmail);
            this.SplitContainer.Panel2.Controls.Add(this.BtnOutlook);
            this.SplitContainer.Panel2.Controls.Add(this.BtnExcelConvert);
            this.SplitContainer.Panel2.Controls.Add(this.groupBox4);
            this.SplitContainer.Panel2.Controls.Add(this.BtnPdfConvert);
            this.SplitContainer.Panel2.Controls.Add(this.groupBox2);
            this.SplitContainer.Panel2.Controls.Add(this.GroupBox1);
            this.SplitContainer.Size = new System.Drawing.Size(1350, 707);
            this.SplitContainer.SplitterDistance = 1128;
            this.SplitContainer.TabIndex = 1;
            // 
            // FpicBoxSearch
            // 
            this.FpicBoxSearch.Image = ((System.Drawing.Image)(resources.GetObject("FpicBoxSearch.Image")));
            this.FpicBoxSearch.Location = new System.Drawing.Point(692, 394);
            this.FpicBoxSearch.Name = "FpicBoxSearch";
            this.FpicBoxSearch.Size = new System.Drawing.Size(36, 25);
            this.FpicBoxSearch.TabIndex = 3;
            this.FpicBoxSearch.TabStop = false;
            this.FpicBoxSearch.Visible = false;
            // 
            // IGridMain
            // 
            this.IGridMain.DefaultCol.CellStyle = this.iGrid1DefaultCellStyle;
            this.IGridMain.DefaultCol.ColHdrStyle = this.iGrid1DefaultColHdrStyle;
            this.IGridMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IGridMain.Location = new System.Drawing.Point(0, 0);
            this.IGridMain.Name = "IGridMain";
            this.IGridMain.Size = new System.Drawing.Size(1128, 707);
            this.IGridMain.TabIndex = 0;
            this.IGridMain.CustomDrawCellForeground += new TenTec.Windows.iGridLib.iGCustomDrawCellEventHandler(this.IGridMain_CustomDrawCellForeground);
            this.IGridMain.CustomDrawCellEllipsisButtonForeground += new TenTec.Windows.iGridLib.iGCustomDrawEllipsisButtonEventHandler(this.IGridMain_CustomDrawCellEllipsisButtonForeground);
            this.IGridMain.ColHdrMouseDown += new TenTec.Windows.iGridLib.iGColHdrMouseDownEventHandler(this.IGridMain_ColHdrMouseDown);
            this.IGridMain.CellEllipsisButtonClick += new TenTec.Windows.iGridLib.iGEllipsisButtonClickEventHandler(this.IGridMain_CellEllipsisButtonClick);
            this.IGridMain.CurRowChanged += new System.EventHandler(this.IGridMain_CurRowChanged);
            this.IGridMain.RequestCellToolTipText += new TenTec.Windows.iGridLib.iGRequestCellToolTipTextEventHandler(this.IGridMain_RequestCellToolTipText);
            // 
            // TxtDateTimeEmail
            // 
            this.TxtDateTimeEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtDateTimeEmail.Location = new System.Drawing.Point(82, 605);
            this.TxtDateTimeEmail.Name = "TxtDateTimeEmail";
            this.TxtDateTimeEmail.ReadOnly = true;
            this.TxtDateTimeEmail.Size = new System.Drawing.Size(120, 20);
            this.TxtDateTimeEmail.TabIndex = 2;
            // 
            // BtnOutlook
            // 
            this.BtnOutlook.Image = ((System.Drawing.Image)(resources.GetObject("BtnOutlook.Image")));
            this.BtnOutlook.Location = new System.Drawing.Point(12, 591);
            this.BtnOutlook.Name = "BtnOutlook";
            this.BtnOutlook.Size = new System.Drawing.Size(56, 46);
            this.BtnOutlook.TabIndex = 1;
            this.BtnOutlook.UseVisualStyleBackColor = true;
            this.BtnOutlook.Click += new System.EventHandler(this.BtnOutlook_Click);
            // 
            // BtnExcelConvert
            // 
            this.BtnExcelConvert.Image = ((System.Drawing.Image)(resources.GetObject("BtnExcelConvert.Image")));
            this.BtnExcelConvert.Location = new System.Drawing.Point(120, 647);
            this.BtnExcelConvert.Name = "BtnExcelConvert";
            this.BtnExcelConvert.Size = new System.Drawing.Size(56, 46);
            this.BtnExcelConvert.TabIndex = 8;
            this.BtnExcelConvert.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.TxtTpoDocRef);
            this.groupBox4.Controls.Add(this.TxtFmaPago);
            this.groupBox4.Controls.Add(this.TxtDscItem);
            this.groupBox4.Controls.Add(this.TxtRznRef);
            this.groupBox4.Controls.Add(this.TxtFolioRef);
            this.groupBox4.Controls.Add(this.TxtNmbItem);
            this.groupBox4.Location = new System.Drawing.Point(2, 184);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(212, 173);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "INVOICE INFO:";
            // 
            // TxtTpoDocRef
            // 
            this.TxtTpoDocRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtTpoDocRef.Location = new System.Drawing.Point(10, 100);
            this.TxtTpoDocRef.Name = "TxtTpoDocRef";
            this.TxtTpoDocRef.ReadOnly = true;
            this.TxtTpoDocRef.Size = new System.Drawing.Size(31, 18);
            this.TxtTpoDocRef.TabIndex = 11;
            // 
            // TxtFmaPago
            // 
            this.TxtFmaPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtFmaPago.Location = new System.Drawing.Point(44, 100);
            this.TxtFmaPago.Name = "TxtFmaPago";
            this.TxtFmaPago.ReadOnly = true;
            this.TxtFmaPago.Size = new System.Drawing.Size(42, 18);
            this.TxtFmaPago.TabIndex = 9;
            this.TxtFmaPago.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtDscItem
            // 
            this.TxtDscItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtDscItem.Location = new System.Drawing.Point(10, 124);
            this.TxtDscItem.Multiline = true;
            this.TxtDscItem.Name = "TxtDscItem";
            this.TxtDscItem.ReadOnly = true;
            this.TxtDscItem.Size = new System.Drawing.Size(192, 28);
            this.TxtDscItem.TabIndex = 10;
            // 
            // TxtRznRef
            // 
            this.TxtRznRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtRznRef.Location = new System.Drawing.Point(10, 76);
            this.TxtRznRef.Name = "TxtRznRef";
            this.TxtRznRef.ReadOnly = true;
            this.TxtRznRef.Size = new System.Drawing.Size(165, 18);
            this.TxtRznRef.TabIndex = 8;
            // 
            // TxtFolioRef
            // 
            this.TxtFolioRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtFolioRef.Location = new System.Drawing.Point(92, 100);
            this.TxtFolioRef.Name = "TxtFolioRef";
            this.TxtFolioRef.ReadOnly = true;
            this.TxtFolioRef.Size = new System.Drawing.Size(110, 18);
            this.TxtFolioRef.TabIndex = 7;
            // 
            // TxtNmbItem
            // 
            this.TxtNmbItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtNmbItem.Location = new System.Drawing.Point(10, 19);
            this.TxtNmbItem.Multiline = true;
            this.TxtNmbItem.Name = "TxtNmbItem";
            this.TxtNmbItem.ReadOnly = true;
            this.TxtNmbItem.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtNmbItem.Size = new System.Drawing.Size(192, 51);
            this.TxtNmbItem.TabIndex = 1;
            // 
            // BtnPdfConvert
            // 
            this.BtnPdfConvert.Image = ((System.Drawing.Image)(resources.GetObject("BtnPdfConvert.Image")));
            this.BtnPdfConvert.Location = new System.Drawing.Point(42, 647);
            this.BtnPdfConvert.Name = "BtnPdfConvert";
            this.BtnPdfConvert.Size = new System.Drawing.Size(56, 46);
            this.BtnPdfConvert.TabIndex = 4;
            this.BtnPdfConvert.UseVisualStyleBackColor = true;
            this.BtnPdfConvert.Click += new System.EventHandler(this.BtnPdfConvert_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.groupBox2.Controls.Add(this.ChkIncludeAll);
            this.groupBox2.Controls.Add(this.ChkIncludeCEN);
            this.groupBox2.Controls.Add(this.BtnHiperLink);
            this.groupBox2.Controls.Add(this.BtnInsertNv);
            this.groupBox2.Controls.Add(this.ChkIncludeReclaimed);
            this.groupBox2.Controls.Add(this.BtnPagar);
            this.groupBox2.Controls.Add(this.BtnInsertRef);
            this.groupBox2.Location = new System.Drawing.Point(2, 363);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(212, 208);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            // 
            // ChkIncludeCEN
            // 
            this.ChkIncludeCEN.AutoSize = true;
            this.ChkIncludeCEN.Location = new System.Drawing.Point(118, 107);
            this.ChkIncludeCEN.Name = "ChkIncludeCEN";
            this.ChkIncludeCEN.Size = new System.Drawing.Size(72, 17);
            this.ChkIncludeCEN.TabIndex = 12;
            this.ChkIncludeCEN.Text = "Only CEN";
            this.ChkIncludeCEN.UseVisualStyleBackColor = true;
            // 
            // BtnHiperLink
            // 
            this.BtnHiperLink.BackColor = System.Drawing.Color.Green;
            this.BtnHiperLink.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnHiperLink.BackgroundImage")));
            this.BtnHiperLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.BtnHiperLink.Location = new System.Drawing.Point(40, 151);
            this.BtnHiperLink.Name = "BtnHiperLink";
            this.BtnHiperLink.Size = new System.Drawing.Size(125, 51);
            this.BtnHiperLink.TabIndex = 11;
            this.BtnHiperLink.UseVisualStyleBackColor = false;
            this.BtnHiperLink.Click += new System.EventHandler(this.BtnHiperLink_Click);
            this.BtnHiperLink.MouseHover += new System.EventHandler(this.BtnHiperLink_MouseHover);
            // 
            // BtnInsertNv
            // 
            this.BtnInsertNv.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.BtnInsertNv.Location = new System.Drawing.Point(21, 19);
            this.BtnInsertNv.Name = "BtnInsertNv";
            this.BtnInsertNv.Size = new System.Drawing.Size(84, 38);
            this.BtnInsertNv.TabIndex = 10;
            this.BtnInsertNv.Text = "Insert NV";
            this.BtnInsertNv.UseVisualStyleBackColor = false;
            this.BtnInsertNv.Click += new System.EventHandler(this.BtnInsertNv_Click);
            // 
            // ChkIncludeReclaimed
            // 
            this.ChkIncludeReclaimed.AutoSize = true;
            this.ChkIncludeReclaimed.Location = new System.Drawing.Point(124, 31);
            this.ChkIncludeReclaimed.Name = "ChkIncludeReclaimed";
            this.ChkIncludeReclaimed.Size = new System.Drawing.Size(76, 17);
            this.ChkIncludeReclaimed.TabIndex = 9;
            this.ChkIncludeReclaimed.Text = "Reclaimed";
            this.ChkIncludeReclaimed.UseVisualStyleBackColor = true;
            // 
            // BtnPagar
            // 
            this.BtnPagar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.BtnPagar.Location = new System.Drawing.Point(21, 107);
            this.BtnPagar.Name = "BtnPagar";
            this.BtnPagar.Size = new System.Drawing.Size(89, 38);
            this.BtnPagar.TabIndex = 6;
            this.BtnPagar.Text = "Pagar";
            this.BtnPagar.UseVisualStyleBackColor = false;
            this.BtnPagar.Click += new System.EventHandler(this.BtnPagar_Click);
            // 
            // BtnInsertRef
            // 
            this.BtnInsertRef.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.BtnInsertRef.Location = new System.Drawing.Point(21, 63);
            this.BtnInsertRef.Name = "BtnInsertRef";
            this.BtnInsertRef.Size = new System.Drawing.Size(154, 38);
            this.BtnInsertRef.TabIndex = 8;
            this.BtnInsertRef.Text = "Insert References";
            this.BtnInsertRef.UseVisualStyleBackColor = false;
            this.BtnInsertRef.Click += new System.EventHandler(this.BtnInsertRef_Click);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox1.Controls.Add(this.TxtCtaCteParticipant);
            this.GroupBox1.Controls.Add(this.TxtRutParticipant);
            this.GroupBox1.Controls.Add(this.BtnCreditor);
            this.GroupBox1.Controls.Add(this.CboParticipants);
            this.GroupBox1.Controls.Add(this.CboYears);
            this.GroupBox1.Controls.Add(this.CboMonths);
            this.GroupBox1.Controls.Add(this.BtnDebtor);
            this.GroupBox1.Location = new System.Drawing.Point(3, 3);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(212, 175);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            // 
            // TxtCtaCteParticipant
            // 
            this.TxtCtaCteParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtCtaCteParticipant.Location = new System.Drawing.Point(110, 73);
            this.TxtCtaCteParticipant.Name = "TxtCtaCteParticipant";
            this.TxtCtaCteParticipant.ReadOnly = true;
            this.TxtCtaCteParticipant.Size = new System.Drawing.Size(91, 21);
            this.TxtCtaCteParticipant.TabIndex = 10;
            this.TxtCtaCteParticipant.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtRutParticipant
            // 
            this.TxtRutParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtRutParticipant.Location = new System.Drawing.Point(10, 73);
            this.TxtRutParticipant.Name = "TxtRutParticipant";
            this.TxtRutParticipant.ReadOnly = true;
            this.TxtRutParticipant.Size = new System.Drawing.Size(94, 21);
            this.TxtRutParticipant.TabIndex = 9;
            this.TxtRutParticipant.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // BtnCreditor
            // 
            this.BtnCreditor.Location = new System.Drawing.Point(10, 99);
            this.BtnCreditor.Name = "BtnCreditor";
            this.BtnCreditor.Size = new System.Drawing.Size(85, 51);
            this.BtnCreditor.TabIndex = 4;
            this.BtnCreditor.Text = "Creditor";
            this.BtnCreditor.UseVisualStyleBackColor = true;
            this.BtnCreditor.Click += new System.EventHandler(this.BtnCreditor_Click);
            // 
            // CboParticipants
            // 
            this.CboParticipants.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboParticipants.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboParticipants.FormattingEnabled = true;
            this.CboParticipants.Location = new System.Drawing.Point(10, 46);
            this.CboParticipants.Name = "CboParticipants";
            this.CboParticipants.Size = new System.Drawing.Size(191, 23);
            this.CboParticipants.TabIndex = 0;
            this.CboParticipants.SelectionChangeCommitted += new System.EventHandler(this.CboParticipants_SelectionChangeCommitted);
            // 
            // CboYears
            // 
            this.CboYears.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboYears.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboYears.FormattingEnabled = true;
            this.CboYears.Location = new System.Drawing.Point(123, 19);
            this.CboYears.Name = "CboYears";
            this.CboYears.Size = new System.Drawing.Size(78, 23);
            this.CboYears.TabIndex = 3;
            // 
            // CboMonths
            // 
            this.CboMonths.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboMonths.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboMonths.FormattingEnabled = true;
            this.CboMonths.Location = new System.Drawing.Point(10, 19);
            this.CboMonths.Name = "CboMonths";
            this.CboMonths.Size = new System.Drawing.Size(107, 23);
            this.CboMonths.TabIndex = 2;
            // 
            // BtnDebtor
            // 
            this.BtnDebtor.Location = new System.Drawing.Point(116, 99);
            this.BtnDebtor.Name = "BtnDebtor";
            this.BtnDebtor.Size = new System.Drawing.Size(85, 51);
            this.BtnDebtor.TabIndex = 5;
            this.BtnDebtor.Text = "Debtor";
            this.BtnDebtor.UseVisualStyleBackColor = true;
            this.BtnDebtor.Click += new System.EventHandler(this.BtnDebtor_Click);
            // 
            // FListPics
            // 
            this.FListPics.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("FListPics.ImageStream")));
            this.FListPics.TransparentColor = System.Drawing.Color.Transparent;
            this.FListPics.Images.SetKeyName(0, "");
            this.FListPics.Images.SetKeyName(1, "");
            this.FListPics.Images.SetKeyName(2, "");
            this.FListPics.Images.SetKeyName(3, "");
            this.FListPics.Images.SetKeyName(4, "");
            this.FListPics.Images.SetKeyName(5, "");
            this.FListPics.Images.SetKeyName(6, "");
            this.FListPics.Images.SetKeyName(7, "");
            this.FListPics.Images.SetKeyName(8, "");
            this.FListPics.Images.SetKeyName(9, "");
            this.FListPics.Images.SetKeyName(10, "");
            this.FListPics.Images.SetKeyName(11, "");
            this.FListPics.Images.SetKeyName(12, "");
            this.FListPics.Images.SetKeyName(13, "");
            this.FListPics.Images.SetKeyName(14, "");
            this.FListPics.Images.SetKeyName(15, "");
            this.FListPics.Images.SetKeyName(16, "");
            // 
            // ChkIncludeAll
            // 
            this.ChkIncludeAll.AutoSize = true;
            this.ChkIncludeAll.Location = new System.Drawing.Point(117, 128);
            this.ChkIncludeAll.Name = "ChkIncludeAll";
            this.ChkIncludeAll.Size = new System.Drawing.Size(37, 17);
            this.ChkIncludeAll.TabIndex = 13;
            this.ChkIncludeAll.Text = "All";
            this.ChkIncludeAll.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this.SplitContainer);
            this.Controls.Add(this.StatusStrip);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            this.SplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FpicBoxSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.ToolStripStatusLabel TssLblMensaje;
        private System.Windows.Forms.ToolStripStatusLabel TssLblFechaHora;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStripStatusLabel TssLblUserEmail;
        private System.Windows.Forms.Button BtnPdfConvert;
        private System.Windows.Forms.Button BtnOutlook;
        private System.Windows.Forms.TextBox TxtDateTimeEmail;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox TxtRznRef;
        private System.Windows.Forms.TextBox TxtFolioRef;
        private System.Windows.Forms.Button BtnInsertRef;
        private System.Windows.Forms.Button BtnPagar;
        private System.Windows.Forms.TextBox TxtCtaCteParticipant;
        private System.Windows.Forms.TextBox TxtRutParticipant;
        private System.Windows.Forms.TextBox TxtNmbItem;
        private System.Windows.Forms.TextBox TxtFmaPago;
        private System.Windows.Forms.PictureBox FpicBoxSearch;
        private System.Windows.Forms.ImageList FListPics;
        private System.Windows.Forms.TextBox TxtDscItem;
        private System.Windows.Forms.TextBox TxtTpoDocRef;
        private System.Windows.Forms.CheckBox ChkIncludeReclaimed;
        private System.Windows.Forms.Button BtnInsertNv;
        private System.Windows.Forms.Button BtnExcelConvert;
        private System.Windows.Forms.Button BtnHiperLink;
        private System.Windows.Forms.CheckBox ChkIncludeCEN;
        private System.Windows.Forms.CheckBox ChkIncludeAll;
    }
}