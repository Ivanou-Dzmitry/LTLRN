using SQLite;
using System.Linq;

[Table("Themes")]
public class ThemesDB
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Name { get; set; }
}
