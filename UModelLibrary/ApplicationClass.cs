using System;
using System.Collections.Generic;
using UModelLib;
using CAD;
using SystemsEngineering;

namespace SystemsEngineering
{
    public class ApplicationClass
    {
        public ApplicationClass()
        {
        }

        public string AppName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string CurrentExtensionType { get; set; } = string.Empty;

        public List<string> MyExtensions { get; set; } = new();

        //public List<ApplicationProgrammingInterface> APIs { get; set; } = new();

        //public SoftwareProgram? MySoftware { get; set; }

        public List<AppFile> AppFiles { get; set; } = new();

        public string HomePage { get; set; } = string.Empty;

        public bool IsInstalled { get; set; }
    }
}
