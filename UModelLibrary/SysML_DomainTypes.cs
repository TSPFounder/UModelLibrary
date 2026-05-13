#nullable enable

using System;
using System.Collections.Generic;

namespace SysML
{
    public enum RequirementRelationshipType
    {
        DeriveReqt,
        Refine,
        Trace,
        Copy,
        Satisfy,
        Verify
    }

    public class SysML_RequirementRelationship
    {
        public SysML_RequirementRelationship(RequirementRelationshipType type, SysML_Requirement client, SysML_Requirement supplier)
        {
            Type = type;
            Client = client;
            Supplier = supplier;
        }

        public RequirementRelationshipType Type { get; set; }
        public SysML_Requirement Client { get; set; }
        public SysML_Requirement Supplier { get; set; }
    }

    public class SysML_Requirement
    {
        public string RequirementId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class SysML_Actor
    {
        public string Name { get; set; } = string.Empty;
        public List<SysML_UseCase> AssociatedUseCases { get; } = new();
    }

    public class SysML_UseCase
    {
        public string Name { get; set; } = string.Empty;
        public List<SysML_Actor> AssociatedUseCases { get; } = new();
    }

    public class SysML_Block
    {
        public string Name { get; set; } = string.Empty;
    }

    public class SysML_Line
    {
        public enum SysML_LineTypeEnum
        {
            Association,
            Generalization,
            Composition,
            Aggregation,
            Dependency,
            Realization
        }

        public SysML_Block? Source { get; set; }
        public SysML_Block? Target { get; set; }
        public SysML_LineTypeEnum LineType { get; set; }
    }

    public class SysML_Port
    {
        public string Name { get; set; } = string.Empty;
    }

    public class SysML_Part
    {
        public string Name { get; set; } = string.Empty;
        public SysML_Block BlockType { get; set; } = new();
        public List<SysML_Port> Ports { get; } = new();
    }

    public class SysML_Connector
    {
        public SysML_Port SourcePort { get; set; } = new();
        public SysML_Port TargetPort { get; set; } = new();
        public string? ItemFlowType { get; set; }
    }
}
