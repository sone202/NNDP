using AutoMapper;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Models;
using System.Linq;

namespace BubbleChartOilWells.BusinessLogic.Mappers.ConfigureProfiles
{
    public class OilWellProfile : Profile
    {
        public OilWellProfile()
        {
            // Excel to JSON

            CreateMap<OilWellExcelDto, OilWell>()
                .ForMember(x => x.Objectives, y => y.Ignore());

            CreateMap<MonthlyObjectiveProductionExcelDto, MonthlyObjectiveProduction>()
                .ForMember(x => x.Objective, y => y.Ignore());

            // JSON to view models

            CreateMap<OilWell, OilWellVM>()
                // TODO: добавить маппинг статусов после того как будет известно как их сопоставлять
                .ForMember(x => x.OilWellStatus, y => y.MapFrom(z => OilWellStatus.TrExpLiq));

            CreateMap<Objective, ObjectiveVM>()
                .ForMember(x => x.MonthlyObjectiveProduction, y => y.MapFrom(z => z.MonthlyObjectiveProduction.FirstOrDefault()));

            CreateMap<MonthlyObjectiveProduction, MonthlyObjectiveProductionVM>();
        }
    }
}
