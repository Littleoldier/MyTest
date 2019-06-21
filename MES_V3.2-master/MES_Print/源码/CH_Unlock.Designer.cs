namespace WindowsForms_print
{
    partial class CH_Unlock
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
            this.label1 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.sure = new System.Windows.Forms.Button();
            this.reminder = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(119, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "请输入解锁密码：";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(115, 105);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.Size = new System.Drawing.Size(230, 25);
            this.Password.TabIndex = 1;
            this.Password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Password_KeyPress);
            // 
            // sure
            // 
            this.sure.Location = new System.Drawing.Point(115, 153);
            this.sure.Name = "sure";
            this.sure.Size = new System.Drawing.Size(230, 32);
            this.sure.TabIndex = 2;
            this.sure.Text = "确定";
            this.sure.UseVisualStyleBackColor = true;
            this.sure.Click += new System.EventHandler(this.sure_Click);
            // 
            // reminder
            // 
            this.reminder.BackColor = System.Drawing.SystemColors.Menu;
            this.reminder.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.reminder.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.reminder.ForeColor = System.Drawing.Color.Red;
            this.reminder.Location = new System.Drawing.Point(12, 207);
            this.reminder.Multiline = true;
            this.reminder.Name = "reminder";
            this.reminder.Size = new System.Drawing.Size(454, 38);
            this.reminder.TabIndex = 3;
            this.reminder.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CH_Unlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 276);
            this.Controls.Add(this.reminder);
            this.Controls.Add(this.sure);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CH_Unlock";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "彩盒解锁";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Button sure;
        private System.Windows.Forms.TextBox reminder;
    }
}