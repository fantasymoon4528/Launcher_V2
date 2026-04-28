using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace KartRider
{
    partial class Launcher
    {
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            Start_Button = new Button();
            Setting_Button = new Button();
            button_ToggleTerminal = new Button();
            ConsoleLogger = new Button();
            label_Client = new Label();
            ClientVersion = new Label();
            VersionLabel = new Label();
            GitHub = new Label();
            KartInfo = new Label();
            Launcher_label = new Label();
            SuspendLayout();
            // 
            // Start_Button
            // 
            Start_Button.Location = new Point(19, 20);
            Start_Button.Name = "Start_Button";
            Start_Button.Size = new Size(114, 23);
            Start_Button.TabIndex = 0;
            Start_Button.Text = "啟動遊戲";
            Start_Button.UseVisualStyleBackColor = true;
            Start_Button.Click += Start_Button_Click;
            // 
            // Setting_Button
            // 
            Setting_Button.Location = new Point(19, 49);
            Setting_Button.Name = "Setting_Button";
            Setting_Button.Size = new Size(114, 23);
            Setting_Button.TabIndex = 1;
            Setting_Button.Text = "設置";
            Setting_Button.UseVisualStyleBackColor = true;
            Setting_Button.Click += Setting_Button_Click;
            // 
            // button_ToggleTerminal
            // 
            button_ToggleTerminal.Location = new Point(19, 78);
            button_ToggleTerminal.Name = "button_ToggleTerminal";
            button_ToggleTerminal.Size = new Size(57, 23);
            button_ToggleTerminal.TabIndex = 2;
            button_ToggleTerminal.Text = "控制台";
            button_ToggleTerminal.UseVisualStyleBackColor = true;
            button_ToggleTerminal.Click += button_ToggleTerminal_Click;
            // 
            // ConsoleLogger
            // 
            ConsoleLogger.Location = new Point(76, 78);
            ConsoleLogger.Name = "ConsoleLogger";
            ConsoleLogger.Size = new Size(57, 23);
            ConsoleLogger.TabIndex = 3;
            ConsoleLogger.Text = "输出";
            ConsoleLogger.UseVisualStyleBackColor = true;
            ConsoleLogger.Click += ConsoleLogger_Click;
            // 
            // label_Client
            // 
            label_Client.AutoSize = true;
            label_Client.BackColor = SystemColors.Control;
            label_Client.Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Client.ForeColor = Color.Blue;
            label_Client.Location = new Point(2, 144);
            label_Client.Name = "label_Client";
            label_Client.Size = new Size(47, 12);
            label_Client.TabIndex = 4;
            label_Client.Text = "Client:";
            label_Client.Click += label_Client_Click;
            // 
            // ClientVersion
            // 
            ClientVersion.AutoSize = true;
            ClientVersion.BackColor = SystemColors.Control;
            ClientVersion.Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ClientVersion.ForeColor = Color.Red;
            ClientVersion.Location = new Point(45, 144);
            ClientVersion.Name = "ClientVersion";
            ClientVersion.Size = new Size(0, 12);
            ClientVersion.TabIndex = 5;
            ClientVersion.Click += label_Client_Click;
            // 
            // VersionLabel
            // 
            VersionLabel.AutoSize = true;
            VersionLabel.BackColor = SystemColors.Control;
            VersionLabel.Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            VersionLabel.ForeColor = Color.Red;
            VersionLabel.Location = new Point(57, 160);
            VersionLabel.Name = "VersionLabel";
            VersionLabel.Size = new Size(0, 12);
            VersionLabel.TabIndex = 7;
            VersionLabel.Click += GitHub_Click;
            // 
            // GitHub
            // 
            GitHub.AutoSize = true;
            GitHub.ForeColor = Color.Blue;
            GitHub.Location = new Point(213, 144);
            GitHub.Name = "GitHub";
            GitHub.Size = new Size(41, 12);
            GitHub.TabIndex = 8;
            GitHub.Text = "GitHub";
            GitHub.Click += GitHub_Click;
            // 
            // KartInfo
            // 
            KartInfo.AutoSize = true;
            KartInfo.ForeColor = Color.Blue;
            KartInfo.Location = new Point(201, 160);
            KartInfo.Name = "KartInfo";
            KartInfo.Size = new Size(53, 12);
            KartInfo.TabIndex = 9;
            KartInfo.Text = "KartInfo";
            KartInfo.Click += KartInfo_Click;
            // 
            // Launcher_label
            // 
            Launcher_label.AutoSize = true;
            Launcher_label.ForeColor = Color.Blue;
            Launcher_label.Location = new Point(2, 160);
            Launcher_label.Name = "Launcher_label";
            Launcher_label.Size = new Size(59, 12);
            Launcher_label.TabIndex = 6;
            Launcher_label.Text = "Launcher:";
            Launcher_label.Click += GitHub_Click;
            // 
            // Launcher
            // 
            AutoScaleDimensions = new SizeF(6F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(257, 180);
            Controls.Add(Start_Button);
            Controls.Add(Setting_Button);
            Controls.Add(button_ToggleTerminal);
            Controls.Add(ConsoleLogger);
            Controls.Add(ClientVersion);
            Controls.Add(label_Client);
            Controls.Add(VersionLabel);
            Controls.Add(Launcher_label);
            Controls.Add(GitHub);
            Controls.Add(KartInfo);
            Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Launcher";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Launcher";
            FormClosing += OnFormClosing;
            Load += OnLoad;
            ResumeLayout(false);
            PerformLayout();
        }

        private Button Start_Button;
        private Button Setting_Button;
        private Button button_ToggleTerminal;
        private Button ConsoleLogger;
        private Label label_Client;
        private Label ClientVersion;
        private Label Launcher_label;
        private Label VersionLabel;
        private Label GitHub;
        private Label KartInfo;
    }
}