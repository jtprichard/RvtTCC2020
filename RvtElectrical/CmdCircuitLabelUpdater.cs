using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;

namespace RvtElectrical
{
    [Transaction(TransactionMode.ReadOnly)]
    public class CmdCircuitLabelUpdater : IExternalCommand
    {

        static IUCircuitLabel _updater = null;
        
        // DMU updater to call class to update device box concat label to new entry

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;

            if(null == _updater)
            {

                _updater = new IUCircuitLabel(app.ActiveAddInId);

                UpdaterRegistry.RegisterUpdater(_updater);
                ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);
                UpdaterRegistry.AddTrigger(_updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
            }
            else
            {
                UpdaterRegistry.UnregisterUpdater(_updater.GetUpdaterId());
                _updater = null;
            }

            return Result.Succeeded;
        }
    }
}
