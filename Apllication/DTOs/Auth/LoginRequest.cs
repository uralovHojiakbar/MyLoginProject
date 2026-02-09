using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public sealed record LoginRequest(string Email, string Password);
}
