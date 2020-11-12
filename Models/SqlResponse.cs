using System;

namespace StudentList.Models
{
    public class SqlResponse
    {
        public int rowcount { get; set; }

        public object[][] rows { get; set; }
    }
}