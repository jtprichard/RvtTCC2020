using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.Attributes;
using System.IO;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Autodesk.Revit.UI.Events;

namespace RvtElectrical
{
    class ExternalApplication : IExternalApplication
    {

        #region GLOBAL PARAMETERS
        //Document
        public Document CurrentDocument { get; set; }

        // ExternalCommands assembly path
        internal static string _addInPath = typeof(ExternalApplication).Assembly.Location;
        // Button icons directory
        internal static string _ButtonIconsFolder = "pack://application:,,,/RvtElectrical;component/Resources/";
        // uiApplication for AutoBoxUpdate
        internal static UIApplication _uiApplicationAutoUpdate = null;
        // Application
        internal static ExternalApplication _app = null;

        //Updater for Circuit Label Watcher
        static IUCircuitLabel _circuitLabelUpdater = null;

        //Updater for Duplicate Box Wather
        static IUDuplicateBox _duplicateBoxUpdater = null;
        public bool DuplicateBoxOn = true;

        //Updater for Box AutoNumber
        static IUAutoNumberBox _autoNumberBoxUpdater = null;
        public bool AutoNumberBoxOn = false;

        //Stores reference to button to toggle the circuit label button image and image location reference
        RibbonButton _toggleCircuitLabelButton;
        BitmapImage _toggleCircuitLabelButtonImageOn = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder, 
            "spreadsheet_auto_update.png")));
        BitmapImage _toggleCircuitLabelButtonImageOff = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder, 
            "spreadsheet_no_auto_update.png")));

        RibbonButton _toggleDuplicateBoxButton;
        BitmapImage _toggleDuplicateButtonImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
            "duplicate.png")));
        BitmapImage _toggleDuplicateOffButtonImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
            "duplicate_off.png")));

        RibbonButton _toggleAutoNumberButton;
        BitmapImage _toggleAutoNumberButtonImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
            "autonumber_16x16.png")));
        BitmapImage _toggleAutoNumberOffButtonImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
            "autonumber_no_16x16.png")));


        #endregion

        #region IEXTERNALAPPLICATION MEMBERS
        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // create customer Ribbon Items
                CreateRibbonPanels(application);
                
                //Event Handler for document opened events
                application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(OnDocumentOpened);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Application Startup Error", ex.ToString());

                return Result.Failed;
            }
        }
        #endregion

        #region RIBBON PANEL CREATION
        private void CreateRibbonPanels(UIControlledApplication application)
        {
            _app = this;
            

            string tabName = "TCC Technical";
            string path = Assembly.GetExecutingAssembly().Location;

            //Create Ribbon Tab
            application.CreateRibbonTab(tabName);

            //Create Ribbon Panels
            CreateRPDeviceSchedule(application, path, tabName);
            CreateRPDeviceBoxes(application, path, tabName);
            CreateRPPanelBoard(application, path, tabName);
            CreateRPConduit(application, path, tabName);
            CreateRPFaceplates(application, path, tabName);
            //CreateRPSignalRiser(application, path, tabName);
            CreateRPFamilyEditor(application, path, tabName);

            //TEST BUTTON INITIATION
            CreateRPTestButton(application, path, tabName);

        }
        #endregion

        #region RIBBON PANEL - SCHEDULE LABELS
        private void CreateRPDeviceSchedule(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Schedule Labels");

            //BUTTON TO TURN ON AUTOMATIC UPDATING
            PushButtonData toggleButtonData = new PushButtonData("Toggle_Circuit_Label_Button", "Auto On", 
                path, "RvtElectrical.CmdToggleCircuitLabelButton");
            toggleButtonData.AvailabilityClassName = "RvtElectrical.AvailabilityProj";
            toggleButtonData.LargeImage = _toggleCircuitLabelButtonImageOn;

            //BUTTON TO MANUALLY UPDATE DEVICE SCHEDULES
            PushButtonData buttonManualUpdate = new PushButtonData("Manual_Circuit_Label_Button", "Manual\nUpdate",
                path, "RvtElectrical.CmdUpdateDeviceSchedules");
            buttonManualUpdate.ToolTip = "Update the circuit numbers on the sheet device schedules";
            buttonManualUpdate.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder, 
                "spreadsheet.png")));
            buttonManualUpdate.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            //ADD BUTTONS
            _ = panel.AddItem(buttonManualUpdate) as PushButton;
            _toggleCircuitLabelButton = panel.AddItem(toggleButtonData) as PushButton;
        }
        #endregion

        #region RIBBON PANEL - DEVICE BOX NUMBERING
        private void CreateRPDeviceBoxes(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Device Boxes");

            //BUTTON TO TURN ON DUPLICATE REVIEW
            PushButtonData toggleButtonData = new PushButtonData("Toggle_Duplicate_Box_Button", "Dup Chk\nOn",
                path, "RvtElectrical.CmdToggleDuplicateButton");
            toggleButtonData.AvailabilityClassName = "RvtElectrical.AvailabilityProj";
            toggleButtonData.LargeImage = _toggleDuplicateButtonImage;

            _toggleDuplicateBoxButton = panel.AddItem(toggleButtonData) as PushButton;

            panel.AddSeparator();

            //AUTO-NUMBERING BUTTON
            PushButtonData toggleButton2Data = new PushButtonData("Toggle_Auto_Number_Button", "Auto Off",
                path, "RvtElectrical.CmdToggleAutoNumberBoxButton");
            toggleButton2Data.AvailabilityClassName = "RvtElectrical.AvailabilityProj";
            toggleButton2Data.Image = _toggleAutoNumberOffButtonImage;
            toggleButton2Data.ToolTip = "Auto Number Device Boxes";
            toggleButton2Data.LongDescription = "Revit will automatically number device boxes as they are inserted into the model, sequencing to the next avaiable box number";

            //RE-NUMBERING BUTTON
            PushButtonData buttonData = new PushButtonData("Renumber_Button", "Renumber \nBoxes",
                path, "RvtElectrical.CmdRenumberBoxes");
            buttonData.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "number_16x16.png")));
            buttonData.ToolTip = "Renumbers a series of boxes";
            buttonData.LongDescription = "Click boxes in order.  Routine will continue numbering sequencing to the next available box number until the user hits escape";
            buttonData.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            //BOX NUMBER TEXTBOX
            TextBoxData boxNumberText = new TextBoxData("Box_Number_Text");
            BitmapImage boxNumberTextImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "checkmark_16x16.png")));
            boxNumberText.ToolTip = "Enter Starting Box Number";
            boxNumberText.LongDescription = "Box number will auto-advance to next available number after either automatic" +
                "insertion or automatic replacement.  One or both of those functions must be active";
            boxNumberText.Image = boxNumberTextImage;

            //CREATE STACKED PANEL
            IList<RibbonItem> stackedItems = panel.AddStackedItems(toggleButton2Data, buttonData, boxNumberText);
            _toggleAutoNumberButton = stackedItems[0] as RibbonButton;

            RibbonButton renumberBoxButton = stackedItems[1] as RibbonButton;
            TextBox tb1 = stackedItems[2] as TextBox;
            tb1.PromptText = "Box Number";
            tb1.Width = 120;
            tb1.ShowImageAsButton = true;
            tb1.EnterPressed += CallbackBoxNumberTextBox;

        }
        #endregion

        #region RIBBON PANEL - PANEL BOARD
        //PANEL SCHEDULE RIBBON PANEL
        private void CreateRPPanelBoard(UIControlledApplication application, string path, string tabName)
        {
            //BUTTON GROUP TO UPDATE PANEL SCHEDULES
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Specialty PanelBoards");

            //PANEL SCHEDULE SPLIT BUTTONS
            PushButtonData sbButton1 = new PushButtonData("Update_Panel_Schedule_Button", "Update Current\nPanel Schedule",
                path, "RvtElectrical.CmdPBUpdateSchedule");
            sbButton1.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "electrical_panel.png")));
            sbButton1.ToolTip = "Update the current specialty panel schedule";
            sbButton1.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton2 = new PushButtonData("Update_PL_Panel_Schedule_Button", "Update All\nLighting Panels",
                path, "RvtElectrical.CmdPBUpdatePLSchedule");
            sbButton2.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "electrical_panel_light.png")));
            sbButton2.ToolTip = "Update all performance lighting panel schedules";
            sbButton2.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton3 = new PushButtonData("Update_PS_Panel_Schedule_Button", "Update All\nSound Panels",
                path, "RvtElectrical.CmdPBUpdatePSSchedule");
            sbButton3.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "electrical_panel_sound.png")));
            sbButton3.ToolTip = "Update all performance sound panel schedules";
            sbButton3.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton4 = new PushButtonData("Update_All_Panel_Schedule_Button", "Update All\nPanel Schedules",
                path, "RvtElectrical.CmdPBUpdateAllSchedule");
            sbButton4.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "electrical_panel_all.png")));
            sbButton4.ToolTip = "Update all specialty panel schedules";
            sbButton4.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            SplitButtonData sb1 = new SplitButtonData("sbPanel_Schedule_Update", "Panel Schedules");
            SplitButton sb = panel.AddItem(sb1) as SplitButton;
            sb.AddPushButton(sbButton1);
            sb.AddPushButton(sbButton2);
            sb.AddPushButton(sbButton3);
            sb.AddPushButton(sbButton4);


            //PANEL SCHEDULES UTILTIES
            //Stacked Buttons
            PushButtonData button1 = new PushButtonData("Fill_Spare_Panel_Schedule_Button", "Fill\nSpares",
                path, "RvtElectrical.CmdPBFillSpare");
            button1.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "spare.png")));
            button1.ToolTip = "Automatically infill unused slots with spares";
            button1.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData button2 = new PushButtonData("Remove_Spare_Panel_Schedule_Button", "Remove\nSpares",
                path, "RvtElectrical.CmdPBRemoveSpare");
            button2.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "removespare.png")));
            button2.ToolTip = "Automatically remove spares from slots";
            button2.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData button3 = new PushButtonData("Remove_Circuits_Button", "Remove\nCircuits",
                path, "RvtElectrical.CmdPBRemoveCircuits");
            button3.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "remove_circuit.png")));
            button3.ToolTip = "Automatically remove spares from slots";
            button3.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            // Add 3 stacked buttons to the panel
            panel.AddStackedItems(button1, button2, button3);
        }
        #endregion

        #region RIBBON PANEL - CONDUIT
        //CONDUIT
        private void CreateRPConduit(UIControlledApplication application, string path, string tabName)
        {
            //BUTTON GROUP TO UPDATE PANEL SCHEDULES
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Conduit");

            //CONDUIT BUTTON DROPDOWN
            PushButtonData sbButton1 = new PushButtonData("Other_Conduit_Button", "Other\nLevel",
                path, "RvtElectrical.CmdCreateConduitOther");
            sbButton1.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "conduit_other.png")));
            sbButton1.ToolTip = "Create an Other Level Conduit";
            sbButton1.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton2 = new PushButtonData("Low_Conduit_Button", "Low\nLevel",
                path, "RvtElectrical.CmdCreateConduitLow");
            sbButton2.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "conduit_low.png")));
            sbButton2.ToolTip = "Create a Low Level Conduit";
            sbButton2.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton3 = new PushButtonData("Medium_Conduit_Button", "Medium\nLevel",
                path, "RvtElectrical.CmdCreateConduitMedium");
            sbButton3.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "conduit_medium.png")));
            sbButton3.ToolTip = "Create an Medium Level Conduit";
            sbButton3.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton4 = new PushButtonData("High_Conduit_Button", "High\nLevel",
                path, "RvtElectrical.CmdCreateConduitHigh");
            sbButton4.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "conduit_high.png")));
            sbButton4.ToolTip = "Create an High Level Conduit";
            sbButton4.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            PushButtonData sbButton5 = new PushButtonData("Live_Conduit_Button", "Line\nLevel",
                path, "RvtElectrical.CmdCreateConduitLine");
            sbButton5.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "conduit_line.png")));
            sbButton5.ToolTip = "Create a Line Level Conduit";
            sbButton5.AvailabilityClassName = "RvtElectrical.AvailabilityProj";

            SplitButtonData sb1 = new SplitButtonData("sbConduit_Place", "Place Conduit");
            SplitButton sb = panel.AddItem(sb1) as SplitButton;
            sb.AddPushButton(sbButton1);
            sb.AddPushButton(sbButton2);
            sb.AddPushButton(sbButton3);
            sb.AddPushButton(sbButton4);
            sb.AddPushButton(sbButton5);

            //CONDUIT INFORMATION COMBOBOXES
            ComboBoxData cbLevel = new ComboBoxData("Level");

            ComboBoxData cbSize = new ComboBoxData("Size");
            ComboBoxData cbQty = new ComboBoxData("Quantity");
            TextBoxData tbDestination = new TextBoxData("Destination");

            //CREATE STACKED PANEL
            IList<RibbonItem> stackedItems = panel.AddStackedItems(cbSize, cbQty, tbDestination);
            ComboBox combo1 = stackedItems[0] as ComboBox;
            ComboBox combo2 = stackedItems[1] as ComboBox;
            TextBox tb3 = stackedItems[2] as TextBox;

            //Conduit Size Members
            ComboBoxMemberData cb1MemData1 = new ComboBoxMemberData("B", "3/4\"");
            ComboBoxMember cb1Mem1 = combo1.AddItem(cb1MemData1);
            //cb1Mem1.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb1MemData2 = new ComboBoxMemberData("C", "1\"");
            ComboBoxMember cb1Mem2 = combo1.AddItem(cb1MemData2);
            //cb1Mem2.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb1MemData3 = new ComboBoxMemberData("D", "1.25\"");
            ComboBoxMember cb1Mem3 = combo1.AddItem(cb1MemData3);
            //cb1Mem3.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb1MemData4 = new ComboBoxMemberData("E", "1.5\"");
            ComboBoxMember cb1Mem4 = combo1.AddItem(cb1MemData4);
            //cb1Mem4.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb1MemData5 = new ComboBoxMemberData("F", "2\"");
            ComboBoxMember cb1Mem5 = combo1.AddItem(cb1MemData5);
            //cb1Mem5.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb1MemData6 = new ComboBoxMemberData("G", "4\"");
            ComboBoxMember cb1Mem6 = combo1.AddItem(cb1MemData6);
            //cb1Mem6.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));


            //Conduit Quantity Members
            ComboBoxMemberData cb2MemData1 = new ComboBoxMemberData("1", "1");
            ComboBoxMember cb2Mem1 = combo2.AddItem(cb2MemData1);
            //cb2Mem1.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb2MemData2 = new ComboBoxMemberData("2", "2");
            ComboBoxMember cb2Mem2 = combo2.AddItem(cb2MemData2);
            //cb2Mem2.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb2MemData3 = new ComboBoxMemberData("3", "3");
            ComboBoxMember cb2Mem3 = combo2.AddItem(cb2MemData3);
            //cb2Mem3.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));

            ComboBoxMemberData cb2MemData4 = new ComboBoxMemberData("4", "4");
            ComboBoxMember cb2Mem4 = combo2.AddItem(cb2MemData4);
            //cb2Mem4.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                //"16x16.png")));


            //Destination TextBox
            tb3.ToolTip = "Enter default conduit destination";
            tb3.LongDescription = "Destination should be noted as a Box Number or Rack Location";
            tb3.PromptText = "Destination";
            tb3.Image = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "checkmark_16x16.png")));
            tb3.ShowImageAsButton = true;
            
        }

        #endregion

        #region RIBBON PANEL - FACEPLATES
        private void CreateRPFaceplates(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Faceplates");

            //BUTTON TO COPY PLATE SCHEDULE
            PushButtonData buttonCreatePlateSchedules = new PushButtonData("Copy_Plate_Schedule", "Create\nPlate Schedules",
                path, "RvtElectrical.CmdCreateFaceplateSchedules");
            buttonCreatePlateSchedules.ToolTip = "Creates Faceplate Schedules from schedule template and faceplates in model";

            _ = panel.AddItem(buttonCreatePlateSchedules);
        }
        #endregion

        #region RIBBON PANEL - SIGNAL RISER
        private void CreateRPSignalRiser(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Signal Riser");
        }
        #endregion

        #region RIBBON PANEL - FAMILY EDITOR
        private void CreateRPFamilyEditor(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Family Editor");

            PushButtonData button = new PushButtonData("Associate_FP_Parameter_Button", "Finalize\nDevice",
                path, "RvtElectrical.CmdFPParamAssoc");
            button.ToolTip = "Associate parameters of connectors to faceplate family and provide plate & family codes";
            button.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder,
                "Faceplate New.png")));
            button.AvailabilityClassName = "RvtElectrical.AvailabilityFam";
            _ = panel.AddItem(button) as PushButton;
        }
        #endregion

        #region RIBBON PANEL -  TESTING
        private void CreateRPTestButton(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Test Area");

            //BUTTON TO INITIATE TEST
            //PushButtonData buttonManualUpdate = new PushButtonData("Test_Button", "Initiate \nTest",
            //    path, "RvtElectrical.CmdTests");
            PushButtonData buttonManualUpdate = new PushButtonData("Test_Button", "Initiate \nTest",
                path, "RvtElectrical.CmdTagDeviceBoxPower");
            buttonManualUpdate.ToolTip = "PB Test - Do Not Use";
            //buttonManualUpdate.LargeImage = new BitmapImage(new Uri(Path.Combine(_ButtonIconsFolder, "")));

            _ = panel.AddItem(buttonManualUpdate);
        }
        #endregion

        #region INSTANCE APPLICATION
        public static ExternalApplication Instance
        {
            get { return _app; }
        }
        #endregion

        #region TOGGLE BUTTON APPLICATIONS
        public void ToggleCircuitLabel(UIApplication uiapp)
        {
            string s = _toggleCircuitLabelButton.ItemText;
            Application app = uiapp.Application;

            _toggleCircuitLabelButton.ItemText = s.Equals("Auto On") ? "Auto Off" : "Auto On";
            if (s.Equals("Auto On"))
            {
                _toggleCircuitLabelButton.LargeImage = _toggleCircuitLabelButtonImageOff;
                CircuitLabelUpdaterOff(app);
            }

            else
            {
                _toggleCircuitLabelButton.LargeImage = _toggleCircuitLabelButtonImageOn;
                CircuitLabelUpdaterOn(app);
            }
        }

        public void ToggleDuplicateBox(UIApplication uiapp)
        {

            string s = _toggleDuplicateBoxButton.ItemText;
            Application app = uiapp.Application;

            _toggleDuplicateBoxButton.ItemText = s.Equals("Dup Chk\nOn") ? "Dup Chk\nOff" : "Dup Chk\nOn";
            if (s.Equals("Dup Chk\nOn"))
            {
                _toggleDuplicateBoxButton.LargeImage = _toggleDuplicateOffButtonImage;
                DuplicateBoxUpdaterOff(app);
                DuplicateBoxOn = false;
            }

            else
            {
                _toggleDuplicateBoxButton.LargeImage = _toggleDuplicateButtonImage;
                DuplicateBoxUpdaterOn(app);
                DuplicateBoxOn = true;
            }
        }

        public void ToggleAutoNumberBox(UIApplication uiapp)
        {
            string s = _toggleAutoNumberButton.ItemText;
            Application app = uiapp.Application;
            _uiApplicationAutoUpdate = uiapp;

            _toggleAutoNumberButton.ItemText = s.Equals("Auto Number") ? "Auto Off" : "Auto Number";
            if (s.Equals("Auto Number"))
            {
                _toggleAutoNumberButton.Image = _toggleAutoNumberOffButtonImage;
                AutoNumberBoxUpdaterOff(app);
                AutoNumberBoxOn = false;
            }

            else
            {
                _toggleAutoNumberButton.Image = _toggleAutoNumberButtonImage;
                AutoNumberBoxUpdaterOn(app);
                AutoNumberBoxOn = true;
            }
        }
        #endregion

        #region IUPDATER TRIGGERS
        public void CircuitLabelUpdaterOn(Application app)
        {
            if (null == _circuitLabelUpdater)
            {

                //Get the parameter of Shared Parameters
                ElementId eid = GetSharedParameterEid("TCC_CONNECTOR_CIRCUIT", TCCElecSettings.ConnectorCircuitGuid);
                ElementId eid2 = GetSharedParameterEid("TCC_CONNECTOR_LABEL_OTHER", TCCElecSettings.ConnectorLabelOtherGuid);
                ElementId eid3 = GetSharedParameterEid("TCC_CONNECTOR_LABEL_PREFIX", TCCElecSettings.ConnectorLabelPrefixGuid);

                if (eid == null)
                {
                    _toggleCircuitLabelButton.LargeImage = _toggleCircuitLabelButtonImageOff;
                    _toggleCircuitLabelButton.ItemText = "Auto Off";
                    return;
                }

                _circuitLabelUpdater = new IUCircuitLabel(app.ActiveAddInId);
                
                UpdaterRegistry.RegisterUpdater(_circuitLabelUpdater);
                ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);
                UpdaterRegistry.AddTrigger(_circuitLabelUpdater.GetUpdaterId(), filter, Element.GetChangeTypeParameter(eid));
                UpdaterRegistry.AddTrigger(_circuitLabelUpdater.GetUpdaterId(), filter, Element.GetChangeTypeParameter(eid2));
                UpdaterRegistry.AddTrigger(_circuitLabelUpdater.GetUpdaterId(), filter, Element.GetChangeTypeParameter(eid3));
                UpdaterRegistry.SetIsUpdaterOptional(_circuitLabelUpdater.GetUpdaterId(), true);
            }
        }

        public void CircuitLabelUpdaterOff(Application app)
        {

            if (null != _circuitLabelUpdater)
            {
                UpdaterRegistry.UnregisterUpdater(_circuitLabelUpdater.GetUpdaterId());
                _circuitLabelUpdater = null;
            }
        }

        public void DuplicateBoxUpdaterOn(Application app)
        {
            if (null == _duplicateBoxUpdater)
            {
                //Get ElementID of Shared Parameter TCC_BOX_ID
                ElementId eid = GetSharedParameterEid("TCC_BOX_ID", TCCElecSettings.BoxIdGuid);

                if (eid == null)
                {
                    _toggleDuplicateBoxButton.LargeImage = _toggleDuplicateOffButtonImage;
                    _toggleDuplicateBoxButton.ItemText = "Dup Chk\nOff";
                    return;
                }

                _duplicateBoxUpdater = new IUDuplicateBox(app.ActiveAddInId);
                UpdaterRegistry.RegisterUpdater(_duplicateBoxUpdater);
                ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);
                UpdaterRegistry.AddTrigger(_duplicateBoxUpdater.GetUpdaterId(), filter, Element.GetChangeTypeParameter(eid));
            }
        }

        public void DuplicateBoxUpdaterOff(Application app)
        {

            if (null != _duplicateBoxUpdater)
            {
                UpdaterRegistry.UnregisterUpdater(_duplicateBoxUpdater.GetUpdaterId());
                _duplicateBoxUpdater = null;
            }
        }

        public void AutoNumberBoxUpdaterOn(Application app)
        {
            if (null == _autoNumberBoxUpdater)
            {
                _autoNumberBoxUpdater = new IUAutoNumberBox(app.ActiveAddInId);
                UpdaterRegistry.RegisterUpdater(_autoNumberBoxUpdater);
                ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);
                UpdaterRegistry.AddTrigger(_autoNumberBoxUpdater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
                UpdaterRegistry.SetIsUpdaterOptional(_autoNumberBoxUpdater.GetUpdaterId(), true);
            }
        }

        public void AutoNumberBoxUpdaterOff(Application app)
        {

            if (null != _autoNumberBoxUpdater)
            {
                UpdaterRegistry.UnregisterUpdater(_autoNumberBoxUpdater.GetUpdaterId());
                _autoNumberBoxUpdater = null;
            }
        }
        #endregion

        #region EVENT HANDLERS
        private void OnDocumentOpened(object sender, DocumentOpenedEventArgs args) 
        // Document Opened Events
        {
            Document doc = args.Document;
            Application app = doc.Application;

            CurrentDocument = doc;

            //Start Circuit Label IUpdater
            CircuitLabelUpdaterOn(app);
            DuplicateBoxUpdaterOn(app);
        }

        public static void CallbackBoxNumberTextBox(object sender, TextBoxEnterPressedEventArgs args)
        {
            //Retrieve text box information
            TextBox textBox = sender as TextBox;

            if (int.TryParse(textBox.Value.ToString(), out int result))
                if (result > 0)
                    textBox.Value = result.ToString();
                else
                    textBox.Value = null;
            else
                textBox.Value = null;
        }
        #endregion

        #region Private Filter Methods for Startup

        /// <summary>
        /// Returns the ElementID of a shared parameter based on a GUID
        /// Only works if a model has a family with the shared parameter in it
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramGuid"></param>
        /// <returns></returns>
        private ElementId GetSharedParameterEid(string paramName, Guid paramGuid)
        {
            var fRule = new SharedParameterApplicableRule(paramName);
            var filter = new ElementParameterFilter(fRule);
            var collector = new FilteredElementCollector(CurrentDocument);
            IList<Element> eles = collector.WhereElementIsNotElementType().WherePasses(filter).ToElements();

            if(null != eles)
            {
                foreach(Element e in eles)
                {
                    Parameter p = e.get_Parameter(paramGuid);
                    if (null != p)
                        return p.Id; 
                }
            }
            return null;

        }


        #endregion
    }
}
