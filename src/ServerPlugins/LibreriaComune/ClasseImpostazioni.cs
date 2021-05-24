using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LibreriaComune
{
    [Serializable]
    public class ClasseImpostazioni
    {
        [Description("Etichetta da applicare all'oggetto")]             
        [Category("Proprietà di configurazione")]
        [DisplayName("Etichetta")]       
        public string Etichetta { get; set; }


    }
}
