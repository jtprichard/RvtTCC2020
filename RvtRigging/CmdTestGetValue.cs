using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RvtRigging
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdTestGetValue : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //STATIC VARIABLES FOR TESTING
            int lsNumber = 20;
            double lsSpacing = 18.5;
            
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only available in Model");
                return Result.Failed;
            }

            Lineset lsObject = null;

            //List<RibbonPanel> rps = uiapp.GetRibbonPanels("Performance_Rigging");
            //RibbonPanel rp = rps[0];
            //IList<RibbonItem> ris = rp.GetItems();
            //string tempResult = "";
            //foreach (RibbonItem ri in ris)
            //{
            //    TextBox tb = ri as TextBox;
            //    if (ri.Name == "Lineset_Spacing")
            //        tempResult = tb.Value.ToString();
            //}
            bool tempBool = RibbonUtils.GetRibbonItem("Performance_Rigging", "Lineset_Spacing", uiapp, out RibbonItem ri);
            TextBox tb = ri as TextBox;
            TaskDialog.Show("TESTING", "Lineset Spacing Value: " + tb.Value.ToString());



            return Result.Succeeded;
        }
    }
}
