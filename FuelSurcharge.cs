using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Get_Weekly_fuelSurcharge
{
    public class FuelSurcharge
    {
        public string EffectiveStartDate { get; set; }
        public string DomesticGroundSurcharge { get; set; }
        public string DomesticAirSurcharge { get; set; }
        public string InternationalAirExportSurcharge { get; set; }
        public string InternationalAirImportSurcharge { get; set; }
        public string InternationalGroundExportImportSurcharge { get; set; }
    }
}
