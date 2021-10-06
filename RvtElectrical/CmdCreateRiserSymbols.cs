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
    public class CmdCreateRiserSymbols : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            if (view.ViewType != ViewType.DraftingView)
            {
                TaskDialog.Show("Error", "Must be in drafting view.  Exiting");
                return Result.Failed;
            }

            var riserAnnoFamilyName = "RiserBoxAnno";
            var boxIdGuid = Guid.Parse("db446056-e38b-48a7-88ce-8f2e3279a214");
            var plateCodeGuid = Guid.Parse("d6e3c843-f345-423a-ae7c-eb745db1540c");


            ////Define a reference Object to accept the pick result
            //Reference pickedObj = null;

            ////Pick an object
            //Selection sel = uiapp.ActiveUIDocument.Selection;

            //pickedObj = sel.PickObject(ObjectType.Element, "Select an element");
            //Element elem = doc.GetElement(pickedObj);



            try
            {


                //DeviceBox db = new DeviceBox(doc, elem);



                


                //string dbBoxId = db.DeviceId.Value.ToString();
                //string dbNumber = db.BoxId.ToString();
                //string dbVenue = db.Venue.ToString();
                //string dbNumConnectors = db.Connectors.Count().ToString();

                //TaskDialog.Show("TEST MESSAGE", "DeviceBox ID: " + dbBoxId + Environment.NewLine +
                //    "Box Number: " + dbNumber + Environment.NewLine +
                //    "Venue: " + dbVenue + Environment.NewLine +
                //    "Connector Qty: " + dbNumConnectors);

                //IList<DeviceBox> deviceBoxes = DeviceBox.GetDeviceBoxes(doc);

                //IList<DeviceBox> lightingDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfLighting);

                //IList<DeviceBox> svcDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfSVC);

                //IList<DeviceBox> machDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfMachinery);

                //IList<DeviceBox> cablePasses = DeviceBox.GetDeviceBoxes(doc, System.CablePasses);

                //Locate a list of deviceboxes
                //IList<DeviceBox> lightingDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfLighting);

                //Locate a schedule to duplicate



                using (Transaction trans = new Transaction(doc, "Command Test"))
                {
                    try
                    {
                        
                        trans.Start();

                        var SvcDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, DeviceSystem.PerfSVC);

                        var fsym = new FilteredElementCollector(doc)
                            .OfClass(typeof(FamilySymbol))
                            .Cast<FamilySymbol>().ToList()
                            .Where(f => f.FamilyName == riserAnnoFamilyName)
                            .FirstOrDefault(y => y.Name == riserAnnoFamilyName);

                        XYZ insertPoint = new XYZ(0.0, 0.0, 0.0);
                        XYZ movePoint = new XYZ(0.0, 0.1, 0.0);

                        foreach (var deviceBox in SvcDeviceBoxes)
                        {
                            var connectors = DeviceConnector.GetConnectorsByBox(deviceBox);
                            foreach (var connector in connectors)
                            {
                                var famInstance = doc.Create.NewFamilyInstance(insertPoint, fsym, view);
                                var boxIdParam = famInstance.get_Parameter(boxIdGuid);
                                var plateCodeParam = famInstance.get_Parameter(plateCodeGuid);

                                boxIdParam.Set(deviceBox.BoxId);
                                plateCodeParam.Set(connector.ConnectorGroupCode);

                                insertPoint = insertPoint + movePoint;
                            }
                            


                        }

                       


                        trans.Commit();
                    }

                    catch (Exception e)
                    {
                        TaskDialog.Show("ERROR", e.Message);
                        trans.Dispose();
                    }

                }



            }
            catch
            {
                TaskDialog.Show("ERROR", "Testing Error has occurred");
            }

            return Result.Succeeded;
        }
    }
}
