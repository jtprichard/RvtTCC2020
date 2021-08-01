using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RvtElectrical
{
    public static class RibbonUtils
    {
        public static bool GetRibbonItem(string ribbonTab, string ribbonPanel, string ribbonItem, UIApplication uiapp, out RibbonItem result)
        {
            try
            {
                List<RibbonPanel> rps = uiapp.GetRibbonPanels(ribbonTab);

                RibbonPanel rp = null;
                foreach (RibbonPanel r in rps)
                {
                    if (r.Name == ribbonPanel)
                        rp = r;
                }

                IList<RibbonItem> ris = rp.GetItems();
                result = null;
                foreach (RibbonItem ri in ris)
                {
                    //TextBox tb = ri as TextBox;
                    if (ri.Name == ribbonItem)
                        result = ri;
                }
                if (result != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                UIUtils.EBSGenException(ex);
                result = null;
                return false;
            }

        }
        public static bool GetRibbonItemCb(string ribbonPanel, string ribbonItem, UIApplication uiapp, out RibbonItem result)
        {
            List<RibbonPanel> rps = uiapp.GetRibbonPanels(ribbonPanel);
            RibbonPanel rp = rps[2];
            IList<RibbonItem> ris = rp.GetItems();
            result = null;
            foreach (RibbonItem ri in ris)
            {
                ComboBox cb = ri as ComboBox;
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
