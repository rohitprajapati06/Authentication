namespace OIDC.Models;

public class ExternalLoginViewModel
{
    public string Provider { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string GivenName { get; set; }
    public string Surname { get; set; }
    public List<ClaimViewModel> AllClaims { get; set; }
}

public class ClaimViewModel
{
    public string Type { get; set; }
    public string Value { get; set; }
}
