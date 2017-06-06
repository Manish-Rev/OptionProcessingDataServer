using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataServer
{
    public class ApnSettings
    {
        public String CertPathIOS { get; set; }
        public String CertPasswordIOS { get; set; }
        public Boolean IsProductionIOS { get; set; }
        public String CertPathOSX { get; set; }
        public String CertPasswordOSX { get; set; }
        public Boolean IsProductionOSX { get; set; }
    }
}
