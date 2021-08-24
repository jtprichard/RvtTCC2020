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
    public class CmdTagDeviceBoxPower : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            //Define a reference Object to accept the pick result
            Reference pickedObj = null;
            
            //Pick an object
            Selection sel = uiapp.ActiveUIDocument.Selection;
            SelectionFilterDeviceBox selFilter = new SelectionFilterDeviceBox();

            pickedObj = sel.PickObject(ObjectType.Element, selFilter, "Select a Device Box");
            Element elem = doc.GetElement(pickedObj);

            try
            {
                DeviceBox db = new DeviceBox(doc, elem);

                //GET THE POWER CONNECTORS
                //INSTEAD OF JUST GETTING THE FIRST, CREATE A TAG FOR EACH ONE SLIGHTLY OFFSET
                //USE THIS TECHNIQUE FOR TAGGING POWER VS. NON POWER ITEMS
                //THIS CAN BE THE TAGGING TECHNIQUE FOR DEVICE BOXES IN GENERAL
                //CHANGE SEARCH PARAMETERS TO LOOK FOR CONNECTORS BY VOLTAGE
                //WILL NEED TO ADD GETCONNECTORSBYBOX OVERLOAD TO SEARCH FOR VOLTAGE

                var powerConnectors = DeviceConnector.GetConnectorsByBox(db,Voltage.High120V1Ph);
                Element powerConnectorElement = powerConnectors.First().Connector;

                var powerConnectorElements = powerConnectors
                    .GroupBy(n => n.Connector.Name)
                    .Select(g => g.First())
                    .ToList();

                using (Transaction trans = new Transaction(doc, "Tag DeviceBoxes"))
                {
                    //A TAG IS ONLY VISIBLE IF IT'S TAGGED ELEMENT IS VISIBLE IN A VIEW
                    //FOR CONNECTORS, IF THEY ARE OUTSIDE OF A VIEW RANGE, THE TAG WILL NOT SHOW UP REGARDLESS OF THE FACT THAT
                    //WE ARE USING THE DEVICE BOX AS THE XYZ GEOMETRY
                    //CHOICES:  ENSURE ALL CONNECTORS ARE ON THE FACE OF THE DEVICE BOX, OR ADJUST THE VIEW DEPTH IN THE MODEL

                    trans.Start();
                    foreach (var connectorElement in powerConnectorElements)
                    {

                        try
                        {

                            var deviceTag = new DeviceTag()
                            {
                                TagFamily = "Power Tag Multicategory",
                                TagFamilyType = "Power",
                                HasLeader = true,
                                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                                TagOrient = TagOrientation.Horizontal,
                                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

                            };

                            if (connectorElement.IsIG)
                                deviceTag.TagFamilyType = "Power_IG";
                            
                            LocationPoint elemLocation =  elem.Location as LocationPoint;
                            deviceTag.Location = elemLocation.Point;
                            deviceTag.TagReference = new Reference(connectorElement.Connector);

                            deviceTag.TagDeviceBox(doc, view);


                        }

                        catch (Exception e)
                        {
                            TaskDialog.Show("ERROR", e.Message);
                            trans.Dispose();
                        }
                    }

                    trans.Commit();
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
