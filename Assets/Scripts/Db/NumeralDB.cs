using SQLite;
using System.Linq;

[Table("Numerals")]
public class NumeralDB
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Word { get; set; }
    public string Transcription { get; set; }
    public string Type { get; set; }
    public int Digit { get; set; }
    public string RU { get; set; }
    public string EN { get; set; }
    public string Sound { get; set; }
    public string Lang { get; set; }
}
