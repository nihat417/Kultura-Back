namespace Kultura.Application.Dto.RestaurntDtos
{
    public class AddSocialLinkRequest
    {
        public string Url { get; set; } = default!;
        public string SocialType { get; set; } = default!;
    }
}
