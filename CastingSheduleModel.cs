using Newtonsoft.Json;
using OfficeOpenXml.Table.PivotTable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;

namespace CastingSchedule.Models
{
    public class ProductionUnit
    {
        [JsonPropertyName("UNIT_NAME")]
        public string? UNIT_NAME { get; set; } = "null";
        [JsonPropertyName("UNIT_NUM")]
        public string UNIT_NUM { get; set; } = "null";

        public string FullUnitName => UNIT_NAME + UNIT_NUM;

        public List<CastingSheduleModelTest> Schedule { get; set; }
    }


    public class CastingSheduleModel
    {
        [JsonPropertyName("ccm_num")]
        public string? TASK_ID { get; set; } = "null";
        [JsonPropertyName("res_id")]
        public string? UNIT_NAME { get; set; } = "null";
        [JsonPropertyName("res_num")]
        public string UNIT_NUM { get; set; } = "null";
        [JsonPropertyName("task")]
        public string? TASK_NUM { get; set; } = "null";
        [JsonPropertyName("heat_no")]
        public string? HEAT_NO { get; set; } = "null";
        [JsonPropertyName("start_date")]
        public DateTime DATE_START { get; set; }
        [JsonPropertyName("end_date")]
        public DateTime DATE_END { get; set; }
        [JsonPropertyName("steel_grade")]
        public string? STEEL_GRADE { get; set; } = "cdvvv";
        [JsonPropertyName("steel_grade_droup")]
        public int? steel_grade_droup { get; set; } = 1;
        [JsonPropertyName("status")]
        public string? STATUS { get; set; } = "null";
        public string? route { get; set; } = "null";
    }

    public class CastingSheduleModelTest
    {
        [JsonPropertyName("SEQ")]
        public double SEQ { get; set; } = 0;

        [JsonPropertyName("TASK_ID")]
        public string? TASK_ID { get; set; } = "null";

        [JsonPropertyName("UNIT_NAME")]
        public string? UNIT_NAME { get; set; } = null;

        [JsonPropertyName("UNIT_NUM")]
        public string UNIT_NUM { get; set; } = null;

        [JsonPropertyName("UNIT_POS")]
        public string UNIT_POS { get; set; } = null;

        [JsonPropertyName("TASK_NUM")]
        public string? TASK_NUM { get; set; } = "null";

        [JsonPropertyName("HEAT_NO")]
        public string? HEAT_NO { get; set; } = null;

        [JsonPropertyName("DATE_START")]
        public DateTime DATE_START { get; set; }

        [JsonPropertyName("DATE_END")]
        public DateTime DATE_END { get; set; }

        [JsonPropertyName("STEEL_GRADE")]
        public string? STEEL_GRADE { get; set; } = "";

        [JsonPropertyName("STEEL_GRADE_GROUP")]
        public int? STEEL_GRADE_GROUP { get; set; } = null;

        [JsonPropertyName("STATUS")]
        public string? STATUS { get; set; } = "null";

        [JsonPropertyName("OPERATION_TYPE")]
        public string? OPERATION_TYPE { get; set; } = null;

        [JsonPropertyName("FullUnitName")]
        public string FullUnitName { get; set; }

        [JsonPropertyName("ROUTE")]
        public int? route { get; set; }

        [JsonPropertyName("DaySerialNumber")]
        public int? DaySerialNumber { get; set; } = null;

        [JsonPropertyName("SeriesSerialNumber")]
        public int? SeriesSerialNumber { get; set; } = null;
    }

    public class DateRequest
    {
        public DateTime? Date { get; set; }
    }

    public class RangeRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class mindata
    {
        [JsonPropertyName("SeriesSerialNumber")]
        public int SeriesSerialNumber { get; set; }

        [JsonPropertyName("STEEL_GRADE")]
        public string? STEEL_GRADE { get; set; } = null;

        [JsonPropertyName("DaySerialNumber")]
        public int DaySerialNumber { get; set; }
    }

    public class dropHeatModel
    {
        public string HEAT_NO { get; set; }
        public DateTime DATE_START { get; set; }
        public DateTime DATE_END { get; set; }
    }
    public class stopsModel
    {
        public int AGGR_CODE { get; set; }

        [JsonPropertyName("FullUnitName")]
        public string FullUnitName => AGGR_CODE switch
        {
            101 => "ГМП-1",
            102 => "ГМП-2",
            103 => "УКП-1А",
            104 => "УКП-1Б",
            105 => "УКП-2А",
            106 => "УКП-2Б",
            107 => "УВС-1",
            108 => "УВС-2",
            109 => "МНЛЗ-1",
            110 => "МНЛЗ-2"
        };

        public string DESCR { get; set; }

        [JsonPropertyName("DATE_START")]
        public DateTime DT_BEG { get; set; }

        [JsonPropertyName("DATE_END")]
        public DateTime DT_END { get; set; }

        [JsonPropertyName("OPERATION_TYPE")]
        public string NOTE { get; set; }

        public DateTime DT_INS { get; set; }
    }
}
