using System.Collections.Generic;
using System.Windows.Documents;
using Newtonsoft.Json;

namespace BubbleChartOilWells.Contracts.Models.ViewModels
{
    /// <summary>
    /// Объект учета добычи.
    /// </summary>
    public class ObjectiveVM
    {
        /// <summary>
        /// Объект учета добычи
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Скважина
        /// </summary>
        [JsonIgnore]
        public OilWellVM OilWell { get; set; }

        /// <summary>
        /// Ежемесячная добыча объекта учета добычи
        /// </summary>
        public MonthlyObjectiveProductionVM MonthlyObjectiveProduction { get; set; }

        /// <summary>
        /// Добыча по объекту за все время
        /// </summary>
        public List<MonthlyObjectiveProductionVM> MonthlyObjectiveProductions { get; set; }
    }
}