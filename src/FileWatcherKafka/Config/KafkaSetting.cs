namespace FileWatcherKafka;

public record KafkaSetting
{
    public string? Server { get; init; }
    public string? Topic { get; init; }
}
