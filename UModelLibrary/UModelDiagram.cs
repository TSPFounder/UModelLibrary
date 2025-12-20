using System;
using System.Collections.Generic;
using UModelLib;
using CAD;
using Applications;

namespace SysML
{
    public class UModelDiagram
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
        //  Mu UModel Document
        private UModelDocument _MyDoc;
        //
        //  My Diagram Window Object
        private IDiagramWindow _MyActiveDiagram;
        //
        //  My UModel Diagram Object
        //private UModelLib.
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
        public enum SysML_DiagramTypeEnum
        {
            Package = 0,
            BDD,
            IBD,
            Parametric,
            Activity,
            StateMachine,
            Sequence,
            UseCase,
            Requirements
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  UMODELDIAGRAM CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public UModelDiagram(UModelDocument myDoc)
        {
            MyDoc = myDoc ?? throw new ArgumentNullException(nameof(myDoc));
            MyActiveDiagram = myDoc.MyUModelDocumentObj?.ActiveDiagramWindow;
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
        public UModelDocument MyDoc { get; }
        //
        //  My Diagram Window Object
       public IDiagramWindow? MyActiveDiagram { get; }
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
