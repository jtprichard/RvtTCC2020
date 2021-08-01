using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RvtRigging
{
    public class SelectionFilterLineset : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
           
            if(e.Category != null)
            {
                if (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_SpecialityEquipment))
                {
                    if (e.get_Parameter(TCCRiggingSettings.IsLinesetGuid) != null)
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
