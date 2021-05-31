using BubbleChartOilWells.DataAccess.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BubbleChartOilWells.DataAccess.Repositories
{
    public class OilWellRepository
    {
        private readonly string jsonFileName;

        public OilWellRepository(string jsonFileName)
        {
            this.jsonFileName = jsonFileName;
        }

        public void BulkAdd(List<OilWell> oilWells)
        {
            var options = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            
            var jsonData = JsonConvert.SerializeObject(oilWells, options);
            File.WriteAllText(jsonFileName, jsonData);
        }

        public List<OilWell> GetAll()
        {
            var jsonData = File.ReadAllText(jsonFileName);
            var oilWells = JsonConvert.DeserializeObject<List<OilWell>>(jsonData);
            return oilWells;
        }
    }
}
