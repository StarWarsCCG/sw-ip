namespace SwIpToGemp;

record CardMapping
{
    public int SwIpId { get; init; }
    public string GempId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}
