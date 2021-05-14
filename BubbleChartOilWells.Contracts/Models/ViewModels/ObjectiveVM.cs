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
    }
}
