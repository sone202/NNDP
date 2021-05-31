using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Models;
using System.Linq;
using AutoMapper;

namespace BubbleChartOilWells.BusinessLogic.Mappers.ConfigureProfiles
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<MapVM, Map>();
            CreateMap<Map, MapVM>();
        }
    }
}