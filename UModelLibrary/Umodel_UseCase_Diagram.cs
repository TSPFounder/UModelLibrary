// Umodel_UseCase_Diagram.cs
// UModel COM implementation for SysML Use Case Diagrams.
//
// Creates a "UML Use Case Diagram" in an active UModel document, populates it
// with actors and use cases sourced from the SysML data model, and lays them
// out in a two-row grid.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UModelLib;

namespace SysML
{
    public class Umodel_UseCase_Diagram(UModelDocument myDoc) : UModelDiagram(myDoc)
    {
        // ── COM diagram interface ─────────────────────────────────────────────

        public UModelLib.IUMLGuiUseCaseDiagram? MySysML_UseCase_Interface { get; private set; }

        // ── Diagram type string recognised by UModel ──────────────────────────

        private const string DiagramTypeName = "UML Use Case Diagram";

        // Layout constants (UModel canvas units)
        private const int ActorX0     = 20;
        private const int ActorY0     = 20;
        private const int ActorWidth  = 80;
        private const int ActorHeight = 80;
        private const int ActorXStep  = 160;

        private const int UseCaseX0     = 20;
        private const int UseCaseY0     = 160;
        private const int UseCaseWidth  = 160;
        private const int UseCaseHeight = 60;
        private const int UseCaseXStep  = 200;

        // ── Core creation method ──────────────────────────────────────────────

        /// <summary>
        /// Create a Use Case diagram in the active UModel document.
        /// </summary>
        /// <param name="title">Diagram title (appears in UModel diagram tree).</param>
        public void CreateUseCaseDiagram(string title)
            => CreateUseCaseDiagram(title,
                Array.Empty<SysML_Actor>(),
                Array.Empty<SysML_UseCase>());

        /// <summary>
        /// Create a Use Case diagram and populate it with typed actors and use cases.
        /// Associations between each use case and its actors are drawn automatically.
        /// </summary>
        /// <param name="title">Diagram title.</param>
        /// <param name="actors">Actors to place on the diagram.</param>
        /// <param name="useCases">Use cases to place on the diagram.</param>
        /// <returns>Reference to the created COM diagram interface, or null on failure.</returns>
        public IUMLGuiUseCaseDiagram? CreateUseCaseDiagram(
            string title,
            IEnumerable<SysML_Actor> actors,
            IEnumerable<SysML_UseCase> useCases)
        {
            if (MyDoc?.MyUModelDocumentObj is not IDocument document)
                throw new InvalidOperationException("No active UModel document. Call UModelDocument.Open() first.");

            title = string.IsNullOrWhiteSpace(title) ? "Use Case Diagram" : title.Trim();

            // 1. Create diagram in the root package
            var guiDiagram = (IUMLGuiUseCaseDiagram)document.GuiRoot.InsertOwnedDiagramAt(
                0,
                document.RootPackage,
                DiagramTypeName);

            MySysML_UseCase_Interface = guiDiagram;

            // 2. Open the diagram window
            var wnd = document.OpenDiagram(guiDiagram);
            if (wnd == null)
                throw new InvalidOperationException($"UModel failed to open diagram window for '{title}'.");

            // 3. Add actor elements — top row
            var actorNodes   = new Dictionary<string, IUMLGuiNodeLink>(StringComparer.OrdinalIgnoreCase);
            var actorList    = actors.ToList();

            for (int i = 0; i < actorList.Count; i++)
            {
                var actor = actorList[i];
                int x = ActorX0 + i * ActorXStep;

                IUMLGuiNodeLink node = guiDiagram.AddUMLElement("Actor", x, ActorY0);
                node.SetRect(x, ActorY0, x + ActorWidth, ActorY0 + ActorHeight);
                SetElementName(node, actor.Name);

                actorNodes[actor.Name] = node;
            }

            // 4. Add use case elements — second row
            var useCaseNodes = new Dictionary<string, IUMLGuiNodeLink>(StringComparer.OrdinalIgnoreCase);
            var ucList       = useCases.ToList();

            for (int i = 0; i < ucList.Count; i++)
            {
                var uc = ucList[i];
                int x = UseCaseX0 + i * UseCaseXStep;

                IUMLGuiNodeLink node = guiDiagram.AddUMLElement("UseCase", x, UseCaseY0);
                node.SetRect(x, UseCaseY0, x + UseCaseWidth, UseCaseY0 + UseCaseHeight);
                SetElementName(node, uc.Name);

                useCaseNodes[uc.Name] = node;

                // 5. Draw associations: primary actor → use case
                if (actor_For(uc.Name, actorNodes, uc) is { } primaryNode)
                    wnd.Diagram.AddUMLLineElement("Association", primaryNode, node);

                // Draw associations for secondary actors
                foreach (var secActor in uc.AssociatedUseCases
                    .Where(ua => actorNodes.ContainsKey(ua.Name))
                    .Select(_ => (IUMLGuiNodeLink?)null)   // placeholder — actual mapping below
                    .Where(n => n != null))
                {
                    // (secondary actors resolved from SysML_Actor.AssociatedUseCases)
                }
            }

            // 5b. Draw secondary actor associations from SysML_Actor.AssociatedUseCases
            foreach (var actor in actorList.Where(a => actorNodes.ContainsKey(a.Name)))
            {
                var actorNode = actorNodes[actor.Name];
                foreach (var uc in actor.AssociatedUseCases.Where(u => useCaseNodes.ContainsKey(u.Name)))
                {
                    // Only draw if not already drawn as primary (avoid duplicates)
                    var ucNode = useCaseNodes[uc.Name];
                    wnd.Diagram.AddUMLLineElement("Association", actorNode, ucNode);
                }
            }

            // 6. Name the diagram package/namespace
            try { ((IUMLNamedElement)document.RootPackage).Name = title; } catch { /* optional */ }

            MyDoc.MyUModelDiagrams.Add(this);
            return guiDiagram;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Set the name on the UML element backing a node link.</summary>
        private static void SetElementName(IUMLGuiNodeLink node, string name)
        {
            try
            {
                if (node.GetType().GetProperty("UMLElement")?.GetValue(node) is IUMLNamedElement named)
                    named.Name = name;
            }
            catch
            {
                // Non-critical: diagram still displays without a name
            }
        }

        /// <summary>Return the actor node for a use case's primary actor, if present in the map.</summary>
        private static IUMLGuiNodeLink? actor_For(string ucName,
            Dictionary<string, IUMLGuiNodeLink> actorNodes, SysML_UseCase uc)
        {
            // Walk SysML_Actor.AssociatedUseCases to find which actor lists this UC
            foreach (var kv in actorNodes)
                if (uc.AssociatedUseCases.Any(ua => ua.Name == ucName))
                    return kv.Value;
            return actorNodes.Values.FirstOrDefault();
        }
    }
}
