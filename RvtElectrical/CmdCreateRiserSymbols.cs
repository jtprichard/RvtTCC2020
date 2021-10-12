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

            var riserAnnoFamilyName = "Riser_SVC_Anno_RevB";
            var boxIdGuid = Guid.Parse("db446056-e38b-48a7-88ce-8f2e3279a214");
            var plateCodeGuid = Guid.Parse("d6e3c843-f345-423a-ae7c-eb745db1540c");
            var connectorLabelGuid = Guid.Parse("4c6aca32-a79b-4bc6-be43-3bc323fde032");
            var connectorGroupCodeGuid = Guid.Parse("02e83556-85a3-4f36-b06f-989d0772adcf");
            var riserCircuitGuid = TCCElecSettings.RiserConnectorCircuitGuid;

            var riserElecSettings = new TCCElecRiserSettings();


            try
            {


                using (Transaction trans = new Transaction(doc, "Command Test"))
                {
                    try
                    {
                        
                        trans.Start();

                        //Get the annotation family
                        var fsym = new FilteredElementCollector(doc)
                            .OfClass(typeof(FamilySymbol))
                            .Cast<FamilySymbol>().ToList()
                            .Where(f => f.FamilyName == riserAnnoFamilyName)
                            .FirstOrDefault(y => y.Name == riserAnnoFamilyName);

                        //Get all available connectors
                        var allSvcDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, DeviceSystem.PerfSVC);
                        var distinctDeviceConnectorIds = new List<int>();

                        foreach (var deviceBox in allSvcDeviceBoxes)
                        {
                            var connectors = DeviceConnector.GetConnectorsByBox(deviceBox);
                            foreach (var connector in connectors)
                            {
                                if (!distinctDeviceConnectorIds.Contains(connector.DeviceId.Value))
                                {
                                    distinctDeviceConnectorIds.Add(connector.DeviceId.Value);
                                }
                            }
                        }

                        

                        XYZ insertPoint = new XYZ(0.0, 0.0, 0.0);
                        XYZ movePointDown = new XYZ(0.0, (.125/12), 0.0);
                        XYZ movePointDownBase = new XYZ(0.0, (.75 / 12), 0.0);
                        XYZ movePointOver = new XYZ(0.166, 0.0, 0.0);
                        XYZ movePointOverLarge = new XYZ(0.0, 0.25, 0.0);
                        int groupCount = 0;
                        double movePointOverValue = .125;


                        var levels = GetLevels(doc);
                        foreach (Level level in levels)
                        {

                            var SvcDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, DeviceSystem.PerfSVC, level).OrderBy(x => x.BoxId).ToList();
                            if(SvcDeviceBoxes.Count == 0)
                                continue;

                            foreach (var deviceConnectorId in distinctDeviceConnectorIds)
                            {

                                insertPoint = new XYZ(0.0, 0.0, 0.0) +
                                              new XYZ((groupCount * movePointOverValue), 0.0, 0.0);

                                var connectorText =
                                    GetConnectorCodeByConnectorId(doc, allSvcDeviceBoxes, deviceConnectorId);

                                var textIds = new FilteredElementCollector(doc)
                                    .OfCategory(BuiltInCategory.OST_TextNotes).ToElementIds();

                                var textType = TextNote.GetValidTypes(doc, textIds);

                                var selectedTextType = textType.FirstOrDefault();
                                var note = TextNote.Create(doc, view.Id, insertPoint-(movePointOver/3), connectorText, selectedTextType);
                                
                                insertPoint = insertPoint - movePointDownBase - movePointDown;

                                foreach (var deviceBox in SvcDeviceBoxes)
                                {

                                    var connectors = DeviceConnector.GetConnectorsByBox(deviceBox);
                                    var filteredConnectors = connectors
                                        .Where(x => x.DeviceId.Value == deviceConnectorId)
                                        .GroupBy(g => g.DeviceId.Value)
                                        .Select(s => s.First())
                                        .ToList();

                                    foreach (var connector in filteredConnectors)
                                    {
                                        var connectorCount = connectors
                                            .Where(x => x.DeviceId.Value == connector.DeviceId.Value)
                                            .ToList()
                                            .Where(y => y.ConnectorSubPosition == 0)
                                            .Count();

                                        List<DeviceConnector> boxServiceConnectors = connectors
                                            .Where(x => x.DeviceId.Value == connector.DeviceId.Value)
                                            .ToList()
                                            .Where(y => y.ConnectorSubPosition == 0)
                                            .ToList();

                                        var boxCircuits = GetBoxCircuits(boxServiceConnectors);

                                        var famInstance = doc.Create.NewFamilyInstance(insertPoint, fsym, view);
                                        var boxIdParam = famInstance.get_Parameter(boxIdGuid);
                                        var plateCodeParam = famInstance.get_Parameter(plateCodeGuid);
                                        var connectorLabelParam = famInstance.get_Parameter(connectorLabelGuid);
                                        var connectorQtyParam =
                                            famInstance.get_Parameter(TCCElecSettings.RiserSubQtyGuid);
                                        
                                        boxIdParam.Set(deviceBox.BoxId);
                                        plateCodeParam.Set(deviceBox.PlateCode);

                                        var connectorLabel = connector.ConnectorGroupCode + connectorCount;
                                        connectorLabelParam.Set(connectorLabel);

                                        int numCircuits = boxCircuits.Count;
                                        connectorQtyParam.Set(numCircuits);
                                        doc.Regenerate();


                                        for (int i = 0; i < boxCircuits.Count; i++)
                                        {
                                            if (i > 5)
                                                break;
                                            
                                            string circuitParamName = "RiserCircuit" + (i + 1);
                                            var circuitParamGuid = (Guid)riserElecSettings[circuitParamName];

                                            var circuitParam = famInstance.get_Parameter(circuitParamGuid);
                                            circuitParam.Set(boxCircuits[i]);

                                        }

                                        int moveDownValue = boxCircuits.Count;
                                        if (boxCircuits.Count == 0)
                                            moveDownValue = 1;

                                        insertPoint = insertPoint - movePointDownBase - (movePointDown * moveDownValue);
                                    }

                                }

                                groupCount += 1;
                            }

                            groupCount += 2;


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

        private IList<Level> GetLevels(Document doc)
        {
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            return levels;
        }


        private string GetConnectorCodeByConnectorId(Document doc, IList<DeviceBox> deviceBoxes, int Id)
        {
            IList<DeviceConnector> connectors = new List<DeviceConnector>();
            foreach (var deviceBox in deviceBoxes)
            {
                var boxConnectors = DeviceConnector.GetConnectorsByBox(deviceBox);
                foreach (var boxConnector in boxConnectors)
                {
                    connectors.Add(boxConnector);
                }

            }

            var foundConnector = connectors.Where(x => x.DeviceId.Value == Id).FirstOrDefault();
            return foundConnector.ConnectorGroupText;

        }

        private List<string> GetBoxCircuits(List<DeviceConnector> connectors)
        {
            var circuits = new List<string>();
            foreach (var connector in connectors)
            {
                var prefix = connector.ConnectorLabelPrefix;
                var circuit = connector.ConnectorCircuit;
                var other = connector.ConnectorLabelOther;
                string value = "";

                if (other != "" && other!=null)
                    value = other;
                else
                {
                    if(circuit != 0)
                        value = prefix + " " + circuit;
                }

                circuits.Add(value);
            }

            return circuits;

        }

        private FamilyInstance GetSubComponentInstance(FamilyInstance fi, int id, Document doc)
        {
            var subComponentIds = fi.GetSubComponentIds();

            var eid = subComponentIds.ElementAt(id);
            return doc.GetElement(eid) as FamilyInstance;
        }




    }
}
