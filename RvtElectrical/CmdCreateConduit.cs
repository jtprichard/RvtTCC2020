using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdCreateConduit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            UIApplication uiapp = commandData.Application;

            //GET INFORMATION FROM RIBBON
            string rpStr = "Performance Electrical";
            string riLevelStr = "Level";
            string riSizeStr = "Size";
            string riDestinationStr = "Destination";

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only available in Model");
                return Result.Failed;
            }

            string level = "";
            string size = "";
            string destination = "";

            if (RibbonUtils.GetRibbonItemCb(rpStr, riLevelStr, uiapp, out RibbonItem riLevel))
            {
                ComboBox cb = riLevel as ComboBox;
                //level = cb.Name.ToString();
                level = cb.Current.Name;
                //level = cb.ItemText.ToString();
            }
            else
            {
                TaskDialog.Show("Error", "System Error - Contact Provider");
                return Result.Failed;
            }

            if (RibbonUtils.GetRibbonItemCb(rpStr, riSizeStr, uiapp, out RibbonItem riSize))
            {
                ComboBox cb = riSize as ComboBox;
                size = cb.Current.Name;
            }
            else
            {
                TaskDialog.Show("Error", "System Error - Contact Provider");
                return Result.Failed;
            }

            if (RibbonUtils.GetRibbonItemTb(rpStr, riDestinationStr, uiapp, out RibbonItem riDest))
            {
                TextBox tb = riDest as TextBox;
                if(tb.Value == null)
                {
                    TaskDialog.Show("Error", "Please provide a default destination");
                    return Result.Failed;
                }
                destination = tb.Value.ToString();
            }
            else
            {
                TaskDialog.Show("Error", "System Error - Contact Provider");
                return Result.Failed;
            }

            string conduitType = level + size;



            //GET CONDUIT TYPE
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> wireTypes = collector.OfCategory(BuiltInCategory.OST_Wire).WhereElementIsElementType().ToElements();
            WireType wireType = null;

            foreach (Element wt in wireTypes)
            {
                if (wt.Name == conduitType)
                    wireType = wt as WireType;
            }
            if(wireType == null)
            {
                TaskDialog.Show("Conduit Type Error", "Conduit Type " + conduitType + " is not configured as a Wire Type in Electrical Settings");
                return Result.Failed;
            }


            //USER INPUT
            ObjectSnapTypes snapTypes = ObjectSnapTypes.Points;
            IList<XYZ> points = new List<XYZ>();
            try
            {
                for (int count = 0; count < 2; count++)
                {
                    points.Add(uidoc.Selection.PickPoint(snapTypes, "Select Points for Conduit"));
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Failed;
            }


            XYZ pt1 = points[0];
            XYZ pt3 = points[1];
            double pt2x;
            double pt2y;
            double pt2z;



            if (Math.Abs(pt3.X - pt1.X) > Math.Abs(pt3.Y - pt1.Y))
            {
                pt2x = pt1.X + ((pt3.X - pt1.X) / 2.0);
                pt2y = pt1.Y + ((pt3.Y - pt1.Y) * 2.0);
                pt2z = pt1.Z;
            }
            else
            {
                pt2x = pt1.X + ((pt3.X - pt1.X) * 2.0);
                pt2y = pt1.Y + ((pt3.Y - pt1.Y) / 2.0);
                pt2z = pt1.Z;
            }


            XYZ pt2 = new XYZ(pt2x, pt2y, pt2z);




            //ADD CENTER POINT AND REORDER
            IList<XYZ> newPoints = new List<XYZ>();
            newPoints.Add(pt1);
            newPoints.Add(pt2);
            newPoints.Add(pt3);


            //PLACE WIRE
            using (Transaction trans = new Transaction(doc, "Place Conduit"))
            {
                trans.Start();

                Wire wire = null;

                //FilteredElementCollector collector = new FilteredElementCollector(doc);
                //IList<Element> wireTypes = collector.OfCategory(BuiltInCategory.OST_Wire).WhereElementIsElementType().ToElements();
                //WireType wireType = wireTypes.First() as WireType;

                if (wireType != null)
                {
                    wire = Wire.Create(doc, wireType.Id, doc.ActiveView.Id, WiringType.Arc, newPoints, null, null);
                    Parameter wireDestParam = wire.LookupParameter("TCC_CONDUIT_ALT_DEST");
                    wireDestParam.Set(destination);
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
