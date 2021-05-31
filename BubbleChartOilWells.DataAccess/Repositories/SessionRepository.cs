using System.Collections.Generic;
using System.IO;
using BubbleChartOilWells.DataAccess.Models;
using Newtonsoft.Json;

namespace BubbleChartOilWells.DataAccess.Repositories
{
    public class SessionRepository
    {
        private readonly string jsonFileName;

        public SessionRepository(string jsonFileName)
        {
            this.jsonFileName = jsonFileName;
        }

        public void BulkAdd(Session session)
        {
            var options = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            
            var jsonData = JsonConvert.SerializeObject(session, options);
            File.WriteAllText(jsonFileName, jsonData);
        }

        public Session GetAll()
        {
            var jsonData = File.ReadAllText(jsonFileName);
            var session = JsonConvert.DeserializeObject<Session>(jsonData);
            return session;
        }
    }
}
