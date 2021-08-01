using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    public class SelectionFilterDeviceBox : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {

            if(e.Category != null)
            {
                Document doc = e.Document;
                if (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_ElectricalFixtures))
                {
                    if (DeviceBox.IsDeviceBox(e, doc))
                        return true;
                }
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
