namespace BubbleChartOilWells.Contracts.Models.Dto
{
    /// <summary>
    /// Скважина. (Excel)
    /// </summary>
    public class OilWellExcelDto
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
    }
}
