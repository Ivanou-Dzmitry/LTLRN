using SQLite;
using System.Linq;

[Table("QuestionsData")]
public class QuestionsDataDB
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Name { get; set; }
    public string Correct { get; set; }  // "true" or "false" as text 
}
