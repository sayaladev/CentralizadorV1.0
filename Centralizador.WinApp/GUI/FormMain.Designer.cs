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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.TssLblFechaHora = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblUserEmail = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblProgBar = new System.Windows.Forms.ToolStripProgressBar();
            this.TssLblMensaje = new System.Windows.Forms.ToolStripStatusLabel();
            this.TssLblDBName = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.FpicBoxSearch = new System.Windows.Forms.PictureBox();
            this.IGridMain = new TenTec.Windows.iGridLib.iGrid();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BtnExcelConvert = new System.Windows.Forms.Button();
            this.BtnPdfConvert = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.TxtTpoDocRef = new System.Windows.Forms.TextBox();
            this.TxtFmaPago = new System.Windows.Forms.TextBox();
            this.TxtRznRef = new System.Windows.Forms.TextBox();
            this.TxtFolioRef = new System.Windows.Forms.TextBox();
            this.TxtNmbItem = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ChkNoIncludeCEN = new System.Windows.Forms.CheckBox();
            this.ChkIncludeCEN = new System.Windows.Forms.CheckBox();
            this.BtnInsertNv = new System.Windows.Forms.Button();
            this.ChkIncludeReclaimed = new System.Windows.Forms.CheckBox();
            this.BtnPagar = new System.Windows.Forms.Button();
            this.BtnInsertRef = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnCancelTak = new System.Windows.Forms.Button();
            this.FListPics = new System.Windows.Forms.ImageList(this.components);
            this.BtnCreditor = new System.Windows.Forms.Button();
            this.BtnDebtor = new System.Windows.Forms.Button();
            this.TxtCtaCteParticipant = new System.Windows.Forms.TextBox();
            this.BtnOutlook = new System.Windows.Forms.Button();
            this.TxtRutParticipant = new System.Windows.Forms.TextBox();
            this.BtnHiperLink = new System.Windows.Forms.Button();
            this.CboParticipants = new System.Windows.Forms.ComboBox();
            this.CboYears = new System.Windows.Forms.ComboBox();
            this.CboMonths = new System.Windows.Forms.ComboBox();
            this.fImageListSmall = new System.Windows.Forms.ImageList(this.components);
            this.StatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FpicBoxSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).BeginInit();
            this.groupBox3.SuspendLayout();
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
            this.TssLblMensaje,
            this.TssLblDBName});
            this.StatusStrip.Location = new System.Drawing.Point(0, 707);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1396, 22);
            this.StatusStrip.TabIndex = 0;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // TssLblFechaHora
            // 
            this.TssLblFechaHora.AutoSize = false;
            this.TssLblFechaHora.Name = "TssLblFechaHora";
            this.TssLblFechaHora.Size = new System.Drawing.Size(100, 17);
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
            this.TssLblProgBar.Size = new System.Drawing.Size(200, 16);
            // 
            // TssLblMensaje
            // 
            this.TssLblMensaje.AutoSize = false;
            this.TssLblMensaje.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.TssLblMensaje.Name = "TssLblMensaje";
            this.TssLblMensaje.Size = new System.Drawing.Size(750, 17);
            this.TssLblMensaje.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TssLblDBName
            // 
            this.TssLblDBName.AutoSize = false;
            this.TssLblDBName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.TssLblDBName.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TssLblDBName.Name = "TssLblDBName";
            this.TssLblDBName.Size = new System.Drawing.Size(100, 17);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.IsSplitterFixed = true;
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
            this.SplitContainer.Panel2.Controls.Add(this.groupBox3);
            this.SplitContainer.Panel2.Controls.Add(this.groupBox4);
            this.SplitContainer.Panel2.Controls.Add(this.groupBox2);
            this.SplitContainer.Panel2.Controls.Add(this.GroupBox1);
            this.SplitContainer.Size = new System.Drawing.Size(1396, 707);
            this.SplitContainer.SplitterDistance = 1166;
            this.SplitContainer.TabIndex = 1;
            // 
            // FpicBoxSearch
            // 
            this.FpicBoxSearch.Image = ((System.Drawing.Image)(resources.GetObject("FpicBoxSearch.Image")));
            this.FpicBoxSearch.Location = new System.Drawing.Point(12, 679);
            this.FpicBoxSearch.Name = "FpicBoxSearch";
            this.FpicBoxSearch.Size = new System.Drawing.Size(36, 25);
            this.FpicBoxSearch.TabIndex = 3;
            this.FpicBoxSearch.TabStop = false;
            this.FpicBoxSearch.Visible = false;
            // 
            // IGridMain
            // 
            this.IGridMain.Appearance = TenTec.Windows.iGridLib.iGControlPaintAppearance.StyleFlat;
            this.IGridMain.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("IGridMain.BackgroundImage")));
            this.IGridMain.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.IGridMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IGridMain.Header.BackColor = System.Drawing.SystemColors.Info;
            this.IGridMain.Header.DrawSystem = false;
            this.IGridMain.Header.Height = 16;
            this.IGridMain.Header.HotTrackFlags = ((TenTec.Windows.iGridLib.iGHdrHotTrackFlags)((TenTec.Windows.iGridLib.iGHdrHotTrackFlags.Icon | TenTec.Windows.iGridLib.iGHdrHotTrackFlags.Text)));
            this.IGridMain.Header.HotTrackForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(132)))), ((int)(((byte)(191)))), ((int)(((byte)(65)))));
            this.IGridMain.Header.SeparatingLine = new TenTec.Windows.iGridLib.iGPenStyle(System.Drawing.SystemColors.ControlDark, 1, System.Drawing.Drawing2D.DashStyle.Solid);
            this.IGridMain.Location = new System.Drawing.Point(0, 0);
            this.IGridMain.Name = "IGridMain";
            this.IGridMain.RowHeader.BackColor = System.Drawing.SystemColors.Info;
            this.IGridMain.SelCellsBackColor = System.Drawing.SystemColors.ControlLight;
            this.IGridMain.SelCellsForeColor = System.Drawing.Color.Black;
            this.IGridMain.Size = new System.Drawing.Size(1166, 707);
            this.IGridMain.TabIndex = 0;
            this.IGridMain.CustomDrawCellForeground += new TenTec.Windows.iGridLib.iGCustomDrawCellEventHandler(this.IGridMain_CustomDrawCellForeground);
            this.IGridMain.CustomDrawCellEllipsisButtonForeground += new TenTec.Windows.iGridLib.iGCustomDrawEllipsisButtonEventHandler(this.IGridMain_CustomDrawCellEllipsisButtonForeground);
            this.IGridMain.ColHdrMouseDown += new TenTec.Windows.iGridLib.iGColHdrMouseDownEventHandler(this.IGridMain_ColHdrMouseDown);
            this.IGridMain.CellEllipsisButtonClick += new TenTec.Windows.iGridLib.iGEllipsisButtonClickEventHandler(this.IGridMain_CellEllipsisButtonClick);
            this.IGridMain.CurRowChanged += new System.EventHandler(this.IGridMain_CurRowChanged);
            this.IGridMain.RequestCellToolTipText += new TenTec.Windows.iGridLib.iGRequestCellToolTipTextEventHandler(this.IGridMain_RequestCellToolTipText);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BtnExcelConvert);
            this.groupBox3.Controls.Add(this.BtnPdfConvert);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(22, 622);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(174, 75);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Export To:";
            // 
            // BtnExcelConvert
            // 
            this.BtnExcelConvert.BackColor = System.Drawing.SystemColors.Control;
            this.BtnExcelConvert.Image = ((System.Drawing.Image)(resources.GetObject("BtnExcelConvert.Image")));
            this.BtnExcelConvert.Location = new System.Drawing.Point(98, 19);
            this.BtnExcelConvert.Name = "BtnExcelConvert";
            this.BtnExcelConvert.Size = new System.Drawing.Size(56, 46);
            this.BtnExcelConvert.TabIndex = 8;
            this.BtnExcelConvert.UseVisualStyleBackColor = false;
            this.BtnExcelConvert.Click += new System.EventHandler(this.BtnExcelConvert_Click);
            // 
            // BtnPdfConvert
            // 
            this.BtnPdfConvert.BackColor = System.Drawing.SystemColors.Control;
            this.BtnPdfConvert.Image = ((System.Drawing.Image)(resources.GetObject("BtnPdfConvert.Image")));
            this.BtnPdfConvert.Location = new System.Drawing.Point(20, 19);
            this.BtnPdfConvert.Name = "BtnPdfConvert";
            this.BtnPdfConvert.Size = new System.Drawing.Size(56, 46);
            this.BtnPdfConvert.TabIndex = 4;
            this.BtnPdfConvert.UseVisualStyleBackColor = false;
            this.BtnPdfConvert.Click += new System.EventHandler(this.BtnPdfConvert_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox4.BackgroundImage")));
            this.groupBox4.Controls.Add(this.TxtTpoDocRef);
            this.groupBox4.Controls.Add(this.TxtFmaPago);
            this.groupBox4.Controls.Add(this.TxtRznRef);
            this.groupBox4.Controls.Add(this.TxtFolioRef);
            this.groupBox4.Controls.Add(this.TxtNmbItem);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(2, 281);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(220, 150);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Invoice Info:";
            // 
            // TxtTpoDocRef
            // 
            this.TxtTpoDocRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtTpoDocRef.Location = new System.Drawing.Point(10, 107);
            this.TxtTpoDocRef.Name = "TxtTpoDocRef";
            this.TxtTpoDocRef.ReadOnly = true;
            this.TxtTpoDocRef.Size = new System.Drawing.Size(31, 18);
            this.TxtTpoDocRef.TabIndex = 11;
            this.TxtTpoDocRef.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtFmaPago
            // 
            this.TxtFmaPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtFmaPago.Location = new System.Drawing.Point(44, 107);
            this.TxtFmaPago.Name = "TxtFmaPago";
            this.TxtFmaPago.ReadOnly = true;
            this.TxtFmaPago.Size = new System.Drawing.Size(42, 18);
            this.TxtFmaPago.TabIndex = 9;
            this.TxtFmaPago.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtRznRef
            // 
            this.TxtRznRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtRznRef.Location = new System.Drawing.Point(24, 83);
            this.TxtRznRef.Name = "TxtRznRef";
            this.TxtRznRef.ReadOnly = true;
            this.TxtRznRef.Size = new System.Drawing.Size(165, 18);
            this.TxtRznRef.TabIndex = 8;
            this.TxtRznRef.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtFolioRef
            // 
            this.TxtFolioRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtFolioRef.Location = new System.Drawing.Point(92, 107);
            this.TxtFolioRef.Name = "TxtFolioRef";
            this.TxtFolioRef.ReadOnly = true;
            this.TxtFolioRef.Size = new System.Drawing.Size(110, 18);
            this.TxtFolioRef.TabIndex = 7;
            this.TxtFolioRef.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtNmbItem
            // 
            this.TxtNmbItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtNmbItem.Location = new System.Drawing.Point(10, 26);
            this.TxtNmbItem.Multiline = true;
            this.TxtNmbItem.Name = "TxtNmbItem";
            this.TxtNmbItem.ReadOnly = true;
            this.TxtNmbItem.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtNmbItem.Size = new System.Drawing.Size(192, 51);
            this.TxtNmbItem.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Controls.Add(this.ChkNoIncludeCEN);
            this.groupBox2.Controls.Add(this.ChkIncludeCEN);
            this.groupBox2.Controls.Add(this.BtnInsertNv);
            this.groupBox2.Controls.Add(this.ChkIncludeReclaimed);
            this.groupBox2.Controls.Add(this.BtnPagar);
            this.groupBox2.Controls.Add(this.BtnInsertRef);
            this.groupBox2.Location = new System.Drawing.Point(2, 437);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(220, 176);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            // 
            // ChkNoIncludeCEN
            // 
            this.ChkNoIncludeCEN.AutoSize = true;
            this.ChkNoIncludeCEN.Location = new System.Drawing.Point(114, 131);
            this.ChkNoIncludeCEN.Name = "ChkNoIncludeCEN";
            this.ChkNoIncludeCEN.Size = new System.Drawing.Size(65, 17);
            this.ChkNoIncludeCEN.TabIndex = 13;
            this.ChkNoIncludeCEN.Text = "No CEN";
            this.ChkNoIncludeCEN.UseVisualStyleBackColor = true;
            this.ChkNoIncludeCEN.CheckedChanged += new System.EventHandler(this.ChkNoIncludeCEN_CheckedChanged);
            // 
            // ChkIncludeCEN
            // 
            this.ChkIncludeCEN.AutoSize = true;
            this.ChkIncludeCEN.Location = new System.Drawing.Point(114, 110);
            this.ChkIncludeCEN.Name = "ChkIncludeCEN";
            this.ChkIncludeCEN.Size = new System.Drawing.Size(72, 17);
            this.ChkIncludeCEN.TabIndex = 12;
            this.ChkIncludeCEN.Text = "Only CEN";
            this.ChkIncludeCEN.UseVisualStyleBackColor = true;
            this.ChkIncludeCEN.CheckedChanged += new System.EventHandler(this.ChkIncludeCEN_CheckedChanged);
            // 
            // BtnInsertNv
            // 
            this.BtnInsertNv.BackColor = System.Drawing.SystemColors.Control;
            this.BtnInsertNv.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnInsertNv.Image = ((System.Drawing.Image)(resources.GetObject("BtnInsertNv.Image")));
            this.BtnInsertNv.Location = new System.Drawing.Point(11, 18);
            this.BtnInsertNv.Name = "BtnInsertNv";
            this.BtnInsertNv.Size = new System.Drawing.Size(98, 86);
            this.BtnInsertNv.TabIndex = 10;
            this.BtnInsertNv.Text = "Nota de Venta";
            this.BtnInsertNv.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.BtnInsertNv.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnInsertNv.UseVisualStyleBackColor = false;
            this.BtnInsertNv.Click += new System.EventHandler(this.BtnInsertNv_ClickAsync);
            // 
            // ChkIncludeReclaimed
            // 
            this.ChkIncludeReclaimed.AutoSize = true;
            this.ChkIncludeReclaimed.Location = new System.Drawing.Point(114, 54);
            this.ChkIncludeReclaimed.Name = "ChkIncludeReclaimed";
            this.ChkIncludeReclaimed.Size = new System.Drawing.Size(100, 17);
            this.ChkIncludeReclaimed.TabIndex = 9;
            this.ChkIncludeReclaimed.Text = "Only Reclaimed";
            this.ChkIncludeReclaimed.UseVisualStyleBackColor = true;
            // 
            // BtnPagar
            // 
            this.BtnPagar.BackColor = System.Drawing.SystemColors.Control;
            this.BtnPagar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnPagar.Image = ((System.Drawing.Image)(resources.GetObject("BtnPagar.Image")));
            this.BtnPagar.Location = new System.Drawing.Point(31, 110);
            this.BtnPagar.Name = "BtnPagar";
            this.BtnPagar.Size = new System.Drawing.Size(66, 38);
            this.BtnPagar.TabIndex = 6;
            this.BtnPagar.Text = "Pay";
            this.BtnPagar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.BtnPagar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnPagar.UseVisualStyleBackColor = false;
            this.BtnPagar.Click += new System.EventHandler(this.BtnPagar_Click);
            // 
            // BtnInsertRef
            // 
            this.BtnInsertRef.BackColor = System.Drawing.SystemColors.Control;
            this.BtnInsertRef.Enabled = false;
            this.BtnInsertRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnInsertRef.Image = ((System.Drawing.Image)(resources.GetObject("BtnInsertRef.Image")));
            this.BtnInsertRef.Location = new System.Drawing.Point(6, 124);
            this.BtnInsertRef.Name = "BtnInsertRef";
            this.BtnInsertRef.Size = new System.Drawing.Size(10, 10);
            this.BtnInsertRef.TabIndex = 8;
            this.BtnInsertRef.Text = "<References>";
            this.BtnInsertRef.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.BtnInsertRef.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnInsertRef.UseVisualStyleBackColor = false;
            // 
            // GroupBox1
            // 
            this.GroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox1.Controls.Add(this.BtnCancelTak);
            this.GroupBox1.Controls.Add(this.BtnCreditor);
            this.GroupBox1.Controls.Add(this.BtnDebtor);
            this.GroupBox1.Controls.Add(this.TxtCtaCteParticipant);
            this.GroupBox1.Controls.Add(this.BtnOutlook);
            this.GroupBox1.Controls.Add(this.TxtRutParticipant);
            this.GroupBox1.Controls.Add(this.BtnHiperLink);
            this.GroupBox1.Controls.Add(this.CboParticipants);
            this.GroupBox1.Controls.Add(this.CboYears);
            this.GroupBox1.Controls.Add(this.CboMonths);
            this.GroupBox1.Location = new System.Drawing.Point(3, 3);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(220, 272);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            // 
            // BtnCancelTak
            // 
            this.BtnCancelTak.ImageIndex = 17;
            this.BtnCancelTak.ImageList = this.FListPics;
            this.BtnCancelTak.Location = new System.Drawing.Point(171, 164);
            this.BtnCancelTak.Name = "BtnCancelTak";
            this.BtnCancelTak.Size = new System.Drawing.Size(30, 23);
            this.BtnCancelTak.TabIndex = 12;
            this.BtnCancelTak.UseVisualStyleBackColor = true;
            this.BtnCancelTak.Click += new System.EventHandler(this.BtnCancelTak_Click);
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
            this.FListPics.Images.SetKeyName(17, "cancel_16.png");
            // 
            // BtnCreditor
            // 
            this.BtnCreditor.BackColor = System.Drawing.SystemColors.Control;
            this.BtnCreditor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnCreditor.Image = ((System.Drawing.Image)(resources.GetObject("BtnCreditor.Image")));
            this.BtnCreditor.Location = new System.Drawing.Point(10, 99);
            this.BtnCreditor.Name = "BtnCreditor";
            this.BtnCreditor.Size = new System.Drawing.Size(85, 59);
            this.BtnCreditor.TabIndex = 4;
            this.BtnCreditor.Text = "Creditor";
            this.BtnCreditor.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BtnCreditor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.BtnCreditor.UseVisualStyleBackColor = false;
            this.BtnCreditor.Click += new System.EventHandler(this.BtnCreditor_Click);
            // 
            // BtnDebtor
            // 
            this.BtnDebtor.BackColor = System.Drawing.SystemColors.Control;
            this.BtnDebtor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnDebtor.Image = ((System.Drawing.Image)(resources.GetObject("BtnDebtor.Image")));
            this.BtnDebtor.Location = new System.Drawing.Point(116, 99);
            this.BtnDebtor.Name = "BtnDebtor";
            this.BtnDebtor.Size = new System.Drawing.Size(85, 59);
            this.BtnDebtor.TabIndex = 5;
            this.BtnDebtor.Text = "Debtor";
            this.BtnDebtor.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BtnDebtor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.BtnDebtor.UseVisualStyleBackColor = false;
            this.BtnDebtor.Click += new System.EventHandler(this.BtnDebtor_Click);
            // 
            // TxtCtaCteParticipant
            // 
            this.TxtCtaCteParticipant.BackColor = System.Drawing.SystemColors.Info;
            this.TxtCtaCteParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtCtaCteParticipant.Location = new System.Drawing.Point(110, 73);
            this.TxtCtaCteParticipant.Name = "TxtCtaCteParticipant";
            this.TxtCtaCteParticipant.ReadOnly = true;
            this.TxtCtaCteParticipant.Size = new System.Drawing.Size(91, 21);
            this.TxtCtaCteParticipant.TabIndex = 10;
            this.TxtCtaCteParticipant.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // BtnOutlook
            // 
            this.BtnOutlook.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnOutlook.Image = ((System.Drawing.Image)(resources.GetObject("BtnOutlook.Image")));
            this.BtnOutlook.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.BtnOutlook.Location = new System.Drawing.Point(30, 220);
            this.BtnOutlook.Name = "BtnOutlook";
            this.BtnOutlook.Size = new System.Drawing.Size(152, 46);
            this.BtnOutlook.TabIndex = 1;
            this.BtnOutlook.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.BtnOutlook.UseVisualStyleBackColor = true;
            this.BtnOutlook.Click += new System.EventHandler(this.BtnOutlook_Click);
            // 
            // TxtRutParticipant
            // 
            this.TxtRutParticipant.BackColor = System.Drawing.SystemColors.Info;
            this.TxtRutParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtRutParticipant.Location = new System.Drawing.Point(10, 73);
            this.TxtRutParticipant.Name = "TxtRutParticipant";
            this.TxtRutParticipant.ReadOnly = true;
            this.TxtRutParticipant.Size = new System.Drawing.Size(94, 21);
            this.TxtRutParticipant.TabIndex = 9;
            this.TxtRutParticipant.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // BtnHiperLink
            // 
            this.BtnHiperLink.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(132)))), ((int)(((byte)(191)))), ((int)(((byte)(65)))));
            this.BtnHiperLink.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnHiperLink.BackgroundImage")));
            this.BtnHiperLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.BtnHiperLink.Location = new System.Drawing.Point(55, 163);
            this.BtnHiperLink.Name = "BtnHiperLink";
            this.BtnHiperLink.Size = new System.Drawing.Size(102, 51);
            this.BtnHiperLink.TabIndex = 11;
            this.BtnHiperLink.UseVisualStyleBackColor = false;
            this.BtnHiperLink.Click += new System.EventHandler(this.BtnHiperLink_Click);
            this.BtnHiperLink.MouseHover += new System.EventHandler(this.BtnHiperLink_MouseHover);
            // 
            // CboParticipants
            // 
            this.CboParticipants.BackColor = System.Drawing.SystemColors.Info;
            this.CboParticipants.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboParticipants.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboParticipants.FormattingEnabled = true;
            this.CboParticipants.Location = new System.Drawing.Point(10, 46);
            this.CboParticipants.Name = "CboParticipants";
            this.CboParticipants.Size = new System.Drawing.Size(191, 23);
            this.CboParticipants.TabIndex = 0;
            this.CboParticipants.SelectionChangeCommitted += new System.EventHandler(this.CboParticipants_SelectionChangeCommittedAsync);
            // 
            // CboYears
            // 
            this.CboYears.BackColor = System.Drawing.SystemColors.Info;
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
            this.CboMonths.BackColor = System.Drawing.SystemColors.Info;
            this.CboMonths.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboMonths.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboMonths.FormattingEnabled = true;
            this.CboMonths.Location = new System.Drawing.Point(10, 19);
            this.CboMonths.Name = "CboMonths";
            this.CboMonths.Size = new System.Drawing.Size(107, 23);
            this.CboMonths.TabIndex = 2;
            // 
            // fImageListSmall
            // 
            this.fImageListSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fImageListSmall.ImageStream")));
            this.fImageListSmall.TransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.fImageListSmall.Images.SetKeyName(0, "");
            this.fImageListSmall.Images.SetKeyName(1, "");
            this.fImageListSmall.Images.SetKeyName(2, "");
            this.fImageListSmall.Images.SetKeyName(3, "");
            this.fImageListSmall.Images.SetKeyName(4, "");
            this.fImageListSmall.Images.SetKeyName(5, "");
            this.fImageListSmall.Images.SetKeyName(6, "");
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1396, 729);
            this.Controls.Add(this.SplitContainer);
            this.Controls.Add(this.StatusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FpicBoxSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IGridMain)).EndInit();
            this.groupBox3.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox TxtTpoDocRef;
        private System.Windows.Forms.CheckBox ChkIncludeReclaimed;
        private System.Windows.Forms.Button BtnInsertNv;
        private System.Windows.Forms.Button BtnExcelConvert;
        private System.Windows.Forms.Button BtnHiperLink;
        private System.Windows.Forms.CheckBox ChkIncludeCEN;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox ChkNoIncludeCEN;
        private System.Windows.Forms.Button BtnCancelTak;
        private System.Windows.Forms.ImageList fImageListSmall;
        private System.Windows.Forms.ToolStripStatusLabel TssLblDBName;
    }
}