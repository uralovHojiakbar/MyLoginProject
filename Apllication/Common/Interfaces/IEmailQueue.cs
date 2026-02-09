using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IEmailQueue
    {
        void Enqueue(string toEmail, string subject, string body);
    }
}
