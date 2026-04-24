using System;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CapeOpen
{
    /// <summary>
    /// Summary for WAR
    /// </summary>
    [Serializable()]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("0BE9CCFD-29B4-4a42-B34E-76F5FE9B6BB4")]
    [CapeOpen.CapeName("WAR Add-In")]
    [CapeOpen.CapeAbout("Waste Reduction Algorithm Add-in")]
    [CapeOpen.CapeDescription("Waste Reduction Algorithm Add-in")]
    [CapeOpen.CapeVersion("1.0")]
    [CapeOpen.CapeVendorURL("http://www.epa.gov/nrmrl/std/sab/war/sim_war.htm")]
    [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)]
    [CapeOpen.CapeFlowsheetMonitoringAttribute(true)]
    public class WARAddIn : CapeObjectBase
    {

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <remarks>
        /// <para>
        /// Clone can be implemented either as a deep copy or a shallow copy. In a deep copy, all objects are duplicated; 
        /// in a shallow copy, only the top-level objects are duplicated and the lower levels contain references.
        /// </para>
        /// <para>
        /// The resulting clone must be of the same type as, or compatible with, the original instance.
        /// </para>
        /// <para>
        /// See <see cref="Object.MemberwiseClone"/> for more information on cloning, deep versus shallow copies, and examples.
        /// </para>
        /// </remarks>
        /// <returns>A new object that is a copy of this instance.</returns>
        override public object Clone()
        {
            return new WARAddIn();
        }

        private System.Data.DataTable warDataTable = new System.Data.DataTable();
        private void AddData(String jsonData)
        {
            var entries = JsonConvert.DeserializeObject<List<WARDataEntry>>(jsonData);
            foreach (var entry in entries)
            {
                System.Data.DataRow dataRow = warDataTable.NewRow();
                warDataTable.Rows.Add(dataRow);

                if (!string.IsNullOrWhiteSpace(entry.Mol_ID))
                    dataRow["Mol Id"] = entry.Mol_ID.Trim();
                if (!string.IsNullOrWhiteSpace(entry.DIPPR_ID))
                    dataRow["DIPPR ID"] = entry.DIPPR_ID.Trim();
                if (!string.IsNullOrWhiteSpace(entry.ASPENID))
                    dataRow["ASPENID"] = entry.ASPENID.Trim();
                if (!string.IsNullOrWhiteSpace(entry.ChemicalName))
                    dataRow["ChemicalName"] = entry.ChemicalName.Trim();
                if (!string.IsNullOrWhiteSpace(entry.CAS))
                    dataRow["CAS"] = entry.CAS.Trim();
                if (!string.IsNullOrWhiteSpace(entry.Formula))
                    dataRow["Formula"] = entry.Formula.Trim();
                if (!string.IsNullOrWhiteSpace(entry.CLASS))
                    dataRow["class"] = entry.CLASS.Trim();
                if (!string.IsNullOrWhiteSpace(entry.MW))
                    dataRow["molecularWeight"] = Convert.ToDouble(entry.MW);
                if (!string.IsNullOrWhiteSpace(entry.Rat_LD50_Value))
                    dataRow["Rat LD50"] = Convert.ToDouble(entry.Rat_LD50_Value);
                if (!string.IsNullOrWhiteSpace(entry.Rat_LD50_Notes))
                    dataRow["Rat LD50 Notes"] = entry.Rat_LD50_Notes.Trim();
                if (!string.IsNullOrWhiteSpace(entry.Rat_LD50_Source))
                    dataRow["Rat LD50 Source"] = entry.Rat_LD50_Source.Trim();
                if (!string.IsNullOrWhiteSpace(entry.OSHA_TWA_Value))
                    dataRow["OSHA PEL"] = Convert.ToDouble(entry.OSHA_TWA_Value);
                if (!string.IsNullOrWhiteSpace(entry.OSHA_TWA_Source))
                    dataRow["OSHA Source"] = entry.OSHA_TWA_Source.Trim();
                if (!string.IsNullOrWhiteSpace(entry.OSHA_TWA_Notes))
                    dataRow["OSHA Notes"] = entry.OSHA_TWA_Notes.Trim();
                if (!string.IsNullOrWhiteSpace(entry.FHM_LC50_Value))
                    dataRow["Fathead LC50"] = Convert.ToDouble(entry.FHM_LC50_Value);
                if (!string.IsNullOrWhiteSpace(entry.FHM_LC50_Notes))
                    dataRow["Fathead LC50 Notes"] = entry.FHM_LC50_Notes.Trim();
                if (!string.IsNullOrWhiteSpace(entry.FHM_LC50_Source))
                    dataRow["Fathead LC50 Source"] = entry.FHM_LC50_Source.Trim();
                if (!string.IsNullOrWhiteSpace(entry.PCO_Value))
                    dataRow["Photochemical Oxidation Potential"] = Convert.ToDouble(entry.PCO_Value);
                if (!string.IsNullOrWhiteSpace(entry.PCO_Source))
                    dataRow["Photochemical Oxidation Potential Source"] = entry.PCO_Source.Trim();
                if (!string.IsNullOrWhiteSpace(entry.GWP_Value))
                    dataRow["Global Warming Potential"] = Convert.ToDouble(entry.GWP_Value);
                if (!string.IsNullOrWhiteSpace(entry.GWP_Source))
                    dataRow["Global Warming Potential Source"] = entry.GWP_Source.Trim();
                if (!string.IsNullOrWhiteSpace(entry.OD_Value))
                    dataRow["Ozone Depletion Potential"] = Convert.ToDouble(entry.OD_Value);
                if (!string.IsNullOrWhiteSpace(entry.OD_Source))
                    dataRow["Ozone Depletion Potential Source"] = entry.OD_Source.Trim();
                if (!string.IsNullOrWhiteSpace(entry.AP_Value))
                    dataRow["Acidification Potential"] = Convert.ToDouble(entry.AP_Value);
                if (!string.IsNullOrWhiteSpace(entry.AP_Source))
                    dataRow["Acidification Potential Source"] = entry.AP_Source.Trim();
            }
        }

        /// <summary>
        ///	Displays the PMC graphic interface, if available.
        /// </summary>
        /// <remarks>
        /// The PMC displays its user interface and allows the Flowsheet User to 
        /// interact with it. If no user interface is available it returns an error.
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        public WARAddIn()
        {
            warDataTable = new System.Data.DataTable();
            warDataTable.Columns.Add("Mol Id", typeof(System.String));
            warDataTable.Columns.Add("DIPPR ID", typeof(System.String));
            warDataTable.Columns.Add("ASPENID", typeof(System.String));
            warDataTable.Columns.Add("ChemicalName", typeof(System.String));
            warDataTable.Columns.Add("CAS", typeof(System.String));
            warDataTable.Columns.Add("Formula", typeof(System.String));
            warDataTable.Columns.Add("class", typeof(System.String));
            warDataTable.Columns.Add("molecularWeight", typeof(double));
            warDataTable.Columns.Add("Rat LD50", typeof(double));
            warDataTable.Columns.Add("Rat LD50 Notes", typeof(System.String));
            warDataTable.Columns.Add("Rat LD50 Source", typeof(System.String));
            warDataTable.Columns.Add("OSHA PEL", typeof(double));
            warDataTable.Columns.Add("OSHA Notes", typeof(System.String));
            warDataTable.Columns.Add("OSHA Source", typeof(System.String));
            warDataTable.Columns.Add("Fathead LC50", typeof(double));
            warDataTable.Columns.Add("Fathead LC50 Notes", typeof(System.String));
            warDataTable.Columns.Add("Fathead LC50 Source", typeof(System.String));
            warDataTable.Columns.Add("Global Warming Potential", typeof(double));
            warDataTable.Columns.Add("Global Warming Potential Source", typeof(System.String));
            warDataTable.Columns.Add("Ozone Depletion Potential", typeof(double));
            warDataTable.Columns.Add("Ozone Depletion Potential Source", typeof(System.String));
            warDataTable.Columns.Add("Photochemical Oxidation Potential", typeof(double));
            warDataTable.Columns.Add("Photochemical Oxidation Potential Source", typeof(System.String));
            warDataTable.Columns.Add("Acidification Potential", typeof(double));
            warDataTable.Columns.Add("Acidification Potential Source", typeof(System.String));
            System.AppDomain domain = System.AppDomain.CurrentDomain;
            //System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            //String[] resources = myAssembly.GetManifestResourceNames();
            //System.IO.Stream resStream = myAssembly.GetManifestResourceStream("CapeOpen.Resources.WARdata.xml.resources");
            //System.Resources.ResourceReader resReader = new System.Resources.ResourceReader(resStream);
            //System.Collections.IDictionaryEnumerator en = resReader.GetEnumerator();
            //String temp = String.Empty;
            //while (en.MoveNext())
            //{
            //    if (en.Key.ToString() == "WARdata") temp = en.Value.ToString();
            //}
            this.AddData(Properties.Resources.WARdata);
        }

        /// <summary>
        ///	Displays the PMC graphic interface, if available.
        /// </summary>
        /// <remarks>
        /// The PMC displays its user interface and allows the Flowsheet User to 
        /// interact with it. If no user interface is available it returns an error.
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        public override System.Windows.Forms.DialogResult Edit()
        {
            try
            {
                CapeOpen.WARalgorithm war = new WARalgorithm(this.warDataTable, this.FlowsheetMonitoring);
                System.Windows.Forms.DialogResult result = war.ShowDialog();
                war.Dispose();
                return result;
             }
            catch (System.Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "War: {Message}", p_Ex.Message);
                this.throwException(new CapeOpen.CapeUnknownException(p_Ex.Message, p_Ex));
                return System.Windows.Forms.DialogResult.Cancel;
            }
        }
    }
}
