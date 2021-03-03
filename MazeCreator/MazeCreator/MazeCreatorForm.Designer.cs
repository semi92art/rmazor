
namespace MazeCreator
{
    partial class MazeCreatorForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCreateMazeDB = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_delay = new System.Windows.Forms.TextBox();
            this.btn_Init = new System.Windows.Forms.Button();
            this.lb_log = new System.Windows.Forms.Label();
            this.pb_creation = new System.Windows.Forms.ProgressBar();
            this.textBoxAmount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCreateMazeDB
            // 
            this.buttonCreateMazeDB.Location = new System.Drawing.Point(426, 6);
            this.buttonCreateMazeDB.Name = "buttonCreateMazeDB";
            this.buttonCreateMazeDB.Size = new System.Drawing.Size(71, 23);
            this.buttonCreateMazeDB.TabIndex = 0;
            this.buttonCreateMazeDB.Text = "Create";
            this.buttonCreateMazeDB.UseVisualStyleBackColor = true;
            this.buttonCreateMazeDB.Click += new System.EventHandler(this.buttonCreateMazeDB_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(13, 99);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(500, 495);
            this.webBrowser1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tb_delay);
            this.panel1.Controls.Add(this.btn_Init);
            this.panel1.Controls.Add(this.lb_log);
            this.panel1.Controls.Add(this.pb_creation);
            this.panel1.Controls.Add(this.textBoxAmount);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.buttonCreateMazeDB);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 80);
            this.panel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(188, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Delay";
            // 
            // tb_delay
            // 
            this.tb_delay.Location = new System.Drawing.Point(228, 10);
            this.tb_delay.Name = "tb_delay";
            this.tb_delay.Size = new System.Drawing.Size(44, 20);
            this.tb_delay.TabIndex = 10;
            this.tb_delay.Text = "0";
            // 
            // btn_Init
            // 
            this.btn_Init.Location = new System.Drawing.Point(3, 7);
            this.btn_Init.Name = "btn_Init";
            this.btn_Init.Size = new System.Drawing.Size(57, 25);
            this.btn_Init.TabIndex = 7;
            this.btn_Init.Text = "Init";
            this.btn_Init.UseVisualStyleBackColor = true;
            this.btn_Init.Click += new System.EventHandler(this.btn_Init_Click);
            // 
            // lb_log
            // 
            this.lb_log.Location = new System.Drawing.Point(3, 57);
            this.lb_log.Name = "lb_log";
            this.lb_log.Size = new System.Drawing.Size(163, 13);
            this.lb_log.TabIndex = 5;
            this.lb_log.Text = "Press \"Create\" to start";
            // 
            // pb_creation
            // 
            this.pb_creation.Location = new System.Drawing.Point(0, 38);
            this.pb_creation.Name = "pb_creation";
            this.pb_creation.Size = new System.Drawing.Size(494, 16);
            this.pb_creation.TabIndex = 4;
            // 
            // textBoxAmount
            // 
            this.textBoxAmount.Location = new System.Drawing.Point(111, 10);
            this.textBoxAmount.Name = "textBoxAmount";
            this.textBoxAmount.Size = new System.Drawing.Size(44, 20);
            this.textBoxAmount.TabIndex = 3;
            this.textBoxAmount.Text = "100";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(62, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Amount";
            // 
            // MazeCreatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 592);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.webBrowser1);
            this.Name = "MazeCreatorForm";
            this.Text = "MazeCreator";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox tb_delay;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Button btn_Init;

        private System.Windows.Forms.Label lb_log;

        private System.Windows.Forms.ProgressBar pb_creation;

        #endregion

        private System.Windows.Forms.Button buttonCreateMazeDB;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBoxAmount;
        private System.Windows.Forms.Label label5;
    }
}

