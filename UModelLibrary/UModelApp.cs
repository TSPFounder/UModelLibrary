using System;
using System.Collections.Generic;
using UModelLib;
using CAD;
using Applications;

namespace SysML
{
    public class UModelApp //: ApplicationClass
    {
        public UModelApp(UModelLib.Application uModelApp)
        {
            CurrentApplication = uModelApp;
        }

        public string Name => "UModel";

        public UModelLib.Application? CurrentApplication { get; set; }

        public UModelDocument? CurrentDocument { get; set; }

        public bool CreateUModelApp()
        {
            try
            {
                CurrentApplication = new UModelLib.Application
                {
                    Visible = true
                };

                return true;
            }

            catch
            {
                return false;
            }
        }
    }
}