using System;

namespace BubbleChartOilWells.Contracts.Models.Dto
{
    /// <summary>
    /// Ежемесячная добыча объекта учета добычи. (Excel)
    /// </summary>
    public class MonthlyObjectiveProductionExcelDto
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
        public string OilWellName { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Объект учета добычи
        /// </summary>
        public string ObjectiveName { get; set; }

        /// <summary>
        /// Характер
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Состояние
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Время работы, ч
        /// </summary>
        public double WorkPeriod { get; set; }

        /// <summary>
        /// Дебит жидкости, т/сут
        /// </summary>
        public double LiquidDebit { get; set; }

        /// <summary>
        /// Дебит нефти, т/сут
        /// </summary>
        public double OilDebit { get; set; }

        /// <summary>
        /// Дебит воды, т/сут
        /// </summary>
        public double WaterDebit { get; set; }

        /// <summary>
        /// Обводненность (весовая), д.ед.
        /// </summary>
        public double WaterEncroachment { get; set; }

        /// <summary>
        /// Приемистость, м3/сут
        /// </summary>
        public double InjectionCapacity { get; set; }

        /// <summary>
        /// Добыча жидкости, т
        /// </summary>
        public double LiquidProd { get; set; }

        /// <summary>
        /// Добыча нефти, т
        /// </summary>
        public double OilProd { get; set; }

        /// <summary>
        /// Добыча воды, т
        /// </summary>
        public double WaterProd { get; set; }

        /// <summary>
        /// Закачка, м3
        /// </summary>
        public double Injection { get; set; }

        /// <summary>
        /// Накопленная добыча жидкости, т/сут
        /// </summary>
        public double LiquidProdSum { get; set; }

        /// <summary>
        /// Накопленная добыча нефти, т/сут
        /// </summary>
        public double OilProdSum { get; set; }

        /// <summary>
        /// Накопленная добыча воды, т/сут
        /// </summary>
        public double WaterProdSum { get; set; }
    }
}
