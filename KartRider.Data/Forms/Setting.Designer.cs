using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace KartRider
{
    partial class Setting
    {
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setting));
            Proxy_comboBox = new ComboBox();
            Proxy_label = new Label();
            AiSpeed_comboBox = new ComboBox();
            AiSpeed_label = new Label();
            Speed_comboBox = new ComboBox();
            Speed_label = new Label();
            Version_comboBox = new ComboBox();
            Version_label = new Label();
            PlayerName = new TextBox();
            Name_label = new Label();
            ServerIP = new TextBox();
            IP_label = new Label();
            ServerPort = new TextBox();
            Port_label = new Label();
            NgsOn = new CheckBox();
            AutoUpdate = new CheckBox();
            SuspendLayout();
            // 
            // Proxy_comboBox
            // 
            Proxy_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Proxy_comboBox.ForeColor = Color.Red;
            Proxy_comboBox.FormattingEnabled = true;
            Proxy_comboBox.Location = new Point(68, 194);
            Proxy_comboBox.Name = "Proxy_comboBox";
            Proxy_comboBox.Size = new Size(114, 20);
            Proxy_comboBox.TabIndex = 7;
            // 
            // Proxy_label
            // 
            Proxy_label.AutoSize = true;
            Proxy_label.ForeColor = Color.Blue;
            Proxy_label.Location = new Point(19, 198);
            Proxy_label.Name = "Proxy_label";
            Proxy_label.Size = new Size(35, 12);
            Proxy_label.TabIndex = 9;
            Proxy_label.Text = "代理:";
            // 
            // AiSpeed_comboBox
            // 
            AiSpeed_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            AiSpeed_comboBox.ForeColor = Color.Red;
            AiSpeed_comboBox.FormattingEnabled = true;
            AiSpeed_comboBox.Location = new Point(68, 165);
            AiSpeed_comboBox.Name = "AiSpeed_comboBox";
            AiSpeed_comboBox.Size = new Size(114, 20);
            AiSpeed_comboBox.TabIndex = 6;
            // 
            // AiSpeed_label
            // 
            AiSpeed_label.AutoSize = true;
            AiSpeed_label.ForeColor = Color.Blue;
            AiSpeed_label.Location = new Point(19, 169);
            AiSpeed_label.Name = "AiSpeed_label";
            AiSpeed_label.Size = new Size(47, 12);
            AiSpeed_label.TabIndex = 8;
            AiSpeed_label.Text = "Ai速度:";
            // 
            // Speed_comboBox
            // 
            Speed_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Speed_comboBox.ForeColor = Color.Red;
            Speed_comboBox.FormattingEnabled = true;
            Speed_comboBox.Location = new Point(68, 136);
            Speed_comboBox.Name = "Speed_comboBox";
            Speed_comboBox.Size = new Size(114, 20);
            Speed_comboBox.TabIndex = 5;
            // 
            // Speed_label
            // 
            Speed_label.AutoSize = true;
            Speed_label.ForeColor = Color.Blue;
            Speed_label.Location = new Point(19, 140);
            Speed_label.Name = "Speed_label";
            Speed_label.Size = new Size(35, 12);
            Speed_label.TabIndex = 6;
            Speed_label.Text = "速度:";
            // 
            // Version_comboBox
            // 
            Version_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Version_comboBox.ForeColor = Color.Red;
            Version_comboBox.FormattingEnabled = true;
            Version_comboBox.Location = new Point(68, 107);
            Version_comboBox.Name = "Version_comboBox";
            Version_comboBox.Size = new Size(114, 20);
            Version_comboBox.TabIndex = 4;
            // 
            // Version_label
            // 
            Version_label.AutoSize = true;
            Version_label.ForeColor = Color.Blue;
            Version_label.Location = new Point(19, 111);
            Version_label.Name = "Version_label";
            Version_label.Size = new Size(35, 12);
            Version_label.TabIndex = 7;
            Version_label.Text = "版本:";
            // 
            // PlayerName
            // 
            PlayerName.Location = new Point(68, 20);
            PlayerName.Name = "PlayerName";
            PlayerName.Size = new Size(114, 21);
            PlayerName.TabIndex = 1;
            // 
            // Name_label
            // 
            Name_label.AutoSize = true;
            Name_label.ForeColor = Color.Blue;
            Name_label.Location = new Point(19, 24);
            Name_label.Name = "Name_label";
            Name_label.Size = new Size(35, 12);
            Name_label.TabIndex = 2;
            Name_label.Text = "暱稱:";
            // 
            // ServerIP
            // 
            ServerIP.Location = new Point(68, 49);
            ServerIP.Name = "ServerIP";
            ServerIP.Size = new Size(114, 21);
            ServerIP.TabIndex = 2;
            ServerIP.Text = "127.0.0.1";
            // 
            // IP_label
            // 
            IP_label.AutoSize = true;
            IP_label.ForeColor = Color.Blue;
            IP_label.Location = new Point(19, 53);
            IP_label.Name = "IP_label";
            IP_label.Size = new Size(23, 12);
            IP_label.TabIndex = 3;
            IP_label.Text = "IP:";
            // 
            // ServerPort
            // 
            ServerPort.Location = new Point(68, 78);
            ServerPort.Name = "ServerPort";
            ServerPort.Size = new Size(114, 21);
            ServerPort.TabIndex = 3;
            ServerPort.Text = "39311";
            // 
            // Port_label
            // 
            Port_label.AutoSize = true;
            Port_label.ForeColor = Color.Blue;
            Port_label.Location = new Point(19, 82);
            Port_label.Name = "Port_label";
            Port_label.Size = new Size(47, 12);
            Port_label.TabIndex = 4;
            Port_label.Text = "連接埠:";
            // 
            // NgsOn
            // 
            NgsOn.AutoSize = true;
            NgsOn.ForeColor = Color.Blue;
            NgsOn.Location = new Point(190, 169);
            NgsOn.Name = "NgsOn";
            NgsOn.Size = new Size(78, 16);
            NgsOn.TabIndex = 8;
            NgsOn.Text = "反作弊Ngs";
            NgsOn.UseVisualStyleBackColor = true;
            // 
            // AutoUpdate
            // 
            AutoUpdate.AutoSize = true;
            AutoUpdate.ForeColor = Color.Blue;
            AutoUpdate.Location = new Point(190, 198);
            AutoUpdate.Name = "AutoUpdate";
            AutoUpdate.Size = new Size(72, 16);
            AutoUpdate.TabIndex = 9;
            AutoUpdate.Text = "自动更新";
            AutoUpdate.UseVisualStyleBackColor = true;
            // 
            // Setting
            // 
            AutoScaleDimensions = new SizeF(6F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(273, 229);
            Controls.Add(PlayerName);
            Controls.Add(Name_label);
            Controls.Add(ServerIP);
            Controls.Add(IP_label);
            Controls.Add(ServerPort);
            Controls.Add(Port_label);
            Controls.Add(Speed_comboBox);
            Controls.Add(Speed_label);
            Controls.Add(Version_comboBox);
            Controls.Add(Version_label);
            Controls.Add(AiSpeed_comboBox);
            Controls.Add(AiSpeed_label);
            Controls.Add(Proxy_comboBox);
            Controls.Add(Proxy_label);
            Controls.Add(NgsOn);
            Controls.Add(AutoUpdate);
            Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Setting";
            StartPosition = FormStartPosition.CenterParent;
            Text = "设置";
            FormClosing += OnFormClosing;
            Load += OnLoad;
            ResumeLayout(false);
            PerformLayout();
        }

        private TextBox PlayerName;
        private TextBox ServerIP;
        private TextBox ServerPort;
        private ComboBox Speed_comboBox;
        private ComboBox Version_comboBox;
        private ComboBox AiSpeed_comboBox;
        private ComboBox Proxy_comboBox;
        private CheckBox NgsOn;
        private CheckBox AutoUpdate;
        private Label Name_label;
        private Label IP_label;
        private Label Port_label;
        private Label Speed_label;
        private Label Version_label;
        private Label AiSpeed_label;
        private Label Proxy_label;
    }
}