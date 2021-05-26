using System;
using System.Collections.Generic;

namespace BubbleChartOilWells.DataAccess.Models
{
    /// <summary>
    /// Объект учета добычи. (модель хранения в .json файле)
    /// </summary>
    public class Objective
    {
        /// <summary>
        /// Объект учета добычи
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Скважина
        /// </summary>
        public OilWell OilWell { get; set; }

        /// <summary>
        /// Ежемесячная добыча объекта учета добычи
        /// </summary>
        public List<MonthlyObjectiveProduction> MonthlyObjectiveProduction { get; set; }
    }
}
