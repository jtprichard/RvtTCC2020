#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

#endregion

namespace IUpdateRvtElec
{
    public class CircuitLabelUpdaterApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            //// Register updater with Revit
            //WallUpdater updater = new WallUpdater(a.ActiveAddInId);
            //UpdaterRegistry.RegisterUpdater(updater);

            //// Change Scope = any Wall element
            //ElementClassFilter wallFilter = new ElementClassFilter(typeof(Wall));

            //// Change type = element addition
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), parameterFilter, Element.GetChangeTypeElementAddition());
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
