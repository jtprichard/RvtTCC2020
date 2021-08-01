using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    /// <summary>
    /// Data class which stores the information of electrical circuit operation
    /// </summary>
    public class ElectricalCircuitDataBackup
    {
        #region Fields

        // Revit Document
        private Document _doc;
        //Circuit Element
        private Element _element;

        // Whether new circuit can be created with selected elements
        private bool _canCreateCircuit;

        // Whether there is a circuit in selected elements
        private bool _hasCircuit;

        // Whether the circuit in selected elements has panel
        private bool _hasPanel;

        // Panel Name
        private string _panelName;

        // All electrical systems contain selected element
        private ElectricalSystemSet _electricalSystemSet;

        // Electrical system
        private ElectricalSystem _electricalSystem;

        #endregion

        #region Properties
        // Get the information whether new circuit can be created
        public Element CircuitElement
        {
            get
            {
                return _element;
            }
        }
        public bool CanCreateCircuit
        {
            get
            {
                return _canCreateCircuit;
            }
        }

        // Get the value of whether there are circuits in selected elements
        public bool HasCircuit
        {
            get
            {
                return _hasCircuit;
            }
        }

        // Get the information whether the circuit in selected elements has panel
        public bool HasPanel
        {
            get
            {
                return _hasPanel;
            }
        }
        //Get the panel name
        public string PanelName
        {
            get
            {
                return _panelName;
            }
        }

        // Number of electrical systems contain elements
        public int ElectricalSystemCount
        {
            get
            {
                return _electricalSystemSet.Size;
            }
        }

        //Electrical System
        public ElectricalSystem DeviceElectricalSystem
        { 
            get
            {
                return _electricalSystem;
            }
        }
        #endregion

        #region Methods
        // Constructor
        public ElectricalCircuitData(Element element, Document doc)
        {
            _element = element;
            _electricalSystemSet = new ElectricalSystemSet();
            //_electricalSystemItems = new List<ElectricalSystemItem>();
            _doc = doc;

            CollectConnectorInfo(element);
            CollectCircuitInfo(element);
        }

        // Verify if element has unused connectors
        private void CollectConnectorInfo(Element element)
        {
            _canCreateCircuit = true;

            if (!(element is FamilyInstance fi))
            {
                _canCreateCircuit = false;
                return;
            }
            //Verify if the family instance has usable connectors
            if (!VerifyUnusedConnectors(fi))
            {
                _canCreateCircuit = false;
                return;
            }
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

                if (null == unusedConnectors || unusedConnectors.IsEmpty)
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
            //bool isElectricalSystem = false;

            bool bInitilzedElectricalSystemSet = false;

            MEPModel mepModel;
            ElectricalSystemSet ess;

            if (element is FamilyInstance fi && (mepModel = fi.MEPModel) != null)
            {
                //
                // If the element is a family instance and its MEP model is not null,
                // retrieve its circuits
                //

                // Get all electrical systems
                ess = mepModel.ElectricalSystems;
                if (null == ess)
                {
                    _hasCircuit = false;
                    _hasPanel = false;
                    return;
                }

                // Remove systems which are not power circuits
                foreach (ElectricalSystem es in ess)
                {
                    if (es.SystemType != ElectricalSystemType.PowerCircuit)
                    {
                        ess.Erase(es);
                    }
                }

                if (ess.IsEmpty)
                {
                    _hasCircuit = false;
                    _hasPanel = false;
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
                        _hasCircuit = false;
                        _hasPanel = false;
                        return;
                    }
                }
            }
            else if (element is ElectricalSystem tempElectricalSystem)
            {
                //
                // If the element is an electrical system, verify if it is a power circuit
                // If not, compare with circuits of other selected elements
                // to get the common ones
                //
                //verify if it is a power circuit
                if (tempElectricalSystem.SystemType != ElectricalSystemType.PowerCircuit)
                {
                    _hasCircuit = false;
                    _hasPanel = false;
                    return;
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
                    _hasCircuit = false;
                    _hasPanel = false;
                    return;
                }

                _electricalSystemSet.Clear();
                _electricalSystemSet.Insert(tempElectricalSystem);
            }
            else
            {
                _hasCircuit = false;
                _hasPanel = false;
                return;
            }

            // Verify if there is any common power circuit
            if (!_electricalSystemSet.IsEmpty)
            {
                _hasCircuit = true;
                if (_electricalSystemSet.Size == 1)
                {
                    foreach (ElectricalSystem es in _electricalSystemSet)
                    {
                        _electricalSystem = es;
                        break;
                    }
                }

                foreach (ElectricalSystem es in _electricalSystemSet)
                {
                    if (!String.IsNullOrEmpty(es.PanelName))
                    {
                        _hasPanel = true;
                        _panelName = es.PanelName;
                        break;
                    }
                }
            }
        }

        // Create a power circuit with selected elements
        public void CreatePowerCircuit()
        {
            List<ElementId> selectionElementId = new List<ElementId>();

            selectionElementId.Add(CircuitElement.Id);
            
            //Transaction transaction = new Transaction(_doc, "Create Power Circuits");
            //transaction.Start();
            try
            {
                // Creation
                ElectricalSystem es = ElectricalSystem.Create(_doc, selectionElementId, ElectricalSystemType.PowerCircuit);
                _electricalSystem = es;
                CollectCircuitInfo(CircuitElement);
                //transaction.Commit();
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToCreateCircuit");
                //transaction.Dispose();
            }
        }

        // Remove power from circuits
        public void RemovePowerCircuit()
        {
            //ERROR CHECKING TO DO
            //Confirm circuit has an electrical system
            
            List<ElementId> selectionElementId = new List<ElementId>();
            selectionElementId.Add(CircuitElement.Id);

            //Transaction transaction = new Transaction(_doc, "Remove Power Circuits");
            //transaction.Start();
            try
            {
                // Remove power system from element
                if (_electricalSystem.Elements.Size > 1)
                {
                    _electricalSystem.Remove(selectionElementId);
                    _panelName = null;
                    _hasCircuit = false;
                    _hasPanel = false;
                }

                else
                {
                    _doc.Delete(_electricalSystem.Id);
                    _panelName = null;
                    _hasPanel = false;
                    _hasCircuit = false;
                }

                //transaction.Commit();
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToRemoveCircuit");
                //transaction.Dispose();
            }
        }

        // Add an elementset to circuit
        public void AddElementToCircuit(ElementSet circuits)
        {

            //ERROR CHECKING TO DO
            //Confirm elements are not empty
            //Confirm circuit element has an electrical system
            //Confirm systems are compatible

            //Transaction transaction = new Transaction(_doc, "Append Circuits");
            //transaction.Start();
            try
            {
                //Disconnect Existing Power Systems from Circuits
                foreach(Element circuit in circuits)
                {
                    FamilyInstance circuitInstance = circuit as FamilyInstance;
                    ElectricalSystemSet ess = circuitInstance.MEPModel.ElectricalSystems;
                    foreach(ElectricalSystem es in ess)
                    {
                        ElementId systemId = es.Id;
                        _doc.Delete(systemId);
                    }
                }

                //Append circuits
                _electricalSystem.AddToCircuit(circuits);

                //transaction.Commit();
            }

            catch
            {
                TaskDialog.Show("Circuit Error", "There is a circuiting error");
                //transaction.Dispose();
            }

            //ConnectorSet cs = fiCircuit.MEPModel.ConnectorManager.Connectors;
            //foreach (Connector c in cs)
            //{
            //    es = ElectricalSystem.Create(c, ElectricalSystemType.PowerCircuit);
            //    es.SelectPanel(fip);
            //}


            //if (null == fi || null == (mepModel = fi.MEPModel))
            //{
            //    ShowErrorMessage("SelectElectricalComponent");
            //    return;
            //}

            //// Verify if the element has usable connector 
            //if (!VerifyUnusedConnectors(fi))
            //{
            //    ShowErrorMessage("NoUsableConnector");
            //    return;
            //}

            //if (IsElementBelongsToCircuit(mepModel, _es))
            //{
            //    ShowErrorMessage("ElementInCircuit");
            //    //TaskDialog.Show("Error", "MEP Model: " + mepModel.ToString() + " System: " + _selectedElectricalSystem.ToString());
            //    //return;
            //}

            //Get the electrical system for the panel

            //_selectedElectricalSystem.SelectPanel(fiPanel);

            //ElectricalSystem testElectricalSystem;
            //testElectricalSystem.SelectPanel(fiPanel);

            //_es.SelectPanel(fiPanel);

            //try
            //{
            //    //ElementSet es = new ElementSet();
            //    //foreach (ElementId elementId in m_selection.GetElementIds())
            //    //{
            //    //    es.Insert(_doc.GetElement(elementId));
            //    //}
            //    //ElementSet es = new ElementSet();
            //    es.Insert(CircuitElement);
            //    //if (!_selectedElectricalSystem.AddToCircuit(es))
            //    if (!_es.AddToCircuit(es))
            //    {
            //        ShowErrorMessage("FailedToAddElement");
            //        return;
            //    }
            //}
            //catch (Exception)
            //{
            //    ShowErrorMessage("FailedToAddElement");
            //}
        }

        // Add an element to circuit
        public void AddElementToCircuit(Element circuit)
        {

            //ERROR CHECKING TO DO
            //Confirm elements are not empty
            //Confirm circuit element has an electrical system
            //Confirm systems are compatible

            //Transaction transaction = new Transaction(_doc, "Append Circuits");
            //transaction.Start();
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
                _electricalSystem.AddToCircuit(circuits);

                //transaction.Commit();
            }

            catch
            {
                TaskDialog.Show("Circuit Error", "There is a circuiting error");
                //transaction.Dispose();
            }

                                                     
        }
        /// <summary>
        /// Remove an element from selected circuit
        /// </summary>
        public void RemoveElementFromCircuit(ElementSet circuits)
        {

            //Transaction transaction = new Transaction(_doc, "Remove Appended Circuits");
            //transaction.Start();
            try
            {
                ////Disconnect Existing Power Systems from Circuits
                //foreach (Element circuit in circuits)
                //{
                //    FamilyInstance circuitInstance = circuit as FamilyInstance;
                //    ElectricalSystemSet ess = circuitInstance.MEPModel.ElectricalSystems;
                //    foreach (ElectricalSystem es in ess)
                //    {
                //        ElementId systemId = es.Id;
                //        _doc.Delete(systemId);
                //    }
                //}

                //Remove appended circuits
                _electricalSystem.RemoveFromCircuit(circuits);

                //transaction.Commit();
            }

            catch
            {
                TaskDialog.Show("Circuit Error", "There is a circuiting error");
                //transaction.Dispose();
            }

            //// Get the MEP model of selected element
            //MEPModel mepModel = null;
            //FamilyInstance fi = selectedElement as FamilyInstance;
            //if (null == fi || null == (mepModel = fi.MEPModel))
            //{
            //    ShowErrorMessage("SelectElectricalComponent");
            //    return;
            //}

            //// Check whether the selected element belongs to the circuit
            //if (!IsElementBelongsToCircuit(mepModel, m_selectedElectricalSystem))
            //{
            //    ShowErrorMessage("ElementNotInCircuit");
            //    return;
            //}

            //try
            //{
            //    // Remove the selected element from circuit
            //   ElementSet es = new ElementSet();
            //   foreach (ElementId elementId in m_revitDoc.Selection.GetElementIds())
            //   {
            //      es.Insert(m_revitDoc.Document.GetElement(elementId));
            //   }
            //    m_selectedElectricalSystem.RemoveFromCircuit(es);
            //}
            //catch (Exception)
            //{
            //    ShowErrorMessage("FailedToRemoveElement");
            //}
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

            //Transaction transaction = new Transaction(_doc, "Circuit Panel");
            //transaction.Start();
            try
            {
                FamilyInstance fi;
                fi = panel as FamilyInstance;
                _electricalSystem.SelectPanel(fi);
                //transaction.Commit();
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToSelectPanel");
                //transaction.Dispose();
            }
        }

        // Disconnect panel for selected circuit
        public void DisconnectPanel()
        {
            //ERROR CHECKING
            //Confirm that electrical system has a panel

            //Transaction transaction = new Transaction(_doc, "Disconnect Circuit");
            //transaction.Start();
            try
            {
                _electricalSystem.DisconnectPanel();

                //transaction.Commit();
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToDisconnectPanel");
                //transaction.Dispose();
            }
        }

        ///// <summary>
        ///// Get selected index from circuit selecting form and locate expected circuit
        ///// </summary>
        ///// <param name="index">Index of selected item in circuit selecting form</param>
        //public void SelectCircuit(int index)
        //{
        //    // Locate ElectricalSystemItem by index
        //    ElectricalSystemItem esi = m_electricalSystemItems[index] as ElectricalSystemItem;
        //    Autodesk.Revit.DB.ElementId ei = esi.Id;

        //    // Locate expected electrical system
        //    m_selectedElectricalSystem = m_revitDoc.Document.GetElement(ei) as ElectricalSystem;
        //    // Select the electrical system
        //    SelectCurrentCircuit();
        //}

        ///// <summary>
        ///// Select created/modified/selected electrical system
        ///// </summary>
        //public void SelectCurrentCircuit()
        //{
        //    m_selection.GetElementIds().Clear();
        //    m_selection.GetElementIds().Add(m_selectedElectricalSystem.Id);
        //}

        ///// <summary>
        ///// Get selected index from circuit selecting form and show the circuit in the center of 
        ///// screen by moving the view.
        ///// </summary>
        ///// <param name="index">Index of selected item in circuit selecting form</param>
        //public void ShowCircuit(int index)
        //{
        //    ElectricalSystemItem esi = m_electricalSystemItems[index] as ElectricalSystemItem;
        //    Autodesk.Revit.DB.ElementId ei = esi.Id;
        //    m_revitDoc.ShowElements(ei);
        //}

        // Show message box with specified string
        static private void ShowErrorMessage(String message)
        {
            TaskDialog.Show("OperationFailed", message, TaskDialogCommonButtons.Ok);
        }
        #endregion
    }
}
