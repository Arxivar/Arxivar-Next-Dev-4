using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgettoClient
{
    public partial class FormConfigurazioneImpostazioni : Form
    {
        public FormConfigurazioneImpostazioni(LibreriaComune.ClasseImpostazioni impostazioni)
        {
            InitializeComponent();
            GestoreOggetto.SelectedObject = impostazioni;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
