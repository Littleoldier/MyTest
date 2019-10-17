namespace WindowsForms_print
{
    partial class UpdateSqlConn
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CIp = new System.Windows.Forms.TextBox();
            this.Cport = new System.Windows.Forms.TextBox();
            this.CUser = new System.Windows.Forms.TextBox();
            this.CPassword = new System.Windows.Forms.TextBox();
            this.TextConn = new System.Windows.Forms.Button();
            this.SaveCon = new System.Windows.Forms.Button();
            this.cansel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(91, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(91, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 14);
            this.label2.TabIndex = 0;
            this.label2.Text = "账号";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(91, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 14);
            this.label3.TabIndex = 0;
            this.label3.Text = "端口号";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(91, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 14);
            this.label4.TabIndex = 0;
            this.label4.Text = "密码";
            // 
            // CIp
            // 
            this.CIp.Location = new System.Drawing.Point(160, 33);
            this.CIp.Name = "CIp";
            this.CIp.Size = new System.Drawing.Size(181, 21);
            this.CIp.TabIndex = 1;
            // 
            // Cport
            // 
            this.Cport.Location = new System.Drawing.Point(160, 84);
            this.Cport.Name = "Cport";
            this.Cport.Size = new System.Drawing.Size(181, 21);
            this.Cport.TabIndex = 2;
            this.Cport.Text = "1433";
            // 
            // CUser
            // 
            this.CUser.Location = new System.Drawing.Point(160, 135);
            this.CUser.Name = "CUser";
            this.CUser.Size = new System.Drawing.Size(181, 21);
            this.CUser.TabIndex = 3;
            // 
            // CPassword
            // 
            this.CPassword.Location = new System.Drawing.Point(160, 186);
            this.CPassword.Name = "CPassword";
            this.CPassword.PasswordChar = '*';
            this.CPassword.Size = new System.Drawing.Size(181, 21);
            this.CPassword.TabIndex = 4;
            // 
            // TextConn
            // 
            this.TextConn.Location = new System.Drawing.Point(94, 238);
            this.TextConn.Name = "TextConn";
            this.TextConn.Size = new System.Drawing.Size(103, 23);
            this.TextConn.TabIndex = 10;
            this.TextConn.Text = "测试连接并保存";
            this.TextConn.UseVisualStyleBackColor = true;
            this.TextConn.Click += new System.EventHandler(this.TextConn_Click);
            // 
            // SaveCon
            // 
            this.SaveCon.Location = new System.Drawing.Point(237, 238);
            this.SaveCon.Name = "SaveCon";
            this.SaveCon.Size = new System.Drawing.Size(103, 23);
            this.SaveCon.TabIndex = 11;
            this.SaveCon.Text = "保存";
            this.SaveCon.UseVisualStyleBackColor = true;
            this.SaveCon.Visible = false;
            this.SaveCon.Click += new System.EventHandler(this.SaveCon_Click);
            // 
            // cansel
            // 
            this.cansel.Location = new System.Drawing.Point(237, 238);
            this.cansel.Name = "cansel";
            this.cansel.Size = new System.Drawing.Size(103, 23);
            this.cansel.TabIndex = 12;
            this.cansel.Text = "取消";
            this.cansel.UseVisualStyleBackColor = true;
            this.cansel.Click += new System.EventHandler(this.cansel_Click);
            // 
            // UpdateSqlConn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 302);
            this.Controls.Add(this.cansel);
            this.Controls.Add(this.SaveCon);
            this.Controls.Add(this.TextConn);
            this.Controls.Add(this.CPassword);
            this.Controls.Add(this.CUser);
            this.Controls.Add(this.Cport);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CIp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "UpdateSqlConn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "数据库配置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox CIp;
        private System.Windows.Forms.TextBox Cport;
        private System.Windows.Forms.TextBox CUser;
        private System.Windows.Forms.TextBox CPassword;
        private System.Windows.Forms.Button TextConn;
        private System.Windows.Forms.Button SaveCon;
        private System.Windows.Forms.Button cansel;
    }
}