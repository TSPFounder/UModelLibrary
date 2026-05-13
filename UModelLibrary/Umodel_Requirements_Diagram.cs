// Umodel_Requirements_Diagram.cs
// UModel COM implementation for SysML Requirements Diagrams.
//
// Creates a "SysML Requirements Diagram" in an active UModel document,
// places requirement boxes in a grid, and draws DeriveReqt/Satisfy/Verify
// relationship lines between them.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UModelLib;

namespace SysML
{
    public class Umodel_Requirements_Diagram(UModelDocument myDoc) : UModelDiagram(myDoc)
    {
        // ── COM diagram interface ─────────────────────────────────────────────

        public UModelLib.IUMLGuiDiagram? MySysML_Requirements_Interface { get; private set; }

        private const string DiagramTypeName = "SysML Requirements Diagram";

        // Layout constants
        private const int ReqX0       = 20;
        private const int ReqY0       = 20;
        private const int ReqWidth    = 200;
        private const int ReqHeight   = 80;
        private const int ReqXStep    = 240;
        private const int ReqYStep    = 120;
        private const int ReqsPerRow  = 4;

        // ── Core creation method ──────────────────────────────────────────────

        /// <summary>
        /// Create an empty Requirements diagram in the active UModel document.
        /// </summary>
        public void CreateRequirementsDiagram(string title)
            => CreateRequirementsDiagram(title,
                Array.Empty<SysML_Requirement>(),
                Array.Empty<SysML_RequirementRelationship>());

        /// <summary>
        /// Create a Requirements diagram and populate it with requirement boxes
        /// and relationship lines.
        /// </summary>
        /// <param name="title">Diagram title.</param>
        /// <param name="requirements">Requirement elements to place.</param>
        /// <param name="relationships">DeriveReqt / Satisfy / Verify lines to draw.</param>
        /// <returns>Reference to the created COM diagram interface, or null on failure.</returns>
        public IUMLGuiDiagram? CreateRequirementsDiagram(
            string title,
            IEnumerable<SysML_Requirement> requirements,
            IEnumerable<SysML_RequirementRelationship> relationships)
        {
            if (MyDoc?.MyUModelDocumentObj is not IDocument document)
                throw new InvalidOperationException("No active UModel document.");

            title = string.IsNullOrWhiteSpace(title) ? "Requirements Diagram" : title.Trim();

            // 1. Create the diagram
            var guiDiagram = (IUMLGuiDiagram)document.GuiRoot.InsertOwnedDiagramAt(
                0,
                document.RootPackage,
                DiagramTypeName);

            MySysML_Requirements_Interface = guiDiagram;

            // 2. Open diagram window
            var wnd = document.OpenDiagram(guiDiagram);
            if (wnd == null)
                throw new InvalidOperationException($"UModel failed to open diagram window for '{title}'.");

            // 3. Add requirement boxes in a grid
            var reqNodes = new Dictionary<string, IUMLGuiNodeLink>(StringComparer.OrdinalIgnoreCase);
            var reqList  = requirements.ToList();

            for (int i = 0; i < reqList.Count; i++)
            {
                var req  = reqList[i];
                int col  = i % ReqsPerRow;
                int row  = i / ReqsPerRow;
                int x    = ReqX0 + col * ReqXStep;
                int y    = ReqY0 + row * ReqYStep;

                IUMLGuiNodeLink node = guiDiagram.AddUMLElement("Requirement", x, y);
                node.SetRect(x, y, x + ReqWidth, y + ReqHeight);
                SetRequirementElement(node, req.RequirementId, req.Name, req.Text);

                reqNodes[req.RequirementId] = node;
            }

            // 4. Draw relationship lines
            foreach (var rel in relationships)
            {
                if (!reqNodes.TryGetValue(rel.Client.RequirementId, out var clientNode)) continue;
                if (!reqNodes.TryGetValue(rel.Supplier.RequirementId, out var supplierNode)) continue;

                string lineType = RequirementRelationshipTypeToString(rel.Type);
                wnd.Diagram.AddUMLLineElement(lineType, clientNode, supplierNode);
            }

            MyDoc.MyUModelDiagrams.Add(this);
            return guiDiagram;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Set the Id and Text properties on a SysML Requirement element via COM.
        /// UModel SysML requirements have «requirement» Id and Text tagged values.
        /// </summary>
        private static void SetRequirementElement(IUMLGuiNodeLink node,
            string reqId, string name, string text)
        {
            try
            {
                if (node.GetType().GetProperty("UMLElement")?.GetValue(node) is not IUMLNamedElement namedEl) return;

                namedEl.Name = name;

                // SysML «requirement» stereotype tagged values
                if (node.GetType().GetProperty("UMLElement")?.GetValue(node) is IUMLElement el)
                {
                    try
                    {
                        dynamic values = el.GetType().GetProperty("TaggedValues")?.GetValue(el);
                        dynamic tv = values?.GetByName("Id");
                        if (tv != null) tv.Value = reqId;
                    }
                    catch { /* tag may not exist if profile is not loaded */ }

                    try
                    {
                        dynamic values = el.GetType().GetProperty("TaggedValues")?.GetValue(el);
                        dynamic tv = values?.GetByName("Text");
                        if (tv != null) tv.Value = text;
                    }
                    catch { /* tag may not exist */ }
                }
            }
            catch
            {
                // Non-critical
            }
        }

        /// <summary>Map SysML requirement relationship type to UModel line element type string.</summary>
        private static string RequirementRelationshipTypeToString(RequirementRelationshipType type)
            => type switch
            {
                RequirementRelationshipType.DeriveReqt => "DeriveReqt",
                RequirementRelationshipType.Satisfy     => "Satisfy",
                RequirementRelationshipType.Verify      => "Verify",
                RequirementRelationshipType.Refine      => "Refine",
                RequirementRelationshipType.Trace       => "Trace",
                RequirementRelationshipType.Copy        => "Copy",
                _                                       => "DeriveReqt"
            };
    }
}
