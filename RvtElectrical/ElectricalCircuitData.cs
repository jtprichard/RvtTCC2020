using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Electrical;

namespace RvtElectrical
{
    // Class which stores electrical circuit information
    public class ElectricalCircuitData
    {
        //FIELDS
        private readonly Document _doc;                                             //Revit Document
        private ElectricalSystemSet _electricalSystemSet;                           //All Electrical systems in current element

        //PROPERTIES
        public Element CircuitElement { get; private set; }                         //Circuit Element
        public ElectricalSystem DeviceElectricalSystem { get; private set; }        //Electrical System Element
        public bool CanCreateCircuit                                                //Whether new circuit can be created
        {
            get
            {
                return CollectConnectorInfo(CircuitElement);
            }
            private set { }
        }
        public bool HasCircuit { get; private set; }                                //Whether element has a circuite
        public ElectricalSystemType SystemType { get; private set; }                //Circuit System Type
        public bool IsPowerCircuit { get; private set; }                            //Whether circuit classified as Power Circuit
        public bool IsDataCircuit { get; private set; }                             //Whether circuit classified as Data Circuit
        public bool HasPanel { get; private set; }                                  //Whether element has a panel
        public string PanelName { get; private set; }                               //Element's Panel Name
        public int ElectricalSystemCount                                            //Size of electrical systems
        {
            get
            {
                return _electricalSystemSet.Size;
            }
        }

        // Constructor
        public ElectricalCircuitData(Element element, Document doc)
        {
            _doc = doc; 
            CircuitElement = element;
            _electricalSystemSet = new ElectricalSystemSet();

            CollectCircuitInfo(element);
        }

        // Verify if element has unused connectors
        private bool CollectConnectorInfo(Element element)
        {
            bool canCreateCircuit = true;

            if (!(element is FamilyInstance fi))
            {
                canCreateCircuit = false;
                return canCreateCircuit;
            }
            //Verify if the family instance has usable connectors
            if (!VerifyUnusedConnectors(fi))
            {
                canCreateCircuit = false;
                return canCreateCircuit;
            }
            return canCreateCircuit;
        }

        // Verify if the family instance has usable connectors
        static private bool VerifyUnusedConnectors(FamilyInstance fi)
        {
            bool hasUnusedElectricalConnector = false;
            try
            {
                MEPModel mepModel = fi.MEPModel;
                if (null == mepModel)
                {
                    return hasUnusedElectricalConnector;
                }

                ConnectorSet unusedConnectors = new ConnectorSet();
                ConnectorManager cm = mepModel.ConnectorManager;
                
                if(cm != null)
                {
                    unusedConnectors = cm.UnusedConnectors;
                }

                if (unusedConnectors == null || unusedConnectors.IsEmpty)
                {
                    return hasUnusedElectricalConnector;
                }

                foreach (Connector connector in unusedConnectors)
                {
                    if (connector.Domain == Domain.DomainElectrical)
                    {
                        hasUnusedElectricalConnector = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return hasUnusedElectricalConnector;
            }

            return hasUnusedElectricalConnector;
        }

        // Get common circuit for element
        private void CollectCircuitInfo(Element element)
        {
            bool bInitilzedElectricalSystemSet = false;

            MEPModel mepModel;
            ElectricalSystemSet ess;

            //Get Power vs. Data Circuit Info

            // If the element is a family instance and its MEP model is not null,
            // retrieve its circuits
            if (element is FamilyInstance fi && (mepModel = fi.MEPModel) != null)
            {
                // Get all electrical systems
                ess = mepModel.ElectricalSystems;

                //CONFIRM IF CIRCUIT IS CONNECTED
                //ALSO CONFIRM POWER VS DATA 
                ConnectorManager cm = mepModel.ConnectorManager;

                if (ess != null)
                {
                    foreach (ElectricalSystem es in ess)
                    {
                        SystemType = es.SystemType;
                        if (SystemType == ElectricalSystemType.PowerCircuit)
                        {
                            IsPowerCircuit = true;
                        }
                        else if (SystemType == ElectricalSystemType.Data)
                        {
                            IsDataCircuit = true;
                        }
                        
                    }
                }

                else
                {
                    if (cm != null)
                    {
                        ConnectorSet cs = cm.Connectors;
                        foreach (Connector c in cs)
                        {
                            SystemType = c.ElectricalSystemType;
                            if (c.ElectricalSystemType == ElectricalSystemType.PowerCircuit)
                            {
                                IsPowerCircuit = true;
                            }
                            else if (c.ElectricalSystemType == ElectricalSystemType.Data)
                            {
                                IsDataCircuit = true;
                            }
                        }
                    }

                    HasCircuit = false;
                    HasPanel = false;
                    return;
                }


                if (ess == null || ess.IsEmpty)
                {
                    HasCircuit = false;
                    HasPanel = false;
                    return;
                }

                // If _electricalSystemSet is not set before, set it
                // otherwise compare the circuits with circuits of other selected elements
                // to get the common ones
                if (!bInitilzedElectricalSystemSet)
                {
                    _electricalSystemSet = ess;
                    bInitilzedElectricalSystemSet = true;
                }
                else
                {
                    foreach (ElectricalSystem es in _electricalSystemSet)
                    {
                        if (!ess.Contains(es))
                        {
                            _electricalSystemSet.Erase(es);
                        }
                    }

                    if (_electricalSystemSet.IsEmpty)
                    {
                        HasCircuit = false;
                        HasPanel = false;
                        return;
                    }
                }
            }
            else if (element is ElectricalSystem tempElectricalSystem)
            {
                // If the element is an electrical system, verify if it is a power circuit
                // If not, compare with circuits of other selected elements
                // to get the common ones


                if (tempElectricalSystem.SystemType == ElectricalSystemType.PowerCircuit)
                {
                    IsPowerCircuit = true;
                }
                else if (tempElectricalSystem.SystemType == ElectricalSystemType.Data)
                {
                    IsDataCircuit = true;
                }

                // If m_electricalSystemSet is not set before, set it
                // otherwise compare with circuits of other selected elements
                // to get the common ones 
                if (!bInitilzedElectricalSystemSet)
                {
                    _electricalSystemSet.Insert(tempElectricalSystem);
                    bInitilzedElectricalSystemSet = true;
                }

                if (!_electricalSystemSet.Contains(tempElectricalSystem))
                {
                    HasCircuit = false;
                    HasPanel = false;
                    return;
                }

                _electricalSystemSet.Clear();
                _electricalSystemSet.Insert(tempElectricalSystem);
            }
            else
            {
                HasCircuit = false;
                HasPanel = false;
                return;
            }

            // Verify if there is any common circuit
            if (!_electricalSystemSet.IsEmpty)
            {
                HasCircuit = true;
                if (_electricalSystemSet.Size == 1)
                {
                    foreach (ElectricalSystem es in _electricalSystemSet)
                    {
                        DeviceElectricalSystem = es;
                        break;
                    }
                }

                foreach (ElectricalSystem es in _electricalSystemSet)
                {
                    if (!String.IsNullOrEmpty(es.PanelName))
                    {
                        HasPanel = true;
                        PanelName = es.PanelName;
                        break;
                    }
                }
            }
        }

        // Create a power circuit with selected elements
        public void CreateCircuit(ElectricalSystemType systemType)
        {
            List<ElementId> selectionElementId = new List<ElementId>();

            selectionElementId.Add(CircuitElement.Id);

            try
            {
                // Creation
                ElectricalSystem es = ElectricalSystem.Create(_doc, selectionElementId, systemType);
                DeviceElectricalSystem = es;
                CollectCircuitInfo(CircuitElement);
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToCreateCircuit");
            }
        }

 
        // Remove connection from circuits
        public void RemoveCircuit()
        {
            //ERROR CHECKING TO DO
            //Confirm circuit has an electrical system
            
            List<ElementId> selectionElementId = new List<ElementId>();
            selectionElementId.Add(CircuitElement.Id);

            try
            {
                // Remove system from element
                if (DeviceElectricalSystem.Elements.Size > 1)
                {
                    DeviceElectricalSystem.Remove(selectionElementId);
                    PanelName = null;
                    HasCircuit = false;
                    HasPanel = false;
                }

                else
                {
                    _doc.Delete(DeviceElectricalSystem.Id);
                    PanelName = null;
                    HasPanel = false;
                    HasCircuit = false;
                }

            }
            catch (Exception)
            {
                ShowErrorMessage("Error - Failed To Remove Circuit");
            }
        }

        // Add an elementset to circuit
        public void AddElementToCircuit(ElementSet circuits)
        {

            //ERROR CHECKING TO DO
            //Confirm elements are not empty
            //Confirm circuit element has an electrical system
            //Confirm systems are compatible

            try
            {
                //Disconnect Existing Power Systems from Circuits
                foreach(Element circuit in circuits)
                {
                    FamilyInstance circuitInstance = circuit as FamilyInstance;
                    ElectricalSystemSet ess = circuitInstance.MEPModel.ElectricalSystems;
                    if(ess != null)
                    {
                        foreach (ElectricalSystem es in ess)
                        {
                            ElementId systemId = es.Id;
                            _doc.Delete(systemId);
                        }
                    }
                }

                //Append circuits
                DeviceElectricalSystem.AddToCircuit(circuits);

            }

            catch
            {
                TaskDialog.Show("Circuit Error", "There is a circuiting error");
            }
        }

        // Add an element to circuit
        public void AddElementToCircuit(Element circuit)
        {

            //ERROR CHECKING TO DO
            //Confirm elements are not empty
            //Confirm circuit element has an electrical system
            //Confirm systems are compatible

            try
            {

                FamilyInstance circuitInstance = circuit as FamilyInstance;
                ElectricalSystemSet ess = circuitInstance.MEPModel.ElectricalSystems;
                foreach (ElectricalSystem es in ess)
                {
                    ElementId systemId = es.Id;
                    _doc.Delete(systemId);
                }

                ElementSet circuits = new ElementSet();
                circuits.Insert(circuit);

                //Append circuits
                DeviceElectricalSystem.AddToCircuit(circuits);
            }

            catch
            {
                TaskDialog.Show("Circuit Error", "There is a circuiting error");
            }

                                                     
        }
        /// <summary>
        /// Remove an element from selected circuit
        /// </summary>
        public void RemoveElementFromCircuit(ElementSet circuits)
        {

            try
            {
                //Remove appended circuits
                DeviceElectricalSystem.RemoveFromCircuit(circuits);
            }

            catch
            {
                TaskDialog.Show("Circuit Error", "There is a circuiting error");
            }

        }

        static private bool IsElementBelongsToCircuit(MEPModel mepModel,
            ElectricalSystem selectedElectricalSystem)
        {
            ElectricalSystemSet ess = mepModel.ElectricalSystems;
            if (null == ess || !ess.Contains(selectedElectricalSystem))
            {
                return false;
            }

            return true;
        }

        // Select a panel for selected circuit
        public void SelectPanel(Element panel)
        {
            //ERROR CHECKING TO DO
            //Verify that panel is not null
            //Verify that panel has a distribution system
            //Verify that panel distribution system matches element

            try
            {
                FamilyInstance fi;
                fi = panel as FamilyInstance;
                DeviceElectricalSystem.SelectPanel(fi);
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToSelectPanel");
            }
        }

        // Disconnect panel for selected circuit
        public void DisconnectPanel()
        {
            //ERROR CHECKING
            //Confirm that electrical system has a panel

            try
            {
                DeviceElectricalSystem.DisconnectPanel();
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToDisconnectPanel");
            }
        }

        // Show message box with specified string
        static private void ShowErrorMessage(String message)
        {
            TaskDialog.Show("OperationFailed", message, TaskDialogCommonButtons.Ok);
        }

    }
}
