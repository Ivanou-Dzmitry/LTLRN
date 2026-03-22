using SQLite;
using System.Linq;

[Table("Sections")]
public class SectionDB
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Name { get; set; }
    public int QDone { get; set; }
    public int QCorrect { get; set; }
    public string Liked { get; set; }  // "true" or "false" as text    
    public float Time { get; set; }
    public string Complete { get; set; }  // "true" or "false" as text    
    public string Bundle { get; set; }  // "true" or "false" as text        
    public string SelectedLevels { get; set; }  // A0, A1, A2, B1, B2, C1, C2 as comma-separated values    
}
