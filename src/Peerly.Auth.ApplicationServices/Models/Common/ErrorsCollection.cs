using System.Collections.Generic;

namespace Peerly.Auth.ApplicationServices.Models.Common;

public sealed class ErrorsCollection : Dictionary<string, string[]>
{
    public bool HasErrors => Count > 0;
}
