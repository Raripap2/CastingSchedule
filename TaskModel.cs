using CastingSchedule.Domain.Models;
using System.Text.Json.Serialization;

namespace CastingShedule.Models
{
    public class TaskModel
    {
        [JsonPropertyName("HEAT")]
        public string HEAT {  get; set; }

        [JsonPropertyName("NUM_AGGR")]
        public int NUM_AGGR { get; set; }

        [JsonPropertyName("TASK_ID")]
        public int TASK_ID { get; set; }

        [JsonPropertyName("START_TIME")]
        public DateTime START_TIME { get; set; }

        [JsonPropertyName("END_TIME")]
        public DateTime END_TIME { get; set; }

        [JsonPropertyName("TCREATED")]
        public DateTime TCREATED { get; set; }
    }
}
