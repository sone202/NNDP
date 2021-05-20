using System.Collections.Generic;
using System.IO;
using BubbleChartOilWells.DataAccess.Models;
using Newtonsoft.Json;

namespace BubbleChartOilWells.DataAccess.Repositories
{
    public class MapRepository
    {
        private readonly string jsonFileName;

        public MapRepository(string jsonFileName)
        {
            this.jsonFileName = jsonFileName;
        }

        public void BulkAdd(List<Map> maps)
        {
            var options = new JsonSerializerSettings()
            {

                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            
            var jsonData = JsonConvert.SerializeObject(maps, options);
            File.WriteAllText(jsonFileName, jsonData);
        }

        public List<OilWell> GetAll()
        {
            //var jsonData = string.Empty;
            //using(var streamReader = new StreamReader(jsonFileName))
            //{
            //    jsonData = streamReader.ReadToEnd();
            //    streamReader.Close();
            //}
            var jsonData = File.ReadAllText(jsonFileName);
            var maps = JsonConvert.DeserializeObject<List<OilWell>>(jsonData);
            return maps;
        }
    }
}
