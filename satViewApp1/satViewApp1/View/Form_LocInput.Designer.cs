namespace satViewApp1.View
{
    partial class Form_LocInput
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
            this.textBox_loc_lon = new System.Windows.Forms.TextBox();
            this.textBox_loc_lat = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_loc_reset = new System.Windows.Forms.Button();
            this.button_loc_submit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_loc_lon
            // 
            this.textBox_loc_lon.Location = new System.Drawing.Point(130, 33);
            this.textBox_loc_lon.Name = "textBox_loc_lon";
            this.textBox_loc_lon.Size = new System.Drawing.Size(155, 25);
            this.textBox_loc_lon.TabIndex = 0;
            this.textBox_loc_lon.Text = "110";
            // 
            // textBox_loc_lat
            // 
            this.textBox_loc_lat.Location = new System.Drawing.Point(130, 73);
            this.textBox_loc_lat.Name = "textBox_loc_lat";
            this.textBox_loc_lat.Size = new System.Drawing.Size(155, 25);
            this.textBox_loc_lat.TabIndex = 1;
            this.textBox_loc_lat.Text = "30";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(48, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 19);
            this.label1.TabIndex = 2;
            this.label1.Text = "经度：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(47, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "纬度：";
            // 
            // button_loc_reset
            // 
            this.button_loc_reset.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_loc_reset.Location = new System.Drawing.Point(107, 118);
            this.button_loc_reset.Name = "button_loc_reset";
            this.button_loc_reset.Size = new System.Drawing.Size(75, 30);
            this.button_loc_reset.TabIndex = 4;
            this.button_loc_reset.Text = "重置";
            this.button_loc_reset.UseVisualStyleBackColor = true;
            this.button_loc_reset.Click += new System.EventHandler(this.button_loc_reset_Click);
            // 
            // button_loc_submit
            // 
            this.button_loc_submit.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_loc_submit.Location = new System.Drawing.Point(210, 118);
            this.button_loc_submit.Name = "button_loc_submit";
            this.button_loc_submit.Size = new System.Drawing.Size(75, 30);
            this.button_loc_submit.TabIndex = 5;
            this.button_loc_submit.Text = "确定";
            this.button_loc_submit.UseVisualStyleBackColor = true;
            this.button_loc_submit.Click += new System.EventHandler(this.button_loc_submit_Click);
            // 
            // Form_LocInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 172);
            this.Controls.Add(this.button_loc_submit);
            this.Controls.Add(this.button_loc_reset);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_loc_lat);
            this.Controls.Add(this.textBox_loc_lon);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_LocInput";
            this.Text = "请输入用户位置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_loc_lon;
        private System.Windows.Forms.TextBox textBox_loc_lat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_loc_reset;
        private System.Windows.Forms.Button button_loc_submit;
    }
}