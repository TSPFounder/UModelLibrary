using System;
using System.Collections.Generic;using UModelLib;
using CAD;
using Applications;


namespace SysML
{
    public class UModelDocument
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
        //
        //  My UModel App
        private UModelApp _MyUModelApp;
        //
        //  UModel Document Object
        private UModelLib.IDocument _MyUModelDocumentObj;
        //
        //  UModel Diagrams
        private UModelDiagram _CurrentUModelDiagram;
        private List<UModelDiagram> _MyUModelDiagrams;
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
        //  ************************************************************
        #region

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
