using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore_EchoBot_With_State
{
    public class ConversationInfo
    {
        public int TurnCount { get; set; } = 0;

        public bool IsNameExpected { get; set; } = false;

        public string Name { get; set; } = null;
    }
}
