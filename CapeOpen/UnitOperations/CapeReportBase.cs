using System;

namespace CapeOpen
{
    /// <summary>
    /// Abstract base class for unit operation reports.
    /// Inherit from this class to provide custom reports that can be
    /// registered with a <see cref="CapeUnitBase"/> via <see cref="CapeUnitBase.AddReport"/>.
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public abstract class CapeReportBase
    {
        /// <summary>
        /// Gets the name of this report. This value is used as the report identifier
        /// in the <see cref="ICapeUnitReport"/> report list.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Reference to the owning unit operation, set automatically when
        /// the report is registered via <see cref="CapeUnitBase.AddReport"/>.
        /// </summary>
        protected CapeUnitBase UnitOperation { get; private set; }

        /// <summary>
        /// Assigns the owning unit operation. Called internally by the framework.
        /// </summary>
        internal void SetUnitOperation(CapeUnitBase unitOperation)
        {
            UnitOperation = unitOperation;
        }

        /// <summary>
        /// Produces the report text.
        /// </summary>
        /// <returns>A string containing the full report content.</returns>
        public abstract string ProduceReport();
    }

    /// <summary>
    /// Built-in report that outputs the current validation status of the unit operation.
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    class StatusReport : CapeReportBase
    {
        public override string Name => "Status";

        public override string ProduceReport()
        {
            string message = null;
            UnitOperation.Validate(ref message);
            if (string.IsNullOrWhiteSpace(message))
                message = "Complete.";
            return message;
        }
    }

    /// <summary>
    /// Built-in report that summarizes the last run: port counts, directions,
    /// and result parameter values.
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    class LastRunReport : CapeReportBase
    {
        public override string Name => "Last Run";

        public override string ProduceReport()
        {
            string report = string.Empty;

            int inlet = 0, outlet = 0, inoutlet = 0;
            for (int i = 0; i < UnitOperation.Ports.Count; i++)
            {
                ICapeUnitPort port = (ICapeUnitPort)UnitOperation.Ports[i];
                switch (port.direction)
                {
                    case CapePortDirection.CAPE_INLET:
                        inlet++;
                        break;
                    case CapePortDirection.CAPE_OUTLET:
                        outlet++;
                        break;
                    case CapePortDirection.CAPE_INLET_OUTLET:
                        inoutlet++;
                        break;
                }
            }

            report += $"Number of Inlet Ports:       {inlet}{Environment.NewLine}";
            report += $"Number of Outlet Ports:      {outlet}{Environment.NewLine}";
            report += $"Number of In & Out Ports:    {inoutlet}{Environment.NewLine}{Environment.NewLine}";

            for (int i = 0; i < UnitOperation.Parameters.Count; i++)
            {
                ICapeParameter param = (ICapeParameter)UnitOperation.Parameters[i];
                CapeIdentification paramId = (CapeIdentification)param;
                ICapeParameterSpec spec = (ICapeParameterSpec)param.Specification;
                string unit = string.Empty;
                if (spec.Type == CapeParamType.CAPE_REAL && param is RealParameter realParam)
                {
                    unit = " " + realParam.Unit;
                }
                report += $"{paramId.ComponentName}:\t{param.value}{unit}{Environment.NewLine}";
            }

            return report;
        }
    }
}
