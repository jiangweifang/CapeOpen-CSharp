using System.Collections.Generic;
using Newtonsoft.Json;

namespace CapeOpen
{
    /// <summary>
    /// Represents a WAR (Waste Reduction Algorithm) data entry loaded from WARdata.json.
    /// </summary>
    class WARDataEntry
    {
        [JsonProperty("Mol_ID")]
        public string Mol_ID { get; set; }

        [JsonProperty("DIPPR_x0020_ID")]
        public string DIPPR_ID { get; set; }

        [JsonProperty("ASPENID")]
        public string ASPENID { get; set; }

        [JsonProperty("ChemicalName")]
        public string ChemicalName { get; set; }

        [JsonProperty("CAS")]
        public string CAS { get; set; }

        [JsonProperty("Formula")]
        public string Formula { get; set; }

        [JsonProperty("CLASS")]
        public string CLASS { get; set; }

        [JsonProperty("MW")]
        public string MW { get; set; }

        [JsonProperty("Rat_LD50_Value")]
        public string Rat_LD50_Value { get; set; }

        [JsonProperty("Rat_LD50_Notes")]
        public string Rat_LD50_Notes { get; set; }

        [JsonProperty("Rat_LD50_Source")]
        public string Rat_LD50_Source { get; set; }

        [JsonProperty("OSHA_TWA_Value")]
        public string OSHA_TWA_Value { get; set; }

        [JsonProperty("OSHA_TWA_Notes")]
        public string OSHA_TWA_Notes { get; set; }

        [JsonProperty("OSHA_TWA_Source")]
        public string OSHA_TWA_Source { get; set; }

        [JsonProperty("FHM_LC50_Value")]
        public string FHM_LC50_Value { get; set; }

        [JsonProperty("FHM_LC50_Notes")]
        public string FHM_LC50_Notes { get; set; }

        [JsonProperty("FHM_LC50_Source")]
        public string FHM_LC50_Source { get; set; }

        [JsonProperty("PCO_Value")]
        public string PCO_Value { get; set; }

        [JsonProperty("PCO_Source")]
        public string PCO_Source { get; set; }

        [JsonProperty("GWP_Value")]
        public string GWP_Value { get; set; }

        [JsonProperty("GWP_Source")]
        public string GWP_Source { get; set; }

        [JsonProperty("OD_Value")]
        public string OD_Value { get; set; }

        [JsonProperty("OD_Source")]
        public string OD_Source { get; set; }

        [JsonProperty("AP_Value")]
        public string AP_Value { get; set; }

        [JsonProperty("AP_Source")]
        public string AP_Source { get; set; }
    }
}
