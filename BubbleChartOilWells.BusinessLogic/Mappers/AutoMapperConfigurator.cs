using AutoMapper;

namespace BubbleChartOilWells.BusinessLogic.Mappers
{
	/// <summary>
	/// Класс для инициализации AutoMapper
	/// </summary>
	public static class AutoMapperConfigurator
	{
		/// <summary>
		/// Инициализация AutoMapper
		/// </summary>
		public static void Initialize()
		{
			var assemblyName = "BubbleChartOilWells.BusinessLogic";
			Mapper.Initialize(cfg => cfg.AddProfiles(assemblyName));
		}
	}
}
