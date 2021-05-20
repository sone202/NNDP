using System.Collections.Generic;

namespace BubbleChartOilWells.DataAccess.Models
{
    /// <summary>
    /// Скважина. (модель хранения в .json файле)
    /// </summary>
    public class OilWell
    {
        /// <summary>
        /// Регион
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Месторождение
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Площадь
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Скважина
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Координата X
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Координата Y
        /// </summary>
        public double Y { get; set; }


        /// <summary>
        /// Объекты учета добычи
        /// </summary>
        /// 
        

        public List<Objective> Objectives { get; set; } = new List<Objective>();
    }
}
