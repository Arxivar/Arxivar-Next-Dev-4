namespace ProgettoClient
{
    partial class FormTaskRichiestaFerie
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
            this.GridDocumenti = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.GridOperazioni = new System.Windows.Forms.DataGridView();
            this.ComboEsiti = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.GridDocumenti)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridOperazioni)).BeginInit();
            this.SuspendLayout();
            // 
            // GridDocumenti
            // 
            this.GridDocumenti.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridDocumenti.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridDocumenti.Location = new System.Drawing.Point(3, 16);
            this.GridDocumenti.MultiSelect = false;
            this.GridDocumenti.Name = "GridDocumenti";
            this.GridDocumenti.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.GridDocumenti.Size = new System.Drawing.Size(722, 108);
            this.GridDocumenti.TabIndex = 0;
            this.GridDocumenti.DoubleClick += new System.EventHandler(this.GridDocumenti_DoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.GridDocumenti);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(728, 127);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Elenco documenti";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.GridOperazioni);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 127);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(728, 127);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Elenco operazioni";
            // 
            // GridOperazioni
            // 
            this.GridOperazioni.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridOperazioni.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridOperazioni.Location = new System.Drawing.Point(3, 16);
            this.GridOperazioni.Name = "GridOperazioni";
            this.GridOperazioni.Size = new System.Drawing.Size(722, 108);
            this.GridOperazioni.TabIndex = 0;
            // 
            // ComboEsiti
            // 
            this.ComboEsiti.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboEsiti.FormattingEnabled = true;
            this.ComboEsiti.Location = new System.Drawing.Point(12, 274);
            this.ComboEsiti.Name = "ComboEsiti";
            this.ComboEsiti.Size = new System.Drawing.Size(226, 21);
            this.ComboEsiti.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(244, 274);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Avanza Task";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormTaskRichiestaFerie
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 311);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ComboEsiti);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormTaskRichiestaFerie";
            this.Text = "FormTaskRichiestaFerie";
            ((System.ComponentModel.ISupportInitialize)(this.GridDocumenti)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridOperazioni)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView GridDocumenti;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView GridOperazioni;
        private System.Windows.Forms.ComboBox ComboEsiti;
        private System.Windows.Forms.Button button1;
    }
}