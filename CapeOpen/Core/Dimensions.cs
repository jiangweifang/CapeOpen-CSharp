using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CapeOpen
{

    /// <summary>
    /// Static class representing support for CAPE-OPEN dimensionalty and units of measures for real-valued
    /// parameters.
    /// </summary>
    /// <remarks>
    /// This class supports the use of CAPE-OPEN dimensionalities and conversion between SI and customary units of
    /// measure for real-valued parameters.
    /// </remarks>
    static class Dimensions
    {
        static List<UnitInfo> units;
        static List<UnitCategoryInfo> unitCategories;

        /// <summary>
        /// Initializes the static fields of the <see cref = "Dimensions"/> class
        /// </summary>
        /// <remarks>Loads units and unit category data from JSON files.</remarks>
        static Dimensions()
        {
            units = new List<UnitInfo>();
            unitCategories = new List<UnitCategoryInfo>();
            System.AppDomain domain = System.AppDomain.CurrentDomain;

            // Load units from embedded JSON resource
            var unitList = JsonConvert.DeserializeObject<List<UnitInfo>>(Properties.Resources.units);
            foreach (var u in unitList)
            {
                var newUnit = u;
                newUnit.Name = u.Name?.Trim();
                units.Add(newUnit);
            }

            // Load user-defined units from runtime file path (JSON or XML)
            String userUnitPath = String.Concat(domain.BaseDirectory, "//data//user_defined_UnitsResult.json");
            if (System.IO.File.Exists(userUnitPath))
            {
                string json = System.IO.File.ReadAllText(userUnitPath);
                var userUnits = JsonConvert.DeserializeObject<List<UnitInfo>>(json);
                foreach (var u in userUnits)
                {
                    var newUnit = u;
                    newUnit.Name = u.Name?.Trim();
                    units.Add(newUnit);
                }
            }
            else
            {
                // Fallback: try legacy XML path
                String userUnitXmlPath = String.Concat(domain.BaseDirectory, "//data//user_defined_UnitsResult.XML");
                if (System.IO.File.Exists(userUnitXmlPath))
                {
                    System.Xml.XmlDocument reader = new System.Xml.XmlDocument();
                    reader.Load(userUnitXmlPath);
                    System.Xml.XmlNodeList list = reader.SelectNodes("Units/Unit_Specs");
                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(0x0409, false);
                    for (int i = 0; i < list.Count; i++)
                    {
                        UnitInfo newUnit = new UnitInfo();
                        String UnitName = list[i].SelectSingleNode("Unit").InnerText;
                        newUnit.Name = UnitName.Trim();
                        newUnit.Category = list[i].SelectSingleNode("Category").InnerText;
                        newUnit.ConversionTimes = Convert.ToDouble(list[i].SelectSingleNode("ConversionTimes").InnerText, ci.NumberFormat);
                        newUnit.ConversionPlus = Convert.ToDouble(list[i].SelectSingleNode("ConversionPlus").InnerText, ci.NumberFormat);
                        units.Add(newUnit);
                    }
                }
            }

            // Load unit categories from embedded JSON resource
            var categoryList = JsonConvert.DeserializeObject<List<UnitCategoryInfo>>(Properties.Resources.unitCategories);
            foreach (var category in categoryList)
            {
                unitCategories.Add(category);
            }

            // Load user-defined unit categories from runtime file path (JSON or XML)
            String userUnitCategoryPath = String.Concat(domain.BaseDirectory, "data//user_defined_units.json");
            if (System.IO.File.Exists(userUnitCategoryPath))
            {
                string json = System.IO.File.ReadAllText(userUnitCategoryPath);
                var userCategories = JsonConvert.DeserializeObject<List<UnitCategoryInfo>>(json);
                foreach (var category in userCategories)
                {
                    unitCategories.Add(category);
                }
            }
            else
            {
                // Fallback: try legacy XML path
                String userUnitCategoryXmlPath = String.Concat(domain.BaseDirectory, "data//user_defined_units.XML");
                if (System.IO.File.Exists(userUnitCategoryXmlPath))
                {
                    System.Xml.XmlDocument reader = new System.Xml.XmlDocument();
                    reader.Load(userUnitCategoryXmlPath);
                    System.Xml.XmlNodeList list = reader.SelectNodes("CategorySpecifications/Category_Spec");
                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(0x0409, false);
                    for (int i = 0; i < list.Count; i++)
                    {
                        String UnitName = list[i].SelectSingleNode("Category").InnerText;
                        UnitCategoryInfo category;
                        category.Name = UnitName;
                        category.AspenUnit = list[i].SelectSingleNode("Aspen").InnerText;
                        category.SI_Unit = list[i].SelectSingleNode("SI_Unit").InnerText;
                        category.Mass = Convert.ToDouble(list[i].SelectSingleNode("Mass").InnerText, ci.NumberFormat);
                        category.Time = Convert.ToDouble(list[i].SelectSingleNode("Time").InnerText, ci.NumberFormat);
                        category.Length = Convert.ToDouble(list[i].SelectSingleNode("Length").InnerText, ci.NumberFormat);
                        category.ElectricalCurrent = Convert.ToDouble(list[i].SelectSingleNode("ElectricalCurrent").InnerText, ci.NumberFormat);
                        category.Temperature = Convert.ToDouble(list[i].SelectSingleNode("Temperature").InnerText, ci.NumberFormat);
                        category.AmountOfSubstance = Convert.ToDouble(list[i].SelectSingleNode("AmountOfSubstance").InnerText, ci.NumberFormat);
                        category.Luminous = Convert.ToDouble(list[i].SelectSingleNode("Luminous").InnerText, ci.NumberFormat);
                        category.Currency = Convert.ToDouble(list[i].SelectSingleNode("Currency").InnerText, ci.NumberFormat);
                        unitCategories.Add(category);
                    }
                }
            }
        }


        /// <summary>
        /// The SI unit asscoiated with a dimensionality.
        /// </summary>
        /// <remarks>
        /// <para>The SI unit of measure associated with a dimensionality.</para>
        /// <para>The dimensionality of the parameter represents the physical dimensional axes of this parameter. It 
        /// is expected that the dimensionality must cover at least 6 fundamental axes (length, mass, time, angle, 
        /// temperature and charge). A possible implementation could consist in being a constant length array vector 
        /// that contains the exponents of each basic SI unit, following directives of SI-brochure (from 
        /// http://www.bipm.fr/). So if we agree on order &lt;m kg s A K,&gt; ... velocity would be &lt;1,0,-1,0,0,0&gt;: 
        /// that is m1 * s-1 =m/s. We have suggested to the  CO Scientific Committee to use the SI base units plus the 
        /// SI derived units with special symbols (for a better usability and for allowing the definition of angles).
        /// </para>
        /// </remarks>
        /// <param name="dimensions">The dimensionality of the unit.</param>
        /// <returns>The SI unit having the desired dimensionality</returns>
        public static String SIUnit(int[] dimensions)
        {
            foreach (UnitCategoryInfo category in unitCategories)
            {
                if (dimensions[0] == category.Length &&
                    dimensions[1] == category.Mass &&
                    dimensions[2] == category.Time &&
                    dimensions[3] == category.ElectricalCurrent &&
                    dimensions[4] == category.Temperature &&
                    dimensions[5] == category.AmountOfSubstance &&
                    dimensions[6] == category.Luminous)
                    return category.SI_Unit;
            }
            return string.Empty;
        }

        /// <summary>
        /// The SI unit asscoiated with a dimensionality.
        /// </summary>
        /// <remarks>
        /// <para>The SI unit of measure associated with a dimensionality.</para>
        /// <para>The dimensionality of the parameter represents the physical dimensional axes of this parameter. It 
        /// is expected that the dimensionality must cover at least 6 fundamental axes (length, mass, time, angle, 
        /// temperature and charge). A possible implementation could consist in being a constant length array vector 
        /// that contains the exponents of each basic SI unit, following directives of SI-brochure (from 
        /// http://www.bipm.fr/). So if we agree on order &lt;m kg s A K,&gt; ... velocity would be &lt;1,0,-1,0,0,0&gt;: 
        /// that is m1 * s-1 =m/s. We have suggested to the  CO Scientific Committee to use the SI base units plus the 
        /// SI derived units with special symbols (for a better usability and for allowing the definition of angles).
        /// </para>
        /// </remarks>
        /// <param name="dimensions">The dimensionality of the unit.</param>
        /// <returns>The SI unit having the desired dimensionality</returns>
        public static String SIUnit(double[] dimensions)
        {
            foreach (UnitCategoryInfo category in unitCategories)
            {
                if (dimensions[0] == category.Length &&
                    dimensions[1] == category.Mass &&
                    dimensions[2] == category.Time &&
                    dimensions[3] == category.ElectricalCurrent &&
                    dimensions[4] == category.Temperature &&
                    dimensions[5] == category.AmountOfSubstance &&
                    dimensions[6] == category.Luminous)
                    return category.SI_Unit;
            }
            return string.Empty;
        }

        /// <summary>
        /// Provides all units supported by the dimensionality package.
        /// </summary>
        /// <remarks>Provides a list of all the units of measure available in this unit conversion package.</remarks>
        /// <value>The list of all units.</value>
        public static String[] Units
        {
            get
            {
                String[] retVal = new String[units.Count];
                for (int i = 0; i < units.Count; i++)
                {
                    retVal[i] = units[i].Name;
                }
                return retVal;
            }
        }

        /// <summary>
        /// A conversion factor used to multiply the value of the measurement by to convert the unit to its SI
        /// equivalent.
        /// </summary>
        /// <remarks>
        /// <para>Units are converted to and from the SI equivalent for the unit category. Unit conversions are 
        /// accomplished by first adding any offset, stored in <see cref = "ConverionsPlus"/> to the value of the unit. 
        /// The sum is then multiplied by the value of the <see cref = "ConverionsTimes"/> for the unit to get the 
        /// measured value in SI units.</para>
        /// </remarks>
        /// <param name="unit">The unit to get the conversion factor for.</param>
        /// <returns>The multiplicative part of the conversion factor.</returns>
        public static double ConverionsTimes(String unit)
        {
            double retVal = 0;
            bool found = false;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Name == unit)
                {
                    retVal = units[i].ConversionTimes;
                    found = true;
                }
            }
            if (!found) throw new CapeOpen.CapeBadArgumentException(String.Concat("Unit: ", unit, " was not found"), 1);
            return retVal;
        }

        /// <summary>
        /// An offset factor used in converting the value of the measurement to its SI equivalent.
        /// </summary>
        /// <remarks>
        /// <para>Units are converted to and from the SI equivalent for the unit category. Unit conversions are 
        /// accomplished by first adding any offset, stored in <see cref = "ConverionsPlus"/> to the value of the unit. 
        /// The sum is then multiplied by the value of the <see cref = "ConverionsTimes"/> for the unit to get the 
        /// measured value in SI units.</para>
        /// </remarks>
        /// <param name="unit">The unit to get the conversion factor for.</param>
        /// <returns>The additive part of the conversion factor.</returns>
        public static double ConverionsPlus(String unit)
        {
            double retVal = 0;
            bool found = false;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Name == unit)
                {
                    retVal = units[i].ConversionPlus;
                    found = true;
                }
            }
            if (!found) throw new CapeOpen.CapeBadArgumentException(String.Concat("Unit: ", unit, " was not found"), 1);
            return retVal;
        }

        /// <summary>
        /// The category  of the unit of measure
        /// </summary>
        /// <remarks><para>The category for a unit of measure defines the dimensionality of the unit.</para>
        /// </remarks>
        /// <param name="unit">The unit to get the category of.</param>
        /// <returns>The unit category.</returns>
        public static String UnitCategory(String unit)
        {
            String retVal = String.Empty;
            bool found = false;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Name == unit)
                {
                    retVal = units[i].Category;
                    found = true;
                }
            }
            if (!found) throw new CapeOpen.CapeBadArgumentException(String.Concat("Unit: ", unit, " was not found"), 1);
            return retVal;
        }

        /// <summary>
        /// Gets the name of the SI unit associated with the unit category, e.g. Pascals for pressure.
        /// </summary>
        /// <remarks>
        /// <para>The SI unit is the basis for conversions between any two units of the same category, either SI or 
        /// customary.
        /// </para>
        /// </remarks>
        /// <returns>The Aspen(TM) display unit that corresponds to the current unit.</returns>
        /// <param name="unit">The unit to get the Aspen (TM) unit for.</param>
        public static String AspenUnit(String unit)
        {
            String retVal = String.Empty;
            String category = String.Empty;
            bool found = false;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Name == unit)
                {
                    category = units[i].Category;
                    found = true;
                }
            }
            for (int i = 0; i < unitCategories.Count; i++)
            {
                if (unitCategories[i].Name == category)
                {
                    retVal = unitCategories[i].AspenUnit;
                    found = true;
                }
            }
            if (!found) throw new CapeOpen.CapeBadArgumentException(String.Concat("Unit: ", unit, " was not found"), 1);
            return retVal;
        }

        /// <summary>
        /// A description of the unit of measure
        /// </summary>
        /// <remarks>The description of the unit of measure.</remarks>
        /// <param name="unit">The unit to get the conversion factor for.</param>
        /// <returns>The description of the unit of measure.</returns>
        public static String UnitDescription(String unit)
        {
            String retVal = String.Empty;
            bool found = false;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Name == unit)
                {
                    retVal = units[i].Description;
                    found = true;
                }
            }
            if (!found) throw new CapeOpen.CapeBadArgumentException(String.Concat("Unit: ", unit, " was not found"), 1);
            return retVal;
        }

        ///// <summary>
        ///// Changes the description of the unit of measure
        ///// </summary>
        ///// <remarks>Changes the description of the unit of measure.</remarks>
        //public static void ChangeUnitDescription(String unit, String newDescription)
        //{
        //    bool found = false;
        //    for (int i = 0; i < units.Count; i++)
        //    {
        //        CapeOpen.UnitInfo current = (CapeOpen.UnitInfo)units[i];
        //        if (current.Name == unit)
        //        {
        //            current.Description = newDescription;
        //            found = true;
        //        }
        //    }
        //    if (!found) throw new CapeOpen.CapeBadArgumentException(String.Concat("Unit: ", unit, " was not found"), 1);
        //}

        /// <summary>
        /// Returns all units matching the unit category.
        /// </summary>
        /// <remarks>A unit category represents a specific combination of dimsionality values. Examples would be 
        /// pressure or temperature. This method would return all units that are in the category, such as Celius,
        /// Kelvin, Farehnheit, and Rankine for temperature.</remarks>
        /// <param name="category">The catgeory of the desired units.</param>
        /// <returns>All units that represent the categoery.</returns>
        public static String[] UnitsMatchingCategory(String category)
        {
            List<string> unitNames = new List<string>();
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Category == category)
                {
                    unitNames.Add(units[i].Name);
                }
            }
            return unitNames.ToArray();
        }

        /// <summary>
        /// Returns the SI unit associated with the unit.
        /// </summary>
        /// <remarks>A unit category represents a specific combination of dimsionality values. Examples would be 
        /// pressure or temperature. This method would return the SI unit for the category, such as Kelvin (K) for 
        /// temperature or Pascal (N/m^2) for pressure..</remarks>
        /// <param name="Unit">The unit to get the SI unit of.</param>
        /// <returns>The SI unit that corresponds to the unit.</returns>
        public static String FindSIUnit(String Unit)
        {
            String retVal = String.Empty;
            String category = UnitCategory(Unit);
            for (int i = 0; i < unitCategories.Count; i++)
            {
                if (unitCategories[i].Name == category)
                {
                    retVal = unitCategories[i].SI_Unit;
                }
            }
            return retVal;
        }

        /// <summary>
        /// The dimensioality of the unit of measure.
        /// </summary>
        /// <remarks>
        /// <para>The dimensionality of the parameter represents the physical dimensional axes of this parameter. It 
        /// is expected that the dimensionality must cover at least 6 fundamental axes (length, mass, time, angle, 
        /// temperature and charge). A possible implementation could consist in being a constant length array vector 
        /// that contains the exponents of each basic SI unit, following directives of SI-brochure (from 
        /// http://www.bipm.fr/). So if we agree on order &lt;m kg s A K,&gt; ... velocity would be &lt;1,0,-1,0,0,0&gt;: 
        /// that is m1 * s-1 =m/s. We have suggested to the  CO Scientific Committee to use the SI base units plus the 
        /// SI derived units with special symbols (for a better usability and for allowing the definition of angles).
        /// </para>
        /// </remarks>
        /// <param name="unit">The unit to get the dimensionality of.</param>
        /// <returns>The dimenality of the unit.</returns>
        public static double[] Dimensionality(String unit)
        {
            string category = CapeOpen.Dimensions.UnitCategory(unit);
            double[] retVal = { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < unitCategories.Count; i++)
            {
                if (unitCategories[i].Name == category)
                {
                    retVal[0] = unitCategories[i].Length;
                    retVal[1] = unitCategories[i].Mass;
                    retVal[2] = unitCategories[i].Time;
                    retVal[3] = unitCategories[i].ElectricalCurrent;
                    retVal[4] = unitCategories[i].Temperature;
                    retVal[5] = unitCategories[i].AmountOfSubstance;
                    retVal[6] = unitCategories[i].Luminous;
                    retVal[7] = unitCategories[i].Currency;
                }
            }
            return retVal;
        }
    };
}
