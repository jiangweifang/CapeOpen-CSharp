using System;
using Newtonsoft.Json;

namespace CapeOpen
{

    struct UnitCategoryInfo
    {
        /// <summary>
        /// Gets the name of the unit category, e.g. pressure, tempurature.
        /// </summary>
        /// <remarks>
        /// <para>The unit category repsresents the unique combination of dimensions (mass, length, 
        /// time, temperature, amount of substance (moles), electrical current, luminosity) associated with a particular
        /// unit of measure.
        /// </para>
        /// </remarks>
        [JsonProperty("Category")]
        public String Name;
        /// <summary>
        /// Gets the display unit for the parameter. Used by AspenPlus(TM).
        /// </summary>
        /// <remarks>
        /// <para>DisplayUnits defines the unit of measurement symbol for a parameter.</para>
        /// <para>Note: The symbol must be one of the uppercase strings recognized by Aspen
        /// Plus to ensure that it can perform unit of measurement conversions on the 
        /// parameter value. The system converts the parameter's value from SI units for
        /// display in the data browser and converts updated values back into SI.
        /// </para>
        /// </remarks>
        [JsonProperty("Aspen")]
        public String AspenUnit;
        /// <summary>
        /// Gets the name of the SI unit associated with the unit category, e.g. Pascals for pressure.
        /// </summary>
        /// <remarks>
        /// <para>The SI unit is the basis for conversions between any two units of the same category, either SI or 
        /// customary.
        /// </para>
        /// </remarks>
        [JsonProperty("SI_Unit")]
        public String SI_Unit;
        /// <summary>
        /// Gets the mass dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The mass dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("Mass")]
        public double Mass;
        /// <summary>
        /// Gets the time dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The time dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("Time")]
        public double Time;
        /// <summary>
        /// Gets the length dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The length dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("Length")]
        public double Length;
        /// <summary>
        /// Gets the electrical current (amperage) dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The electrical current (amperage) dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("ElectricalCurrent")]
        public double ElectricalCurrent;
        /// <summary>
        /// Gets the temperature dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The temperature dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("Temperature")]
        public double Temperature;
        /// <summary>
        /// Gets the amount of substance (moles) dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The amount of substance (moles) dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("AmountOfSubstance")]
        public double AmountOfSubstance;
        /// <summary>
        /// Gets the luminousity dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The luminousity dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("Luminous")]
        public double Luminous;
        /// <summary>
        /// Gets the financial currency dimensionality associated with the unit category.
        /// </summary>
        /// <remarks>
        /// <para>The financial currency dimensionality of the unit category.
        /// </para>
        /// </remarks>
        [JsonProperty("Currency")]
        public double Currency;
    };
}
