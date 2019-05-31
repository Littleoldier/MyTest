namespace WindowsForms_print
{
    partial class PrintFromExcel
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ImportPath = new System.Windows.Forms.TextBox();
            this.Import = new System.Windows.Forms.Button();
            this.RowNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label5 = new System.Windows.Forms.Label();
            this.RePrintIMEI1 = new System.Windows.Forms.TextBox();
            this.RePrint = new System.Windows.Forms.Button();
            this.RePrintCheck = new System.Windows.Forms.Button();
            this.RefeshTemplate = new System.Windows.Forms.Button();
            this.Debug_Print = new System.Windows.Forms.Button();
            this.OpenTemplate = new System.Windows.Forms.Button();
            this.WhatToCheck = new System.Windows.Forms.TextBox();
            this.OpenExcel = new System.Windows.Forms.Button();
            this.E_Check = new System.Windows.Forms.Button();
            this.TemplateNum = new System.Windows.Forms.TextBox();
            this.Printer = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Select_Template = new System.Windows.Forms.TextBox();
            this.Open_Template = new System.Windows.Forms.Button();
            this.ExcelToPrint = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.remined = new System.Windows.Forms.TextBox();
            this.CB_Zhidan = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // ImportPath
            // 
            this.ImportPath.Location = new System.Drawing.Point(118, 10);
            this.ImportPath.Margin = new System.Windows.Forms.Padding(2);
            this.ImportPath.Name = "ImportPath";
            this.ImportPath.Size = new System.Drawing.Size(310, 21);
            this.ImportPath.TabIndex = 0;
            // 
            // Import
            // 
            this.Import.Location = new System.Drawing.Point(5, 10);
            this.Import.Margin = new System.Windows.Forms.Padding(2);
            this.Import.Name = "Import";
            this.Import.Size = new System.Drawing.Size(108, 20);
            this.Import.TabIndex = 1;
            this.Import.Text = "导入Excel";
            this.Import.UseVisualStyleBackColor = true;
            this.Import.Click += new System.EventHandler(this.Import_Click);
            // 
            // RowNumber
            // 
            this.RowNumber.Location = new System.Drawing.Point(118, 72);
            this.RowNumber.Margin = new System.Windows.Forms.Padding(2);
            this.RowNumber.Name = "RowNumber";
            this.RowNumber.Size = new System.Drawing.Size(71, 21);
            this.RowNumber.TabIndex = 2;
            this.RowNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RowNumber_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(43, 77);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "请输入行数：";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(3, 2);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Linen;
            this.splitContainer1.Panel1.Controls.Add(this.CB_Zhidan);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.RePrintIMEI1);
            this.splitContainer1.Panel1.Controls.Add(this.RePrint);
            this.splitContainer1.Panel1.Controls.Add(this.RePrintCheck);
            this.splitContainer1.Panel1.Controls.Add(this.RefeshTemplate);
            this.splitContainer1.Panel1.Controls.Add(this.Debug_Print);
            this.splitContainer1.Panel1.Controls.Add(this.OpenTemplate);
            this.splitContainer1.Panel1.Controls.Add(this.WhatToCheck);
            this.splitContainer1.Panel1.Controls.Add(this.OpenExcel);
            this.splitContainer1.Panel1.Controls.Add(this.E_Check);
            this.splitContainer1.Panel1.Controls.Add(this.TemplateNum);
            this.splitContainer1.Panel1.Controls.Add(this.Printer);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.Select_Template);
            this.splitContainer1.Panel1.Controls.Add(this.Open_Template);
            this.splitContainer1.Panel1.Controls.Add(this.ExcelToPrint);
            this.splitContainer1.Panel1.Controls.Add(this.ImportPath);
            this.splitContainer1.Panel1.Controls.Add(this.RowNumber);
            this.splitContainer1.Panel1.Controls.Add(this.Import);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainer1.Panel2.Controls.Add(this.dataGridView1);
            this.splitContainer1.Size = new System.Drawing.Size(1253, 538);
            this.splitContainer1.SplitterDistance = 112;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(869, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "请扫入IMEI1进行重打";
            // 
            // RePrintIMEI1
            // 
            this.RePrintIMEI1.Location = new System.Drawing.Point(871, 68);
            this.RePrintIMEI1.Name = "RePrintIMEI1";
            this.RePrintIMEI1.Size = new System.Drawing.Size(167, 21);
            this.RePrintIMEI1.TabIndex = 21;
            this.RePrintIMEI1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RePrintIMEI1_KeyPress);
            // 
            // RePrint
            // 
            this.RePrint.Location = new System.Drawing.Point(355, 72);
            this.RePrint.Margin = new System.Windows.Forms.Padding(2);
            this.RePrint.Name = "RePrint";
            this.RePrint.Size = new System.Drawing.Size(72, 21);
            this.RePrint.TabIndex = 20;
            this.RePrint.Text = "重打";
            this.RePrint.UseVisualStyleBackColor = true;
            this.RePrint.Click += new System.EventHandler(this.RePrint_Click);
            // 
            // RePrintCheck
            // 
            this.RePrintCheck.Location = new System.Drawing.Point(774, 70);
            this.RePrintCheck.Margin = new System.Windows.Forms.Padding(2);
            this.RePrintCheck.Name = "RePrintCheck";
            this.RePrintCheck.Size = new System.Drawing.Size(72, 20);
            this.RePrintCheck.TabIndex = 19;
            this.RePrintCheck.Text = "重打查询";
            this.RePrintCheck.UseVisualStyleBackColor = true;
            this.RePrintCheck.Click += new System.EventHandler(this.RePrintCheck_Click);
            // 
            // RefeshTemplate
            // 
            this.RefeshTemplate.Location = new System.Drawing.Point(774, 39);
            this.RefeshTemplate.Margin = new System.Windows.Forms.Padding(2);
            this.RefeshTemplate.Name = "RefeshTemplate";
            this.RefeshTemplate.Size = new System.Drawing.Size(72, 22);
            this.RefeshTemplate.TabIndex = 18;
            this.RefeshTemplate.Text = "刷新模板";
            this.RefeshTemplate.UseVisualStyleBackColor = true;
            this.RefeshTemplate.Click += new System.EventHandler(this.RefeshTemplate_Click);
            // 
            // Debug_Print
            // 
            this.Debug_Print.Location = new System.Drawing.Point(273, 72);
            this.Debug_Print.Margin = new System.Windows.Forms.Padding(2);
            this.Debug_Print.Name = "Debug_Print";
            this.Debug_Print.Size = new System.Drawing.Size(72, 21);
            this.Debug_Print.TabIndex = 17;
            this.Debug_Print.Text = "调试打印";
            this.Debug_Print.UseVisualStyleBackColor = true;
            this.Debug_Print.Click += new System.EventHandler(this.Debug_Print_Click);
            // 
            // OpenTemplate
            // 
            this.OpenTemplate.Location = new System.Drawing.Point(694, 39);
            this.OpenTemplate.Margin = new System.Windows.Forms.Padding(2);
            this.OpenTemplate.Name = "OpenTemplate";
            this.OpenTemplate.Size = new System.Drawing.Size(72, 22);
            this.OpenTemplate.TabIndex = 16;
            this.OpenTemplate.Text = "打开模板";
            this.OpenTemplate.UseVisualStyleBackColor = true;
            this.OpenTemplate.Click += new System.EventHandler(this.OpenTemplate_Click);
            // 
            // WhatToCheck
            // 
            this.WhatToCheck.Location = new System.Drawing.Point(533, 69);
            this.WhatToCheck.Margin = new System.Windows.Forms.Padding(2);
            this.WhatToCheck.Name = "WhatToCheck";
            this.WhatToCheck.Size = new System.Drawing.Size(155, 21);
            this.WhatToCheck.TabIndex = 15;
            // 
            // OpenExcel
            // 
            this.OpenExcel.Location = new System.Drawing.Point(615, 39);
            this.OpenExcel.Margin = new System.Windows.Forms.Padding(2);
            this.OpenExcel.Name = "OpenExcel";
            this.OpenExcel.Size = new System.Drawing.Size(72, 22);
            this.OpenExcel.TabIndex = 14;
            this.OpenExcel.Text = "打开Excel";
            this.OpenExcel.UseVisualStyleBackColor = true;
            this.OpenExcel.Click += new System.EventHandler(this.OpenExcel_Click);
            // 
            // E_Check
            // 
            this.E_Check.Location = new System.Drawing.Point(694, 70);
            this.E_Check.Margin = new System.Windows.Forms.Padding(2);
            this.E_Check.Name = "E_Check";
            this.E_Check.Size = new System.Drawing.Size(72, 20);
            this.E_Check.TabIndex = 12;
            this.E_Check.Text = "查询";
            this.E_Check.UseVisualStyleBackColor = true;
            this.E_Check.Click += new System.EventHandler(this.E_Check_Click);
            // 
            // TemplateNum
            // 
            this.TemplateNum.Location = new System.Drawing.Point(533, 41);
            this.TemplateNum.Margin = new System.Windows.Forms.Padding(2);
            this.TemplateNum.Name = "TemplateNum";
            this.TemplateNum.Size = new System.Drawing.Size(72, 21);
            this.TemplateNum.TabIndex = 9;
            this.TemplateNum.Text = "1";
            this.TemplateNum.Leave += new System.EventHandler(this.TemplateNum_Leave);
            // 
            // Printer
            // 
            this.Printer.FormattingEnabled = true;
            this.Printer.Location = new System.Drawing.Point(533, 12);
            this.Printer.Margin = new System.Windows.Forms.Padding(2);
            this.Printer.Name = "Printer";
            this.Printer.Size = new System.Drawing.Size(314, 20);
            this.Printer.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(459, 16);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "选择打印机：";
            // 
            // Select_Template
            // 
            this.Select_Template.Location = new System.Drawing.Point(118, 39);
            this.Select_Template.Margin = new System.Windows.Forms.Padding(2);
            this.Select_Template.Name = "Select_Template";
            this.Select_Template.Size = new System.Drawing.Size(310, 21);
            this.Select_Template.TabIndex = 6;
            // 
            // Open_Template
            // 
            this.Open_Template.Location = new System.Drawing.Point(5, 39);
            this.Open_Template.Margin = new System.Windows.Forms.Padding(2);
            this.Open_Template.Name = "Open_Template";
            this.Open_Template.Size = new System.Drawing.Size(108, 20);
            this.Open_Template.TabIndex = 5;
            this.Open_Template.Text = "选择模板";
            this.Open_Template.UseVisualStyleBackColor = true;
            this.Open_Template.Click += new System.EventHandler(this.Open_Template_Click);
            // 
            // ExcelToPrint
            // 
            this.ExcelToPrint.Location = new System.Drawing.Point(192, 72);
            this.ExcelToPrint.Margin = new System.Windows.Forms.Padding(2);
            this.ExcelToPrint.Name = "ExcelToPrint";
            this.ExcelToPrint.Size = new System.Drawing.Size(72, 21);
            this.ExcelToPrint.TabIndex = 4;
            this.ExcelToPrint.Text = "打印";
            this.ExcelToPrint.UseVisualStyleBackColor = true;
            this.ExcelToPrint.Click += new System.EventHandler(this.ExcelToPrint_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(448, 46);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "模板打印份数：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(459, 74);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "请输入查询：";
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.Location = new System.Drawing.Point(2, 2);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(1249, 420);
            this.dataGridView1.TabIndex = 0;
            // 
            // remined
            // 
            this.remined.BackColor = System.Drawing.Color.Linen;
            this.remined.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.remined.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.remined.ForeColor = System.Drawing.Color.Red;
            this.remined.Location = new System.Drawing.Point(5, 546);
            this.remined.Margin = new System.Windows.Forms.Padding(2);
            this.remined.Multiline = true;
            this.remined.Name = "remined";
            this.remined.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.remined.Size = new System.Drawing.Size(1244, 102);
            this.remined.TabIndex = 5;
            // 
            // CB_Zhidan
            // 
            this.CB_Zhidan.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_Zhidan.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_Zhidan.FormattingEnabled = true;
            this.CB_Zhidan.Location = new System.Drawing.Point(916, 13);
            this.CB_Zhidan.Name = "CB_Zhidan";
            this.CB_Zhidan.Size = new System.Drawing.Size(122, 20);
            this.CB_Zhidan.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(869, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 24;
            this.label6.Text = "制单号:";
            // 
            // PrintFromExcel
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Linen;
            this.ClientSize = new System.Drawing.Size(1258, 658);
            this.Controls.Add(this.remined);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PrintFromExcel";
            this.Text = "导入Excel打印";
            this.Load += new System.EventHandler(this.PrintFromExcel_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ImportPath;
        private System.Windows.Forms.Button Import;
        private System.Windows.Forms.TextBox RowNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox Select_Template;
        private System.Windows.Forms.Button Open_Template;
        private System.Windows.Forms.Button ExcelToPrint;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox Printer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button E_Check;
        private System.Windows.Forms.TextBox TemplateNum;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button OpenExcel;
        private System.Windows.Forms.TextBox remined;
        private System.Windows.Forms.TextBox WhatToCheck;
        private System.Windows.Forms.Button OpenTemplate;
        private System.Windows.Forms.Button Debug_Print;
        private System.Windows.Forms.Button RefeshTemplate;
        private System.Windows.Forms.Button RePrintCheck;
        private System.Windows.Forms.Button RePrint;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox RePrintIMEI1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox CB_Zhidan;
    }
}