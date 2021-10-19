namespace FileWatcherKafka
{
    public record KafkaSetting
    {
        public string? Consumer { get; set; }
        public string? Server { get; set; }
        public string? Topic { get; set; }
    }
}
