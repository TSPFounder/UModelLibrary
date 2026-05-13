using Applications;
using CAD;
using System;
using System.Collections.Generic;
using UModelLib;
using UModelLib;
using static System.Net.Mime.MediaTypeNames;

namespace SysML
{
    public class UModelDocument : AppDocument
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region
        //  
        //  Identification

        //
        //  Data

        //
        //  Owned & Owning Objects
        #endregion
        //  *****************************************************************************************


        //  ****************************************************************************************
        //  INITIALIZATIONS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  UMODELDOCUMENT CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public UModelDocument(UModelApp myApp)
        {
            MyUModelApp = myApp ?? throw new ArgumentNullException(nameof(myApp));
            MyUModelDocumentObj = myApp.CurrentApplication?.ActiveDocument;
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PROPERTIES
        //
        //  ************************************************************
        #region
        //
        //  Owned & Owning Objects
        //
        //  My UModel App
        public UModelApp MyUModelApp { get; }

        //  My UModel Document Object
        public UModelLib.IDocument? MyUModelDocumentObj { get; private set; }

        //  My UModel Diagrams
        public UModelDiagram? CurrentUModelDiagram { get; set; }

        public List<UModelDiagram> MyUModelDiagrams { get; } = new();
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  **************************************8cdvf x**********************
        #region
        public void CreateAircraftSysMLBDD()
        {
            
            IApplication uiApp = new UModelLib.Application();
            uiApp.Visible = true;

            UModelLib.Document document = uiApp.ActiveDocument;
            if (document == null) document = uiApp.ActiveDocument;

            UModelLib.IUMLGuiSysMLBlockDefinitionDiagram BDD_Diagram = (UModelLib.IUMLGuiSysMLBlockDefinitionDiagram)document.GuiRoot.InsertOwnedDiagramAt(0, document.RootPackage, "SysML Block Definition Diagram");

            DiagramWindow wnd = document.OpenDiagram(BDD_Diagram);


            IUMLPackage sysmlRootPackage = (IUMLPackage)document.RootPackage;

            

            sysmlRootPackage.InsertPackagedElementAt(1,"Package");
            sysmlRootPackage.Name = "AircraftDesign";

            IUMLGuiNodeLink aircraft = BDD_Diagram.AddUMLElement("Block", 60, 20);
            IUMLGuiNodeLink wing = BDD_Diagram.AddUMLElement("Block", 10, 30);

            IUMLGuiLineLink line = wnd.Diagram.AddUMLLineElement("Association", aircraft, wing);

            aircraft.SetRect(60, 20, 120, 80);

            //IUMLAssociation association = BDD_Diagram.AddUMLAssociation(aircraft, wing);
            

            Console.WriteLine("SysML BDD for Aircraft created successfully.");
        }

        // Helper method to apply the <<block>> stereotype required for SysML
        private void ApplyBlockStereotype(IUMLElement element)
        {
            // SysML profiles must be loaded in the UModel project for this to work
            //element.ApplyStereotype("block");
        }


        public static void reverseEngineerAndCreateSequenceDiagram(IApplication application, IUMLOperation operation)
        {
            GenerateSequenceDiagramDlg dialog = application.Dialogs.GenerateSequenceDiagramDlg;

            // set some options
            dialog.ShowEmptyCombinedFragments = false;
            dialog.UseDedicatedLineForStaticCalls = true;
            dialog.ShowCodeOfMessagesDisplayedDirectlyBelow = true;
            dialog.ShowCodeInNotes = true;

            dialog.ShowDialog = true; // set this to true if you want the dialog to be displayed

            // generated the sequence diagram now
            application.ActiveDocument.GenerateSequenceDiagram(dialog, operation);
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  EVENTS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************
    }
}
