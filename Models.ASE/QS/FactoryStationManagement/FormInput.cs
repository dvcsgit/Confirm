using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.FactoryStationManagement
{
    public class FormInput
    {
        public string Stations { get; set; }

        public List<string> StationList
        {
            get
            {
                if (!string.IsNullOrEmpty(Stations))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Stations);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
