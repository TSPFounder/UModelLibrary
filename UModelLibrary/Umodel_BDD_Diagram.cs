// Umodel_BDD_Diagram.cs
// UModel COM implementation for SysML Block Definition Diagrams.
//
// Creates a "SysML Block Definition Diagram", places blocks in a grid,
// and draws association/generalisation lines between them.
// Mirrors the proof-of-concept in UModelDocument.CreateAircraftSysMLBDD()
// but works from SysML_Library typed data.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UModelLib;

namespace SysML
{
    public class Umodel_BDD_Diagram(UModelDocument myDoc) : UModelDiagram(myDoc)
    {
        // ── COM diagram interface ─────────────────────────────────────────────

        public UModelLib.IUMLGuiSysMLBlockDefinitionDiagram? MySysML_BDD_Interface { get; private set; }

        private const string DiagramTypeName = "SysML Block Definition Diagram";

        // Layout constants
        private const int BlockX0     = 20;
        private const int BlockY0     = 20;
        private const int BlockWidth  = 160;
        private const int BlockHeight = 80;
        private const int BlockXStep  = 200;
        private const int BlockYStep  = 120;
        private const int BlocksPerRow = 4;

        // ── Core creation method ──────────────────────────────────────────────

        /// <summary>Create an empty BDD in the active UModel document.</summary>
        public void CreateBDD(string title)
            => CreateBDD(title,
                Array.Empty<SysML_Block>(),
                Array.Empty<SysML_Line>(),
                parentPackageName: null);

        /// <summary>
        /// Create a Block Definition Diagram from SysML_Library typed blocks and lines.
        /// </summary>
        /// <param name="title">Diagram title.</param>
        /// <param name="blocks">Blocks to place on the diagram.</param>
        /// <param name="lines">Association/generalisation/composition lines to draw.</param>
        /// <param name="parentPackageName">
        ///   Optional UModel package name to create the diagram inside.
        ///   Null → places diagram in root package.
        /// </param>
        /// <returns>Reference to the created COM diagram interface.</returns>
        public IUMLGuiSysMLBlockDefinitionDiagram? CreateBDD(
            string title,
            IEnumerable<SysML_Block> blocks,
            IEnumerable<SysML_Line> lines,
            string? parentPackageName = null)
        {
            if (MyDoc?.MyUModelDocumentObj is not IDocument document)
                throw new InvalidOperationException("No active UModel document.");

            title = string.IsNullOrWhiteSpace(title) ? "Block Definition Diagram" : title.Trim();

            // 1. Resolve or create the owner package
            IUMLPackage ownerPackage = ResolvePackage(document, parentPackageName);

            // 2. Create the BDD
            var guiDiagram = (IUMLGuiSysMLBlockDefinitionDiagram)document.GuiRoot.InsertOwnedDiagramAt(
                0,
                ownerPackage,
                DiagramTypeName);

            MySysML_BDD_Interface = guiDiagram;

            // 3. Open diagram window
            var wnd = document.OpenDiagram(guiDiagram);
            if (wnd == null)
                throw new InvalidOperationException($"UModel failed to open diagram window for '{title}'.");

            // 4. Add blocks in a grid and collect node map (keyed by block Name)
            var blockNodes = new Dictionary<string, IUMLGuiNodeLink>(StringComparer.OrdinalIgnoreCase);
            var blockList  = blocks.ToList();

            for (int i = 0; i < blockList.Count; i++)
            {
                var block = blockList[i];
                int col   = i % BlocksPerRow;
                int row   = i / BlocksPerRow;
                int x     = BlockX0 + col * BlockXStep;
                int y     = BlockY0 + row * BlockYStep;

                // UModel element type for a SysML block is "Block"
                IUMLGuiNodeLink node = guiDiagram.AddUMLElement("Block", x, y);
                node.SetRect(x, y, x + BlockWidth, y + BlockHeight);
                SetElementName(node, block.Name);

                blockNodes[block.Name] = node;
            }

            // 5. Draw lines
            foreach (var line in lines)
            {
                if (line.Source == null || line.Target == null) continue;
                if (!blockNodes.TryGetValue(line.Source.Name, out var sourceNode)) continue;
                if (!blockNodes.TryGetValue(line.Target.Name, out var targetNode)) continue;

                string lineType = LineTypeToString(line.LineType);
                wnd.Diagram.AddUMLLineElement(lineType, sourceNode, targetNode);
            }

            MyDoc.MyUModelDiagrams.Add(this);
            return guiDiagram;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static IUMLPackage ResolvePackage(IDocument document, string? packageName)
        {
            var root = (IUMLPackage)document.RootPackage;
            if (string.IsNullOrWhiteSpace(packageName)) return root;

            // Try to find an existing package with this name; create one if absent
            try
            {
                dynamic dynRoot = root;
                int ownedMembersCount = dynRoot.OwnedMembersCount;
                for (int i = 0; i < ownedMembersCount; i++)
                {
                    var member = dynRoot.OwnedMemberAt(i);
                    if (member is IUMLPackage pkg && pkg.Name == packageName)
                        return pkg;
                }

                dynRoot.InsertPackagedElementAt(ownedMembersCount, "Package");
                var newPkg = (IUMLPackage)dynRoot.OwnedMemberAt(ownedMembersCount);
                newPkg.Name = packageName;
                return newPkg;
            }
            catch
            {
                return root;
            }
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

        private static string LineTypeToString(SysML_Line.SysML_LineTypeEnum lineType)
            => lineType switch
            {
                SysML_Line.SysML_LineTypeEnum.Association    => "Association",
                SysML_Line.SysML_LineTypeEnum.Generalization => "Generalization",
                SysML_Line.SysML_LineTypeEnum.Composition    => "Composition",
                SysML_Line.SysML_LineTypeEnum.Aggregation    => "Aggregation",
                SysML_Line.SysML_LineTypeEnum.Dependency     => "Dependency",
                SysML_Line.SysML_LineTypeEnum.Realization     => "Realization",
                _                                             => "Association"
            };
    }
}
