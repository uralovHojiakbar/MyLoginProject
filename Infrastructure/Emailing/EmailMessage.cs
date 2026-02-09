using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Emailing;

public sealed record EmailMessage(string ToEmail, string Subject, string Body);
