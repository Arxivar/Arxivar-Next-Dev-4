using System;
using System.ComponentModel;

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
