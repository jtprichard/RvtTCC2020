using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace RvtRigging
{
    class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Performance_Rigging";
            string path = Assembly.GetExecutingAssembly().Location;

            //Create Ribbon Tab
            application.CreateRibbonTab(tabName);

            //Create Ribbon Panels
            RibbonPanels.RibbonPanelRigging(application, path, tabName);

            return Result.Succeeded;
        }

    }
}
