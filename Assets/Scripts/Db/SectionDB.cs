using SQLite;
using System.Linq;

[Table("Sections")]
public class SectionDB
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Name { get; set; }
    public int QDone { get; set; }
    public string Liked { get; set; }  // "true" or "false" as text    
    public float Time { get; set; }
}
