using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RvtRigging
{
    public static class RibbonUtils
    {
        public static bool GetRibbonItem(string ribbonPanel, string ribbonItem, UIApplication uiapp, out RibbonItem result)
        {
            List<RibbonPanel> rps = uiapp.GetRibbonPanels(ribbonPanel);
            RibbonPanel rp = rps[0];
            IList<RibbonItem> ris = rp.GetItems();
            result = null;
            foreach (RibbonItem ri in ris)
            {
                TextBox tb = ri as TextBox;
                if (ri.Name == ribbonItem)
                    result = ri;
            }
            if (result != null)
                return true;
            else
                return false;
           
        }

    }
}
