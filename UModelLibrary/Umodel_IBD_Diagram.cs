// Umodel_IBD_Diagram.cs
// UModel COM implementation for SysML Internal Block Diagrams.
//
// Creates a "SysML Internal Block Diagram" showing the internal structure
// of a context block — its parts, ports, and connectors.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UModelLib;

namespace SysML
{
    public class Umodel_IBD_Diagram(UModelDocument myDoc) : UModelDiagram(myDoc)
    {
        // ── COM diagram interface ─────────────────────────────────────────────

        public UModelLib.IUMLGuiSysMLInternalBlockDiagram? MySysML_IBD_Interface { get; private set; }

        private const string DiagramTypeName = "SysML Internal Block Diagram";

        // Layout constants
        private const int PartX0      = 40;
        private const int PartY0      = 60;
        private const int PartWidth   = 160;
        private const int PartHeight  = 100;
        private const int PartXStep   = 220;

        private const int PortWidth   = 20;
        private const int PortHeight  = 20;

        // ── Core creation method ──────────────────────────────────────────────

        /// <summary>Create an empty IBD in the active UModel document.</summary>
        public void CreateIBD(string title)
            => CreateIBD(title,
                contextBlockName: title,
                parts: Array.Empty<SysML_Part>(),
                connectors: Array.Empty<SysML_Connector>());

        /// <summary>
        /// Create an Internal Block Diagram from typed SysML_Library parts and connectors.
        /// </summary>
        /// <param name="title">Diagram title.</param>
        /// <param name="contextBlockName">Name of the context block (appears in diagram frame).</param>
        /// <param name="parts">Part instances to place inside the context block.</param>
        /// <param name="connectors">Connectors between part ports.</param>
        /// <returns>Reference to the created COM diagram interface.</returns>
        public IUMLGuiSysMLInternalBlockDiagram? CreateIBD(
            string title,
            string contextBlockName,
            IEnumerable<SysML_Part> parts,
            IEnumerable<SysML_Connector> connectors)
        {
            if (MyDoc?.MyUModelDocumentObj is not IDocument document)
                throw new InvalidOperationException("No active UModel document.");

            title = string.IsNullOrWhiteSpace(title) ? "Internal Block Diagram" : title.Trim();

            // 1. Create diagram
            var guiDiagram = (IUMLGuiSysMLInternalBlockDiagram)document.GuiRoot.InsertOwnedDiagramAt(
                0,
                document.RootPackage,
                DiagramTypeName);

            MySysML_IBD_Interface = guiDiagram;

            // 2. Open window
            var wnd = document.OpenDiagram(guiDiagram);
            if (wnd == null)
                throw new InvalidOperationException($"UModel failed to open diagram window for '{title}'.");

            // 3. Set the context block name on the diagram frame
            try { SetElementName((IUMLGuiNodeLink)guiDiagram, contextBlockName); } catch { /* optional */ }

            // 4. Add part instances in a horizontal row
            var partNodes = new Dictionary<string, IUMLGuiNodeLink>(StringComparer.OrdinalIgnoreCase);
            var partList  = parts.ToList();

            for (int i = 0; i < partList.Count; i++)
            {
                var part = partList[i];
                int x    = PartX0 + i * PartXStep;

                // "Part" is the element type for an IBD part property
                IUMLGuiNodeLink node = guiDiagram.AddUMLElement("Part", x, PartY0);
                node.SetRect(x, PartY0, x + PartWidth, PartY0 + PartHeight);
                SetElementName(node, $"{part.Name} : {part.BlockType.Name}");

                partNodes[part.Name] = node;

                // 4a. Add ports on this part
                int portIdx = 0;
                foreach (var port in part.Ports)
                {
                    int px = x + PartWidth - PortWidth; // right edge
                    int py = PartY0 + portIdx * (PortHeight + 4);

                    IUMLGuiNodeLink portNode = guiDiagram.AddUMLElement("Port", px, py);
                    portNode.SetRect(px, py, px + PortWidth, py + PortHeight);
                    SetElementName(portNode, port.Name);
                    portIdx++;
                }
            }

            // 5. Draw connectors
            // Connectors connect ports on parts; here we connect the owning part nodes as a proxy
            var connectorList = connectors.ToList();
            foreach (var connector in connectorList)
            {
                // Find which parts own the source/target ports
                string? sourcePart = FindPartForPort(partList, connector.SourcePort.Name);
                string? targetPart = FindPartForPort(partList, connector.TargetPort.Name);

                if (sourcePart != null && targetPart != null
                    && partNodes.TryGetValue(sourcePart, out var srcNode)
                    && partNodes.TryGetValue(targetPart, out var tgtNode))
                {
                    var lineLabel = string.IsNullOrWhiteSpace(connector.ItemFlowType)
                        ? null
                        : connector.ItemFlowType;

                    // "Connector" is the UModel IBD connector element type
                    var connLine = wnd.Diagram.AddUMLLineElement("Connector", srcNode, tgtNode);
                    // Label item flow type if present (stored in line metadata)
                    try
                    {
                        if (connLine is IUMLGuiLineLink ll && lineLabel != null
                            && ll.GetType().GetProperty("UMLElement")?.GetValue(ll) is IUMLNamedElement named)
                        {
                            named.Name = lineLabel;
                        }
                    }
                    catch { /* non-critical */ }
                }
            }

            MyDoc.MyUModelDiagrams.Add(this);
            return guiDiagram;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string? FindPartForPort(List<SysML_Part> parts, string portName)
        {
            foreach (var part in parts)
                if (part.Ports.Any(p => p.Name == portName))
                    return part.Name;
            return null;
        }

        private static void SetElementName(IUMLGuiNodeLink node, string name)
        {
            try
            {
                if (node.GetType().GetProperty("UMLElement")?.GetValue(node) is IUMLNamedElement named)
                    named.Name = name;
            }
            catch { /* Non-critical */ }
        }
    }
}
