using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Abstractions;

public interface IKmsClientReader
{
    KmsClientInfo GetClientInfo();
}
