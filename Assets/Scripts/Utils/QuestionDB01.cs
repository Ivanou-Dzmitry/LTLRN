using SQLite;

[Table("questions")]
public class QuestionDB
{
    [PrimaryKey]
    public string qSpriteFile { get; set; }
    // Store as comma-separated values
    public string answerVariantsText { get; set; }
    public string answerVariantsSprite { get; set; }
    public string qSoundClipName { get; set; }

}
