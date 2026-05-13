// UModelFactory.cs
// High-level factory that orchestrates all Stage 1 diagram creation for a
// single DWM item. SRSES calls the methods here rather than driving each
// diagram class directly.
//
// Workflow per item:
//   1. factory.OpenOrCreateProject(filePath)
//   2. factory.CreateUseCaseDiagram(title, oosemModel)
//   3. factory.CreateRequirementsDiagram(title, oosemModel)
//   4. factory.CreateArchitectureDiagrams(title, oosemModel)
//   5. factory.SaveAndClose()

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UModelLib;

namespace SysML
{
    /// <summary>
    /// Result bundle returned after creating diagrams for one item.
    /// Carries the diagram references so SRSES can store UModel diagram IDs in SQLite.
    /// </summary>
    public class UModelDiagramResult
    {
        public string ItemId { get; set; } = string.Empty;

        /// <summary>Title of the use case diagram created (or null if skipped).</summary>
        public string? UseCaseDiagramTitle { get; set; }

        /// <summary>Title of the requirements diagram created (or null if skipped).</summary>
        public string? RequirementsDiagramTitle { get; set; }

        /// <summary>Titles of BDD diagrams created (logical + physical architecture).</summary>
        public List<string> BDD_DiagramTitles { get; } = new();

        /// <summary>Title of the IBD diagram created (or null if skipped).</summary>
        public string? IBD_DiagramTitle { get; set; }

        /// <summary>Path to the UModel project file (.ump).</summary>
        public string? ProjectFilePath { get; set; }
    }

    /// <summary>
    /// Orchestrates UModel COM calls for the full DWM Stage 1 diagram set.
    /// </summary>
    public class UModelFactory : IDisposable
    {
        private readonly UModelApp  _app;
        private UModelDocument?     _doc;
        private bool                _disposed;

        public UModelFactory()
        {
            _app = new UModelApp();
            if (!_app.CreateUModelApp())
                throw new InvalidOperationException(
                    "Could not launch UModel. Verify Altova UModel is installed and its COM library is registered.");
        }

        /// <summary>
        /// Open an existing UModel project file, or create a new one.
        /// </summary>
        /// <param name="filePath">Full path to the .ump file.</param>
        public void OpenOrCreateProject(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided.", nameof(filePath));

            // Ensure directory exists
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (_app.CurrentApplication == null)
                throw new InvalidOperationException("UModel application is not running.");

            if (File.Exists(filePath))
            {
                // OpenDocument makes the file the active document automatically
                _app.CurrentApplication.OpenDocument(filePath);
            }
            else
            {
                // NewDocument creates a blank doc and makes it active
                var newDoc = _app.CurrentApplication.NewDocument();
                newDoc.SaveAs(filePath);
            }

            // UModelDocument reads CurrentApplication.ActiveDocument in its constructor
            _doc = new UModelDocument(_app);
        }

        /// <summary>
        /// Create the Stage 1 Use Case diagram for an item from its OOSEM model.
        /// </summary>
        public UModelDiagramResult CreateStage1Diagrams(
            string itemId,
            string itemName,
            IEnumerable<SysML_Actor>   actors,
            IEnumerable<SysML_UseCase> useCases,
            IEnumerable<SysML_Requirement> requirements,
            IEnumerable<SysML_RequirementRelationship> requirementRelationships,
            IEnumerable<SysML_Block> logicalBlocks,
            IEnumerable<SysML_Line>  logicalLines)
        {
            EnsureDocumentOpen();

            var result = new UModelDiagramResult
            {
                ItemId          = itemId,
                ProjectFilePath = null
            };

            // 1. Use Case Diagram
            string ucTitle = $"{itemName} — Use Cases";
            var ucDiagram = new Umodel_UseCase_Diagram(_doc!);
            ucDiagram.CreateUseCaseDiagram(ucTitle, actors, useCases);
            result.UseCaseDiagramTitle = ucTitle;

            // 2. Requirements Diagram
            string reqTitle = $"{itemName} — Requirements";
            var reqDiagram = new Umodel_Requirements_Diagram(_doc!);
            reqDiagram.CreateRequirementsDiagram(reqTitle, requirements, requirementRelationships);
            result.RequirementsDiagramTitle = reqTitle;

            // 3. BDD — Logical Architecture
            string bddLogTitle = $"{itemName} — Logical Architecture [BDD]";
            var bddDiagram = new Umodel_BDD_Diagram(_doc!);
            bddDiagram.CreateBDD(bddLogTitle, logicalBlocks, logicalLines, parentPackageName: itemName);
            result.BDD_DiagramTitles.Add(bddLogTitle);

            // Save after creating diagrams
            try { _doc!.MyUModelDocumentObj?.Save(); } catch { /* non-critical */ }

            return result;
        }

        /// <summary>
        /// Create a standalone Use Case diagram (called from DWM_CreateUseCases).
        /// </summary>
        public string CreateUseCaseDiagram(string title,
            IEnumerable<SysML_Actor>   actors,
            IEnumerable<SysML_UseCase> useCases)
        {
            EnsureDocumentOpen();
            var diagram = new Umodel_UseCase_Diagram(_doc!);
            diagram.CreateUseCaseDiagram(title, actors, useCases);
            return title;
        }

        /// <summary>
        /// Create a standalone Requirements diagram (called from DWM_CreateRequirements).
        /// </summary>
        public string CreateRequirementsDiagram(string title,
            IEnumerable<SysML_Requirement> requirements,
            IEnumerable<SysML_RequirementRelationship> relationships)
        {
            EnsureDocumentOpen();
            var diagram = new Umodel_Requirements_Diagram(_doc!);
            diagram.CreateRequirementsDiagram(title, requirements, relationships);
            return title;
        }

        /// <summary>
        /// Create a BDD for an architecture layer (called from DWM_CreateArchitecture).
        /// </summary>
        public string CreateBDD(string title,
            IEnumerable<SysML_Block> blocks,
            IEnumerable<SysML_Line>  lines,
            string? packageName = null)
        {
            EnsureDocumentOpen();
            var diagram = new Umodel_BDD_Diagram(_doc!);
            diagram.CreateBDD(title, blocks, lines, packageName);
            return title;
        }

        /// <summary>
        /// Create an IBD for a context block (called from DWM_CreateArchitecture).
        /// </summary>
        public string CreateIBD(string title, string contextBlockName,
            IEnumerable<SysML_Part>      parts,
            IEnumerable<SysML_Connector> connectors)
        {
            EnsureDocumentOpen();
            var diagram = new Umodel_IBD_Diagram(_doc!);
            diagram.CreateIBD(title, contextBlockName, parts, connectors);
            return title;
        }

        /// <summary>Save and close the active UModel document.</summary>
        public void SaveAndClose()
        {
            if (_doc?.MyUModelDocumentObj == null) return;
            try
            {
                _doc.MyUModelDocumentObj.Save();
            }
            catch { /* best-effort */ }
        }

        /// <summary>Save without closing.</summary>
        public void Save()
        {
            try { _doc?.MyUModelDocumentObj?.Save(); } catch { /* best-effort */ }
        }

        // ── IDisposable ───────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            SaveAndClose();
            _disposed = true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void EnsureDocumentOpen()
        {
            if (_doc?.MyUModelDocumentObj == null)
                throw new InvalidOperationException(
                    "No UModel project is open. Call OpenOrCreateProject() first.");
        }
    }
}
