using System;
using NLog;

namespace CapeOpen
{
    /// <summary>
    /// Default calculator for <see cref="MixerExample"/> that performs an adiabatic
    /// mixing calculation (material + energy balance). This calculator can be replaced
    /// at runtime with any other <see cref="CapeCalculator"/> implementation, allowing
    /// users to choose a different mixing strategy.
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public class MixerCalculator : CapeCalculator
    {
        private String[] in1Comps;
        private double[] in1Flow;
        private double[] in1Enthalpy;
        private double[] in2Flow;
        private double[] in2Enthalpy;
        private double[] pressure;
        private double totalFlow1;
        private double totalFlow2;

        private ICapeThermoMaterialObject in1;
        private ICapeThermoMaterialObject in2;

        /// <inheritdoc/>
        public override void BeforeCalculate()
        {
            if (UnitOperation.SimulationContext != null)
                ((ICapeDiagnostic)UnitOperation.SimulationContext).LogMessage("Starting Mixer Calculation");

            String[] phases = { "Overall" };
            String[] props = { "enthalpy" };

            // --- Inlet 1 ---
            ICapeThermoMaterialObject tempMO;
            try
            {
                tempMO = (ICapeThermoMaterialObject)UnitOperation.Ports[0].connectedObject;
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                throw new CapeInvalidOperationException("Material object does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
            }
            try
            {
                in1 = (ICapeThermoMaterialObject)tempMO.Duplicate();
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                throw new CapeInvalidOperationException("Object connected to Inlet Port 1 does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
            }

            try
            {
                in1Comps = (String[])in1.ComponentIds;
                in1Flow = (double[])in1.GetProp("flow", "Overall", null, null, "mole");
                pressure = (double[])in1.GetProp("Pressure", "Overall", null, "Mixture", null);
                totalFlow1 = 0;
                for (int i = 0; i < in1Flow.Length; i++)
                    totalFlow1 += in1Flow[i];
                in1.CalcProp(props, phases, "Mixture");
                in1Enthalpy = (double[])in1.GetProp("enthalpy", "Overall", null, "Mixture", "mole");
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                if (p_Ex is System.Runtime.InteropServices.COMException)
                    p_Ex = COMExceptionHandler.ExceptionForHRESULT(in1, p_Ex);
                UnitOperation.throwException(p_Ex);
            }
            (in1 as IDisposable)?.Dispose();

            // --- Inlet 2 ---
            try
            {
                tempMO = (ICapeThermoMaterialObject)UnitOperation.Ports[1].connectedObject;
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                throw new CapeInvalidOperationException("Material object does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
            }
            try
            {
                in2 = (ICapeThermoMaterialObject)tempMO.Duplicate();
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                throw new CapeInvalidOperationException("Object connected to Inlet Port 2 does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
            }

            try
            {
                in2Flow = in2.GetProp("flow", "Overall", null, null, "mole");
                double[] press = in2.GetProp("Pressure", "Overall", null, "Mixture", null);
                if (press[0] < pressure[0]) pressure[0] = press[0];
                totalFlow2 = 0;
                for (int i = 0; i < in2Flow.Length; i++)
                    totalFlow2 += in2Flow[i];
                in2.CalcProp(props, phases, "Mixture");
                in2Enthalpy = in2.GetProp("enthalpy", "Overall", null, "Mixture", "mole");
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                if (p_Ex is System.Runtime.InteropServices.COMException)
                    p_Ex = COMExceptionHandler.ExceptionForHRESULT(in2, p_Ex);
                UnitOperation.throwException(p_Ex);
            }
            (in2 as IDisposable)?.Dispose();
        }

        /// <inheritdoc/>
        public override void Calculate()
        {
            // Pure mixing: no additional calculation needed beyond what BeforeCalculate gathered.
            // Subclasses could override to add reaction kinetics, heat transfer, etc.
        }

        /// <inheritdoc/>
        public override void OutputResult()
        {
            ICapeThermoMaterialObject outPort = (ICapeThermoMaterialObject)UnitOperation.Ports[2].connectedObject;
            double[] values = new double[1];

            // Energy balance: outlet enthalpy
            values[0] = (in1Enthalpy[0] * totalFlow1 + in2Enthalpy[0] * totalFlow2) / (totalFlow1 + totalFlow2);
            try
            {
                outPort.SetProp("enthalpy", "Overall", null, "Mixture", "mole", values);

                // Pressure = min(inlet pressures) - pressure drop
                pressure[0] = pressure[0] - ((RealParameter)UnitOperation.Parameters[0]).SIValue;
                outPort.SetProp("Pressure", "Overall", null, null, null, pressure);

                // Component flows
                values = new double[in1Comps.Length];
                for (int i = 0; i < in1Comps.Length; i++)
                    values[i] = in1Flow[i] + in2Flow[i];
                outPort.SetProp("flow", "Overall", in1Comps, null, "mole", values);

                // Flash
                outPort.CalcEquilibrium("PH", null);

                if (outPort.GetType().IsCOMObject)
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(outPort);
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerCalculator: {Message}", p_Ex.Message);
                if (p_Ex is System.Runtime.InteropServices.COMException)
                    p_Ex = COMExceptionHandler.ExceptionForHRESULT(outPort, p_Ex);
                UnitOperation.throwException(p_Ex);
            }

            if (UnitOperation.SimulationContext != null)
                ((ICapeDiagnostic)UnitOperation.SimulationContext).LogMessage("Ending Mixer Calculation");
        }
    }
}
