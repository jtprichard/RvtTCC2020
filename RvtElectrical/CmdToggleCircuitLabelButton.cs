using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdToggleCircuitLabelButton : IExternalCommand
    //Trigger to Toggle Circuit Label Function On and Off
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ExternalApplication.Instance.ToggleCircuitLabel(commandData.Application);

            return Result.Succeeded;
        }
    }
}
