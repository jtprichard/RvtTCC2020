using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Collections;
using Autodesk.Revit.UI.Events;

namespace RvtElectrical
{
    static class RibbonPanelsTest
    {
        public static void RibbonPanelTest(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Test Commands");
            PushButtonData button = new PushButtonData("Button1", "GetId", path, "RvtElectrical.GetElementId3");
            panel.AddItem(button);
            button = new PushButtonData("Button2", "GetParameter", path, "RvtElectrical.GetParameter");
            panel.AddItem(button);
            button = new PushButtonData("Button3", "Collect Elements", path, "RvtElectrical.CollectElements");
            panel.AddItem(button);

            //COMBO BOX FOR PANELBOARD SELECTION - THIS IS A TEST
            BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            button.LargeImage = buttonImage;

            ComboBoxData cbData = new ComboBoxData("Panelboard");

            TextBoxData textData = new TextBoxData("Text Box");
            textData.Name = "Text Box";
            textData.Image = buttonImage;  //FOR TESTING
            textData.ToolTip = "Tooltip for Textbox";
            textData.LongDescription = "This is the text that will appear next to the image";
            textData.ToolTipImage = buttonImage; //FOR TESTING

            IList<RibbonItem> stackedItems = panel.AddStackedItems(textData, cbData);
            if (stackedItems.Count > 1)
            {
                if (stackedItems[0] is TextBox tBox)
                {
                    tBox.PromptText = "Enter a comment";
                    tBox.ShowImageAsButton = true;
                    tBox.ToolTip = "Enter some text";
                    // Register event handler ProcessText
                    tBox.EnterPressed +=
            new EventHandler<Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs>(ProcessText);
                }

                if (stackedItems[1] is ComboBox cBox)
                {
                    cBox.ItemText = "Panelboard";
                    cBox.ToolTip = "Select a Panelboard";
                    cBox.LongDescription = "Select a panelboard to circuit to";

                    ComboBoxMemberData cboxMemDataA = new ComboBoxMemberData("A", "Option A");
                    //cboxMemDataA.Image =
                    //        new BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\A.bmp"));
                    cboxMemDataA.GroupName = "Letters";
                    cBox.AddItem(cboxMemDataA);

                    ComboBoxMemberData cboxMemDataB = new ComboBoxMemberData("B", "Option B");
                    //cboxMemDataB.Image =
                    //        new BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\B.bmp"));
                    cboxMemDataB.GroupName = "Letters";
                    cBox.AddItem(cboxMemDataB);

                    ComboBoxMemberData cboxMemData = new ComboBoxMemberData("One", "Option 1");
                    //cboxMemData.Image =
                    //        new BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\One.bmp"));
                    cboxMemData.GroupName = "Numbers";
                    cBox.AddItem(cboxMemData);
                    ComboBoxMemberData cboxMemData2 = new ComboBoxMemberData("Two", "Option 2");
                    //cboxMemData2.Image =
                    //        new BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\Two.bmp"));
                    cboxMemData2.GroupName = "Numbers";
                    cBox.AddItem(cboxMemData2);
                    ComboBoxMemberData cboxMemData3 = new ComboBoxMemberData("Three", "Option 3");
                    //cboxMemData3.Image =
                    //        new BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\Three.bmp"));
                    cboxMemData3.GroupName = "Numbers";
                    cBox.AddItem(cboxMemData3);


                }
            }
            //BUTTON TO TEST PANEL CIRCUIT FUNCTIONS
            button = new PushButtonData("Circuit_Panel_Schedule_Button", "Circuit\nPanel", path, "RvtElectrical.UpdateConnectCircuits");

            button.ToolTip = "Circuit the specialty panel schedules";

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            //button.LargeImage = buttonImage;

            //_ =panel.AddItem(button) as PushButton;
            panel.AddItem(button);

            //BUTTON TO TEST PANEL APPEND FUNCTIONS
            button = new PushButtonData("Append_Panel_Schedule_Button", "Append\nCircuits", path, "RvtElectrical.UpdateAppendCircuits");

            button.ToolTip = "Append Circuits to Panels";

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            //button.LargeImage = buttonImage;

            //_ =panel.AddItem(button) as PushButton;
            panel.AddItem(button);


            //BUTTON TO TEST PANEL DISCONNECT FUNCTIONS
            button = new PushButtonData("Disconnect_Panel_Schedule_Button", "Disconnect\nCircuits", path, "RvtElectrical.UpdateDisconnectCircuits");

            button.ToolTip = "Disconnect Circuits from Panels";

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            //button.LargeImage = buttonImage;

            //_ =panel.AddItem(button) as PushButton;
            panel.AddItem(button);

            //BUTTON TO TEST PANEL DISCONNECT APPEND FUNCTIONS
            button = new PushButtonData("Disconnect_Append_Schedule_Button", "Disconnect\nAppended", path, "RvtElectrical.UpdateRemoveAppendCircuits");

            button.ToolTip = "Disconnect Appended Circuits from Panels";

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            //button.LargeImage = buttonImage;

            //_ =panel.AddItem(button) as PushButton;
            panel.AddItem(button);

            //BUTTON TO TEST MOVE CIRCUIT FUNCTIONS
            button = new PushButtonData("Move_Circuits_Button", "Move\nCircuits", path, "RvtElectrical.UpdateMoveCircuits");

            button.ToolTip = "Move Circuits";

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            //button.LargeImage = buttonImage;

            //_ =panel.AddItem(button) as PushButton;
            panel.AddItem(button);
        }


            private static void ProcessText(object sender, TextBoxEnterPressedEventArgs e)
        {
            // cast sender as TextBox to retrieve text value
            TextBox textBox = sender as TextBox;
            string strText = textBox.Value as string;
        }
    }

}
