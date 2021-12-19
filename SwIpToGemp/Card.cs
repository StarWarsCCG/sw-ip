namespace SwIpToGemp;

record Card
{
    public int Id { get; init; }
    public string GempId { get; init; } = string.Empty;
    public CardFace Front { get; init; } = CardFace.Default;

    public CardMapping ToCardMapping()
    {
        var result = new CardMapping
        {
            SwIpId = Id,
            GempId = GempId,
            Title = Front.Title.TrimStart('•')
        };

        return result;
    }
}
