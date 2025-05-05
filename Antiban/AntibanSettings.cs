using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antiban
{
    static public class AntibanSettings
    {
        static public readonly int MinimumSecondsBetweenAnyMessages = 10;
        static public readonly int MinimumSecondsBetweenMessagesSameNumber = 60;
        static public readonly int MinimumSecondsBetweenHighPriorityMessagesSameNumber = 24 * 60 * 60;
        static public readonly int NumberOfNormalizationAttempts = 10;
    }
}
