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
    public class Conduits
    {
        public static bool CreateConduit(UIDocument uidoc, UIApplication uiapp, string level)
        {
            Document doc = uidoc.Document;

            //GET INFORMATION FROM RIBBON
            string rtStr = "TCC Technical";
            string rpStr = "Conduit";
            string riQtyStr = "Quantity";
            string riSizeStr = "Size";
            string riDestinationStr = "Destination";

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only available in Model");
                return false;
            }

            string quantity;
            string size;
            string destination;

            try 
            {
                if (RibbonUtils.GetRibbonItem(rtStr, rpStr, riQtyStr, uiapp, out RibbonItem riQty))
                {
                    if(level == "X")
                    {
                        quantity = "";
                    }
                    else
                    {
                        ComboBox cb = riQty as ComboBox;
                        quantity = cb.Current.Name;
                    }
                }
                else
                {
                    throw new EBSException();
                }

                if (RibbonUtils.GetRibbonItem(rtStr, rpStr, riSizeStr, uiapp, out RibbonItem riSize))
                {
                    if(level == "X")
                    {
                        size = "AR";
                    }
                    else
                    {
                        ComboBox cb = riSize as ComboBox;
                        size = cb.Current.Name;
                    }
                }
                else
                {
                    throw new EBSException();
                }

                if (RibbonUtils.GetRibbonItem(rtStr, rpStr, riDestinationStr, uiapp, out RibbonItem riDest))
                {
                    TextBox tb = riDest as TextBox;
                    if (tb.Value == null)
                    {

                        TaskDialog.Show("Error", "Please provide a default destination");
                        return false;
                    }
                    destination = tb.Value.ToString();
                }
                else
                {
                    throw new EBSException();
                }
            }

            catch(Exception ex)
            {
                UIUtils.EBSGenException(ex);
                return false;
            }

            string conduitType = level + size + quantity;


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
                return false;
            }

            try
            {
                do
                {

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

                    //catch Escape from routine
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return true;
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
                        try
                        {
                            trans.Start();

                            Wire wire = null;

                            if (wireType != null)
                            {
                                wire = Wire.Create(doc, wireType.Id, doc.ActiveView.Id, WiringType.Arc, newPoints, null, null);
                                Parameter wireDestParam = wire.LookupParameter("TCC_CONDUIT_ALT_DEST");
                                wireDestParam.Set(destination);
                            }

                            trans.Commit();
                        }

                        catch (Exception ex)
                        {
                            trans.Dispose();
                            UIUtils.EBSGenException(ex);
                            return false;
                        }

                    }
                } while (1 == 1);

            }

            catch (Exception ex)
            {
                UIUtils.EBSGenException(ex);
                return false;
            }

            return true;
        }
    }
}
