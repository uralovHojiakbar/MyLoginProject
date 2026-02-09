using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}
