namespace SwIpToGemp;

record CardFace
{
    public static readonly CardFace Default = new CardFace();
    
    public string Title { get; init; } = string.Empty;
}
