
namespace RPS
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button_LoadGPX = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_ElevDown = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_path = new System.Windows.Forms.TextBox();
            this.textBox_way = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_road = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_Mail = new System.Windows.Forms.Button();
            this.button_saveImage = new System.Windows.Forms.Button();
            this.button_saveGPX = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_Reg = new System.Windows.Forms.TextBox();
            this.checkBox_Boundary = new System.Windows.Forms.CheckBox();
            this.label_Reg = new System.Windows.Forms.Label();
            this.button_newRouteDB = new System.Windows.Forms.Button();
            this.label_delta = new System.Windows.Forms.Label();
            this.groupBox_Sport = new System.Windows.Forms.GroupBox();
            this.radioButton_Race = new System.Windows.Forms.RadioButton();
            this.radioButton_Cross = new System.Windows.Forms.RadioButton();
            this.radioButton_Hik = new System.Windows.Forms.RadioButton();
            this.groupBox_MapKind = new System.Windows.Forms.GroupBox();
            this.radioButton_Hyb = new System.Windows.Forms.RadioButton();
            this.radioButton_Sat = new System.Windows.Forms.RadioButton();
            this.radioButton_Map = new System.Windows.Forms.RadioButton();
            this.chart_ele = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.checkBox_Routing = new System.Windows.Forms.CheckBox();
            this.label_TotT = new System.Windows.Forms.Label();
            this.label_Dis = new System.Windows.Forms.Label();
            this.textBox_ElevUp = new System.Windows.Forms.TextBox();
            this.textBox_Dis = new System.Windows.Forms.TextBox();
            this.button_CenterTrack = new System.Windows.Forms.Button();
            this.button_SkipLastPart = new System.Windows.Forms.Button();
            this.button_newRoute = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox_Sport.SuspendLayout();
            this.groupBox_MapKind.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_ele)).BeginInit();
            this.SuspendLayout();
            // 
            // button_LoadGPX
            // 
            this.button_LoadGPX.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_LoadGPX.Location = new System.Drawing.Point(12, 148);
            this.button_LoadGPX.Name = "button_LoadGPX";
            this.button_LoadGPX.Size = new System.Drawing.Size(248, 37);
            this.button_LoadGPX.TabIndex = 13;
            this.button_LoadGPX.Text = "Load Route from GPX-file";
            this.button_LoadGPX.UseVisualStyleBackColor = true;
            this.button_LoadGPX.Click += new System.EventHandler(this.Button_LoadGPX_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBox_ElevDown);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBox_path);
            this.panel1.Controls.Add(this.textBox_way);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBox_road);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.label_delta);
            this.panel1.Controls.Add(this.groupBox_Sport);
            this.panel1.Controls.Add(this.groupBox_MapKind);
            this.panel1.Controls.Add(this.chart_ele);
            this.panel1.Controls.Add(this.checkBox_Routing);
            this.panel1.Controls.Add(this.label_TotT);
            this.panel1.Controls.Add(this.label_Dis);
            this.panel1.Controls.Add(this.textBox_ElevUp);
            this.panel1.Controls.Add(this.textBox_Dis);
            this.panel1.Controls.Add(this.button_CenterTrack);
            this.panel1.Controls.Add(this.button_SkipLastPart);
            this.panel1.Controls.Add(this.button_newRoute);
            this.panel1.Controls.Add(this.button_LoadGPX);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(1111, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(273, 861);
            this.panel1.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(187, 326);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 56;
            this.label4.Text = "Elev. (descent)";
            // 
            // textBox_ElevDown
            // 
            this.textBox_ElevDown.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_ElevDown.Location = new System.Drawing.Point(182, 304);
            this.textBox_ElevDown.Name = "textBox_ElevDown";
            this.textBox_ElevDown.Size = new System.Drawing.Size(86, 20);
            this.textBox_ElevDown.TabIndex = 55;
            this.textBox_ElevDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(203, 369);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 54;
            this.label3.Text = "Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(117, 369);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 53;
            this.label2.Text = "Way";
            // 
            // textBox_path
            // 
            this.textBox_path.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_path.Location = new System.Drawing.Point(181, 348);
            this.textBox_path.Name = "textBox_path";
            this.textBox_path.Size = new System.Drawing.Size(86, 20);
            this.textBox_path.TabIndex = 52;
            this.textBox_path.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_way
            // 
            this.textBox_way.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_way.Location = new System.Drawing.Point(93, 348);
            this.textBox_way.Name = "textBox_way";
            this.textBox_way.Size = new System.Drawing.Size(86, 20);
            this.textBox_way.TabIndex = 51;
            this.textBox_way.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 369);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 50;
            this.label1.Text = "Road";
            // 
            // textBox_road
            // 
            this.textBox_road.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_road.Location = new System.Drawing.Point(3, 348);
            this.textBox_road.Name = "textBox_road";
            this.textBox_road.Size = new System.Drawing.Size(86, 20);
            this.textBox_road.TabIndex = 49;
            this.textBox_road.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_Mail);
            this.groupBox2.Controls.Add(this.button_saveImage);
            this.groupBox2.Controls.Add(this.button_saveGPX);
            this.groupBox2.Location = new System.Drawing.Point(6, 387);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(258, 138);
            this.groupBox2.TabIndex = 48;
            this.groupBox2.TabStop = false;
            // 
            // button_Mail
            // 
            this.button_Mail.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Mail.Location = new System.Drawing.Point(8, 94);
            this.button_Mail.Name = "button_Mail";
            this.button_Mail.Size = new System.Drawing.Size(248, 37);
            this.button_Mail.TabIndex = 30;
            this.button_Mail.Text = "Send Route via Mail";
            this.button_Mail.UseVisualStyleBackColor = true;
            this.button_Mail.Click += new System.EventHandler(this.Button_Mail_Click);
            // 
            // button_saveImage
            // 
            this.button_saveImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_saveImage.Location = new System.Drawing.Point(5, 52);
            this.button_saveImage.Name = "button_saveImage";
            this.button_saveImage.Size = new System.Drawing.Size(248, 37);
            this.button_saveImage.TabIndex = 29;
            this.button_saveImage.Text = "Save Route as Image";
            this.button_saveImage.UseVisualStyleBackColor = true;
            this.button_saveImage.Click += new System.EventHandler(this.Button_saveImage_Click);
            // 
            // button_saveGPX
            // 
            this.button_saveGPX.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_saveGPX.Location = new System.Drawing.Point(5, 10);
            this.button_saveGPX.Name = "button_saveGPX";
            this.button_saveGPX.Size = new System.Drawing.Size(248, 37);
            this.button_saveGPX.TabIndex = 28;
            this.button_saveGPX.Text = "Save Route as GPX-file";
            this.button_saveGPX.UseVisualStyleBackColor = true;
            this.button_saveGPX.Click += new System.EventHandler(this.Button_saveGPX_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_Reg);
            this.groupBox1.Controls.Add(this.checkBox_Boundary);
            this.groupBox1.Controls.Add(this.label_Reg);
            this.groupBox1.Controls.Add(this.button_newRouteDB);
            this.groupBox1.Location = new System.Drawing.Point(5, 510);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 123);
            this.groupBox1.TabIndex = 47;
            this.groupBox1.TabStop = false;
            // 
            // textBox_Reg
            // 
            this.textBox_Reg.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_Reg.Location = new System.Drawing.Point(50, 48);
            this.textBox_Reg.Name = "textBox_Reg";
            this.textBox_Reg.Size = new System.Drawing.Size(197, 20);
            this.textBox_Reg.TabIndex = 40;
            this.textBox_Reg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // checkBox_Boundary
            // 
            this.checkBox_Boundary.AutoSize = true;
            this.checkBox_Boundary.Checked = true;
            this.checkBox_Boundary.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Boundary.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox_Boundary.Location = new System.Drawing.Point(50, 18);
            this.checkBox_Boundary.Name = "checkBox_Boundary";
            this.checkBox_Boundary.Size = new System.Drawing.Size(164, 28);
            this.checkBox_Boundary.TabIndex = 45;
            this.checkBox_Boundary.Text = "Show-Boundary";
            this.checkBox_Boundary.UseVisualStyleBackColor = true;
            this.checkBox_Boundary.Click += new System.EventHandler(this.CheckBox_Boundary_Click);
            // 
            // label_Reg
            // 
            this.label_Reg.AutoSize = true;
            this.label_Reg.Location = new System.Drawing.Point(3, 48);
            this.label_Reg.Name = "label_Reg";
            this.label_Reg.Size = new System.Drawing.Size(41, 13);
            this.label_Reg.TabIndex = 39;
            this.label_Reg.Text = "Region";
            // 
            // button_newRouteDB
            // 
            this.button_newRouteDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_newRouteDB.Location = new System.Drawing.Point(6, 74);
            this.button_newRouteDB.Name = "button_newRouteDB";
            this.button_newRouteDB.Size = new System.Drawing.Size(248, 40);
            this.button_newRouteDB.TabIndex = 30;
            this.button_newRouteDB.Text = "Load new Route-DataBase";
            this.button_newRouteDB.UseVisualStyleBackColor = true;
            this.button_newRouteDB.Click += new System.EventHandler(this.Button_newRouteDB_Click);
            // 
            // label_delta
            // 
            this.label_delta.AutoSize = true;
            this.label_delta.BackColor = System.Drawing.Color.White;
            this.label_delta.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_delta.ForeColor = System.Drawing.Color.Red;
            this.label_delta.Location = new System.Drawing.Point(81, 666);
            this.label_delta.Name = "label_delta";
            this.label_delta.Size = new System.Drawing.Size(0, 16);
            this.label_delta.TabIndex = 44;
            this.label_delta.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_delta.Visible = false;
            // 
            // groupBox_Sport
            // 
            this.groupBox_Sport.Controls.Add(this.radioButton_Race);
            this.groupBox_Sport.Controls.Add(this.radioButton_Cross);
            this.groupBox_Sport.Controls.Add(this.radioButton_Hik);
            this.groupBox_Sport.Location = new System.Drawing.Point(11, 58);
            this.groupBox_Sport.Name = "groupBox_Sport";
            this.groupBox_Sport.Size = new System.Drawing.Size(250, 54);
            this.groupBox_Sport.TabIndex = 43;
            this.groupBox_Sport.TabStop = false;
            // 
            // radioButton_Race
            // 
            this.radioButton_Race.AutoSize = true;
            this.radioButton_Race.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton_Race.Location = new System.Drawing.Point(161, 19);
            this.radioButton_Race.Name = "radioButton_Race";
            this.radioButton_Race.Size = new System.Drawing.Size(69, 24);
            this.radioButton_Race.TabIndex = 2;
            this.radioButton_Race.Text = "Race";
            this.radioButton_Race.UseVisualStyleBackColor = true;
            this.radioButton_Race.CheckedChanged += new System.EventHandler(this.RadioButton_Race_CheckedChanged);
            // 
            // radioButton_Cross
            // 
            this.radioButton_Cross.AutoSize = true;
            this.radioButton_Cross.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton_Cross.Location = new System.Drawing.Point(82, 19);
            this.radioButton_Cross.Name = "radioButton_Cross";
            this.radioButton_Cross.Size = new System.Drawing.Size(73, 24);
            this.radioButton_Cross.TabIndex = 1;
            this.radioButton_Cross.Text = "Cross";
            this.radioButton_Cross.UseVisualStyleBackColor = true;
            this.radioButton_Cross.CheckedChanged += new System.EventHandler(this.RadioButton_Cross_CheckedChanged);
            // 
            // radioButton_Hik
            // 
            this.radioButton_Hik.AutoSize = true;
            this.radioButton_Hik.Checked = true;
            this.radioButton_Hik.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton_Hik.Location = new System.Drawing.Point(13, 19);
            this.radioButton_Hik.Name = "radioButton_Hik";
            this.radioButton_Hik.Size = new System.Drawing.Size(63, 24);
            this.radioButton_Hik.TabIndex = 0;
            this.radioButton_Hik.TabStop = true;
            this.radioButton_Hik.Text = "Hike";
            this.radioButton_Hik.UseVisualStyleBackColor = true;
            this.radioButton_Hik.CheckedChanged += new System.EventHandler(this.RadioButton_Hik_CheckedChanged);
            // 
            // groupBox_MapKind
            // 
            this.groupBox_MapKind.Controls.Add(this.radioButton_Hyb);
            this.groupBox_MapKind.Controls.Add(this.radioButton_Sat);
            this.groupBox_MapKind.Controls.Add(this.radioButton_Map);
            this.groupBox_MapKind.Location = new System.Drawing.Point(12, 6);
            this.groupBox_MapKind.Name = "groupBox_MapKind";
            this.groupBox_MapKind.Size = new System.Drawing.Size(250, 53);
            this.groupBox_MapKind.TabIndex = 42;
            this.groupBox_MapKind.TabStop = false;
            // 
            // radioButton_Hyb
            // 
            this.radioButton_Hyb.AutoSize = true;
            this.radioButton_Hyb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton_Hyb.Location = new System.Drawing.Point(169, 19);
            this.radioButton_Hyb.Name = "radioButton_Hyb";
            this.radioButton_Hyb.Size = new System.Drawing.Size(78, 24);
            this.radioButton_Hyb.TabIndex = 2;
            this.radioButton_Hyb.Text = "Hybrid";
            this.radioButton_Hyb.UseVisualStyleBackColor = true;
            this.radioButton_Hyb.CheckedChanged += new System.EventHandler(this.RadioButton_Hyb_CheckedChanged);
            // 
            // radioButton_Sat
            // 
            this.radioButton_Sat.AutoSize = true;
            this.radioButton_Sat.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton_Sat.Location = new System.Drawing.Point(73, 19);
            this.radioButton_Sat.Name = "radioButton_Sat";
            this.radioButton_Sat.Size = new System.Drawing.Size(93, 24);
            this.radioButton_Sat.TabIndex = 1;
            this.radioButton_Sat.Text = "Satellite";
            this.radioButton_Sat.UseVisualStyleBackColor = true;
            this.radioButton_Sat.CheckedChanged += new System.EventHandler(this.RadioButton_Sat_CheckedChanged);
            // 
            // radioButton_Map
            // 
            this.radioButton_Map.AutoSize = true;
            this.radioButton_Map.Checked = true;
            this.radioButton_Map.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton_Map.Location = new System.Drawing.Point(6, 19);
            this.radioButton_Map.Name = "radioButton_Map";
            this.radioButton_Map.Size = new System.Drawing.Size(61, 24);
            this.radioButton_Map.TabIndex = 0;
            this.radioButton_Map.TabStop = true;
            this.radioButton_Map.Text = "Map";
            this.radioButton_Map.UseVisualStyleBackColor = true;
            this.radioButton_Map.CheckedChanged += new System.EventHandler(this.RadioButton_Map_CheckedChanged);
            // 
            // chart_ele
            // 
            this.chart_ele.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.chart_ele.BorderSkin.BorderWidth = 5;
            chartArea1.Name = "ChartArea1";
            this.chart_ele.ChartAreas.Add(chartArea1);
            this.chart_ele.Location = new System.Drawing.Point(0, 639);
            this.chart_ele.Name = "chart_ele";
            this.chart_ele.Size = new System.Drawing.Size(273, 209);
            this.chart_ele.TabIndex = 41;
            this.chart_ele.Text = "chart1";
            title1.Name = "Elevation_Dummy";
            title1.Text = "Altitude";
            this.chart_ele.Titles.Add(title1);
            this.chart_ele.Paint += new System.Windows.Forms.PaintEventHandler(this.Chart_ele_Paint);
            this.chart_ele.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Chart_ele_MouseDown);
            this.chart_ele.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Chart_ele_MouseMove);
            this.chart_ele.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Chart_ele_MouseUp);
            // 
            // checkBox_Routing
            // 
            this.checkBox_Routing.AutoSize = true;
            this.checkBox_Routing.Checked = true;
            this.checkBox_Routing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Routing.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox_Routing.Location = new System.Drawing.Point(59, 118);
            this.checkBox_Routing.Name = "checkBox_Routing";
            this.checkBox_Routing.Size = new System.Drawing.Size(139, 28);
            this.checkBox_Routing.TabIndex = 35;
            this.checkBox_Routing.Text = "Auto-Routing";
            this.checkBox_Routing.UseVisualStyleBackColor = true;
            // 
            // label_TotT
            // 
            this.label_TotT.AutoSize = true;
            this.label_TotT.Location = new System.Drawing.Point(99, 326);
            this.label_TotT.Name = "label_TotT";
            this.label_TotT.Size = new System.Drawing.Size(73, 13);
            this.label_TotT.TabIndex = 27;
            this.label_TotT.Text = "Elev. (Ascent)";
            // 
            // label_Dis
            // 
            this.label_Dis.AutoSize = true;
            this.label_Dis.Location = new System.Drawing.Point(20, 326);
            this.label_Dis.Name = "label_Dis";
            this.label_Dis.Size = new System.Drawing.Size(49, 13);
            this.label_Dis.TabIndex = 26;
            this.label_Dis.Text = "Distance";
            // 
            // textBox_ElevUp
            // 
            this.textBox_ElevUp.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_ElevUp.Location = new System.Drawing.Point(93, 304);
            this.textBox_ElevUp.Name = "textBox_ElevUp";
            this.textBox_ElevUp.Size = new System.Drawing.Size(86, 20);
            this.textBox_ElevUp.TabIndex = 26;
            this.textBox_ElevUp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_Dis
            // 
            this.textBox_Dis.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_Dis.Location = new System.Drawing.Point(3, 304);
            this.textBox_Dis.Name = "textBox_Dis";
            this.textBox_Dis.Size = new System.Drawing.Size(86, 20);
            this.textBox_Dis.TabIndex = 25;
            this.textBox_Dis.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_CenterTrack
            // 
            this.button_CenterTrack.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_CenterTrack.Location = new System.Drawing.Point(12, 261);
            this.button_CenterTrack.Name = "button_CenterTrack";
            this.button_CenterTrack.Size = new System.Drawing.Size(248, 37);
            this.button_CenterTrack.TabIndex = 37;
            this.button_CenterTrack.Text = "Center Track";
            this.button_CenterTrack.UseVisualStyleBackColor = true;
            this.button_CenterTrack.Click += new System.EventHandler(this.Button_CenterTrack_Click);
            // 
            // button_SkipLastPart
            // 
            this.button_SkipLastPart.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_SkipLastPart.Location = new System.Drawing.Point(12, 222);
            this.button_SkipLastPart.Name = "button_SkipLastPart";
            this.button_SkipLastPart.Size = new System.Drawing.Size(248, 37);
            this.button_SkipLastPart.TabIndex = 19;
            this.button_SkipLastPart.Text = "Skip Last Part of Route";
            this.button_SkipLastPart.UseVisualStyleBackColor = true;
            this.button_SkipLastPart.Click += new System.EventHandler(this.Button_SkipLastPart_Click);
            // 
            // button_newRoute
            // 
            this.button_newRoute.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_newRoute.Location = new System.Drawing.Point(12, 185);
            this.button_newRoute.Name = "button_newRoute";
            this.button_newRoute.Size = new System.Drawing.Size(248, 37);
            this.button_newRoute.TabIndex = 27;
            this.button_newRoute.Text = " New Route (Clear Map)";
            this.button_newRoute.UseVisualStyleBackColor = true;
            this.button_newRoute.Click += new System.EventHandler(this.Button_newRoute_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1384, 861);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Routing Sport Tracks";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDoubleClick);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox_Sport.ResumeLayout(false);
            this.groupBox_Sport.PerformLayout();
            this.groupBox_MapKind.ResumeLayout(false);
            this.groupBox_MapKind.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_ele)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_LoadGPX;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBox_Routing;
        private System.Windows.Forms.Label label_TotT;
        private System.Windows.Forms.Label label_Dis;
        private System.Windows.Forms.TextBox textBox_ElevUp;
        private System.Windows.Forms.TextBox textBox_Dis;
        private System.Windows.Forms.TextBox textBox_Reg;
        private System.Windows.Forms.Label label_Reg;
        private System.Windows.Forms.Button button_CenterTrack;
        private System.Windows.Forms.Button button_newRouteDB;
        private System.Windows.Forms.Button button_SkipLastPart;
        private System.Windows.Forms.Button button_newRoute;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_ele;
        private System.Windows.Forms.GroupBox groupBox_MapKind;
        private System.Windows.Forms.RadioButton radioButton_Hyb;
        private System.Windows.Forms.RadioButton radioButton_Sat;
        private System.Windows.Forms.RadioButton radioButton_Map;
        private System.Windows.Forms.GroupBox groupBox_Sport;
        private System.Windows.Forms.RadioButton radioButton_Race;
        private System.Windows.Forms.RadioButton radioButton_Cross;
        private System.Windows.Forms.RadioButton radioButton_Hik;
        private System.Windows.Forms.Label label_delta;
        private System.Windows.Forms.CheckBox checkBox_Boundary;
        private System.Windows.Forms.Button button_saveImage;
        private System.Windows.Forms.Button button_saveGPX;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_road;
        private System.Windows.Forms.TextBox textBox_way;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_path;
        private System.Windows.Forms.Button button_Mail;
        private System.Windows.Forms.TextBox textBox_ElevDown;
        private System.Windows.Forms.Label label4;
    }
}

