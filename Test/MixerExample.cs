using CapeOpen;
using System;
using NLog;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Test
{
    [Serializable]
    [ComVisible(true)]
    [Guid("883D46FE-5713-424C-BF10-7ED34263CD6D")] // ICapeThermoMaterialObject_IID
    [Description("测试混合器模块。")]
    [CapeName("MyMixerTest")]
    [CapeDescription("一个用 C# 开发的测试 CO 单元模块。")]
    [CapeVersion("1.1")]
    [CapeVendorURL("https:\\www.epa.gov")]
    [CapeHelpURL("https:\\www.imbhj.com")]
    [CapeAbout("大白萝卜嘻嘻。")]
    [CapeConsumesThermo(true)]
    [CapeUnitOperation(true)]
    [CapeSupportsThermodynamics10(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class MixerExample : CapeUnitBase
    {
        /// <summary>
        /// Creates an instance of the CMixerExample unit operation.
        /// </summary>
        /// <remarks>
        /// This constructor demonstates the addition of a <see cref = "BooleanParameter"/>,
        /// <see cref = "IntegerParameter"/>, <see cref = "OptionParameter"/>,
        /// and a <see cref = "RealParameter"/> parameter to the parameter collection.
        /// In addition, the mixer unit has three <see cref = "UnitPort"/> ports
        /// added to the port collection. See the documentation for the 
        /// <see cref = "MixerExample.Calculate"/> method for details on its implementation.
        /// </remarks>
        /// <example>
        /// An example of how to create a unit operation. Parameter and port objects are created 
        /// and added the their respective collections. Ports are implemented by the <see cref="UnitPort"/> 
        /// class and are placed in the Port Collection. Parameters are added to the Parameter Collection 
        /// class. The available parameter classes are <see cref="RealParameter"/>, <see cref="IntegerParameter"/>, 
        /// <see cref="BooleanParameter"/>, and <see cref="OptionParameter"/>.
        /// <code>
		/// public MixerExample()
		/// {
		///     // Add Ports using the UnitPort constructor.
		///     this.Ports.Add(new UnitPort("Inlet Port1", "Test Inlet Port1", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL));
		///     this.Ports.Add(new UnitPort("Inlet Port2", "Test Inlet Port2", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL));
		///     this.Ports.Add(new UnitPort("Outlet Port", "Test Outlet Port", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL));
		/// 
		///     // Add a real valued parameter using the RealParameter  constructor.
		///     RealParameter real = new RealParameter("PressureDrop", "Drop in pressure between the outlet from the mixer and the pressure of the lower pressure inlet.", 0.0, 0.0, 0.0, 100000000.0, CapeParamMode.CAPE_INPUT, "Pa");
		///     this.Parameters.Add(real);
		/// 
		///     // Add a real valued parameter using the IntegerParameter  constructor.
		///     this.Parameters.Add(new IntegerParameter("Integer Parameter", "This is an example of an integer parameter.", 12, 12, 0, 100, CapeParamMode.CAPE_INPUT_OUTPUT));
		/// 
		///     // Add a real valued parameter using the BooleanParameter  constructor.
		///     this.Parameters.Add(new BooleanParameter("Boolean Parameter", "This is an example of a boolean parameter.", false, false, CapeOpen.CapeParamMode.CAPE_INPUT_OUTPUT));
		/// 
		///     // Create an array of strings for the option parameter restricted value list.
		///     String[] options = { "Test Value", "Another Value" };
		/// 
		///     // Add a string valued parameter using the OptionParameter constructor.
		///     this.Parameters.Add(new OptionParameter("OptionParameter", "This is an example of an option parameter.", "Test Value", "Test Value", options, true, CapeParamMode.CAPE_INPUT_OUTPUT));
		/// 
		///     // Add an available report.
		///     this.Reports.Add("Report 2");
		/// }
		/// </code>
        /// </example>
        public MixerExample()
        {
            // Add Ports using the UnitPort constructor.
            Ports.Add(new UnitPort("输入端口1", "输入端口1", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL));
            Ports.Add(new UnitPort("输入端口2", "输入端口2", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL));
            Ports.Add(new UnitPort("输出端口", "输出端口", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL));

            // Add a real valued parameter using the RealParameter  constructor.
            RealParameter real = new RealParameter("PressureDrop", "Drop in pressure between the outlet from the mixer and the pressure of the lower pressure inlet.", 0.0, 0.0, 0.0, 100000000.0, CapeParamMode.CAPE_INPUT, "Pa");
            Parameters.Add(real);

            // Add a real valued parameter using the IntegerParameter  constructor.
            Parameters.Add(new IntegerParameter("Integer Parameter", "This is an example of an integer parameter.", 12, 12, 0, 100, CapeParamMode.CAPE_INPUT_OUTPUT));

            // Add a real valued parameter using the BooleanParameter  constructor.
            Parameters.Add(new BooleanParameter("Boolean Parameter", "This is an example of a boolean parameter.", false, false, CapeParamMode.CAPE_INPUT_OUTPUT));

            // Create an array of strings for the option parameter restricted value list.
            String[] options = { "Test Value", "Another Value" };

            // Add a string valued parameter using the OptionParameter constructor.
            Parameters.Add(new OptionParameter("OptionParameter", "This is an example of an option parameter.", "Test Value", "Test Value", options, true, CapeParamMode.CAPE_INPUT_OUTPUT));

            // Add an array valued parameter with RealParameter element specifications.
            // Each element is validated against the RealParameter spec (range 0.0 ~ 1000.0, unit "Pa").
            RealParameter elemSpec = new RealParameter("ElemSpec", "Element specification", 0.0, 0.0, 0.0, 1000.0, CapeParamMode.CAPE_INPUT, "Pa");
            object[] arrayValues = { 100.0, 200.0, 300.0 };
            object[] arrayDefaults = { 100.0, 200.0, 300.0 };
            object[] itemSpecs = { elemSpec, elemSpec, elemSpec };
            Parameters.Add(new ArrayParameter("ArrayParameter", "This is an example of an array parameter with RealParameter element specs.", arrayValues, arrayDefaults, new int[] { 3 }, itemSpecs, CapeParamMode.CAPE_INPUT_OUTPUT));

            // Add an available report.
            Reports.Add("Report 2");
        }

        public override void Initialize()
        {
            base.Initialize();
        }


        protected override void Calculate()
        {
            // Log a message using the simulation context (pop-up message commented out.
            if (SimulationContext != null)
                ((ICapeDiagnostic)SimulationContext).LogMessage("Starting Mixer Calculation");
            //(CapeOpen.ICapeDiagnostic>(this.SimulationContext).PopUpMessage("Starting Mixer Calculation");

            // Get the material Object from Port 0.
            ICapeThermoMaterialObject in1 = null;
            ICapeThermoMaterialObject tempMO = null;
            try
            {
                tempMO = (ICapeThermoMaterialObject)Ports[0].connectedObject;
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                OnUnitOperationEndCalculation("Error - Material object does not support CAPE-OPEN Thermodynamics 1.0.");
                CapeInvalidOperationException ex = new CapeInvalidOperationException("Material object does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
                throwException(ex);
            }

            // Duplicate the port, its an input port, always use a duplicate.
            try
            {
                in1 = tempMO.Duplicate();
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                OnUnitOperationEndCalculation("Error - Object connected to Inlet Port 1 does not support CAPE-OPEN Thermodynamics 1.0.");
                CapeInvalidOperationException ex = new CapeInvalidOperationException("Object connected to Inlet Port 1 does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
                throwException(ex);
            }
            // Arrays for the GetProps and SetProps call for enthaply.
            String[] phases = { "Overall" };
            String[] props = { "enthalpy" };

            // Declare variables for calculations.
            String[] in1Comps = null;
            double[] in1Flow = null;
            double[] in1Enthalpy = null;
            double[] pressure = null;
            double totalFlow1 = 0;

            // Exception catching code...
            try
            {
                // Get Strings, must cast to string array data type.
                in1Comps = in1.ComponentIds;

                // Get flow. Arguments are the property; phase, in this case, Overall; compound identifications
                // in this case, the null returns the property for all components; calculation type, in this case,  
                // null, no calculation type; and lastly, the basis, moles. Result is cast to a double array, and will contain one value.
                in1Flow = in1.GetProp("flow", "Overall", null, null, "mole");

                // Get pressure. Arguments are the property; phase, in this case, Overall; compound identifications
                // in this case, the null returns the property for all components; calculation type, in this case, the 
                // mixture; and lastly, the basis, moles. Result is cast to a double array, and will contain one value.
                pressure = in1.GetProp("Pressure", "Overall", null, "Mixture", null);

                // The following code adds the individual flows to get the total flow for the stream.
                for (int i = 0; i < in1Flow.Length; i++)
                {
                    totalFlow1 = totalFlow1 + in1Flow[i];
                }
                // Calculates the mixture enthalpy of the stream.
                in1.CalcProp(props, phases, "Mixture");

                // Get the enthalpy of the stream. Arguments are the property, enthalpy; the phase, overall;
                // a null pointer, required as the overall enthalpy is desired; the calculation type is
                // mixture; and the basis is moles.
                in1Enthalpy = in1.GetProp("enthalpy", "Overall", null, "Mixture", "mole");
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                // Exception handling, wraps a COM exception, shows the message, and re-throws the excecption.

                if (p_Ex is COMException)
                {
                    COMException comException = (COMException)p_Ex;
                    p_Ex = COMExceptionHandler.ExceptionForHRESULT(in1, p_Ex);
                }
                throwException(p_Ex);
            }
            IDisposable disp = in1 as IDisposable;
            if (disp != null)
            {
                disp.Dispose();
            }


            // Get the material Object from Port 0.
            ICapeThermoMaterialObject in2 = null;
            tempMO = null;
            try
            {
                tempMO = (ICapeThermoMaterialObject)Ports[1].connectedObject;
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                OnUnitOperationEndCalculation("Error - Material object does not support CAPE-OPEN Thermodynamics 1.0.");
                CapeInvalidOperationException ex = new CapeInvalidOperationException("Material object does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
                throwException(ex);
            }

            // Duplicate the port, its an input port, always use a duplicate.
            try
            {
                in2 = tempMO.Duplicate();
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                OnUnitOperationEndCalculation("Error - Object connected to Inlet Port 1 does not support CAPE-OPEN Thermodynamics 1.0.");
                CapeInvalidOperationException ex = new CapeInvalidOperationException("Object connected to Inlet Port 1 does not support CAPE-OPEN Thermodynamics 1.0.", p_Ex);
                throwException(ex);
            }

            // Declare variables.
            String[] in2Comps = null;
            double[] in2Flow = null;
            double[] in2Enthalpy = null;
            double totalFlow2 = 0;

            // Try block.
            try
            {
                // Get the component identifications.
                in2Comps = in2.ComponentIds;

                // Get flow. Arguments are the property; phase, in this case, Overall; compound identifications
                // in this case, the null returns the property for all components; calculation type, in this case,  
                // null, no calculation type; and lastly, the basis, moles. Result is cast to a double array, and will contain one value.
                in2Flow = in2.GetProp("flow", "Overall", null, null, "mole");

                // Get pressure. Arguments are the property; phase, in this case, Overall; compound identifications
                // in this case, the null returns the property for all components; calculation type, in this case, the 
                // mixture; and lastly, the basis, moles. Result is cast to a double array, and will contain one value.
                double[] press = in2.GetProp("Pressure", "Overall", null, "Mixture", null);
                if (press[0] < pressure[0]) pressure[0] = press[0];

                // The following code adds the individual flows to get the total flow for the stream.
                for (int i = 0; i < in2Flow.Length; i++)
                {
                    totalFlow2 = totalFlow2 + in2Flow[i];
                }

                // Calculates the mixture enthalpy of the stream.
                in2.CalcProp(props, phases, "Mixture");

                // Get the enthalpy of the stream. Arguments are the property, enthalpy; the phase, overall;
                // a null pointer, required as the overall enthalpy is desired; the calculation type is
                // mixture; and the basis is moles.
                in2Enthalpy = in2.GetProp("enthalpy", "Overall", null, "Mixture", "mole");
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                COMException comException = (COMException)p_Ex;
                if (comException != null)
                {
                    p_Ex = COMExceptionHandler.ExceptionForHRESULT(in2, p_Ex);
                }
                throwException(p_Ex);
            }
            // Release the material object if it is a COM object.
            disp = in2 as IDisposable;
            if (disp != null)
            {
                disp.Dispose();
            }


            // Get the outlet material object.
            ICapeThermoMaterialObject outPort = (ICapeThermoMaterialObject)Ports[2].connectedObject;

            // An empty, one-member array to set values in the outlet material stream.
            double[] values = new double[1];

            // Use energy balanace to calculate the outlet enthalpy.
            values[0] = (in1Enthalpy[0] * totalFlow1 + in2Enthalpy[0] * totalFlow2) / (totalFlow1 + totalFlow2);
            try
            {
                // Set the outlet enthalpy, for the overall phase, with a mixture calculation type
                // to the value calculated above.
                outPort.SetProp("enthalpy", "Overall", null, "Mixture", "mole", values);

                // Set the outlet pressure to the lower of the to inlet pressures less the value of the 
                // pressure drop parameter.
                pressure[0] = pressure[0] - (((RealParameter)(Parameters[0])).SIValue);

                // Set the outlet pressure.
                outPort.SetProp("Pressure", "Overall", null, null, null, pressure);

                // Resize the value array for the number of components.
                values = new double[in1Comps.Length];

                // Calculate the individual flow for each component.
                for (int i = 0; i < in1Comps.Length; i++)
                {
                    values[i] = in1Flow[i] + in2Flow[i];
                }
                // Set the outlet flow by component. Note, this is for overall phase and mole flows.
                // The component Identifications are used as a check.
                outPort.SetProp("flow", "Overall", in1Comps, null, "mole", values);

                // Calculate equilibrium using a "pressure-enthalpy" flash type.
                outPort.CalcEquilibrium("PH", null);

                // Release the material object if it is a COM object.
                if (outPort.GetType().IsCOMObject)
                {
                    Marshal.FinalReleaseComObject(outPort);
                }
            }
            catch (Exception p_Ex)
            {
                CrashLogger.Logger.Error(p_Ex, "MixerExample: {Message}", p_Ex.Message);
                COMException comException = (COMException)p_Ex;
                if (comException != null)
                {
                    p_Ex = COMExceptionHandler.ExceptionForHRESULT(outPort, p_Ex);
                }
                throwException(p_Ex);
            }

            // Log the end of the calculation.
            if (SimulationContext != null)
                ((ICapeDiagnostic)SimulationContext).LogMessage("Ending Mixer Calculation");
            //(CapeOpen.ICapeDiagnostic>(this.SimulationContext).PopUpMessage("Ending Mixer Calculation");
        }

        /// <summary>
        ///	Produces the active report for the Mixer Example unit operation.
        /// </summary>
        /// <remarks>
        /// The ProduceReport method creates the active report for the unit operation. The method looks to the 
        /// <see cref="CapeUnitBase.selectedReport"/> and generates the required report. If a local report has
        /// ben added likne in <see cref="MixerExample.MixerExample"/>, this method must generate that report.
        /// </remarks>
        /// <example>
        /// An example of how to produce a report for a unit operation. In this case, the report can be either 
        /// "Report 2" defined in the <see cref="MixerExample.MixerExample"/> or the "Default Report" from 
        /// <see cref="CapeUnitBase"/>. If "Default Report" is selected, then the <see cref="CapeUnitBase.ProduceReport"/>
        /// method is called, and the message parameter forwarded. Otherwise, the report is generated in this method.
        /// "Default Report" gen.
        /// <code>
        /// public override void ProduceReport(ref String message)
        /// {
        ///     if (this.selectedReport == "Default Report") base.ProduceReport(ref message);
        ///     if (this.selectedReport == "Report 2") message = "This is the alternative Report.";
        /// }
        /// </code>
        /// </example>
        /// <returns>The report text.</returns>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeNoImpl">ECapeNoImpl</exception>
        public override string ProduceReport()
        {
            if (selectedReport == "Default Report") return base.ProduceReport();
            if (selectedReport == "Report 2") return "This is the alternative Report.";
            return string.Empty;
        }
    }
}