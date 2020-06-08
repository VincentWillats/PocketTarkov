using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PocketTarkov
{
    [Serializable]
    public class SettingsData
    {
        public string googleDocURL;

        public Keys hotkey01 = Keys.LMenu;
        public Keys hotkey02 = Keys.V;
    }
}
