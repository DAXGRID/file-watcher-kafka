namespace FileWatcherKafka
{
    public record KafkaSetting
    {
        public string? Consumer { get; init; }
        public string? Server { get; init; }
        public string? Topic { get; init; }
    }
}
