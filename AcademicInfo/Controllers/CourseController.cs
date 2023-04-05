using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json.Serialization;

namespace AcademicInfo.Controllers
{
    public class CourseRequest
    {
        private string AccessToken;
    }
    public class CourseResponse
    {
        [JsonPropertyName("number")]
        public int Number { get; set; } // 강좌 번호

        [JsonPropertyName("year")]
        public int Year { get; set; } // 개설 년도

        [JsonPropertyName("semester")]
        public int Semester { get; set; } // 개설 학기

        [JsonPropertyName("name")]
        public string Name { get; set; } // 교과목명

        [JsonPropertyName("instructor")]
        public string Instructor { get; set; } // 담당교수

        [JsonPropertyName("day")]
        public DayOfWeek Day { get; set; } // 강의요일

        [JsonPropertyName("startTime")]
        public int StartTime { get; set; } // 강의 시작시간

        [JsonPropertyName("endTime")]
        public int EndTime { get; set; } // 강의 종료시간

        [JsonPropertyName("classRoom")]
        public string ClassRoom { get; set; } // 강의실
    }

    [ApiController]
    [Route("[controller]")]
    public class CourseController : ControllerBase
    {

        private readonly ILogger<CourseController> _logger;

        public CourseController(ILogger<CourseController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> POST()
        {
            Console.WriteLine("요청받음");
            var connectionString = "Host=\r\nacademic-info-db.postgres.database.azure.com\r\n;Username=classhub;Password=ch55361!;Database=AcademicInfo";
            var connection = new NpgsqlConnection(connectionString);

            var query = "SELECT * FROM course";
            var results = connection.Query<CourseResponse>(query);

            connection.Dispose();

            return Ok(results);
        }
    };
}