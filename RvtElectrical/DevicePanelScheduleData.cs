using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure.StructuralSections;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RvtElectrical
{
    public class DevicePanelScheduleData
    {
        // Revit Document
        private readonly Document _doc;
        public PanelScheduleView PanelScheduleView { get; private set; }    //Panelscheduleview reference in Panel
        public DevicePanel Panel { get; private set; }                      //Device Panel reference
        public PanelScheduleTemplate PanelTemplate { get; private set; }    //Panel Schedulel Template Object
        public PanelScheduleType PanelType { get; private set; }            //Panel Schedule Type enum Object
        public bool IsPowerPanel { get; private set; }                      //Confirms if this is a Power panel
        public bool IsDataPanel { get; private set; }                       //Confirms if this is a Data panel
        public PanelScheduleData PanelData { get; private set; }            //Panel Schedule Data object
        public PanelConfiguration PanelConfig { get; private set; }         //Panel Confirguration enum object
        public bool HasAddressColumns { get; private set; }                 //Confirms if address or motorized column exists in template

        private int CircuitTableStartRow { get; set; }                      //Starting row of circuit table
        private int CircuitOddColumn { get; set; }                          //Circuit column number for odd circuits
        private int CircuitEvenColumn { get; set; }                         //Circuit column for even circuits
        private int DescriptionOddColumn { get; set; }                      //Description column for odd circuits
        private int DescriptionEvenColumn { get; set; }                     //Description column for even circuits
        private int AddressOddColumn { get; set; }                          //Address or motorized desc column for odd circuits
        private int AddressEvenColumn { get; set; }                         //Addres or motorized desc column for even circuits
        private TableSectionData CircuitTableData { get; set; }             //Circuit table's Table Section object


                
        public DevicePanelScheduleData(Document doc, DevicePanel element)
        //Constructor
        {
            _doc = doc;
            GetPanelScheduleView(element);
            PanelTemplate = _doc.GetElement(PanelScheduleView.GetTemplate()) as PanelScheduleTemplate;
            PanelType = PanelTemplate.GetPanelScheduleType();
            IsPowerPanel = PanelTemplate.IsBranchPanelSchedule;
            IsDataPanel = PanelTemplate.IsDataPanelSchedule;
            PanelData = PanelScheduleView.GetTableData();
            PanelConfig = PanelData.PanelConfiguration;
            CircuitTableData = PanelScheduleView.GetSectionData(SectionType.Body);

            GetCircuitStartRow();
            GetCircuitColumn();
            GetDescriptionColumn();
            HasAddressColumns = GetAddressColumn();
        }

       private void GetPanelScheduleView(DevicePanel panel)
        {
            // get all PanelScheduleView instances in the Revit document.
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            ElementClassFilter PanelScheduleViews = new ElementClassFilter(typeof(PanelScheduleView));
            collector.WherePasses(PanelScheduleViews);
            List<Element> psViews = collector.ToElements() as List<Element>;

            foreach (Element psView in psViews)
            {
                ElementId panelId = panel.PanelElement.Id;
                PanelScheduleView psv = psView as PanelScheduleView;
                if (psv.GetPanel() == panelId)
                {
                    PanelScheduleView = psv;
                    Panel = panel;
                }
            }
        }

        public static DevicePanel GetDevicePanelFromPanelSchedule(PanelScheduleView panelScheduleView, Document doc)
        //Returns a new DevicePanel from a PanelScheduleView
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalEquipment);

            //Apply Filter
            IList<Element> equipment = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Use Linq query to find panel that matches panelscheduleview
            var query = from element in collector
                        where element.Id == panelScheduleView.GetPanel()
                        select element;

            Element panel = query.First<Element>();

            return new DevicePanel(doc, panel);
        }
        private void GetCircuitStartRow()
        //Test to Find First Row of Circuit Table
        {
            for (int i = 1; i <= CircuitTableData.LastRowNumber; i++)
            {
                if (PanelScheduleView.IsRowInCircuitTable(i))
                {
                    CircuitTableStartRow = i+1;
                    break;
                }
            }
        }
        
        //FIX TO THROW REAL EXCEPTION
        private void GetCircuitColumn()
        //Find columns used for circuits
        {
            int count = 0;
            for(int i = CircuitTableData.FirstColumnNumber; i <= CircuitTableData.LastColumnNumber; i++)
            {
                if(CircuitTableData.GetCellParamId(i).IntegerValue == (int)BuiltInParameter.RBS_ELEC_CIRCUIT_NUMBER)
                {
                    count++;
                    if (count == 2)
                        CircuitEvenColumn = i;
                    else
                        CircuitOddColumn = i;
                }
            }
            if (PanelConfig == PanelConfiguration.OneColumn)
                CircuitEvenColumn = CircuitOddColumn; 

            if(count == 0)
            {
                TaskDialog.Show("Panel Schedule Error", "No Valid Circuit Column Was Found in the Panel Schedule.");
            }
        }
        private bool GetAddressColumn()
        //Find columns used for addresses using Circuit Notes Parameter
        {
            int count = 0;
            for (int i = CircuitTableData.FirstColumnNumber; i <= CircuitTableData.LastColumnNumber; i++)
            {
                if (CircuitTableData.GetCellParamId(i).IntegerValue == (int)BuiltInParameter.RBS_ELEC_CIRCUIT_NOTES_PARAM)
                {
                    count++;
                    if (count == 2)
                        AddressEvenColumn = i;
                    else
                        AddressOddColumn = i;
                }
            }
            if (PanelConfig == PanelConfiguration.OneColumn)
                AddressEvenColumn = AddressOddColumn;
            if (count == 0)
            {
                return false;
            }
            return true;
        }
        private bool GetDescriptionColumn()
        //Find columns used for circuit descriptions
        {
            int count = 0;
            for (int i = CircuitTableData.FirstColumnNumber; i <= CircuitTableData.LastColumnNumber; i++)
            {
                if (CircuitTableData.GetCellParamId(i).IntegerValue == (int)BuiltInParameter.RBS_ELEC_CIRCUIT_NAME)
                {
                    count++;
                    if (count == 2)
                        DescriptionEvenColumn = i;
                    else
                        DescriptionOddColumn = i;
                }
            }
            if (PanelConfig == PanelConfiguration.OneColumn)
                DescriptionEvenColumn = DescriptionOddColumn;
            if (count == 0)
            {
                return false;
            }
            return true;
        }
        private List<int> GetCircuitRowColumn(int slot)
        //Locate a Row and Column based on Circuit Number
        {
            List<int> rowcolumn = new List<int>();
            int row;
            int column;

            if (CircuitOddColumn == CircuitEvenColumn)
            {
                column = CircuitOddColumn;
                row = CircuitTableStartRow - 1 + slot;
            }
            else if (slot % 2 > 0)
            {
                column = CircuitOddColumn;
                row = CircuitTableStartRow + (slot / 2);
            }
            else
            {
                column = CircuitEvenColumn;
                row = CircuitTableStartRow - 1 + (slot / 2);
            }
            rowcolumn.Add(row);
            rowcolumn.Add(column);
            return rowcolumn;
        }

        private List<int> GetDescriptionRowColumn(int slot)
        //Locate a Row and Column based on Circuit Number
        {
            List<int> rowcolumn = new List<int>();
            int row;
            int column;

            if (DescriptionOddColumn == DescriptionEvenColumn)
            {
                column = DescriptionOddColumn;
                row = CircuitTableStartRow - 1 + slot;
            }
            else if (slot % 2 > 0)
            {
                column = DescriptionOddColumn;
                row = CircuitTableStartRow + (slot / 2);
            }
            else
            {
                column = DescriptionEvenColumn;
                row = CircuitTableStartRow - 1 + (slot / 2);
            }
            rowcolumn.Add(row);
            rowcolumn.Add(column);
            return rowcolumn;
        }
        private List<int> GetAddressRowColumn(int slot)
        //Locate a Row and Column based on Circuit Number
        {
            List<int> rowcolumn = new List<int>();
            int row;
            int column;

            if (AddressOddColumn == AddressEvenColumn)
            {
                column = AddressOddColumn;
                row = CircuitTableStartRow - 1 + slot;
            }
            else if (slot % 2 > 0)
            {
                column = AddressOddColumn;
                row = CircuitTableStartRow + (slot / 2);
            }
            else
            {
                column = AddressEvenColumn;
                row = CircuitTableStartRow - 1 + (slot / 2);
            }
            rowcolumn.Add(row);
            rowcolumn.Add(column);
            return rowcolumn;
        }
        public string GetSlotDescription(int slot)
        //Get a slot description from a slot location
        {
            int row = GetDescriptionRowColumn(slot)[0];
            int column = GetDescriptionRowColumn(slot)[1];

            string cellText = PanelScheduleView.GetCellText(SectionType.Body, row, column);

            return cellText;
        }
        public bool SetSlotDescription(int slot, string description)
        //Set a slot description from a slot location
        {
            bool success = false;
            int row = GetDescriptionRowColumn(slot)[0];
            int column = GetDescriptionRowColumn(slot)[1];

            if (PanelScheduleView.SetParamValue(SectionType.Body, row, column, description))
                success = true;

            return success;
        }
        public string GetSlotAddress(int slot)
        //Get the slot address description based on slot number
        {
            int row = GetAddressRowColumn(slot)[0];
            int column = GetAddressRowColumn(slot)[1];

            string cellText = PanelScheduleView.GetCellText(SectionType.Body, row, column);

            return cellText;
        }
        public bool SetSlotAddress(int slot, string description)
        //Set the slot address description based on the slot number
        {
            bool success = false;
            int row = GetAddressRowColumn(slot)[0];
            int column = GetAddressRowColumn(slot)[1];
            if (column == 0)
                return success;

            if (PanelScheduleView.SetParamValue(SectionType.Body, row, column, description))
                success = true;

            return success;
        }
        public bool SetSlotAddress(int slot, int addressNumber, DeviceConnector deviceConnector)
        //Set the slot address description based on the slot number
        {
            //Determine the description based on the panel type
            string description = "";
            switch (Panel.System)
            {
                case System.PerfLighting:
                    if(IsPowerPanel)
                    {
                        description = addressNumber.ToString();
                    }
                    else
                    {
                        if (deviceConnector.ConnectorLabelOther == null || deviceConnector.ConnectorLabelOther == "")
                        {
                            if (deviceConnector.ConnectorLabelPrefix == null || deviceConnector.ConnectorLabelPrefix == "")
                                description = addressNumber.ToString();
                            else
                                description = deviceConnector.ConnectorLabelPrefix + ": " + addressNumber.ToString();
                        }
                        else
                            description = deviceConnector.ConnectorLabelOther;
                    }
                    break;
                case System.PerfSVC:
                    if (IsPowerPanel)
                    {
                        description = "Y";
                    }
                    else
                    {
                        if (deviceConnector.ConnectorLabelOther == null || deviceConnector.ConnectorLabelOther == "")
                        {
                            if (deviceConnector.ConnectorLabelPrefix == null || deviceConnector.ConnectorLabelPrefix == "")
                                description = addressNumber.ToString();
                            else
                                description = deviceConnector.ConnectorLabelPrefix + ": " + addressNumber.ToString();
                        }
                        else
                            description = deviceConnector.ConnectorLabelOther;
                    }
                    break;

                default:
                    break;
            }
            
            //Set the description
            bool success = false;
            int row = GetAddressRowColumn(slot)[0];
            int column = GetAddressRowColumn(slot)[1];
            if (column == 0)
                return success;

            if (PanelScheduleView.SetParamValue(SectionType.Body, row, column, description))
                success = true;

            return success;
        }
        public bool SetSlotSpare(int slot)
        //Set the slot as a spare
        {
            bool success = false;
            int row = GetCircuitRowColumn(slot)[0];
            int column = GetCircuitRowColumn(slot)[1];
            try
            {
                PanelScheduleView.AddSpare(row, column);
                success = true;
            }
            catch
            {
                //TaskDialog.Show("Set Spare Error", "Spare Not Created");
            }
            return success;
        }
        public bool RemoveSlotSpare(int slot)
        //Remove a spare from a slot
        {
            bool success = false;
            int row = GetCircuitRowColumn(slot)[0];
            int column = GetCircuitRowColumn(slot)[1];
            try
            {
                PanelScheduleView.RemoveSpare(row, column);
                success = true;
            }
            catch
            {
                //TaskDialog.Show("Remove Spare Error", "Spare Not Removed");
            }
            return success;
        }
        public ElectricalSystem GetCircuitBySlot(int slot)
        //Get the circuit by the slot number
        {
            int row = GetCircuitRowColumn(slot)[0];
            int column = GetCircuitRowColumn(slot)[1];

            return this.PanelScheduleView.GetCircuitByCell(row, column);
        }

        public bool IsSlotSpare(int slot)
        //Checks to see if a slot is spare
        {
            int row = GetCircuitRowColumn(slot)[0];
            int column = GetCircuitRowColumn(slot)[1];

            if (PanelScheduleView.IsSpare(row, column))
                return true;
            else
                return false;
        }
        public bool IsSlotLocked(int slot)
        //Checks to see if a slot is locked
        {
            int row = GetCircuitRowColumn(slot)[0];
            int column = GetCircuitRowColumn(slot)[1];

            if (PanelScheduleView.IsSlotLocked(row, column))
                return true;
            else
                return false;
        }

        public bool IsSlotSubPole(int slot)
        //Checks to see if a slot is circuited as part of a multi-pole breaker
        {
            bool success = false;
            int row = GetCircuitRowColumn(slot)[0];
            int column = GetCircuitRowColumn(slot)[1];

            if(PanelScheduleView.GetCircuitByCell(row,column) != null)
            {
                ElectricalSystem circuit = PanelScheduleView.GetCircuitByCell(row, column);
                if (circuit.StartSlot != slot)
                    success = true;
            }
            return success;
        }
        
        public bool MoveSlot(int startSlot, int targetSlot)
        //Moves a circuit from a start slot to a target slot by slot number
        {
            bool success = false;

            if (!IsSlotLocked(targetSlot))
            {
                try
                {
                    PanelScheduleView.MoveSlotTo(GetCircuitRowColumn(startSlot)[0],
                    GetCircuitRowColumn(startSlot)[1],
                    GetCircuitRowColumn(targetSlot)[0],
                    GetCircuitRowColumn(targetSlot)[1]);
                    success = true;
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        }
        
        //Next Method Starts Here

    }

}
